/* ShellExecute «Показать содержимое»: не копить вкладки Sumatra.
 * Закрыть старое окно, открыть только этот файл. */
#include <windows.h>

static void close_sumatra(void)
{
    int n;
    for (n = 0; n < 25; n++) {
        HWND w = FindWindowA("SUMATRA_PDF_FRAME", NULL);
        DWORD pid = 0;
        HANDLE h;
        if (!w) return;
        GetWindowThreadProcessId(w, &pid);
        PostMessageA(w, WM_CLOSE, 0, 0);
        Sleep(80);
        if (n < 12 || !pid) continue;
        h = OpenProcess(PROCESS_TERMINATE, FALSE, pid);
        if (h) {
            TerminateProcess(h, 0);
            CloseHandle(h);
        }
    }
}

int WINAPI WinMain(HINSTANCE a, HINSTANCE b, char *cmd, int show)
{
    char exe[260], dest[260], line[1024];
    char *slash;
    int i;
    STARTUPINFOA si;
    PROCESS_INFORMATION pi;
    (void)a;
    (void)b;
    (void)show;
    while (*cmd == ' ') cmd++;
    if (!cmd[0]) return 1;
    close_sumatra();
    GetModuleFileNameA(NULL, exe, 260);
    lstrcpynA(dest, exe, 260);
    slash = dest;
    for (i = 0; dest[i]; i++) {
        if (dest[i] == '\\' || dest[i] == '/') slash = dest + i + 1;
    }
    lstrcpyA(slash, "SumatraPDF.exe");
    ZeroMemory(&si, sizeof(si));
    si.cb = sizeof(si);
    wsprintfA(line, "\"%s\" %s", dest, cmd);
    if (!CreateProcessA(NULL, line, NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi))
        return 2;
    CloseHandle(pi.hThread);
    CloseHandle(pi.hProcess);
    return 0;
}
