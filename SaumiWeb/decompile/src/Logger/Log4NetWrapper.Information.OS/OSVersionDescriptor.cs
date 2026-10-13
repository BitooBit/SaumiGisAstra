using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace Log4NetWrapper.Information.OS;

public class OSVersionDescriptor
{
	private class OSNameChecker
	{
		private readonly string _OsName;

		private readonly Func<OSVersionDescriptor, bool> _Checker;

		public string OSName => _OsName;

		public Func<OSVersionDescriptor, bool> Checker => _Checker;

		public OSNameChecker(string osName, Func<OSVersionDescriptor, bool> checker)
		{
			_OsName = osName;
			_Checker = checker;
		}
	}

	private const string VersionStringTemplate = "{0} {1}{2} {3} bits";

	private string _Edition;

	private static List<OSNameChecker> _OsNames;

	private string _Name;

	public MajorVersion MajorVersion { get; internal set; }

	public MinorVersion MinorVersion { get; internal set; }

	public OSBuildNumber BuildNumber { get; internal set; }

	public PlatformID PlatformId { get; internal set; }

	public string CSDVersion { get; internal set; }

	public short ServicePackMajor { get; internal set; }

	public short ServicePackMinor { get; internal set; }

	public OSSuites SuiteMask { get; internal set; }

	public OSProductType ProductType { get; internal set; }

	public OSProductTypeEx? ProductTypeEx { get; internal set; }

	public string VersionString => string.Format("{0} {1}{2} {3} bits", new object[4]
	{
		Name,
		Edition,
		(!string.IsNullOrEmpty(ServicePack)) ? (" " + ServicePack) : string.Empty,
		Bits
	});

	public int AppBits => IntPtr.Size * 8;

