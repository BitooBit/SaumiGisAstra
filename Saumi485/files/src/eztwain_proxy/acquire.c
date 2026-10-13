/* SAUMI: TWAIN_AcquireNative + DIB_* here. Do not hand our HDIB to orig —
 * orig GlobalLock's a GPTR pointer as a moveable handle → AV read 00000004. */
#include <windows.h>

void *p_TWAIN_Shutdown;
void *p_TWAIN_ApplicationLicense;
static HINSTANCE g_inst;
static HWND g_spin;
static int g_spin_frame;
static int g_busy;
static int g_got_page;
static DWORD g_page_tick;
static HANDLE g_keep;

__declspec(dllexport) void WINAPI TWAIN_Shutdown(void)
{
    void (WINAPI *p)(void) = (void (WINAPI *)(void))p_TWAIN_Shutdown;
    if (p) p();
}

__declspec(dllexport) void WINAPI TWAIN_ApplicationLicense(const char *a, const char *b, const char *c)
{
    void (WINAPI *p)(const char *, const char *, const char *) =
        (void (WINAPI *)(const char *, const char *, const char *))p_TWAIN_ApplicationLicense;
    if (p) p(a, b, c);
}

#define REQ_PATH  "C:\\SAUMI\\temp\\twain\\request"
#define DIB_PATH  "C:\\SAUMI\\temp\\twain\\page.dib"
#define DONE_PATH "C:\\SAUMI\\temp\\twain\\page.dib.done"
#define LOG_PATH  "C:\\SAUMI\\temp\\twain\\proxy.log"

static int g_src_idx;
static int g_state = 3;
static char g_multi_path[260];

