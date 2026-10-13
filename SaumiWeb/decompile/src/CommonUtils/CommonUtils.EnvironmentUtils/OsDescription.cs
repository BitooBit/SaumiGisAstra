using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using CommonUtils.EnvironmentUtils.OsEnums;
using CommonUtils.ParseUtils;
using CommonUtils.RegistryUtils;
using CommonUtils.ReportUtils;

namespace CommonUtils.EnvironmentUtils;

public class OsDescription
{
	private class OsNamesChecker
	{
		private readonly string _OsName;

		private readonly Func<OsDescription, bool> _Checker;

		public string OsName => _OsName;

		public Func<OsDescription, bool> Checker => _Checker;

		public OsNamesChecker(string osName, Func<OsDescription, bool> checker)
		{
			_OsName = osName;
			_Checker = checker;
		}
	}

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

	[Flags]
	private enum OsTypeBitMask : uint
	{
		VER_BUILDNUMBER = 4u,
		VER_MAJORVERSION = 2u,
		VER_MINORVERSION = 1u,
		VER_PLATFORMID = 8u,
		VER_SERVICEPACKMAJOR = 0x20u,
		VER_SERVICEPACKMINOR = 0x10u,
		VER_SUITENAME = 0x40u,
		VER_PRODUCT_TYPE = 0x80u
	}

	private enum OsConditionMask : byte
	{
		VER_EQUAL = 1,
		VER_GREATER,
		VER_GREATER_EQUAL,
		VER_LESS,
		VER_LESS_EQUAL,
		VER_AND,
		VER_OR
	}

	private const string STRING_DESCRIPTION_TEMPLATE = "{0}{1}{2} {3} bits ({4}){5}";

	private const string WINDOWS_CURRENTVERSION_REGKEY_FULLPATH = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";

	private const string CURRENTVERSION_REGENTRY_NAME = "CurrentVersion";

	private const string CURRENTBUILDNUMBER_REGENTRY_NAME = "CurrentBuildNumber";

	private const string CSDVERSION_REGENTRY_NAME = "CSDVersion";

	private readonly OSVERSIONINFOEX _NativeOsVersionInfo;

	private readonly bool _IsCommonOsVersionFictitious;

	private Version _NativeVersion;

	private OperatingSystem _ManagedVersion;

	private MajorVersion? _MajorVersion;

	private MinorVersion? _MinorVersion;

	private BuildNumber? _BuildNumber;

	private PlatformID? _PlatformId;

	private SuiteEdition? _SuiteMask;

	private ProductType? _ProductType;

	private ProductTypeEx? _ProductTypeEx;

	private static List<OsNamesChecker> _OsNameCheckers;

	private string _Name;

	private string _Edition;

	public static string CurrentDescription
	{
		get
		{
			try
			{
				OsDescription osDescription = new OsDescription();
				return osDescription.ToString();
			}
			catch (Exception ex)
			{
				UtilsLoggerWrapper.Error("Can't determine OS version. Exception occurred: " + ex);
				return Environment.OSVersion.VersionString;
			}
		}
	}

	public bool IsCommonOsVersionFictitious => _IsCommonOsVersionFictitious;

	public Version NativeVersion
	{
		get
		{
			if (_NativeVersion == null)
			{
				Version nativeVersion = new Version(_NativeOsVersionInfo.dwMajorVersion, _NativeOsVersionInfo.dwMinorVersion, _NativeOsVersionInfo.dwBuildNumber, (_NativeOsVersionInfo.wServicePackMajor << 16) | _NativeOsVersionInfo.wServicePackMinor);
				_NativeVersion = nativeVersion;
			}
			return _NativeVersion;
		}
	}

