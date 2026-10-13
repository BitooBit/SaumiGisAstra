using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CommonUtils.EnvironmentUtils;
using CommonUtils.RegistryUtils;
using OracleUtils.Configuration;

namespace OracleUtils.Scanner;

public class OracleClientInstallation
{
	private readonly RegistryKeyView _InstallationRegKey;

	private readonly AppPlatformBits _Bits;

	private string _OracleHomePath;

	private string _OracleHomeName;

	private string _NlsLangValue;

	private string _OciLibraryPath;

	private bool? _IsOciDirectorySpecifiedAtEnvironmentPathVariable;

	private int? _OciDirectoryPositionAtEnvironmentPathVariable;

	private FileVersionInfo _OciLibraryVersion;

	private bool? _IsDatabase;

	private FileVersionInfo _DatabaseVersion;

	private bool? _IsExpressEdition;

	private IList<OdpNetInstallation> _RelatedOdpNetInstallations = new List<OdpNetInstallation>();

	private ListenerOraFile _ListenerOraFile;

	private SqlNetOraFile _DatabaseSqlNetOraFile;

	private SqlNetOraFile _SqlNetOraFile;

	private TnsNamesOraFile _TnsNamesOraFile;

	public string RegKeyPath => _InstallationRegKey.Name;

	public AppPlatformBits Bits => _Bits;

	public string OracleHomePath
	{
		get
		{
			if (string.IsNullOrEmpty(_OracleHomePath))
			{
				_OracleHomePath = GetOracleHomePath();
			}
			return _OracleHomePath;
		}
	}

	public string OracleHomeName
	{
		get
		{
			if (string.IsNullOrEmpty(_OracleHomeName))
			{
				_OracleHomeName = GetOracleHomeName();
			}
			return _OracleHomeName;
		}
	}

	public string NlsLangValue
	{
		get
		{
			if (string.IsNullOrEmpty(_NlsLangValue))
			{
				_NlsLangValue = GetNlsLangValue();
			}
			return _NlsLangValue;
		}
	}

	public string OciLibraryPath
	{
		get
		{
			if (string.IsNullOrEmpty(_OciLibraryPath))
			{
				_OciLibraryPath = GetOciLibraryPath();
			}
			return _OciLibraryPath;
		}
	}

	public string OciDirectoryPath
	{
		get
		{
			string ociLibraryPath = OciLibraryPath;
			if (!string.IsNullOrEmpty(ociLibraryPath))
			{
				return Path.GetDirectoryName(ociLibraryPath);
			}
			return null;
		}
	}

	public bool IsOciDirectorySpecifiedAtEnvironmentPathVariable
	{
		get
		{
			if (!_IsOciDirectorySpecifiedAtEnvironmentPathVariable.HasValue)
			{
				_IsOciDirectorySpecifiedAtEnvironmentPathVariable = CheckIsOciDirectorySpecifiedAtEnvironmentPathVariable();
			}
			return _IsOciDirectorySpecifiedAtEnvironmentPathVariable.Value;
		}
	}

	public int OciDirectoryPositionAtEnvironmentPathVariable
	{
		get
		{
			if (!_OciDirectoryPositionAtEnvironmentPathVariable.HasValue)
			{
				_OciDirectoryPositionAtEnvironmentPathVariable = GetOciDirectoryPositionAtEnvironmentPathVariable();
			}
			return _OciDirectoryPositionAtEnvironmentPathVariable.Value;
		}
	}

	public FileVersionInfo OciLibraryVersion
	{
		get
		{
			if (_OciLibraryVersion == null)
			{
				_OciLibraryVersion = GetOciLibraryVersion();
			}
			return _OciLibraryVersion;
		}
	}

	public bool IsDatabase
	{
		get
		{
			if (!_IsDatabase.HasValue)
			{
				_IsDatabase = CheckIsDatabase();
			}
			return _IsDatabase.Value;
		}
	}

	public FileVersionInfo DatabaseVersion
	{
		get
		{
			if (_DatabaseVersion == null)
			{
				_DatabaseVersion = GetDatabaseVersion();
			}
			return _DatabaseVersion;
		}
	}

	public bool IsExpressEdition
	{
		get
		{
			if (!_IsExpressEdition.HasValue)
			{
				_IsExpressEdition = CheckIsExpressEdition();
			}
			return _IsExpressEdition.Value;
		}
	}

