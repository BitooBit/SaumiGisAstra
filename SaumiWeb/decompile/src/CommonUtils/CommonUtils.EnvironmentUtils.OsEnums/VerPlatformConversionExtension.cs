using System;

namespace CommonUtils.EnvironmentUtils.OsEnums;

public static class VerPlatformConversionExtension
{
	public static PlatformID ToPlatformID(this VerPlatform dwPlatformId)
	{
		return dwPlatformId switch
		{
			VerPlatform.VER_PLATFORM_WIN32_NT => PlatformID.Win32NT, 
			VerPlatform.VER_PLATFORM_WIN32_WINDOWS => PlatformID.Win32Windows, 
			VerPlatform.VER_PLATFORM_WIN32s => PlatformID.Win32S, 
			VerPlatform.VER_PLATFORM_WINCE => PlatformID.WinCE, 
			VerPlatform.VER_PLATFORM_UNIX => PlatformID.Unix, 
			VerPlatform.VER_PLATFORM_MACOSX => PlatformID.MacOSX, 
			_ => throw new InvalidOperationException("Invalid platform ID specified: " + dwPlatformId), 
		};
	}
}
