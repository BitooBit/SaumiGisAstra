/* Minimal TWAIN 1.9 types. Public protocol constants. packed(2) as required. */
#ifndef SAUMI_TWAIN_H
#define SAUMI_TWAIN_H

#include <windows.h>

#ifdef __cplusplus
extern "C" {
#endif

#define TWON_PROTOCOLMAJOR 1
#define TWON_PROTOCOLMINOR 9

#define DG_CONTROL 0x0001L
#define DG_IMAGE   0x0002L

#define DAT_NULL            0x0000
#define DAT_CAPABILITY      0x0001
#define DAT_EVENT           0x0002
#define DAT_IDENTITY        0x0003
#define DAT_PARENT          0x0004
#define DAT_PENDINGXFERS    0x0005
#define DAT_SETUPMEMXFER    0x0006
#define DAT_SETUPFILEXFER   0x0007
#define DAT_STATUS          0x0008
#define DAT_USERINTERFACE   0x0009
#define DAT_XFERGROUP       0x000a
#define DAT_IMAGEINFO       0x0101
#define DAT_IMAGELAYOUT     0x0102
#define DAT_IMAGEMEMXFER    0x0103
#define DAT_IMAGENATIVEXFER 0x0104
#define DAT_IMAGEFILEXFER   0x0105

#define MSG_GET            0x0001
#define MSG_GETCURRENT     0x0002
#define MSG_GETDEFAULT     0x0003
#define MSG_GETFIRST       0x0004
#define MSG_GETNEXT        0x0005
#define MSG_SET            0x0006
#define MSG_RESET          0x0007
#define MSG_QUERYSUPPORT   0x0008
#define MSG_XFERREADY      0x0101
#define MSG_CLOSEDSREQ     0x0102
#define MSG_CLOSEDSOK      0x0103
#define MSG_OPENDSM        0x0301
#define MSG_CLOSEDSM       0x0302
#define MSG_OPENDS         0x0401
#define MSG_CLOSEDS        0x0402
#define MSG_USERSELECT     0x0403
#define MSG_DISABLEDS      0x0501
#define MSG_ENABLEDS       0x0502
#define MSG_ENABLEDSUIONLY 0x0503
#define MSG_PROCESSEVENT   0x0601
#define MSG_ENDXFER        0x0701

#define TWRC_SUCCESS      0
#define TWRC_FAILURE      1
#define TWRC_CHECKSTATUS  2
#define TWRC_CANCEL       3
#define TWRC_DSEVENT      4
#define TWRC_NOTDSEVENT   5
#define TWRC_XFERDONE     6
#define TWRC_ENDOFLIST    7

#define TWCC_SUCCESS         0
#define TWCC_BUMMER          1
#define TWCC_LOWMEMORY       2
#define TWCC_NODS            3
#define TWCC_MAXCONNECTIONS  4
#define TWCC_OPERATIONERROR  5
#define TWCC_BADCAP          6
#define TWCC_BADPROTOCOL     9
#define TWCC_BADVALUE        10
#define TWCC_SEQERROR        11
#define TWCC_BADDEST         12
#define TWCC_CAPUNSUPPORTED  13
#define TWCC_CAPBADOPERATION 14
#define TWCC_CAPSEQERROR     15

#define TWON_ONEVALUE 5
#define TWON_ENUMERATION 4

#define TWTY_INT8    0x0000
#define TWTY_INT16   0x0001
#define TWTY_INT32   0x0002
#define TWTY_UINT8   0x0003
#define TWTY_UINT16  0x0004
#define TWTY_UINT32  0x0005
#define TWTY_BOOL    0x0006
#define TWTY_FIX32   0x0007
#define TWTY_FRAME   0x0008
#define TWTY_STR32   0x0009
#define TWTY_STR64   0x000a
#define TWTY_STR128  0x000b
#define TWTY_STR255  0x000c

#define CAP_XFERCOUNT       0x0001
#define CAP_UICONTROLLABLE  0x100e
#define CAP_DEVICEONLINE    0x100f
#define CAP_ENABLEDSUIONLY  0x101d
#define CAP_CUSTOMBASE      0x8000
#define ICAP_COMPRESSION      0x0100
#define ICAP_PIXELTYPE        0x0101
#define ICAP_UNITS            0x0102
#define ICAP_XFERMECH         0x0103
#define ICAP_IMAGEFILEFORMAT  0x110c
#define ICAP_BITDEPTH         0x112b

#define TWPT_BW    0
#define TWPT_GRAY  1
#define TWPT_RGB   2
#define TWUN_INCHES 0
#define TWSX_NATIVE 0
#define TWSX_FILE   1
#define TWSX_MEMORY 2
#define TWCP_NONE   0
#define TWFF_TIFF   0
#define TWFF_BMP    2
#define MSG_NULL    0

#define TWON_ARRAY 3

typedef signed char    TW_INT8;
typedef short          TW_INT16;
typedef long           TW_INT32;
typedef unsigned char  TW_UINT8;
typedef unsigned short TW_UINT16;
typedef unsigned long  TW_UINT32;
typedef unsigned short TW_BOOL;
typedef void *         TW_MEMREF;
typedef HANDLE         TW_HANDLE;

typedef char TW_STR32[34];
typedef char TW_STR64[66];
typedef char TW_STR128[130];
typedef char TW_STR255[256];

#pragma pack(push, 2)

typedef struct {
    TW_UINT16 MajorNum;
    TW_UINT16 MinorNum;
    TW_UINT16 Language;
    TW_UINT16 Country;
    TW_STR32  Info;
} TW_VERSION;

typedef struct {
    TW_INT16  Whole;
    TW_UINT16 Frac;
} TW_FIX32;

typedef struct {
    TW_UINT32  Id;
    TW_VERSION Version;
    TW_UINT16  ProtocolMajor;
    TW_UINT16  ProtocolMinor;
    TW_UINT32  SupportedGroups;
    TW_STR32   Manufacturer;
    TW_STR32   ProductFamily;
    TW_STR32   ProductName;
} TW_IDENTITY;

typedef struct {
    TW_UINT16 ConditionCode;
    TW_UINT16 Reserved;
} TW_STATUS;

typedef struct {
    TW_BOOL    ShowUI;
    TW_BOOL    ModalUI;
    TW_HANDLE  hParent;
} TW_USERINTERFACE;

typedef struct {
    TW_UINT16 Count;
    TW_UINT32 EOJ;
} TW_PENDINGXFERS;

typedef struct {
    TW_FIX32   XResolution;
    TW_FIX32   YResolution;
    TW_INT32   ImageWidth;
    TW_INT32   ImageLength;
    TW_INT16   SamplesPerPixel;
    TW_INT16   BitsPerSample[8];
    TW_INT16   BitsPerPixel;
    TW_BOOL    Planar;
    TW_INT16   PixelType;
    TW_UINT16  Compression;
} TW_IMAGEINFO;

typedef struct {
    TW_UINT16 Cap;
    TW_UINT16 ConType;
    TW_HANDLE hContainer;
} TW_CAPABILITY;

typedef struct {
    TW_UINT16 ItemType;
    TW_UINT32 Item;
} TW_ONEVALUE;

typedef struct {
    HWND      pEvent;
    TW_UINT16 TWMessage;
} TW_EVENT;

typedef struct {
    TW_STR255 FileName;
    TW_UINT16 Format;
    TW_INT16  VRefNum;
} TW_SETUPFILEXFER;

typedef struct {
    TW_FIX32 Left;
    TW_FIX32 Top;
    TW_FIX32 Right;
    TW_FIX32 Bottom;
} TW_FRAME;

typedef struct {
    TW_FRAME  Frame;
    TW_UINT32 DocumentNumber;
    TW_UINT32 PageNumber;
    TW_UINT32 FrameNumber;
} TW_IMAGELAYOUT;

#pragma pack(pop)

typedef TW_UINT16 (WINAPI *DSMENTRYPROC)(
    TW_IDENTITY *pOrigin,
    TW_IDENTITY *pDest,
    TW_UINT32 DG,
    TW_UINT16 DAT,
    TW_UINT16 MSG,
    TW_MEMREF pData);

#ifdef __cplusplus
}
#endif
#endif