static void log_line(const char *s)
{
    HANDLE h;
    DWORD n;
    char buf[320];
    SYSTEMTIME st;
    GetLocalTime(&st);
    wsprintfA(buf, "%02u:%02u:%02u %s\r\n", st.wHour, st.wMinute, st.wSecond, s);
    CreateDirectoryA("C:\\SAUMI\\temp", NULL);
    CreateDirectoryA("C:\\SAUMI\\temp\\twain", NULL);
    h = CreateFileA(LOG_PATH, FILE_APPEND_DATA, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (h == INVALID_HANDLE_VALUE) return;
    WriteFile(h, buf, (DWORD)lstrlenA(buf), &n, NULL);
    FlushFileBuffers(h);
    CloseHandle(h);
}

static int u8_to_w(const char *u8, WCHAR *w, int n)
{
    int k = MultiByteToWideChar(CP_UTF8, 0, u8, -1, w, n);
    if (k > 0) return k;
    return MultiByteToWideChar(CP_ACP, 0, u8, -1, w, n);
}

static void msg_u8(HWND hwnd, const char *u8)
{
    WCHAR body[420], cap[16];
    u8_to_w(u8, body, 420);
    u8_to_w("SAUMI", cap, 16);
    MessageBoxW(hwnd, body, cap, MB_OK | MB_ICONWARNING);
}

static const char *spin_text(void)
{
    switch (g_spin_frame & 3) {
    case 0: return "Обращение к сканеру  |";
    case 1: return "Обращение к сканеру  /";
    case 2: return "Обращение к сканеру  -";
    default: return "Обращение к сканеру  \\";
    }
}

static LRESULT CALLBACK spin_proc(HWND w, UINT m, WPARAM wp, LPARAM lp)
{
    PAINTSTRUCT ps;
    HDC dc;
    RECT r;
    if (m == WM_PAINT) {
        dc = BeginPaint(w, &ps);
        GetClientRect(w, &r);
        SetBkMode(dc, TRANSPARENT);
        SetTextColor(dc, GetSysColor(COLOR_BTNTEXT));
        {
            WCHAR wt[80];
            u8_to_w(spin_text(), wt, 80);
            DrawTextW(dc, wt, -1, &r, DT_CENTER | DT_VCENTER | DT_SINGLELINE);
        }
        EndPaint(w, &ps);
        return 0;
    }
    if (m == WM_SETCURSOR) {
        SetCursor(LoadCursorA(NULL, IDC_WAIT));
        return TRUE;
    }
    return DefWindowProcA(w, m, wp, lp);
}

static void spin_pump(void)
{
    MSG msg;
    while (g_spin && PeekMessageA(&msg, g_spin, 0, 0, PM_REMOVE)) {
        TranslateMessage(&msg);
        DispatchMessageA(&msg);
    }
    SetCursor(LoadCursorA(NULL, IDC_WAIT));
}

static void spin_open(HWND parent)
{
    WNDCLASSA wc;
    RECT pr;
    int ww = 380, hh = 96, x, y;
    if (g_spin) return;
    ZeroMemory(&wc, sizeof(wc));
    wc.lpfnWndProc = spin_proc;
    wc.hInstance = g_inst;
    wc.hbrBackground = (HBRUSH)(COLOR_BTNFACE + 1);
    wc.hCursor = LoadCursorA(NULL, IDC_WAIT);
    wc.lpszClassName = "SaumiScanSpin";
    RegisterClassA(&wc);
    x = (GetSystemMetrics(SM_CXSCREEN) - ww) / 2;
    y = (GetSystemMetrics(SM_CYSCREEN) - hh) / 2;
    if (parent && GetWindowRect(parent, &pr)) {
        x = pr.left + (pr.right - pr.left - ww) / 2;
        y = pr.top + (pr.bottom - pr.top - hh) / 2;
    }
    g_spin_frame = 0;
    /* не TOPMOST: иначе окно остаётся поверх «Передать в SAUMI» и клик пропадает */
    g_spin = CreateWindowExA(WS_EX_TOOLWINDOW,
                             "SaumiScanSpin", "SAUMI",
                             WS_POPUP | WS_CAPTION | WS_VISIBLE,
                             x, y, ww, hh, NULL, NULL, g_inst, NULL);
    if (g_spin) {
        ShowWindow(g_spin, SW_SHOW);
        UpdateWindow(g_spin);
    }
}

static void spin_tick(void)
{
    g_spin_frame++;
    if (g_spin) InvalidateRect(g_spin, NULL, TRUE);
    spin_pump();
}

static void spin_close(void)
{
    HWND w = g_spin;
    g_spin = NULL;
    if (w) {
        ShowWindow(w, SW_HIDE);
        DestroyWindow(w);
    }
}

static int wait_scan(HWND parent)
{
    HANDLE hf;
    DWORD n, sz;
    int i;
    const char *msg = "SCAN\n";
    DeleteFileA(DONE_PATH);
    DeleteFileA(DIB_PATH);
    hf = CreateFileA(REQ_PATH, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hf == INVALID_HANDLE_VALUE) {
        log_line("request fail");
        return 0;
    }
    WriteFile(hf, msg, 5, &n, NULL);
    CloseHandle(hf);
    log_line("request written");
    spin_open(parent);
    for (i = 0; i < 450; i++) {
        spin_tick();
        hf = CreateFileA(DONE_PATH, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
        if (hf != INVALID_HANDLE_VALUE) {
            CloseHandle(hf);
            hf = CreateFileA(DIB_PATH, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
            if (hf != INVALID_HANDLE_VALUE) {
                sz = GetFileSize(hf, NULL);
                CloseHandle(hf);
                if (sz >= 40 && sz < 50u * 1024u * 1024u) {
                    log_line("daemon done");
                    spin_close();
                    return 1;
                }
            }
            log_line("daemon fail (no dib)");
            spin_close();
            return 0;
        }
        Sleep(200);
    }
    log_line("daemon timeout");
    spin_close();
    return 0;
}

static HANDLE load_dib(void)
{
    HANDLE hf, mem;
    DWORD sz, rd;
    void *p;
    BITMAPINFOHEADER *bih;
    char msg[120];

    hf = CreateFileA(DIB_PATH, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hf == INVALID_HANDLE_VALUE) {
        log_line("open dib fail");
        return NULL;
    }
    sz = GetFileSize(hf, NULL);
    if (sz < 40 || sz > 50u * 1024u * 1024u) {
        CloseHandle(hf);
        log_line("dib size bad");
        return NULL;
    }
    mem = GlobalAlloc(GHND, sz);
    if (!mem) {
        CloseHandle(hf);
        log_line("GlobalAlloc fail");
        return NULL;
    }
    p = GlobalLock(mem);
    if (!p || !ReadFile(hf, p, sz, &rd, NULL) || rd != sz) {
        if (p) GlobalUnlock(mem);
        GlobalFree(mem);
        CloseHandle(hf);
        log_line("read dib fail");
        return NULL;
    }
    CloseHandle(hf);
    bih = (BITMAPINFOHEADER *)p;
    wsprintfA(msg, "dib %lu bytes bpp=%u w=%d h=%d", sz, bih->biBitCount, bih->biWidth, bih->biHeight);
    log_line(msg);
    if (bih->biSize < 40 || bih->biBitCount != 24) {
        GlobalUnlock(mem);
        GlobalFree(mem);
        log_line("dib hdr bad");
        return NULL;
    }
    GlobalUnlock(mem);
    return mem;
}

static BITMAPINFOHEADER *lock_dib(HANDLE h)
{
    void *p;
    if (!h) return NULL;
    p = GlobalLock(h);
    if (!p) return NULL;
    if (((BITMAPINFOHEADER *)p)->biSize < 40) {
        GlobalUnlock(h);
        return NULL;
    }
    return (BITMAPINFOHEADER *)p;
}

static HANDLE dup_dib(HANDLE h)
{
    DWORD sz;
    HANDLE n;
    void *a, *b;
    if (!h) return NULL;
    sz = GlobalSize(h);
    if (sz < 40) return NULL;
    n = GlobalAlloc(GHND, sz);
    if (!n) return NULL;
    a = GlobalLock(h);
    b = GlobalLock(n);
    if (!a || !b) {
        if (a) GlobalUnlock(h);
        if (b) GlobalUnlock(n);
        GlobalFree(n);
        return NULL;
    }
    CopyMemory(b, a, sz);
    GlobalUnlock(h);
    GlobalUnlock(n);
    return n;
}

static void keep_dib(HANDLE h)
{
    if (g_keep) {
        GlobalFree(g_keep);
        g_keep = NULL;
    }
    g_keep = dup_dib(h);
}

__declspec(dllexport) HANDLE WINAPI TWAIN_AcquireNative(HWND hwnd, unsigned pix)
{
    HANDLE h;
    DWORD now;
    (void)pix;
    spin_close();
    log_line("TWAIN_AcquireNative");
    now = GetTickCount();
    /* второй вызов сразу после кадра = «больше страниц нет», не новое сканирование.
     * Иначе крутилка снова TOP-окно поверх «Передать в SAUMI». */
    if (g_got_page && (now - g_page_tick) < 4000u) {
        log_line("AcquireNative: no more pages");
        return NULL;
    }
    if (g_busy) {
        log_line("busy, skip");
        return NULL;
    }
    g_busy = 1;
    g_got_page = 0;
    if (!wait_scan(hwnd)) {
        g_state = 3;
        g_busy = 0;
        spin_close();
        msg_u8(hwnd,
               "Проверьте устройство (сканер HP).\nМожно нажать «Сканировать» ещё раз.");
        return NULL;
    }
    h = load_dib();
    spin_close();
    g_busy = 0;
    if (!h) {
        g_state = 3;
        msg_u8(hwnd,
               "Проверьте устройство (сканер HP).\nМожно нажать «Сканировать» ещё раз.");
        return NULL;
    }
    keep_dib(h);
    g_got_page = 1;
    g_page_tick = GetTickCount();
    g_state = 4;
    return h;
}

__declspec(dllexport) BOOL WINAPI TWAIN_SelectImageSource(HWND hwnd)
{
    (void)hwnd;
    log_line("TWAIN_SelectImageSource");
    g_got_page = 0;
    g_state = 4;
    return TRUE;
}

__declspec(dllexport) BOOL WINAPI TWAIN_IsAvailable(void)
{
    return TRUE;
}

__declspec(dllexport) int WINAPI TWAIN_State(void)
{
    return g_state;
}

__declspec(dllexport) void WINAPI TWAIN_SetMultiTransfer(BOOL yes)
{
    char msg[40];
    wsprintfA(msg, "SetMultiTransfer %d", (int)yes);
    log_line(msg);
}

__declspec(dllexport) BOOL WINAPI TWAIN_EnableSource(HWND hwnd)
{
    (void)hwnd;
    g_state = 5;
    return TRUE;
}

__declspec(dllexport) BOOL WINAPI TWAIN_DisableSource(void)
{
    g_state = 4;
    return TRUE;
}

__declspec(dllexport) BOOL WINAPI TWAIN_CloseSource(void)
{
    g_state = 3;
    return TRUE;
}

__declspec(dllexport) BOOL WINAPI TWAIN_GetSourceList(void)
{
    g_src_idx = 0;
    return TRUE;
}

__declspec(dllexport) BOOL WINAPI TWAIN_GetNextSourceName(char *name)
{
    HANDLE hf;
    DWORD n;
    char buf[200];
    unsigned i;
    if (!name) return FALSE;
    if (g_src_idx++) return FALSE;
    lstrcpynA(name, "Scanner", 255);
    hf = CreateFileA("C:\\SAUMI\\temp\\twain\\source_name.txt", GENERIC_READ,
                     FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING,
                     FILE_ATTRIBUTE_NORMAL, NULL);
    if (hf != INVALID_HANDLE_VALUE) {
        if (ReadFile(hf, buf, 180, &n, NULL) && n > 0) {
            buf[n] = 0;
            for (i = 0; buf[i]; i++) {
                if (buf[i] == '\r' || buf[i] == '\n') { buf[i] = 0; break; }
            }
            if (buf[0]) lstrcpynA(name, buf, 255);
        }
        CloseHandle(hf);
    }
    return TRUE;
}

static int ext_is(const char *fn, const char *ext)
{
    int n, e;
    if (!fn || !ext) return 0;
    n = lstrlenA(fn);
    e = lstrlenA(ext);
    if (n < e) return 0;
    return lstrcmpiA(fn + n - e, ext) == 0;
}

static int write_all(HANDLE hf, const void *p, DWORD n)
{
    DWORD wr;
    return WriteFile(hf, p, n, &wr, NULL) && wr == n;
}

static int pdf_puts(HANDLE hf, DWORD *pos, const char *s)
{
    DWORD n = (DWORD)lstrlenA(s);
    if (!write_all(hf, s, n)) return 0;
    *pos += n;
    return 1;
}

/* EZTwain: 0 = success */
static int write_bmp(HANDLE hdib, const char *fn)
{
    BITMAPINFOHEADER *b;
    HANDLE hf;
    DWORD dibsz, off;
    BITMAPFILEHEADER bf;

    b = lock_dib(hdib);
    if (!b) return -1;
    off = 14 + b->biSize;
    if (b->biBitCount <= 8)
        off += (DWORD)((b->biClrUsed ? b->biClrUsed : (1 << b->biBitCount)) * 4);
    dibsz = GlobalSize(hdib);
    ZeroMemory(&bf, sizeof(bf));
    bf.bfType = 0x4D42;
    bf.bfSize = 14 + dibsz;
    bf.bfOffBits = off;
    hf = CreateFileA(fn, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hf == INVALID_HANDLE_VALUE) {
        GlobalUnlock(hdib);
        return -1;
    }
    if (!write_all(hf, &bf, 14) || !write_all(hf, b, dibsz)) {
        CloseHandle(hf);
        GlobalUnlock(hdib);
        return -1;
    }
    CloseHandle(hf);
    GlobalUnlock(hdib);
    return 0;
}

static int pdf_hex(HANDLE hf, DWORD *pos, const unsigned char *p, DWORD n)
{
    static const char *H = "0123456789ABCDEF";
    char line[80];
    DWORD i, k = 0;
    for (i = 0; i < n; i++) {
        line[k++] = H[p[i] >> 4];
        line[k++] = H[p[i] & 15];
        if (k >= 64) {
            line[k++] = '\n';
            if (!write_all(hf, line, k)) return 0;
            *pos += k;
            k = 0;
        }
    }
    line[k++] = '>';
    line[k++] = '\n';
    if (!write_all(hf, line, k)) return 0;
    *pos += k;
    return 1;
}

static int write_pdf(HANDLE hdib, const char *fn)
{
    BITMAPINFOHEADER *b;
    HANDLE hf;
    int w, h, y, x, i, dpi, pw, ph;
    DWORD stride, gray_len, hex_chars, stream_len, pos, off[6], xref;
    unsigned char *src, *gray, *srow;
    char buf[360], content[80];
    DWORD clen;
    static const unsigned char bincom[] = { '%', 0xE2, 0xE3, 0xCF, 0xD3, '\n' };

    b = lock_dib(hdib);
    if (!b) return -1;
    w = b->biWidth;
    h = b->biHeight;
    if (h < 0) h = -h;
    if (w < 1 || h < 1 || b->biBitCount != 24) {
        GlobalUnlock(hdib);
        return -1;
    }
    dpi = 150;
    if (b->biXPelsPerMeter >= 1000)
        dpi = (int)((b->biXPelsPerMeter * 254L + 5000) / 10000);
    if (dpi < 72) dpi = 150;
    pw = (w * 72 + dpi / 2) / dpi;
    ph = (h * 72 + dpi / 2) / dpi;
    if (pw < 1) pw = 1;
    if (ph < 1) ph = 1;
    stride = ((DWORD)w * 3 + 3) & ~3u;
    gray_len = (DWORD)w * (DWORD)h;
    gray = (unsigned char *)GlobalAlloc(GPTR, gray_len);
    if (!gray) {
        GlobalUnlock(hdib);
        return -1;
    }
    src = (unsigned char *)b + b->biSize;
    for (y = 0; y < h; y++) {
        srow = src + (h - 1 - y) * stride;
        for (x = 0; x < w; x++) {
            gray[y * w + x] = (unsigned char)((srow[2] * 3 + srow[1] * 6 + srow[0]) / 10);
            srow += 3;
        }
    }
    GlobalUnlock(hdib);
    hex_chars = gray_len * 2;
    stream_len = (hex_chars / 64) * 65 + (hex_chars % 64) + 2;
    wsprintfA(content, "q %d 0 0 %d 0 0 cm /Im0 Do Q\n", pw, ph);
    clen = (DWORD)lstrlenA(content);
    hf = CreateFileA(fn, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hf == INVALID_HANDLE_VALUE) {
        GlobalFree(gray);
        return -1;
    }
    pos = 0;
    if (!pdf_puts(hf, &pos, "%PDF-1.4\n")) goto fail;
    if (!write_all(hf, bincom, 6)) goto fail;
    pos += 6;
    off[1] = pos;
    if (!pdf_puts(hf, &pos, "1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n")) goto fail;
    off[2] = pos;
    if (!pdf_puts(hf, &pos, "2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n")) goto fail;
    off[3] = pos;
    wsprintfA(buf, "3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 %d %d]/Contents 4 0 R/Resources<</XObject<</Im0 5 0 R>>>>>>endobj\n", pw, ph);
    if (!pdf_puts(hf, &pos, buf)) goto fail;
    off[4] = pos;
    wsprintfA(buf, "4 0 obj<</Length %lu>>stream\n", (unsigned long)clen);
    if (!pdf_puts(hf, &pos, buf)) goto fail;
    if (!write_all(hf, content, clen)) goto fail;
    pos += clen;
    if (!pdf_puts(hf, &pos, "endstream\nendobj\n")) goto fail;
    off[5] = pos;
    wsprintfA(buf, "5 0 obj<</Type/XObject/Subtype/Image/Width %d/Height %d/ColorSpace/DeviceGray/BitsPerComponent 8/Filter/ASCIIHexDecode/Length %lu>>stream\n", w, h, (unsigned long)stream_len);
    if (!pdf_puts(hf, &pos, buf)) goto fail;
    if (!pdf_hex(hf, &pos, gray, gray_len)) goto fail;
    if (!pdf_puts(hf, &pos, "endstream\nendobj\n")) goto fail;
    xref = pos;
    if (!pdf_puts(hf, &pos, "xref\n0 6\n0000000000 65535 f \n")) goto fail;
    for (i = 1; i <= 5; i++) {
        wsprintfA(buf, "%010lu 00000 n \n", (unsigned long)off[i]);
        if (!pdf_puts(hf, &pos, buf)) goto fail;
    }
    wsprintfA(buf, "trailer<</Size 6/Root 1 0 R>>\nstartxref\n%lu\n%%%%EOF\n", (unsigned long)xref);
    if (!pdf_puts(hf, &pos, buf)) goto fail;
    CloseHandle(hf);
    GlobalFree(gray);
    return 0;
fail:
    CloseHandle(hf);
    GlobalFree(gray);
    return -1;
}

__declspec(dllexport) int WINAPI DIB_Width(HANDLE h)
{
    BITMAPINFOHEADER *b = lock_dib(h);
    int w;
    if (!b) return 0;
    w = b->biWidth;
    GlobalUnlock(h);
    return w;
}

__declspec(dllexport) int WINAPI DIB_Height(HANDLE h)
{
    BITMAPINFOHEADER *b = lock_dib(h);
    int ht;
    if (!b) return 0;
    ht = b->biHeight;
    if (ht < 0) ht = -ht;
    GlobalUnlock(h);
    return ht;
}

__declspec(dllexport) void WINAPI DIB_Free(HANDLE h)
{
    if (h && h != g_keep) GlobalFree(h);
}

static int rewrite_ext_pdf(char *fn)
{
    int i;
    if (!fn) return 0;
    for (i = lstrlenA(fn) - 1; i >= 0; i--) {
        if (fn[i] == '\\' || fn[i] == '/') return 0;
        if (fn[i] == '.') {
            fn[i + 1] = 'p';
            fn[i + 2] = 'd';
            fn[i + 3] = 'f';
            fn[i + 4] = 0;
            return 1;
        }
    }
    return 0;
}

static void ensure_parent_dir(const char *fn)
{
    char dir[260];
    int i, n;
    lstrcpynA(dir, fn, 260);
    n = lstrlenA(dir);
    for (i = n - 1; i > 0; i--) {
        if (dir[i] == '\\' || dir[i] == '/') {
            dir[i] = 0;
            CreateDirectoryA(dir, NULL);
            return;
        }
    }
}

__declspec(dllexport) int WINAPI DIB_WriteToFilename(HANDLE h, char *fn)
{
    int rc;
    char msg[280];
    char orig[260];

    wsprintfA(msg, "DIB_WriteToFilename %s", fn ? fn : "(null)");
    log_line(msg);
    if (!fn || !fn[0]) return -1;
    lstrcpynA(orig, fn, 260);
    CreateDirectoryA("C:\\SAUMI\\temp", NULL);
    CreateDirectoryA("C:\\SAUMI\\temp\\SAUMI_SCANS", NULL);
    ensure_parent_dir(fn);

    if (ext_is(fn, ".pdf")) {
        rc = write_pdf(h, fn);
    } else {
        if (ext_is(fn, ".bmp") || ext_is(fn, ".dib"))
            write_bmp(h, orig);
        if (rewrite_ext_pdf(fn)) {
            wsprintfA(msg, "attach as %s", fn);
            log_line(msg);
        }
        rc = write_pdf(h, fn);
    }
    if (rc == 0)
        keep_dib(h);
    log_line(rc == 0 ? "write: ok" : "write: fail");
    return rc;
}

__declspec(dllexport) HANDLE WINAPI DIB_LoadFromFilename(const char *fn)
{
    HANDLE hf, mem;
    DWORD sz, rd;
    BITMAPFILEHEADER bf;
    void *p;
    char msg[280];

    wsprintfA(msg, "DIB_LoadFromFilename %s", fn ? fn : "(null)");
    log_line(msg);
    if (fn && fn[0]) {
        hf = CreateFileA(fn, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING,
                         FILE_ATTRIBUTE_NORMAL, NULL);
        if (hf != INVALID_HANDLE_VALUE) {
            sz = GetFileSize(hf, NULL);
            ZeroMemory(&bf, sizeof(bf));
            if (sz > 14 && ReadFile(hf, &bf, 14, &rd, NULL) && rd == 14 && bf.bfType == 0x4D42) {
                sz -= 14;
                mem = GlobalAlloc(GHND, sz);
                p = mem ? GlobalLock(mem) : NULL;
                if (p && ReadFile(hf, p, sz, &rd, NULL) && rd == sz) {
                    GlobalUnlock(mem);
                    CloseHandle(hf);
                    return mem;
                }
                if (p) GlobalUnlock(mem);
                if (mem) GlobalFree(mem);
            }
            CloseHandle(hf);
        }
    }
    if (g_keep) {
        log_line("LoadFromFilename: keep");
        return dup_dib(g_keep);
    }
    log_line("LoadFromFilename: empty");
    return NULL;
}

__declspec(dllexport) HANDLE WINAPI DIB_Thumbnail(HANDLE h, int maxw, int maxh)
{
    BITMAPINFOHEADER *b;
    int w, ht, nw, nh;
    HANDLE dst;
    BITMAPINFOHEADER *db;
    unsigned char *src, *dp;
    DWORD sstride, dstride;
    int x, y, sx, sy;
    char msg[80];

    wsprintfA(msg, "DIB_Thumbnail %d %d", maxw, maxh);
    log_line(msg);
    b = lock_dib(h);
    if (!b) return g_keep ? dup_dib(g_keep) : NULL;
    w = b->biWidth;
    ht = b->biHeight;
    if (ht < 0) ht = -ht;
    if (maxw < 8) maxw = 64;
    if (maxh < 8) maxh = 64;
    nw = w;
    nh = ht;
    if (nw > maxw) {
        nh = nh * maxw / nw;
        nw = maxw;
    }
    if (nh > maxh) {
        nw = nw * maxh / nh;
        nh = maxh;
    }
    if (nw < 1) nw = 1;
    if (nh < 1) nh = 1;
    dstride = ((DWORD)nw * 3 + 3) & ~3u;
    sstride = ((DWORD)w * 3 + 3) & ~3u;
    dst = GlobalAlloc(GHND, 40 + dstride * (DWORD)nh);
    if (!dst) {
        GlobalUnlock(h);
        return dup_dib(h);
    }
    db = (BITMAPINFOHEADER *)GlobalLock(dst);
    ZeroMemory(db, 40 + dstride * (DWORD)nh);
    db->biSize = 40;
    db->biWidth = nw;
    db->biHeight = nh;
    db->biPlanes = 1;
    db->biBitCount = 24;
    src = (unsigned char *)b + b->biSize;
    dp = (unsigned char *)db + 40;
    for (y = 0; y < nh; y++) {
        sy = y * ht / nh;
        for (x = 0; x < nw; x++) {
            sx = x * w / nw;
            dp[y * dstride + x * 3] = src[sy * sstride + sx * 3];
            dp[y * dstride + x * 3 + 1] = src[sy * sstride + sx * 3 + 1];
            dp[y * dstride + x * 3 + 2] = src[sy * sstride + sx * 3 + 2];
        }
    }
    GlobalUnlock(dst);
    GlobalUnlock(h);
    return dst;
}

__declspec(dllexport) int WINAPI DIB_DrawToDC(HANDLE h, HDC dc, int dx, int dy, int w, int hdst, int sx, int sy)
{
    BITMAPINFOHEADER *b;
    int ht, rc;
    char msg[160];
    wsprintfA(msg, "DIB_DrawToDC dx=%d dy=%d w=%d h=%d sx=%d sy=%d", dx, dy, w, hdst, sx, sy);
    log_line(msg);
    if (!dc) return 0;
    b = lock_dib(h);
    if (!b) return 0;
    ht = b->biHeight;
    if (ht < 0) ht = -ht;
    if (w <= 0) w = b->biWidth;
    if (hdst <= 0) hdst = ht;
    SetStretchBltMode(dc, COLORONCOLOR);
    rc = StretchDIBits(dc, dx, dy, w, hdst, sx, sy, b->biWidth, ht,
                       (char *)b + b->biSize, (BITMAPINFO *)b, DIB_RGB_COLORS, SRCCOPY);
    GlobalUnlock(h);
    return rc != 0;
}

__declspec(dllexport) int WINAPI TWAIN_IsPdfAvailable(void)
{
    return 1;
}

__declspec(dllexport) int WINAPI TWAIN_IsTiffAvailable(void)
{
    return 0;
}

__declspec(dllexport) int WINAPI TWAIN_BeginMultipageFile(const char *fn)
{
    char msg[280];
    wsprintfA(msg, "BeginMultipageFile %s", fn ? fn : "(null)");
    log_line(msg);
    g_multi_path[0] = 0;
    if (fn && fn[0]) lstrcpynA(g_multi_path, fn, 260);
    return 0;
}

__declspec(dllexport) int WINAPI TWAIN_DibWritePage(HANDLE h)
{
    int rc;
    if (!g_multi_path[0]) {
        lstrcpynA(g_multi_path, "C:\\SAUMI\\temp\\SAUMI_SCANS\\scan.pdf", 260);
        CreateDirectoryA("C:\\SAUMI\\temp\\SAUMI_SCANS", NULL);
        log_line("DibWritePage default scan.pdf");
    }
    log_line("DibWritePage pdf");
    rc = write_pdf(h, g_multi_path);
    if (rc == 0)
        keep_dib(h);
    return rc;
}

__declspec(dllexport) int WINAPI TWAIN_EndMultipageFile(void)
{
    log_line("EndMultipageFile");
    g_multi_path[0] = 0;
    return 0;
}

BOOL WINAPI DllMain(HINSTANCE inst, DWORD reason, LPVOID res)
{
    HMODULE orig;
    (void)res;
    if (reason == DLL_PROCESS_ATTACH) {
        DisableThreadLibraryCalls(inst);
        g_inst = inst;
        orig = LoadLibraryA("Eztwain3_orig.dll");
        if (!orig) orig = LoadLibraryA("C:\\SAUMI\\Eztwain3_orig.dll");
        if (orig) {
            p_TWAIN_Shutdown = GetProcAddress(orig, "TWAIN_Shutdown");
            p_TWAIN_ApplicationLicense = GetProcAddress(orig, "TWAIN_ApplicationLicense");
        } else {
            log_line("FAIL LoadLibrary Eztwain3_orig.dll");
        }
        /* GDI clip for «Свойства → Движения»: AppInit on Wine 11 never loaded. */
        {
            HMODULE clip = LoadLibraryA("C:\\SAUMI\\SaumiGdiClip.dll");
            if (!clip) clip = LoadLibraryA("SaumiGdiClip.dll");
            log_line(clip ? "SaumiGdiClip loaded" : "SaumiGdiClip missing");
        }
        log_line("proxy attached");
    }
    return TRUE;
}