	public OperatingSystem ManagedVersion
	{
		get
		{
			if (_ManagedVersion == null)
			{
				if (_IsCommonOsVersionFictitious)
				{
					PlatformID platformID = ((VerPlatform)_NativeOsVersionInfo.dwPlatformId).ToPlatformID();
					BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
					Type[] types = new Type[3]
					{
						typeof(PlatformID),
						typeof(Version),
						typeof(string)
					};
					ConstructorInfo constructor = typeof(OperatingSystem).GetConstructor(bindingAttr, null, types, null);
					_ManagedVersion = (OperatingSystem)constructor.Invoke(new object[3] { platformID, NativeVersion, _NativeOsVersionInfo.szCSDVersion });
				}
				else
				{
					_ManagedVersion = Environment.OSVersion;
				}
			}
			return _ManagedVersion;
		}
	}

	public MajorVersion? MajorVersion
	{
		get
		{
			if (!_MajorVersion.HasValue)
			{
				MajorVersion? majorVersion = TryingConvertTo(_NativeOsVersionInfo.dwMajorVersion, (int i) => (MajorVersion)i);
				if (!majorVersion.HasValue)
				{
					return null;
				}
				_MajorVersion = majorVersion.Value;
			}
			return _MajorVersion;
		}
	}

	public MinorVersion? MinorVersion
	{
		get
		{
			if (!_MinorVersion.HasValue)
			{
				MinorVersion? minorVersion = TryingConvertTo(_NativeOsVersionInfo.dwMinorVersion, (int i) => (MinorVersion)i);
				if (!minorVersion.HasValue)
				{
					return null;
				}
				_MinorVersion = minorVersion.Value;
			}
			return _MinorVersion;
		}
	}

	public BuildNumber? BuildNumber
	{
		get
		{
			if (!_BuildNumber.HasValue)
			{
				BuildNumber? buildNumber = TryingConvertTo(_NativeOsVersionInfo.dwBuildNumber, (int i) => (BuildNumber)i);
				if (!buildNumber.HasValue)
				{
					return null;
				}
				_BuildNumber = buildNumber.Value;
			}
			return _BuildNumber;
		}
	}

	public PlatformID? PlatformId
	{
		get
		{
			if (!_PlatformId.HasValue)
			{
				VerPlatform? verPlatform = TryingConvertTo(_NativeOsVersionInfo.dwPlatformId, (int i) => (VerPlatform)i);
				if (!verPlatform.HasValue)
				{
					return null;
				}
				_PlatformId = verPlatform.Value.ToPlatformID();
			}
			return _PlatformId;
		}
	}

	public string CSDVersion => _NativeOsVersionInfo.szCSDVersion;

	public short ServicePackMajor => _NativeOsVersionInfo.wServicePackMajor;

	public short ServicePackMinor => _NativeOsVersionInfo.wServicePackMinor;

	public SuiteEdition? SuiteMask
	{
		get
		{
			if (!_SuiteMask.HasValue)
			{
				SuiteEdition? suiteEdition = TryingConvertTo(_NativeOsVersionInfo.wSuiteMask, (short i) => (SuiteEdition)i);
				if (!suiteEdition.HasValue)
				{
					return null;
				}
				_SuiteMask = suiteEdition.Value;
			}
			return _SuiteMask;
		}
	}

	public ProductType ProductType
	{
		get
		{
			if (!_ProductType.HasValue)
			{
				ProductType? productType = TryingConvertTo(_NativeOsVersionInfo.wProductType, (byte i) => (ProductType)i);
				if (!productType.HasValue)
				{
					return ProductType.Invalid;
				}
				_ProductType = productType.Value;
			}
			return _ProductType.Value;
		}
	}

	public ProductTypeEx? ProductTypeEx
	{
		get
		{
			if (!_ProductTypeEx.HasValue)
			{
				MajorVersion? majorVersion = MajorVersion;
				if (majorVersion.GetValueOrDefault() >= CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista && majorVersion.HasValue && GetProductInfo(_NativeOsVersionInfo.dwMajorVersion, _NativeOsVersionInfo.dwMinorVersion, _NativeOsVersionInfo.wServicePackMajor, _NativeOsVersionInfo.wServicePackMinor, out var edition))
				{
					ProductTypeEx? productTypeEx = TryingConvertTo(edition, (int i) => (ProductTypeEx)i);
					if (!productTypeEx.HasValue)
					{
						return null;
					}
					_ProductTypeEx = productTypeEx.Value;
				}
			}
			return _ProductTypeEx;
		}
	}