	public int Bits
	{
		get
		{
			int num = AppBits;
			if (num == 32)
			{
				Exception exception = null;
				if (TryCheckIsWow64Process(out exception))
				{
					num = 64;
				}
			}
			return num;
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
			if (MajorVersion == MajorVersion.Win95)
			{
				if (ProductType == OSProductType.Workstation)
				{
					text = "Workstation";
				}
				else if (ProductType == OSProductType.Server || ProductType == OSProductType.DomainController)
				{
					text = (((SuiteMask & OSSuites.Enterprise) == 0) ? "Standard Server" : "Enterprise Server");
				}
			}
			else if (MajorVersion == MajorVersion.WinNT5 || MajorVersion == MajorVersion.WinNT5 || MajorVersion == MajorVersion.WinNT5 || MajorVersion == MajorVersion.WinNT5 || MajorVersion == MajorVersion.WinNT5)
			{
				if (ProductType == OSProductType.Workstation)
				{
					text = (((SuiteMask & OSSuites.Personal) == 0) ? "Professional" : "Home");
				}
				else if (ProductType == OSProductType.Server || ProductType == OSProductType.DomainController)
				{
					text = ((MinorVersion == MinorVersion.Win32s) ? (((SuiteMask & OSSuites.Datacenter) != OSSuites.None) ? "Datacenter Server" : (((SuiteMask & OSSuites.Enterprise) == 0) ? "Server" : "Advanced Server")) : (((SuiteMask & OSSuites.Datacenter) != OSSuites.None) ? "Datacenter" : (((SuiteMask & OSSuites.Enterprise) != OSSuites.None) ? "Enterprise" : (((SuiteMask & OSSuites.Blade) == 0) ? "Standard" : "Web Edition"))));
				}
			}
			else if ((MajorVersion == MajorVersion.Vista || MajorVersion == MajorVersion.Vista || MajorVersion == MajorVersion.Vista || MajorVersion == MajorVersion.Vista) && ProductTypeEx.HasValue)
			{
				switch (ProductTypeEx.Value)
				{
				case OSProductTypeEx.PRODUCT_BUSINESS:
					text = "Business";
					break;
				case OSProductTypeEx.PRODUCT_BUSINESS_N:
					text = "Business N";
					break;
				case OSProductTypeEx.PRODUCT_CLUSTER_SERVER:
					text = "HPC Edition";
					break;
				case OSProductTypeEx.PRODUCT_DATACENTER_SERVER:
					text = "Datacenter Server";
					break;
				case OSProductTypeEx.PRODUCT_DATACENTER_SERVER_CORE:
					text = "Datacenter Server (core installation)";
					break;
				case OSProductTypeEx.PRODUCT_ENTERPRISE:
					text = "Enterprise";
					break;
				case OSProductTypeEx.PRODUCT_ENTERPRISE_N:
					text = "Enterprise N";
					break;
				case OSProductTypeEx.PRODUCT_ENTERPRISE_SERVER:
					text = "Enterprise Server";
					break;
				case OSProductTypeEx.PRODUCT_ENTERPRISE_SERVER_CORE:
					text = "Enterprise Server (core installation)";
					break;
				case OSProductTypeEx.PRODUCT_ENTERPRISE_SERVER_CORE_V:
					text = "Enterprise Server without Hyper-V (core installation)";
					break;
				case OSProductTypeEx.PRODUCT_ENTERPRISE_SERVER_IA64:
					text = "Enterprise Server for Itanium-based Systems";
					break;
				case OSProductTypeEx.PRODUCT_ENTERPRISE_SERVER_V:
					text = "Enterprise Server without Hyper-V";
					break;
				case OSProductTypeEx.PRODUCT_HOME_BASIC:
					text = "Home Basic";
					break;
				case OSProductTypeEx.PRODUCT_HOME_BASIC_N:
					text = "Home Basic N";
					break;
				case OSProductTypeEx.PRODUCT_HOME_PREMIUM:
					text = "Home Premium";
					break;
				case OSProductTypeEx.PRODUCT_HOME_PREMIUM_N:
					text = "Home Premium N";
					break;
				case OSProductTypeEx.PRODUCT_HYPERV:
					text = "Microsoft Hyper-V Server";
					break;
				case OSProductTypeEx.PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
					text = "Windows Essential Business Management Server";
					break;
				case OSProductTypeEx.PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
					text = "Windows Essential Business Messaging Server";
					break;
				case OSProductTypeEx.PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
					text = "Windows Essential Business Security Server";
					break;
				case OSProductTypeEx.PRODUCT_SERVER_FOR_SMALLBUSINESS:
					text = "Windows Essential Server Solutions";
					break;
				case OSProductTypeEx.PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
					text = "Windows Essential Server Solutions without Hyper-V";
					break;
				case OSProductTypeEx.PRODUCT_SMALLBUSINESS_SERVER:
					text = "Windows Small Business Server";
					break;
				case OSProductTypeEx.PRODUCT_STANDARD_SERVER:
					text = "Standard Server";
					break;
				case OSProductTypeEx.PRODUCT_STANDARD_SERVER_CORE:
					text = "Standard Server (core installation)";
					break;
				case OSProductTypeEx.PRODUCT_STANDARD_SERVER_CORE_V:
					text = "Standard Server without Hyper-V (core installation)";
					break;
				case OSProductTypeEx.PRODUCT_STANDARD_SERVER_V:
					text = "Standard Server without Hyper-V";
					break;
				case OSProductTypeEx.PRODUCT_STARTER:
					text = "Starter";
					break;
				case OSProductTypeEx.PRODUCT_STORAGE_ENTERPRISE_SERVER:
					text = "Enterprise Storage Server";
					break;
				case OSProductTypeEx.PRODUCT_STORAGE_EXPRESS_SERVER:
					text = "Express Storage Server";
					break;
				case OSProductTypeEx.PRODUCT_STORAGE_STANDARD_SERVER:
					text = "Standard Storage Server";
					break;
				case OSProductTypeEx.PRODUCT_STORAGE_WORKGROUP_SERVER:
					text = "Workgroup Storage Server";
					break;
				case OSProductTypeEx.PRODUCT_UNDEFINED:
					text = "Unknown product";
					break;
				case OSProductTypeEx.PRODUCT_ULTIMATE:
					text = "Ultimate";
					break;
				case OSProductTypeEx.PRODUCT_ULTIMATE_N:
					text = "Ultimate N";
					break;
				case OSProductTypeEx.PRODUCT_WEB_SERVER:
					text = "Web Server";
					break;
				case OSProductTypeEx.PRODUCT_WEB_SERVER_CORE:
					text = "Web Server (core installation)";
					break;
				}
			}
			_Edition = text;
			return text;
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
			string text = "unknown";
			OSNameChecker oSNameChecker = _OsNames.FirstOrDefault((OSNameChecker descr) => descr.Checker(this));
			if (oSNameChecker != null)
			{
				text = oSNameChecker.OSName;
			}
			_Name = text;
			return text;
		}
	}

	public string ServicePack => CSDVersion;

	public int BuildVersion => Environment.OSVersion.Version.Build;

	public int RevisionVersion => Environment.OSVersion.Version.Revision;

	internal OSVersionDescriptor()
	{
	}

