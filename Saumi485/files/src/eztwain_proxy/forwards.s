.data
.global p_TWAIN_Shutdown
p_TWAIN_Shutdown:
    .long 0
.global p_TWAIN_ApplicationLicense
p_TWAIN_ApplicationLicense:
    .long 0
.text
.global TWAIN_Shutdown
TWAIN_Shutdown:
    movl p_TWAIN_Shutdown, %eax
    jmp *%eax
.global TWAIN_ApplicationLicense
TWAIN_ApplicationLicense:
    movl p_TWAIN_ApplicationLicense, %eax
    jmp *%eax
