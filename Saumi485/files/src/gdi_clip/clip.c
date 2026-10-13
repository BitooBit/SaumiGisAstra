/* Заглушка. Патчи 20–24 хукали GDI и SetWindowRgn — сужали карточку, полосу не убрали.
 * DLL остаётся, потому что Eztwain её грузит. Ничего не хукает. */
#include <windows.h>

static void log_line(const char *s)
{
    HANDLE h;
    DWORD n;
    CreateDirectoryA("C:\\SAUMI\\temp\\twain", NULL);
    h = CreateFileA("C:\\SAUMI\\temp\\twain\\proxy.log", FILE_APPEND_DATA, FILE_SHARE_READ, NULL,
                    OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (h == INVALID_HANDLE_VALUE) return;
    WriteFile(h, s, (DWORD)lstrlenA(s), &n, NULL);
    CloseHandle(h);
}

BOOL WINAPI DllMain(HINSTANCE inst, DWORD reason, LPVOID res)
{
    (void)inst;
    (void)res;
    if (reason == DLL_PROCESS_ATTACH)
        log_line("SaumiGdiClip idle\r\n");
    return TRUE;
}

__declspec(dllexport) void WINAPI SaumiGdiClipDummy(void)
{
}