	public IList<OdpNetInstallation> RelatedOdpNetInstallations => _RelatedOdpNetInstallations;

	public bool HasRelatedOdpNetInstallation => RelatedOdpNetInstallations.Count > 0;

	public string SqlPlusUtilityPath
	{
		get
		{
			string ociDirectoryPath = OciDirectoryPath;
			if (!string.IsNullOrEmpty(ociDirectoryPath))
			{
				return Path.Combine(ociDirectoryPath, "sqlplus.exe");
			}
			return null;
		}
	}

	public bool HasSqlPlusUtility
	{
		get
		{
			string sqlPlusUtilityPath = SqlPlusUtilityPath;
			if (!string.IsNullOrEmpty(sqlPlusUtilityPath))
			{
				return File.Exists(sqlPlusUtilityPath);
			}
			return false;
		}
	}

	public string ExportUtilityPath
	{
		get
		{
			string ociDirectoryPath = OciDirectoryPath;
			if (!string.IsNullOrEmpty(ociDirectoryPath))
			{
				return Path.Combine(ociDirectoryPath, "exp.exe");
			}
			return null;
		}
	}

	public string ImportUtilityPath
	{
		get
		{
			string ociDirectoryPath = OciDirectoryPath;
			if (!string.IsNullOrEmpty(ociDirectoryPath))
			{
				return Path.Combine(ociDirectoryPath, "imp.exe");
			}
			return null;
		}
	}

	public bool HasExportImportUtilities
	{
		get
		{
			string exportUtilityPath = ExportUtilityPath;
			string importUtilityPath = ImportUtilityPath;
			if (!string.IsNullOrEmpty(exportUtilityPath) && !string.IsNullOrEmpty(importUtilityPath))
			{
				bool flag = File.Exists(exportUtilityPath);
				bool result = File.Exists(importUtilityPath);
				if (flag)
				{
					return result;
				}
				return false;
			}
			return false;
		}
	}

	public string TnsPingUtilityPath
	{
		get
		{
			string ociDirectoryPath = OciDirectoryPath;
			if (!string.IsNullOrEmpty(ociDirectoryPath))
			{
				return Path.Combine(ociDirectoryPath, "tnsping.exe");
			}
			return null;
		}
	}

	public bool HasTnsPingUtility
	{
		get
		{
			string tnsPingUtilityPath = TnsPingUtilityPath;
			if (!string.IsNullOrEmpty(tnsPingUtilityPath))
			{
				return File.Exists(tnsPingUtilityPath);
			}
			return false;
		}
	}

	public string LsnrCtlUtilityPath
	{
		get
		{
			string ociDirectoryPath = OciDirectoryPath;
			if (!string.IsNullOrEmpty(ociDirectoryPath))
			{
				return Path.Combine(ociDirectoryPath, "lsnrctl.exe");
			}
			return null;
		}
	}

	public bool HasLsnrCtlUtility
	{
		get
		{
			string lsnrCtlUtilityPath = LsnrCtlUtilityPath;
			if (!string.IsNullOrEmpty(lsnrCtlUtilityPath))
			{
				return File.Exists(lsnrCtlUtilityPath);
			}
			return false;
		}
	}

	public ListenerOraFile ListenerOraFile => _ListenerOraFile ?? (_ListenerOraFile = new ListenerOraFile(_InstallationRegKey, OracleHomePath));

	[Obsolete("Use property ListenerOraFile.Path instead")]
	public string ListenerOraFilePath => ListenerOraFile.Path;

	[Obsolete("Use property ListenerOraFile.Content instead")]
	public string ListenerOraFileContent => ListenerOraFile.Content;

	public SqlNetOraFile DatabaseSqlNetOraFile => _DatabaseSqlNetOraFile ?? (_DatabaseSqlNetOraFile = new SqlNetOraFile(_InstallationRegKey, OracleHomePath, isDatabaseConfigFile: true));

	public SqlNetOraFile SqlNetOraFile => _SqlNetOraFile ?? (_SqlNetOraFile = new SqlNetOraFile(_InstallationRegKey, OracleHomePath, isDatabaseConfigFile: false));

	[Obsolete("Use property SqlNetOraFile.Path instead")]
	public string SqlNetOraFilePath => SqlNetOraFile.Path;

	[Obsolete("Use property SqlNetOraFile.Content instead")]
	public string SqlNetOraFileContent => SqlNetOraFile.Content;

