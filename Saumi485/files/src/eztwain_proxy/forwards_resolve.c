#include <windows.h>

extern void *p_TWAIN_Shutdown;
extern void *p_TWAIN_ApplicationLicense;

void resolve_forwards(HMODULE orig)
{
    p_TWAIN_Shutdown = GetProcAddress(orig, "TWAIN_Shutdown");
    p_TWAIN_ApplicationLicense = GetProcAddress(orig, "TWAIN_ApplicationLicense");
}