	public string Name
	{
		get
		{
			if (_Name != null)
			{
				return _Name;
			}
			string text = "UNDEFINED";
			OsNamesChecker osNamesChecker = _OsNameCheckers.FirstOrDefault((OsNamesChecker descr) => descr.Checker(this));
			if (osNamesChecker != null)
			{
				text = osNamesChecker.OsName;
			}
			_Name = text;
			return text;
		}
	}

	public string Edition
	{
		get
		{
			if (_Edition != null)
			{
				return _Edition;
			}
			string text = string.Empty;
			if (MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win95)
			{
				if (ProductType == ProductType.Workstation)
				{
					text = "Workstation";
				}
				else if (ProductType == ProductType.Server || ProductType == ProductType.DomainController)
				{
					SuiteEdition? suiteEdition = (SuiteEdition?)((int?)SuiteMask & 2);
					text = ((suiteEdition.GetValueOrDefault() == SuiteEdition.None && suiteEdition.HasValue) ? "Standard Server" : "Enterprise Server");
				}
			}
			else if (MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win2k || MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win2k || MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win2k || MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win2k)
			{
				if (ProductType == ProductType.Workstation)
				{
					SuiteEdition? suiteEdition2 = (SuiteEdition?)((int?)SuiteMask & 0x200);
					text = ((suiteEdition2.GetValueOrDefault() == SuiteEdition.None && suiteEdition2.HasValue) ? "Professional" : "Home");
				}
				else if (ProductType == ProductType.Server || ProductType == ProductType.DomainController)
				{
					MinorVersion? minorVersion = MinorVersion;
					if (minorVersion.GetValueOrDefault() == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Undefined && minorVersion.HasValue)
					{
						SuiteEdition? suiteEdition3 = (SuiteEdition?)((int?)SuiteMask & 0x80);
						if (suiteEdition3.GetValueOrDefault() != SuiteEdition.None || !suiteEdition3.HasValue)
						{
							text = "Datacenter Server";
						}
						else
						{
							SuiteEdition? suiteEdition4 = (SuiteEdition?)((int?)SuiteMask & 2);
							text = ((suiteEdition4.GetValueOrDefault() == SuiteEdition.None && suiteEdition4.HasValue) ? "Server" : "Advanced Server");
						}
					}
					else
					{
						SuiteEdition? suiteEdition5 = (SuiteEdition?)((int?)SuiteMask & 0x80);
						if (suiteEdition5.GetValueOrDefault() != SuiteEdition.None || !suiteEdition5.HasValue)
						{
							text = "Datacenter";
						}
						else
						{
							SuiteEdition? suiteEdition6 = (SuiteEdition?)((int?)SuiteMask & 2);
							if (suiteEdition6.GetValueOrDefault() != SuiteEdition.None || !suiteEdition6.HasValue)
							{
								text = "Enterprise";
							}
							else
							{
								SuiteEdition? suiteEdition7 = (SuiteEdition?)((int?)SuiteMask & 0x400);
								text = ((suiteEdition7.GetValueOrDefault() == SuiteEdition.None && suiteEdition7.HasValue) ? "Standard" : "Web Edition");
							}
						}
					}
				}
			}
			else if ((MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista || MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista || MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista || MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista) && ProductTypeEx.HasValue)
			{
				text = ProductTypeEx.Value.GetName();
			}
			_Edition = text;
			return text;
		}
	}

	public string ServicePack => CSDVersion;

	public int Bits => (int)EnvironmentScanner.SystemEnvironmentBits;

	public bool IsNT5 => NativeVersion.Major == 5;

	public bool IsNT6 => NativeVersion.Major == 6;

	static OsDescription()
	{
		FillOsNameCheckers();
	}