	public TnsNamesOraFile TnsNamesOraFile => _TnsNamesOraFile ?? (_TnsNamesOraFile = new TnsNamesOraFile(_InstallationRegKey, OracleHomePath));

	[Obsolete("Use property TnsNamesOraFile.Path instead")]
	public string TnsNamesOraFilePath => TnsNamesOraFile.Path;

	[Obsolete("Use property TnsNamesOraFile.Content instead")]
	public string TnsNamesOraFileContent => TnsNamesOraFile.Content;

	public OracleClientInstallation(RegistryKeyView installationRegKey, AppPlatformBits clientBits)
	{
		if (installationRegKey == null)
		{
			throw new ArgumentNullException("installationRegKey", "Oracle database installation registry key cannot be NULL");
		}
		_InstallationRegKey = installationRegKey;
		_Bits = clientBits;
	}

	public OracleClientInstallation(RegistryKeyView installationRegKey, string oracleHomePath, AppPlatformBits clientBits)
		: this(installationRegKey, clientBits)
	{
		if (string.IsNullOrEmpty(oracleHomePath))
		{
			throw new ArgumentNullException("oracleHomePath", "Oracle client home directory path cannot be NULL or EMPTY");
		}
		_OracleHomePath = oracleHomePath;
	}

	public string ToString(bool showRelatedOdpNetInstallations)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Concat(new object[5]
		{
			"Oracle ",
			IsDatabase ? "DATABASE " : "CLIENT ",
			IsExpressEdition ? "XE " : string.Empty,
			Bits,
			" installation description: {"
		}));
		stringBuilder.AppendLine("RegKeyPath: " + RegKeyPath + ";");
		stringBuilder.AppendLine("OracleHomePath: " + OracleHomePath + ";");
		stringBuilder.AppendLine("OracleHomeName: " + OracleHomeName + ";");
		stringBuilder.AppendLine("NlsLangValue: " + NlsLangValue + ";");
		stringBuilder.AppendLine("OciLibraryPath: " + OciLibraryPath + " (" + (IsOciDirectorySpecifiedAtEnvironmentPathVariable ? ("registered on " + OciDirectoryPositionAtEnvironmentPathVariable + " position") : "not registered") + " at PATH);");
		stringBuilder.AppendLine("OciLibraryVersion: " + ((OciLibraryVersion != null) ? OciLibraryVersion.FileVersion : "UNDEFINED") + ";");
		stringBuilder.AppendLine(string.Concat("SqlNetOraFilePath: ", SqlNetOraFile, ";"));
		stringBuilder.AppendLine(string.Concat("TnsNamesOraFilePath: ", TnsNamesOraFile, ";"));
		if (IsDatabase)
		{
			stringBuilder.AppendLine("DatabaseVersion: " + ((DatabaseVersion != null) ? DatabaseVersion.FileVersion : "UNDEFINED") + ";");
			stringBuilder.AppendLine(string.Concat("ListenerOraFilePath: ", ListenerOraFile, ";"));
			if (!SqlNetOraFile.EqualsByPath(DatabaseSqlNetOraFile))
			{
				stringBuilder.AppendLine(string.Concat("DatabaseSqlNetOraFilePath: ", DatabaseSqlNetOraFile, ";"));
			}
		}
		if (HasSqlPlusUtility)
		{
			stringBuilder.AppendLine("Embedded SQL*PLus utility is available;");
		}
		if (HasExportImportUtilities)
		{
			stringBuilder.AppendLine("Embedded Export & Import utilities are available;");
		}
		if (HasTnsPingUtility)
		{
			stringBuilder.AppendLine("Embedded TNSPING utility is available;");
		}
		int count = RelatedOdpNetInstallations.Count;
		if (showRelatedOdpNetInstallations)
		{
			switch (count)
			{
			case 0:
				stringBuilder.AppendLine("Related ODP.NET installations were NOT found;");
				break;
			case 1:
				stringBuilder.AppendLine("Related ODP.NET installation: {");
				stringBuilder.Append(RelatedOdpNetInstallations[0].ToString());
				stringBuilder.AppendLine("}");
				break;
			default:
			{
				stringBuilder.AppendLine("Related ODP.NET installations: {");
				for (int i = 0; i < count; i++)
				{
					stringBuilder.AppendLine(i + 1 + ":");
					stringBuilder.Append(RelatedOdpNetInstallations[i].ToString());
				}
				stringBuilder.AppendLine("}");
				break;
			}
			}
		}
		else
		{
			stringBuilder.AppendLine("Related ODP.NET installations count: " + count + ";");
		}
		stringBuilder.AppendLine("}");
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		return ToString(showRelatedOdpNetInstallations: true);
	}

	private string GetOracleHomePath()
	{
		return RegistryUtils.GetValue<string>(_InstallationRegKey, "ORACLE_HOME");
	}

	private string GetOracleHomeName()
	{
		return RegistryUtils.GetValue<string>(_InstallationRegKey, "ORACLE_HOME_NAME");
	}

	private string GetNlsLangValue()
	{
		return RegistryUtils.GetValue<string>(_InstallationRegKey, "NLS_LANG");
	}

	private string GetOracleSid()
	{
		return RegistryUtils.GetValue<string>(_InstallationRegKey, "ORACLE_SID");
	}

	private string GetOciLibraryPath()
	{
		string text = Path.Combine(OracleHomePath, "bin" + Path.DirectorySeparatorChar + "oci.dll");
		if (File.Exists(text))
		{
			return text;
		}
		string text2 = Path.Combine(OracleHomePath, "oci.dll");
		if (File.Exists(text2))
		{
			return text2;
		}
		return null;
	}

	private bool CheckIsOciDirectorySpecifiedAtEnvironmentPathVariable()
	{
		string ociLibraryPath = OciLibraryPath;
		if (!string.IsNullOrEmpty(ociLibraryPath))
		{
			string ociDirectoryPath = OciDirectoryPath;
			if (!string.IsNullOrEmpty(ociDirectoryPath))
			{
				ociDirectoryPath = ociDirectoryPath.TrimEnd(new char[2]
				{
					Path.DirectorySeparatorChar,
					Path.AltDirectorySeparatorChar
				});
				return EnvironmentVariablesUtils.ContainsPersistentVariableCustomValue("PATH", ociDirectoryPath);
			}
		}
		return false;
	}

	private int GetOciDirectoryPositionAtEnvironmentPathVariable()
	{
		string ociDirectoryPath = OciDirectoryPath;
		if (!string.IsNullOrEmpty(ociDirectoryPath))
		{
			ociDirectoryPath = ociDirectoryPath.TrimEnd(new char[2]
			{
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar
			});
			return EnvironmentVariablesUtils.GetPersistentVariableCustomValuePosition("PATH", ociDirectoryPath);
		}
		return -1;
	}

	private FileVersionInfo GetOciLibraryVersion()
	{
		string ociLibraryPath = OciLibraryPath;
		if (File.Exists(ociLibraryPath))
		{
			return FileVersionInfo.GetVersionInfo(ociLibraryPath);
		}
		return null;
	}

	private bool CheckIsDatabase()
	{
		bool flag = false;
		string[] valueNames = _InstallationRegKey.GetValueNames();
		string[] array = valueNames;
		foreach (string text in array)
		{
			if (text.ToUpper().StartsWith("ORA_") && text.ToUpper().EndsWith("_AUTOSTART"))
			{
				flag = true;
			}
		}
		bool result = false;
		string ociDirectoryPath = OciDirectoryPath;
		if (!string.IsNullOrEmpty(ociDirectoryPath))
		{
			string path = Path.Combine(ociDirectoryPath, "oracle.exe");
			if (File.Exists(path))
			{
				result = true;
			}
		}
		if (flag)
		{
			return result;
		}
		return false;
	}

	private FileVersionInfo GetDatabaseVersion()
	{
		string ociDirectoryPath = OciDirectoryPath;
		if (!string.IsNullOrEmpty(ociDirectoryPath))
		{
			string text = Path.Combine(ociDirectoryPath, "oracle.exe");
			if (File.Exists(text))
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(text);
				if (versionInfo != null)
				{
					return versionInfo;
				}
			}
		}
		return null;
	}

	private bool CheckIsExpressEdition()
	{
		if (!IsDatabase)
		{
			return string.Compare(OracleHomeName, "XEClient", StringComparison.OrdinalIgnoreCase) == 0;
		}
		if (DatabaseVersion.FileMajorPart >= 10)
		{
			string oracleSid = GetOracleSid();
			return string.Compare(OracleHomeName, "XE", StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(oracleSid, "XE", StringComparison.OrdinalIgnoreCase) == 0;
		}
		return false;
	}
}
