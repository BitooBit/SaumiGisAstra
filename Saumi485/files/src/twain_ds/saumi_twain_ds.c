/* SAUMI TWAIN DS: host HP via scanimage daemon.
 * Wine DSM delivers MSG_XFERREADY only via PROCESSEVENT / DAT_NULL.
 * EZTwain (SAUMI) often uses FILE xfer into TMP — implement both native and file. */
#include "twain.h"
#include <string.h>

#define REQ_PATH  "C:\\SAUMI\\temp\\twain\\request"
#define DIB_PATH  "C:\\SAUMI\\temp\\twain\\page.dib"
#define DONE_PATH "C:\\SAUMI\\temp\\twain\\page.dib.done"
#define LOG_PATH  "C:\\SAUMI\\temp\\twain\\ds.log"
#define BMP_PATH  "C:\\SAUMI\\temp\\twain\\page.bmp"

static TW_IDENTITY g_app;
static TW_IDENTITY g_ds;
static DSMENTRYPROC g_dsm;
static TW_UINT16 g_cc;
static int g_open;
static int g_enabled;
static int g_ready;
static int g_pending;
static int g_xferready_sent;
static HANDLE g_dib;
static int g_w, g_h;
static int g_xfermech;
static char g_file[260];
static TW_UINT16 g_filefmt;

