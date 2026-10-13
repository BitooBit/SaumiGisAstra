using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CommonUtils.AppDomainUtils;
using CommonUtils.EnvironmentUtils;
using CommonUtils.IOUtils;
using CommonUtils.ParseUtils;
using CommonUtils.RegistryUtils;

namespace OracleUtils.Scanner;

public class OdpNetInstallation
{
	private readonly RegistryKeyView _InstallationRegKey;

	private readonly AppPlatformBits _Bits;

	private string _UnmanagedLibrariesPath;

	private string _OdpNetVersion;

	private string _OdpNetLibraryPath;

	private string _ResourcesPath;

	private string _PublisherPoliciesPath;

	private FileVersionInfo _OdpNetLibraryVersion;

	private bool? _IsInstalledToGac;

	public AppPlatformBits Bits => _Bits;

	public string UnmanagedLibrariesPath
	{
		get
		{
			if (string.IsNullOrEmpty(_UnmanagedLibrariesPath))
			{
				_UnmanagedLibrariesPath = GetUnmanagedLibrariesPath();
			}
			return _UnmanagedLibrariesPath;
		}
	}

	public string OdpNetVersion
	{
		get
		{
			if (string.IsNullOrEmpty(_OdpNetVersion))
			{
				_OdpNetVersion = GetOdpNetVersionFromInstallationRegKey();
			}
			return _OdpNetVersion;
		}
	}

	public string OdpNetLibraryPath
	{
		get
		{
			if (string.IsNullOrEmpty(_OdpNetLibraryPath))
			{
				_OdpNetLibraryPath = GetOdpNetLibraryPath();
			}
			return _OdpNetLibraryPath;
		}
	}

	public string OdpNetLibraryDirectoryPath
	{
		get
		{
			string odpNetLibraryPath = OdpNetLibraryPath;
			if (!string.IsNullOrEmpty(odpNetLibraryPath))
			{
				return Path.GetDirectoryName(odpNetLibraryPath);
			}
			return null;
		}
	}

	public string ResourcesPath
	{
		get
		{
			if (string.IsNullOrEmpty(_ResourcesPath))
			{
				_ResourcesPath = GetOdpNetResourcesPath();
			}
			return _ResourcesPath;
		}
	}

	public string PublisherPoliciesPath
	{
		get
		{
			if (string.IsNullOrEmpty(_PublisherPoliciesPath))
			{
				_PublisherPoliciesPath = GetOdpNetPublisherPoliciesPath();
			}
			return _PublisherPoliciesPath;
		}
	}

	public FileVersionInfo OdpNetLibraryVersion
	{
		get
		{
			if (_OdpNetLibraryVersion == null)
			{
				_OdpNetLibraryVersion = GetOdpNetLibraryVersion();
			}
			return _OdpNetLibraryVersion;
		}
	}

	public bool IsInstalledToGac
	{
		get
		{
			if (!_IsInstalledToGac.HasValue)
			{
				_IsInstalledToGac = CheckIsOdpNetLibraryInstalledToGac();
			}
			return _IsInstalledToGac.Value;
		}
	}

	public OdpNetInstallation(RegistryKeyView installationRegKey, AppPlatformBits odpNetBits)
	{
		if (installationRegKey == null)
		{
			throw new ArgumentNullException("installationRegKey", "ODP.NET installation registry key cannot be NULL");
		}
		_InstallationRegKey = installationRegKey;
		_Bits = odpNetBits;
	}

	public OdpNetInstallation(RegistryKeyView installationRegKey, string unmanagedLibrariesPath, AppPlatformBits odpNetBits)
		: this(installationRegKey, odpNetBits)
	{
		if (string.IsNullOrEmpty(unmanagedLibrariesPath))
		{
			throw new ArgumentNullException("unmanagedLibrariesPath", "Unmanaged libraries directory path cannot be NULL or EMPTY");
		}
		_UnmanagedLibrariesPath = unmanagedLibrariesPath;
	}