	static OSVersionDescriptor()
	{
		FillOsNames();
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IsWow64Process([In] IntPtr processHandle, [MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetCurrentProcess();

	private bool TryCheckIsWow64Process(out Exception exception)
	{
		exception = null;
		bool wow64Process = false;
		try
		{
			IntPtr currentProcess = GetCurrentProcess();
			if (!IsWow64Process(currentProcess, out wow64Process))
			{
				exception = new Win32Exception(Marshal.GetLastWin32Error());
			}
		}
		catch (Exception ex)
		{
			exception = ex;
		}
		return wow64Process;
	}

	private static void FillOsNames()
	{
		_OsNames = new List<OSNameChecker>();
		_OsNames.Add(new OSNameChecker("Windows 95", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32Windows && v.MajorVersion == MajorVersion.Win95 && v.MinorVersion == MinorVersion.Win32s && (string.Compare(v.CSDVersion, "B", ignoreCase: true) != 0 || string.Compare(v.CSDVersion, "C", ignoreCase: true) != 0)));
		_OsNames.Add(new OSNameChecker("Windows 95 OSR2", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32Windows && v.MajorVersion == MajorVersion.Win95 && v.MinorVersion == MinorVersion.Win32s && (string.Compare(v.CSDVersion, "B", ignoreCase: true) == 0 || string.Compare(v.CSDVersion, "C", ignoreCase: true) == 0)));
		_OsNames.Add(new OSNameChecker("Windows 98", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32Windows && v.MajorVersion == MajorVersion.Win95 && v.MinorVersion == MinorVersion.Win98 && string.Compare(v.CSDVersion, "A", ignoreCase: true) != 0));
		_OsNames.Add(new OSNameChecker("Windows 98 Second Edition", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32Windows && v.MajorVersion == MajorVersion.Win95 && v.MinorVersion == MinorVersion.Win98 && string.Compare(v.CSDVersion, "A", ignoreCase: true) == 0));
		_OsNames.Add(new OSNameChecker("Windows Me", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32Windows && v.MajorVersion == MajorVersion.Win95 && v.MinorVersion == MinorVersion.WinME));
		_OsNames.Add(new OSNameChecker("Windows NT 3.51", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.WinNT351));
		_OsNames.Add(new OSNameChecker("Windows NT 4.0", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.Win95 && v.MinorVersion == MinorVersion.Win32s && v.ProductType == OSProductType.Workstation));
		_OsNames.Add(new OSNameChecker("Windows NT 4.0 Server", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.Win95 && v.MinorVersion == MinorVersion.Win32s && (v.ProductType == OSProductType.Server || v.ProductType == OSProductType.DomainController)));
		_OsNames.Add(new OSNameChecker("Windows 2000", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.WinNT5 && v.MinorVersion == MinorVersion.Win32s));
		_OsNames.Add(new OSNameChecker("Windows XP", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.WinNT5 && v.MinorVersion == MinorVersion.WinXP));
		_OsNames.Add(new OSNameChecker("Windows XP 64", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.WinNT5 && v.MinorVersion == MinorVersion.Win2003));
		_OsNames.Add(new OSNameChecker("Windows Server 2003", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.WinNT5 && v.MinorVersion == MinorVersion.Win2003));
		_OsNames.Add(new OSNameChecker("Windows Vista", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.Vista && v.MinorVersion == MinorVersion.Win32s && v.ProductType == OSProductType.Workstation));
		_OsNames.Add(new OSNameChecker("Windows 7", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.Vista && v.MinorVersion == MinorVersion.WinXP && v.ProductType == OSProductType.Workstation));
		_OsNames.Add(new OSNameChecker("Windows Server 2008", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.Vista && v.MinorVersion == MinorVersion.Win32s && (v.ProductType == OSProductType.Server || v.ProductType == OSProductType.DomainController)));
		_OsNames.Add(new OSNameChecker("Windows Server 2008 R2", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.Vista && v.MinorVersion == MinorVersion.WinXP && (v.ProductType == OSProductType.Server || v.ProductType == OSProductType.DomainController)));
		_OsNames.Add(new OSNameChecker("Windows 8", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.Vista && v.MinorVersion == MinorVersion.Win2003 && v.ProductType == OSProductType.Workstation));
		_OsNames.Add(new OSNameChecker("Windows Server 2012", (OSVersionDescriptor v) => v.PlatformId == PlatformID.Win32NT && v.MajorVersion == MajorVersion.Vista && v.MinorVersion == MinorVersion.Win2003 && (v.ProductType == OSProductType.Server || v.ProductType == OSProductType.DomainController)));
	}
}