static void log_line(const char *s)
{
    HANDLE h;
    DWORD n;
    char buf[320];
    SYSTEMTIME st;
    GetLocalTime(&st);
    wsprintfA(buf, "%02u:%02u:%02u %s\r\n", st.wHour, st.wMinute, st.wSecond, s);
    h = CreateFileA(LOG_PATH, FILE_APPEND_DATA, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (h == INVALID_HANDLE_VALUE) return;
    WriteFile(h, buf, (DWORD)lstrlenA(buf), &n, NULL);
    CloseHandle(h);
}

static void log_trip(const char *phase, TW_UINT32 DG, TW_UINT16 DAT, TW_UINT16 MSG, TW_UINT16 rc)
{
    char b[200];
    wsprintfA(b, "%s DG=%lu DAT=%u MSG=%u rc=%u cc=%u mech=%d ready=%d pend=%d",
              phase, (unsigned long)DG, DAT, MSG, rc, g_cc, g_xfermech, g_ready, g_pending);
    log_line(b);
}

static void str32(TW_STR32 d, const char *s)
{
    int i;
    for (i = 0; i < 33 && s[i]; i++) d[i] = s[i];
    d[i] = 0;
    for (; i < 34; i++) d[i] = 0;
}

static void fill_identity(TW_IDENTITY *id)
{
    TW_UINT32 keep = id->Id;
    ZeroMemory(id, sizeof(*id));
    id->Id = keep;
    id->Version.MajorNum = 1;
    id->Version.MinorNum = 1;
    id->Version.Language = 0;
    id->Version.Country = 7;
    str32(id->Version.Info, "SAUMI SANE bridge");
    id->ProtocolMajor = TWON_PROTOCOLMAJOR;
    id->ProtocolMinor = TWON_PROTOCOLMINOR;
    id->SupportedGroups = DG_CONTROL | DG_IMAGE;
    str32(id->Manufacturer, "Hewlett-Packard");
    str32(id->ProductFamily, "LaserJet");
    str32(id->ProductName, "HP LaserJet M425dw");
}

static void free_dib(void)
{
    if (g_dib) {
        GlobalFree(g_dib);
        g_dib = NULL;
    }
    g_w = 0;
    g_h = 0;
}

static HANDLE load_dib_file(const char *path, int *out_w, int *out_h)
{
    HANDLE hf, mem;
    DWORD sz, rd;
    unsigned char *b;
    DWORD bisize;
    int width, height;
    WORD bitcount;
    char msg[160];

    hf = CreateFileA(path, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hf == INVALID_HANDLE_VALUE) {
        wsprintfA(msg, "open dib fail gle=%lu", GetLastError());
        log_line(msg);
        return NULL;
    }
    sz = GetFileSize(hf, NULL);
    wsprintfA(msg, "dib size=%lu", sz);
    log_line(msg);
    if (sz < 40 || sz > 50u * 1024u * 1024u) {
        CloseHandle(hf);
        log_line("dib size rejected");
        return NULL;
    }
    mem = GlobalAlloc(GPTR, sz);
    if (!mem) {
        CloseHandle(hf);
        log_line("GlobalAlloc fail");
        return NULL;
    }
    b = (unsigned char *)GlobalLock(mem);
    if (!ReadFile(hf, b, sz, &rd, NULL) || rd != sz) {
        GlobalUnlock(mem);
        GlobalFree(mem);
        CloseHandle(hf);
        log_line("dib read fail");
        return NULL;
    }
    CloseHandle(hf);
    bisize = *(DWORD *)b;
    width = *(int *)(b + 4);
    height = *(int *)(b + 8);
    bitcount = *(WORD *)(b + 14);
    wsprintfA(msg, "dib hdr size=%lu w=%d h=%d bpp=%u", bisize, width, height, bitcount);
    log_line(msg);
    if (bisize < 40 || bitcount != 24 || width < 1 || height == 0) {
        GlobalUnlock(mem);
        GlobalFree(mem);
        log_line("dib header rejected");
        return NULL;
    }
    if (out_w) *out_w = width;
    if (out_h) *out_h = height < 0 ? -height : height;
    GlobalUnlock(mem);
    return mem;
}

static int wait_scan(void)
{
    HANDLE hf;
    int i;
    DWORD n;
    const char *msg = "SCAN\n";

    CreateDirectoryA("C:\\SAUMI\\temp", NULL);
    CreateDirectoryA("C:\\SAUMI\\temp\\twain", NULL);
    DeleteFileA(DONE_PATH);
    DeleteFileA(DIB_PATH);
    hf = CreateFileA(REQ_PATH, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hf == INVALID_HANDLE_VALUE) {
        log_line("request create fail");
        return 0;
    }
    WriteFile(hf, msg, 5, &n, NULL);
    CloseHandle(hf);
    log_line("request written, waiting daemon");
    for (i = 0; i < 450; i++) {
        hf = CreateFileA(DONE_PATH, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
        if (hf != INVALID_HANDLE_VALUE) {
            CloseHandle(hf);
            log_line("daemon done");
            return 1;
        }
        Sleep(200);
    }
    log_line("daemon timeout");
    return 0;
}

static DSMENTRYPROC load_dsm(void)
{
    static const char *names[] = {
        "twain_32", "twain_32.dll", "TWAIN_32.DLL", "twaindsm", "twaindsm.dll", NULL
    };
    HMODULE m = NULL;
    int i;
    if (g_dsm) return g_dsm;
    /* Never LoadLibrary a second DSM — empty activeSources, DAT_NULL dies. */
    for (i = 0; names[i]; i++) {
        m = GetModuleHandleA(names[i]);
        if (m) break;
    }
    if (!m) {
        log_line("DSM GetModuleHandle miss");
        return NULL;
    }
    g_dsm = (DSMENTRYPROC)GetProcAddress(m, "DSM_Entry");
    if (!g_dsm) log_line("DSM_Entry export miss");
    return g_dsm;
}

static TW_UINT16 fail(TW_UINT16 cc)
{
    g_cc = cc;
    return TWRC_FAILURE;
}

static TW_UINT16 ok(void)
{
    g_cc = TWCC_SUCCESS;
    return TWRC_SUCCESS;
}

static TW_UINT16 cap_one(TW_CAPABILITY *cap, TW_UINT16 itemType, TW_UINT32 item)
{
    TW_ONEVALUE *ov;
    cap->ConType = TWON_ONEVALUE;
    cap->hContainer = GlobalAlloc(GHND, sizeof(TW_ONEVALUE));
    if (!cap->hContainer) return fail(TWCC_LOWMEMORY);
    ov = (TW_ONEVALUE *)GlobalLock(cap->hContainer);
    ov->ItemType = itemType;
    ov->Item = item;
    GlobalUnlock(cap->hContainer);
    return ok();
}

static TW_UINT32 cap_get_item(TW_CAPABILITY *cap)
{
    TW_ONEVALUE *ov;
    TW_UINT32 v = 0;
    if (!cap || !cap->hContainer) return 0;
    ov = (TW_ONEVALUE *)GlobalLock(cap->hContainer);
    if (ov) {
        v = ov->Item;
        GlobalUnlock(cap->hContainer);
    }
    return v;
}

static TW_UINT16 do_cap(TW_UINT16 MSG, TW_CAPABILITY *cap)
{
    TW_UINT16 c;
    char b[80];
    if (!cap) return fail(TWCC_BADVALUE);
    c = cap->Cap;
    if (MSG == MSG_QUERYSUPPORT)
        return cap_one(cap, TWTY_UINT32, 0x1 | 0x2 | 0x4 | 0x8);
    switch (c) {
    case CAP_XFERCOUNT:
        if (MSG == MSG_SET) return ok();
        return cap_one(cap, TWTY_INT16, 1);
    case ICAP_XFERMECH:
        if (MSG == MSG_SET) {
            g_xfermech = (int)cap_get_item(cap);
            wsprintfA(b, "SET XFERMECH=%d", g_xfermech);
            log_line(b);
            return ok();
        }
        return cap_one(cap, TWTY_UINT16, (TW_UINT32)g_xfermech);
    case ICAP_IMAGEFILEFORMAT:
        if (MSG == MSG_SET) {
            g_filefmt = (TW_UINT16)cap_get_item(cap);
            return ok();
        }
        return cap_one(cap, TWTY_UINT16, g_filefmt ? g_filefmt : TWFF_BMP);
    case ICAP_PIXELTYPE:
        if (MSG == MSG_SET) return ok();
        return cap_one(cap, TWTY_UINT16, TWPT_RGB);
    case ICAP_UNITS:
        return cap_one(cap, TWTY_UINT16, TWUN_INCHES);
    case ICAP_COMPRESSION:
        return cap_one(cap, TWTY_UINT16, TWCP_NONE);
    case ICAP_BITDEPTH:
        return cap_one(cap, TWTY_UINT16, 24);
    case CAP_UICONTROLLABLE:
        return cap_one(cap, TWTY_BOOL, 1);
    case CAP_DEVICEONLINE:
        return cap_one(cap, TWTY_BOOL, 1);
    case CAP_ENABLEDSUIONLY:
        return cap_one(cap, TWTY_BOOL, 0);
    default:
        return fail(TWCC_CAPUNSUPPORTED);
    }
}

static void wake_app(HWND parent)
{
    HWND w;
    if (parent)
        PostMessageA(parent, WM_NULL, 0, 0);
    w = GetActiveWindow();
    if (w) PostMessageA(w, WM_NULL, 0, 0);
    w = GetForegroundWindow();
    if (w) PostMessageA(w, WM_NULL, 0, 0);
}

static void notify_xferready(HWND parent)
{
    DSMENTRYPROC dsm = load_dsm();
    TW_UINT16 rc;
    char b[96];
    if (!dsm) {
        log_line("no DSM_Entry (will use PROCESSEVENT)");
        wake_app(parent);
        return;
    }
    rc = dsm(&g_ds, &g_app, DG_CONTROL, DAT_NULL, MSG_XFERREADY, NULL);
    wsprintfA(b, "DAT_NULL MSG_XFERREADY rc=%u dsId=%lu", rc, (unsigned long)g_ds.Id);
    log_line(b);
    wake_app(parent);
}

static TW_UINT16 do_enable(TW_USERINTERFACE *ui)
{
    HANDLE dib;
    int w = 0, h = 0;
    HWND parent = NULL;
    g_open = 1;
    free_dib();
    g_ready = 0;
    g_pending = 0;
    g_xferready_sent = 0;
    if (ui) parent = (HWND)ui->hParent;
    log_line("ENABLEDS -> scan");
    if (!wait_scan()) return fail(TWCC_OPERATIONERROR);
    dib = load_dib_file(DIB_PATH, &w, &h);
    if (!dib) {
        log_line("bad dib");
        return fail(TWCC_OPERATIONERROR);
    }
    g_dib = dib;
    g_w = w;
    g_h = h;
    g_enabled = 1;
    g_ready = 1;
    g_pending = 1;
    notify_xferready(parent);
    return ok();
}

static void set_fix(TW_FIX32 *f, int whole)
{
    f->Whole = (TW_INT16)whole;
    f->Frac = 0;
}

static TW_UINT16 do_layout(TW_UINT16 MSG, TW_IMAGELAYOUT *lay)
{
    int inches_w, inches_h;
    if (!lay) return fail(TWCC_BADVALUE);
    if (MSG == MSG_SET) return ok();
    ZeroMemory(lay, sizeof(*lay));
    inches_w = g_w > 0 ? (g_w + 149) / 150 : 8;
    inches_h = g_h > 0 ? (g_h + 149) / 150 : 11;
    set_fix(&lay->Frame.Left, 0);
    set_fix(&lay->Frame.Top, 0);
    set_fix(&lay->Frame.Right, inches_w);
    set_fix(&lay->Frame.Bottom, inches_h);
    lay->DocumentNumber = 1;
    lay->PageNumber = 1;
    lay->FrameNumber = 1;
    return ok();
}

static TW_UINT16 do_imageinfo(TW_IMAGEINFO *inf)
{
    if (!inf || !g_ready) return fail(TWCC_SEQERROR);
    ZeroMemory(inf, sizeof(*inf));
    inf->XResolution.Whole = 150;
    inf->YResolution.Whole = 150;
    inf->ImageWidth = g_w;
    inf->ImageLength = g_h;
    inf->SamplesPerPixel = 3;
    inf->BitsPerSample[0] = 8;
    inf->BitsPerSample[1] = 8;
    inf->BitsPerSample[2] = 8;
    inf->BitsPerPixel = 24;
    inf->Planar = 0;
    inf->PixelType = TWPT_RGB;
    inf->Compression = TWCP_NONE;
    return ok();
}

static TW_UINT16 do_native(TW_MEMREF pData)
{
    HANDLE *ph;
    if (!pData || !g_dib) return fail(TWCC_SEQERROR);
    ph = (HANDLE *)pData;
    *ph = g_dib;
    g_dib = NULL;
    g_pending = 0;
    log_line("IMAGENATIVEXFER done");
    return TWRC_XFERDONE;
}

static TW_UINT16 do_setupfile(TW_UINT16 MSG, TW_SETUPFILEXFER *sf)
{
    const char *defp = BMP_PATH;
    int i;
    if (!sf) return fail(TWCC_BADVALUE);
    if (MSG == MSG_SET) {
        for (i = 0; i < 259 && sf->FileName[i]; i++)
            g_file[i] = sf->FileName[i];
        g_file[i] = 0;
        g_filefmt = sf->Format ? sf->Format : TWFF_BMP;
        g_xfermech = TWSX_FILE;
        log_line("SETUPFILEXFER SET");
        log_line(g_file);
        return ok();
    }
    ZeroMemory(sf, sizeof(*sf));
    lstrcpynA(sf->FileName, g_file[0] ? g_file : defp, 255);
    sf->Format = g_filefmt ? g_filefmt : TWFF_BMP;
    sf->VRefNum = 0;
    return ok();
}

static TW_UINT16 do_filexfer(void)
{
    HANDLE hf;
    DWORD n, sz;
    void *p;
    BITMAPFILEHEADER bf;
    const char *path;
    if (!g_dib) return fail(TWCC_SEQERROR);
    path = g_file[0] ? g_file : BMP_PATH;
    p = GlobalLock(g_dib);
    if (!p) return fail(TWCC_LOWMEMORY);
    sz = GlobalSize(g_dib);
    ZeroMemory(&bf, sizeof(bf));
    bf.bfType = 0x4D42;
    bf.bfSize = 14 + sz;
    bf.bfOffBits = 14 + 40;
    hf = CreateFileA(path, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hf == INVALID_HANDLE_VALUE) {
        GlobalUnlock(g_dib);
        log_line("filexfer create fail");
        log_line(path);
        return fail(TWCC_OPERATIONERROR);
    }
    WriteFile(hf, &bf, sizeof(bf), &n, NULL);
    WriteFile(hf, p, sz, &n, NULL);
    CloseHandle(hf);
    GlobalUnlock(g_dib);
    g_pending = 0;
    log_line("IMAGEFILEXFER done");
    log_line(path);
    return TWRC_XFERDONE;
}

static TW_UINT16 do_event(TW_EVENT *ev)
{
    if (!ev) return TWRC_NOTDSEVENT;
    if (g_ready && g_pending) {
        ev->TWMessage = MSG_XFERREADY;
        g_xferready_sent = 1;
        log_line("PROCESSEVENT -> MSG_XFERREADY");
        return TWRC_DSEVENT;
    }
    ev->TWMessage = MSG_NULL;
    return TWRC_NOTDSEVENT;
}

__declspec(dllexport) TW_UINT16 WINAPI DS_Entry(
    TW_IDENTITY *pOrigin,
    TW_UINT32 DG,
    TW_UINT16 DAT,
    TW_UINT16 MSG,
    TW_MEMREF pData)
{
    TW_UINT16 rc;

    if (pOrigin && (MSG == MSG_OPENDS || MSG == MSG_ENABLEDS))
        g_app = *pOrigin;

    if (DG == DG_CONTROL && DAT == DAT_IDENTITY && MSG == MSG_GET) {
        TW_IDENTITY *id = (TW_IDENTITY *)pData;
        if (!id) return fail(TWCC_BADVALUE);
        fill_identity(id);
        return ok();
    }
    if (DG == DG_CONTROL && DAT == DAT_STATUS && MSG == MSG_GET) {
        TW_STATUS *st = (TW_STATUS *)pData;
        if (!st) return fail(TWCC_BADVALUE);
        st->ConditionCode = g_cc;
        st->Reserved = 0;
        return TWRC_SUCCESS;
    }
    if (DG == DG_CONTROL && DAT == DAT_IDENTITY && MSG == MSG_OPENDS) {
        if (pData) g_ds = *(TW_IDENTITY *)pData;
        fill_identity(&g_ds);
        if (pData) *(TW_IDENTITY *)pData = g_ds;
        g_open = 1;
        g_enabled = 0;
        g_ready = 0;
        g_pending = 0;
        g_xferready_sent = 0;
        g_xfermech = TWSX_NATIVE;
        g_file[0] = 0;
        g_filefmt = TWFF_BMP;
        load_dsm();
        log_line("OPENDS");
        return ok();
    }
    if (DG == DG_CONTROL && DAT == DAT_IDENTITY && MSG == MSG_CLOSEDS) {
        free_dib();
        g_open = 0;
        g_enabled = 0;
        g_ready = 0;
        log_line("CLOSEDS");
        return ok();
    }
    if (DG == DG_CONTROL && DAT == DAT_USERINTERFACE && MSG == MSG_ENABLEDS) {
        rc = do_enable((TW_USERINTERFACE *)pData);
        log_trip("ENABLEDS", DG, DAT, MSG, rc);
        return rc;
    }
    if (DG == DG_CONTROL && DAT == DAT_USERINTERFACE && (MSG == MSG_DISABLEDS || MSG == MSG_ENABLEDSUIONLY)) {
        g_enabled = 0;
        return ok();
    }
    if (DG == DG_CONTROL && DAT == DAT_CAPABILITY) {
        rc = do_cap(MSG, (TW_CAPABILITY *)pData);
        log_trip("CAP", DG, DAT, MSG, rc);
        return rc;
    }
    if (DG == DG_CONTROL && DAT == DAT_PENDINGXFERS) {
        TW_PENDINGXFERS *px = (TW_PENDINGXFERS *)pData;
        if (!px) return fail(TWCC_BADVALUE);
        if (MSG == MSG_ENDXFER || MSG == MSG_RESET) {
            px->Count = 0;
            g_pending = 0;
            g_ready = 0;
            return ok();
        }
        px->Count = (TW_UINT16)(g_pending ? 1 : 0);
        return ok();
    }
    if (DG == DG_CONTROL && DAT == DAT_XFERGROUP && (MSG == MSG_GET || MSG == MSG_GETCURRENT)) {
        if (pData) *(TW_UINT32 *)pData = DG_IMAGE;
        return ok();
    }
    if (DG == DG_CONTROL && DAT == DAT_SETUPFILEXFER)
        return do_setupfile(MSG, (TW_SETUPFILEXFER *)pData);
    if (DG == DG_CONTROL && DAT == DAT_EVENT && MSG == MSG_PROCESSEVENT)
        return do_event((TW_EVENT *)pData);
    if (DG == DG_IMAGE && DAT == DAT_IMAGELAYOUT)
        return do_layout(MSG, (TW_IMAGELAYOUT *)pData);
    if (DG == DG_IMAGE && DAT == DAT_IMAGEINFO && MSG == MSG_GET)
        return do_imageinfo((TW_IMAGEINFO *)pData);
    if (DG == DG_IMAGE && DAT == DAT_IMAGENATIVEXFER && MSG == MSG_GET)
        return do_native(pData);
    if (DG == DG_IMAGE && DAT == DAT_IMAGEFILEXFER && MSG == MSG_GET)
        return do_filexfer();

    log_trip("UNHANDLED", DG, DAT, MSG, TWRC_FAILURE);
    return fail(TWCC_BADPROTOCOL);
}