	public bool Contains(string assemblyFilePath)
	{
		PePlatformType pePlatformType = PeHeaderScanner.GetPePlatformType(assemblyFilePath);
		if (Bits.Matches(pePlatformType))
		{
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assemblyFilePath);
			string odpNetVersion = OdpNetVersion;
			if (odpNetVersion != "UNDEFINED" && string.Compare(odpNetVersion, versionInfo.FileVersion, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
			FileVersionInfo odpNetLibraryVersion = OdpNetLibraryVersion;
			if (odpNetLibraryVersion != null && string.Compare(odpNetLibraryVersion.FileVersion, versionInfo.FileVersion, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
		}
		string fileName = Path.GetFileName(assemblyFilePath);
		if (!string.IsNullOrEmpty(fileName))
		{
			if (string.Compare(fileName, "Oracle.DataAccess.dll", StringComparison.OrdinalIgnoreCase) == 0 && DiscUtils.CompareFiles(assemblyFilePath, OdpNetLibraryPath))
			{
				return true;
			}
			if (!string.IsNullOrEmpty(ResourcesPath) && string.Compare(fileName, "Oracle.DataAccess.resources.dll", StringComparison.OrdinalIgnoreCase) == 0)
			{
				GacAssemblyDescription gacAssemblyDescription = new GacAssemblyDescription(assemblyFilePath);
				CultureInfo culture = gacAssemblyDescription.Culture;
				if (culture != null)
				{
					string text = culture.ToString();
					if (!string.IsNullOrEmpty(text))
					{
						string path = Path.Combine(Path.Combine(ResourcesPath, text), "Oracle.DataAccess.resources.dll");
						if (DiscUtils.CompareFiles(assemblyFilePath, path))
						{
							return true;
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(PublisherPoliciesPath) && fileName.StartsWith("Policy.", StringComparison.OrdinalIgnoreCase) && fileName.EndsWith(".Oracle.DataAccess.dll", StringComparison.OrdinalIgnoreCase) && DiscUtils.CompareFiles(assemblyFilePath, Path.Combine(PublisherPoliciesPath, fileName)))
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Concat("ODP.NET ", Bits, " installation description: {"));
		stringBuilder.AppendLine("OdpNetVersion: " + OdpNetVersion + ";");
		stringBuilder.AppendLine("UnmanagedLibrariesPath: " + UnmanagedLibrariesPath + ";");
		stringBuilder.AppendLine("OdpNetLibraryPath: " + OdpNetLibraryPath + ";");
		string resourcesPath = ResourcesPath;
		stringBuilder.AppendLine("ResourcesPath: " + ((!string.IsNullOrEmpty(resourcesPath)) ? resourcesPath : "UNDEFINED") + ";");
		string publisherPoliciesPath = PublisherPoliciesPath;
		stringBuilder.AppendLine("PublisherPoliciesPath: " + ((!string.IsNullOrEmpty(publisherPoliciesPath)) ? publisherPoliciesPath : "UNDEFINED") + ";");
		FileVersionInfo odpNetLibraryVersion = OdpNetLibraryVersion;
		stringBuilder.AppendLine("OdpNetLibraryVersion: " + ((odpNetLibraryVersion != null) ? odpNetLibraryVersion.FileVersion : "UNDEFINED") + ";");
		stringBuilder.AppendLine(string.Concat(new object[7]
		{
			"Oracle.DataAccess.dll",
			IsInstalledToGac ? " is " : " is NOT ",
			"installed to GAC for current CLR ",
			EnvironmentScanner.ApplicationEnvironmentBits,
			" v",
			Environment.Version,
			";"
		}));
		stringBuilder.AppendLine("}");
		return stringBuilder.ToString();
	}

	private string GetUnmanagedLibrariesPath()
	{
		return RegistryUtils.GetValue<string>(_InstallationRegKey, "DllPath");
	}

	private string GetOdpNetVersionFromInstallationRegKey()
	{
		string[] valueNames = _InstallationRegKey.GetValueNames();
		string text = ((!Enumerable.Contains(valueNames, "Version")) ? RegistryUtils.GetRegistryKeyShortName(_InstallationRegKey.Name) : RegistryUtils.GetValue<string>(_InstallationRegKey, "Version"));
		if (text.TryParseToVersion(out var _))
		{
			return text;
		}
		return "UNDEFINED";
	}

	private string GetOdpNetLibraryPath()
	{
		string unmanagedLibrariesPath = UnmanagedLibrariesPath;
		if (!string.IsNullOrEmpty(unmanagedLibrariesPath))
		{
			string text = Path.Combine(unmanagedLibrariesPath, "Oracle.DataAccess.dll");
			if (File.Exists(text))
			{
				return text;
			}
			string text2 = Path.Combine(DiscUtils.GetParentDirectory(unmanagedLibrariesPath), "ODP.NET") + Path.DirectorySeparatorChar + "bin";
			if (Directory.Exists(text2))
			{
				text = Path.Combine(text2, "Oracle.DataAccess.dll");
				if (File.Exists(text))
				{
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(text);
					if (string.Compare(versionInfo.FileVersion, OdpNetVersion, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return text;
					}
				}
				string[] directories = Directory.GetDirectories(text2);
				string[] array = directories;
				foreach (string path in array)
				{
					text = Path.Combine(path, "Oracle.DataAccess.dll");
					if (File.Exists(text))
					{
						FileVersionInfo versionInfo2 = FileVersionInfo.GetVersionInfo(text);
						if (string.Compare(versionInfo2.FileVersion, OdpNetVersion, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return text;
						}
					}
				}
			}
		}
		return null;
	}

	private string GetOdpNetResourcesPath()
	{
		string unmanagedLibrariesPath = UnmanagedLibrariesPath;
		if (!string.IsNullOrEmpty(unmanagedLibrariesPath))
		{
			string path = Path.Combine(DiscUtils.GetParentDirectory(unmanagedLibrariesPath), "ODP.NET");
			string text = Path.Combine(path, "Resources");
			if (Directory.Exists(text))
			{
				return text;
			}
			string text2 = Path.Combine(path, "bin");
			if (Directory.Exists(text2))
			{
				text = Path.Combine(text2, "Resources");
				if (Directory.Exists(text))
				{
					return text;
				}
				string[] directories = Directory.GetDirectories(text2);
				string[] array = directories;
				foreach (string path2 in array)
				{
					text = Path.Combine(path2, "Resources");
					if (!Directory.Exists(text))
					{
						continue;
					}
					bool flag = true;
					string[] directories2 = Directory.GetDirectories(text);
					string[] array2 = directories2;
					foreach (string path3 in array2)
					{
						string text3 = Path.Combine(path3, "Oracle.DataAccess.resources.dll");
						if (File.Exists(text3))
						{
							FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(text3);
							if (string.Compare(versionInfo.FileVersion, OdpNetVersion, StringComparison.OrdinalIgnoreCase) != 0)
							{
								flag = false;
							}
						}
					}
					if (flag)
					{
						return text;
					}
				}
			}
		}
		return null;
	}

	private string GetOdpNetPublisherPoliciesPath()
	{
		string unmanagedLibrariesPath = UnmanagedLibrariesPath;
		if (!string.IsNullOrEmpty(unmanagedLibrariesPath))
		{
			string text = Path.Combine(DiscUtils.GetParentDirectory(unmanagedLibrariesPath), "ODP.NET") + Path.DirectorySeparatorChar + "PublisherPolicy";
			if (Directory.Exists(text))
			{
				string[] directories = Directory.GetDirectories(text);
				if (directories.Length == 0)
				{
					return text;
				}
				string[] array = directories;
				foreach (string text2 in array)
				{
					bool flag = true;
					string[] files = Directory.GetFiles(text2);
					string[] array2 = files;
					foreach (string text3 in array2)
					{
						string fileName = Path.GetFileName(text3);
						if (!string.IsNullOrEmpty(fileName) && fileName.StartsWith("Policy.", StringComparison.OrdinalIgnoreCase) && fileName.EndsWith(".Oracle.DataAccess.dll", StringComparison.OrdinalIgnoreCase))
						{
							FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(text3);
							if (string.Compare(versionInfo.FileVersion, OdpNetVersion, StringComparison.OrdinalIgnoreCase) != 0)
							{
								flag = false;
							}
						}
					}
					if (flag)
					{
						return text2;
					}
				}
			}
		}
		return null;
	}

	private FileVersionInfo GetOdpNetLibraryVersion()
	{
		string odpNetLibraryPath = OdpNetLibraryPath;
		if (File.Exists(odpNetLibraryPath))
		{
			return FileVersionInfo.GetVersionInfo(odpNetLibraryPath);
		}
		return null;
	}

	private bool CheckIsOdpNetLibraryInstalledToGac()
	{
		string odpNetLibraryPath = OdpNetLibraryPath;
		if (!string.IsNullOrEmpty(odpNetLibraryPath))
		{
			return AssemblyCheckUtils.CheckAssemblyUsingAppDomain(odpNetLibraryPath, "CheckOdpInstalledToGacDomain");
		}
		return false;
	}
}