	public OsDescription()
	{
		OSVERSIONINFOEX lpVersionInfo = new OSVERSIONINFOEX
		{
			dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX))
		};
		if (!GetVersionEx(ref lpVersionInfo))
		{
			throw new InvalidOperationException("Can't obtain OS version using unmanaged system libraries");
		}
		if (lpVersionInfo.dwMajorVersion == 6 && lpVersionInfo.dwMinorVersion == 2)
		{
			ulong dwlConditionMask = 0uL;
			dwlConditionMask = VerSetConditionMask(dwlConditionMask, 2u, 1);
			dwlConditionMask = VerSetConditionMask(dwlConditionMask, 1u, 1);
			dwlConditionMask = VerSetConditionMask(dwlConditionMask, 32u, 1);
			dwlConditionMask = VerSetConditionMask(dwlConditionMask, 16u, 1);
			dwlConditionMask = VerSetConditionMask(dwlConditionMask, 4u, 1);
			if (!VerifyVersionInfo(ref lpVersionInfo, 55u, dwlConditionMask))
			{
				UtilsLoggerWrapper.Warning("OS version, that obtained through unmanaged system libraries, isn't valid. Obviously OS returns this version value for compatibility.");
				_IsCommonOsVersionFictitious = true;
				UtilsLoggerWrapper.Info("Real OS version will be determined through system registry");
				string value = CommonUtils.RegistryUtils.RegistryUtils.GetValue<string>("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentVersion", RegistryViewOptions.Registry64);
				string value2 = CommonUtils.RegistryUtils.RegistryUtils.GetValue<string>("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber", RegistryViewOptions.Registry64);
				string value3 = CommonUtils.RegistryUtils.RegistryUtils.GetValue<string>("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CSDVersion", RegistryViewOptions.Registry64);
				if (!value.TryParseToVersion(out var version))
				{
					throw new InvalidOperationException("Can't obtain OS version major and minor values from registry");
				}
				lpVersionInfo.dwMajorVersion = version.Major;
				lpVersionInfo.dwMinorVersion = version.Minor;
				if (!int.TryParse(value2, out var result))
				{
					throw new InvalidOperationException("Can't obtain OS version build value from registry");
				}
				lpVersionInfo.dwBuildNumber = result;
				lpVersionInfo.szCSDVersion = value3;
				if (!VerifyVersionInfo(ref lpVersionInfo, 55u, dwlConditionMask))
				{
					throw new InvalidOperationException("Can't obtain real OS version. Version, that obtained through registry, isn't valid.");
				}
			}
		}
		_NativeOsVersionInfo = lpVersionInfo;
	}

	private static void FillOsNameCheckers()
	{
		_OsNameCheckers = new List<OsNamesChecker>();
		_OsNameCheckers.Add(new OsNamesChecker("Windows 95", delegate(OsDescription v)
		{
			PlatformID? platformId = v.PlatformId;
			if (platformId == PlatformID.Win32Windows && platformId.HasValue && v.MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win95)
			{
				MinorVersion? minorVersion = v.MinorVersion;
				if (minorVersion.GetValueOrDefault() == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Undefined && minorVersion.HasValue)
				{
					if (string.Compare(v.CSDVersion, "B", ignoreCase: true) == 0)
					{
						return string.Compare(v.CSDVersion, "C", ignoreCase: true) != 0;
					}
					return true;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows 95 OSR2", delegate(OsDescription v)
		{
			PlatformID? platformId = v.PlatformId;
			if (platformId == PlatformID.Win32Windows && platformId.HasValue && v.MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win95)
			{
				MinorVersion? minorVersion = v.MinorVersion;
				if (minorVersion.GetValueOrDefault() == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Undefined && minorVersion.HasValue)
				{
					if (string.Compare(v.CSDVersion, "B", ignoreCase: true) != 0)
					{
						return string.Compare(v.CSDVersion, "C", ignoreCase: true) == 0;
					}
					return true;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows 98", delegate(OsDescription v)
		{
			if (v.PlatformId == PlatformID.Win32Windows)
			{
				MajorVersion? majorVersion = v.MajorVersion;
				if (majorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win95 && majorVersion.HasValue && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Win98)
				{
					return string.Compare(v.CSDVersion, "A", ignoreCase: true) != 0;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows 98 Second Edition", delegate(OsDescription v)
		{
			if (v.PlatformId == PlatformID.Win32Windows)
			{
				MajorVersion? majorVersion = v.MajorVersion;
				if (majorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win95 && majorVersion.HasValue && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Win98)
				{
					return string.Compare(v.CSDVersion, "A", ignoreCase: true) == 0;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows Me", delegate(OsDescription v)
		{
			PlatformID? platformId = v.PlatformId;
			return platformId == PlatformID.Win32Windows && platformId.HasValue && v.MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win95 && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.WinME;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows NT 3.51", (OsDescription v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.WinNT351));
		_OsNameCheckers.Add(new OsNamesChecker("Windows NT 4.0", delegate(OsDescription v)
		{
			PlatformID? platformId = v.PlatformId;
			if (platformId == PlatformID.Win32NT && platformId.HasValue && v.MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win95)
			{
				MinorVersion? minorVersion = v.MinorVersion;
				if (minorVersion.GetValueOrDefault() == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Undefined && minorVersion.HasValue)
				{
					return v.ProductType == ProductType.Workstation;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows NT 4.0 Server", delegate(OsDescription v)
		{
			PlatformID? platformId = v.PlatformId;
			if (platformId == PlatformID.Win32NT && platformId.HasValue && v.MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win95)
			{
				MinorVersion? minorVersion = v.MinorVersion;
				if (minorVersion.GetValueOrDefault() == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Undefined && minorVersion.HasValue)
				{
					if (v.ProductType != ProductType.Server)
					{
						return v.ProductType == ProductType.DomainController;
					}
					return true;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows 2000", delegate(OsDescription v)
		{
			PlatformID? platformId = v.PlatformId;
			if (platformId == PlatformID.Win32NT && platformId.HasValue && v.MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win2k)
			{
				MinorVersion? minorVersion = v.MinorVersion;
				if (minorVersion.GetValueOrDefault() == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Undefined)
				{
					return minorVersion.HasValue;
				}
				return false;
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows XP", delegate(OsDescription v)
		{
			if (v.PlatformId == PlatformID.Win32NT)
			{
				MajorVersion? majorVersion = v.MajorVersion;
				if (majorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win2k && majorVersion.HasValue && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.WinXP)
				{
					return v.ProductType == ProductType.Workstation;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows XP 64", delegate(OsDescription v)
		{
			if (v.PlatformId == PlatformID.Win32NT)
			{
				MajorVersion? majorVersion = v.MajorVersion;
				if (majorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win2k && majorVersion.HasValue && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Win2003)
				{
					return v.ProductType == ProductType.Workstation;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows Server 2003", delegate(OsDescription v)
		{
			if (v.PlatformId == PlatformID.Win32NT)
			{
				MajorVersion? majorVersion = v.MajorVersion;
				if (majorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Win2k && majorVersion.HasValue && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Win2003)
				{
					if (v.ProductType != ProductType.Server)
					{
						return v.ProductType == ProductType.DomainController;
					}
					return true;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows Vista", delegate(OsDescription v)
		{
			PlatformID? platformId = v.PlatformId;
			if (platformId == PlatformID.Win32NT && platformId.HasValue && v.MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista)
			{
				MinorVersion? minorVersion = v.MinorVersion;
				if (minorVersion.GetValueOrDefault() == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Undefined && minorVersion.HasValue)
				{
					return v.ProductType == ProductType.Workstation;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows Server 2008", delegate(OsDescription v)
		{
			PlatformID? platformId = v.PlatformId;
			if (platformId == PlatformID.Win32NT && platformId.HasValue && v.MajorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista)
			{
				MinorVersion? minorVersion = v.MinorVersion;
				if (minorVersion.GetValueOrDefault() == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Undefined && minorVersion.HasValue)
				{
					if (v.ProductType != ProductType.Server)
					{
						return v.ProductType == ProductType.DomainController;
					}
					return true;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows 7", delegate(OsDescription v)
		{
			if (v.PlatformId == PlatformID.Win32NT)
			{
				MajorVersion? majorVersion = v.MajorVersion;
				if (majorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista && majorVersion.HasValue && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.WinXP)
				{
					return v.ProductType == ProductType.Workstation;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows Server 2008 R2", delegate(OsDescription v)
		{
			if (v.PlatformId == PlatformID.Win32NT)
			{
				MajorVersion? majorVersion = v.MajorVersion;
				if (majorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista && majorVersion.HasValue && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.WinXP)
				{
					if (v.ProductType != ProductType.Server)
					{
						return v.ProductType == ProductType.DomainController;
					}
					return true;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows 8", delegate(OsDescription v)
		{
			if (v.PlatformId == PlatformID.Win32NT)
			{
				MajorVersion? majorVersion = v.MajorVersion;
				if (majorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista && majorVersion.HasValue && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Win2003)
				{
					return v.ProductType == ProductType.Workstation;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows Server 2012", delegate(OsDescription v)
		{
			if (v.PlatformId == PlatformID.Win32NT)
			{
				MajorVersion? majorVersion = v.MajorVersion;
				if (majorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista && majorVersion.HasValue && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Win2003)
				{
					if (v.ProductType != ProductType.Server)
					{
						return v.ProductType == ProductType.DomainController;
					}
					return true;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows 8.1", delegate(OsDescription v)
		{
			if (v.PlatformId == PlatformID.Win32NT)
			{
				MajorVersion? majorVersion = v.MajorVersion;
				if (majorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista && majorVersion.HasValue && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Win81)
				{
					return v.ProductType == ProductType.Workstation;
				}
			}
			return false;
		}));
		_OsNameCheckers.Add(new OsNamesChecker("Windows Server 2012 R2", delegate(OsDescription v)
		{
			if (v.PlatformId == PlatformID.Win32NT)
			{
				MajorVersion? majorVersion = v.MajorVersion;
				if (majorVersion == CommonUtils.EnvironmentUtils.OsEnums.MajorVersion.Vista && majorVersion.HasValue && v.MinorVersion == CommonUtils.EnvironmentUtils.OsEnums.MinorVersion.Win81)
				{
					if (v.ProductType != ProductType.Server)
					{
						return v.ProductType == ProductType.DomainController;
					}
					return true;
				}
			}
			return false;
		}));
	}

	public override string ToString()
	{
		return string.Format("{0}{1}{2} {3} bits ({4}){5}", new object[6]
		{
			Name,
			(!string.IsNullOrEmpty(Edition)) ? (" " + Edition) : string.Empty,
			(!string.IsNullOrEmpty(ServicePack)) ? (" " + ServicePack) : string.Empty,
			Bits,
			NativeVersion,
			_IsCommonOsVersionFictitious ? string.Concat(" [compatible version: ", Environment.OSVersion.Version, "]") : string.Empty
		});
	}

	private static T? TryingConvertTo<T, TUnderlying>(TUnderlying value, Func<TUnderlying, T> converter) where T : struct where TUnderlying : struct
	{
		try
		{
			return converter(value);
		}
		catch (Exception)
		{
			return null;
		}
	}

	[DllImport("kernel32.dll")]
	internal static extern bool GetProductInfo(int osMajorVersion, int osMinorVersion, int spMajorVersion, int spMinorVersion, out int edition);

	[DllImport("kernel32.dll")]
	private static extern bool GetVersionEx(ref OSVERSIONINFOEX lpVersionInfo);

	[DllImport("kernel32.dll")]
	private static extern ulong VerSetConditionMask(ulong dwlConditionMask, uint dwTypeBitMask, byte dwConditionMask);

	[DllImport("kernel32.dll")]
	private static extern bool VerifyVersionInfo([In] ref OSVERSIONINFOEX lpVersionInfo, uint dwTypeMask, ulong dwlConditionMask);
}
