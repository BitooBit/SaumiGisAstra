using System;
using System.Runtime.InteropServices;

namespace Log4NetWrapper.Information.OS;

public static class OSInfo
{
	private struct OSVERSIONINFOEX
	{
		public int dwOSVersionInfoSize;

		public int dwMajorVersion;

		public int dwMinorVersion;

		public int dwBuildNumber;

		public int dwPlatformId;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string szCSDVersion;

		public short wServicePackMajor;

		public short wServicePackMinor;

		public short wSuiteMask;

		public byte wProductType;

		public byte wReserved;
	}

	public static string OsVersionString => GetOsVersion()?.VersionString;

	public static OSVersionDescriptor GetOsVersion()
	{
		OSVersionDescriptor oSVersionDescriptor = new OSVersionDescriptor();
		_ = Environment.OSVersion;
		OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX
		{
			dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX))
		};
		try
		{
			if (!GetVersionEx(ref osVersionInfo))
			{
				return null;
			}
			MajorVersion? majorVersion = TryingConvertTo(osVersionInfo.dwMajorVersion, (int i) => (MajorVersion)i);
			if (!majorVersion.HasValue)
			{
				return null;
			}
			oSVersionDescriptor.MajorVersion = majorVersion.Value;
			MinorVersion? minorVersion = TryingConvertTo(osVersionInfo.dwMinorVersion, (int i) => (MinorVersion)i);
			if (!minorVersion.HasValue)
			{
				return null;
			}
			oSVersionDescriptor.MinorVersion = minorVersion.Value;
			OSBuildNumber? oSBuildNumber = TryingConvertTo(osVersionInfo.dwBuildNumber, (int i) => (OSBuildNumber)i);
			if (!oSBuildNumber.HasValue)
			{
				return null;
			}
			oSVersionDescriptor.BuildNumber = oSBuildNumber.Value;
			oSVersionDescriptor.PlatformId = Environment.OSVersion.Platform;
			oSVersionDescriptor.CSDVersion = osVersionInfo.szCSDVersion;
			oSVersionDescriptor.ServicePackMajor = osVersionInfo.wServicePackMajor;
			oSVersionDescriptor.ServicePackMinor = osVersionInfo.wServicePackMinor;
			OSSuites? oSSuites = TryingConvertTo(osVersionInfo.wSuiteMask, (short i) => (OSSuites)i);
			if (!oSSuites.HasValue)
			{
				return null;
			}
			oSVersionDescriptor.SuiteMask = oSSuites.Value;
			OSProductType? oSProductType = TryingConvertTo(osVersionInfo.wProductType, (byte i) => (OSProductType)i);
			if (!oSProductType.HasValue)
			{
				return null;
			}
			oSVersionDescriptor.ProductType = oSProductType.Value;
			if ((oSVersionDescriptor.MajorVersion == MajorVersion.Vista || oSVersionDescriptor.MajorVersion == MajorVersion.Vista || oSVersionDescriptor.MajorVersion == MajorVersion.Vista || oSVersionDescriptor.MajorVersion == MajorVersion.Vista) && GetProductInfo(osVersionInfo.dwMajorVersion, osVersionInfo.dwMinorVersion, osVersionInfo.wServicePackMajor, osVersionInfo.wServicePackMinor, out var edition))
			{
				OSProductTypeEx? oSProductTypeEx = TryingConvertTo(edition, (int i) => (OSProductTypeEx)i);
				if (!oSProductTypeEx.HasValue)
				{
					return null;
				}
				oSVersionDescriptor.ProductTypeEx = oSProductTypeEx.Value;
			}
		}
		catch (Exception)
		{
			return null;
		}
		return oSVersionDescriptor;
	}

	private static T? TryingConvertTo<T, TUnderlying>(TUnderlying value, Func<TUnderlying, T> converter) where T : struct where TUnderlying : struct
	{
		try
		{
			return converter(value);
		}
		catch (Exception)
		{
		}
		return null;
	}

	[DllImport("Kernel32.dll")]
	internal static extern bool GetProductInfo(int osMajorVersion, int osMinorVersion, int spMajorVersion, int spMinorVersion, out int edition);

	[DllImport("kernel32.dll")]
	private static extern bool GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);
}
