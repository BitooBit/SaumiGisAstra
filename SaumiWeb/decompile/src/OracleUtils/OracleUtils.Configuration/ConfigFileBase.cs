using System;
using System.ComponentModel;
using System.IO;
using CommonUtils.EnvironmentUtils;
using CommonUtils.RegistryUtils;

namespace OracleUtils.Configuration;

public abstract class ConfigFileBase
{
	protected const char COMMENT_CHAR = '#';

	private readonly bool _IsDatabaseConfigFile;

	private readonly ConfigFileLocationType _LocationType;

	private readonly string _Path;

	public ConfigFileLocationType LocationType => _LocationType;

	public string Path => _Path;

	public bool Exists
	{
		get
		{
			if (!string.IsNullOrEmpty(Path))
			{
				return File.Exists(Path);
			}
			return false;
		}
	}

	public string Content
	{
		get
		{
			if (Exists)
			{
				return File.ReadAllText(Path);
			}
			return null;
		}
	}

	protected abstract string FileName { get; }

	protected ConfigFileBase(RegistryKeyView clientRegKey, string clientOracleHomePath, bool isDatabaseConfigFile)
	{
		if (clientRegKey == null)
		{
			throw new ArgumentNullException("clientRegKey", "Related client registry key cannot be null");
		}
		if (string.IsNullOrEmpty(clientOracleHomePath))
		{
			throw new ArgumentNullException("clientOracleHomePath", "Related client ORACLE_HOME path value cannot be null or empty");
		}
		_IsDatabaseConfigFile = isDatabaseConfigFile;
		Localize(clientRegKey, clientOracleHomePath, ref _Path, ref _LocationType);
	}

	public override string ToString()
	{
		return string.Concat(new object[5]
		{
			Path,
			" (",
			LocationType,
			")",
			Exists ? string.Empty : " (MISSING)"
		});
	}

	private void Localize(RegistryKeyView clientRegKey, string clientOracleHomePath, ref string filePath, ref ConfigFileLocationType fileLocationType)
	{
		if (TryGetTnsAdminValue(clientRegKey, out var tnsAdminValue, out var locationType) && !string.IsNullOrEmpty(tnsAdminValue))
		{
			string text = System.IO.Path.Combine(tnsAdminValue, FileName);
			if (File.Exists(text))
			{
				fileLocationType = locationType;
				filePath = text;
				return;
			}
		}
		string environmentVariable;
		EnvironmentVariableTarget variableRealTarget;
		if (_IsDatabaseConfigFile)
		{
			environmentVariable = Environment.GetEnvironmentVariable("ORACLE_HOME", EnvironmentVariableTarget.Machine);
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				fileLocationType = ConfigFileLocationType.OracleHomeMachineEnvironmentVariable;
				filePath = System.IO.Path.Combine(environmentVariable, FileName);
				return;
			}
		}
		else if (EnvironmentVariablesUtils.TryGetVariableValue("ORACLE_HOME", out environmentVariable, out variableRealTarget) && !string.IsNullOrEmpty(environmentVariable))
		{
			switch (variableRealTarget)
			{
			case EnvironmentVariableTarget.Process:
				fileLocationType = ConfigFileLocationType.OracleHomeProcessEnvironmentVariable;
				break;
			case EnvironmentVariableTarget.User:
				fileLocationType = ConfigFileLocationType.OracleHomeUserEnvironmentVariable;
				break;
			case EnvironmentVariableTarget.Machine:
				fileLocationType = ConfigFileLocationType.OracleHomeMachineEnvironmentVariable;
				break;
			default:
				throw new InvalidEnumArgumentException("Unknown environment variable target: " + variableRealTarget);
			}
			filePath = System.IO.Path.Combine(environmentVariable, FileName);
			return;
		}
		fileLocationType = ConfigFileLocationType.DefaultConfigLocation;
		if (!string.IsNullOrEmpty(clientOracleHomePath))
		{
			string path = System.IO.Path.Combine(clientOracleHomePath, OracleConstants.ORACLE_HOME_CONFIG_DIR_NAME);
			filePath = System.IO.Path.Combine(path, FileName);
		}
	}

	private bool TryGetTnsAdminValue(RegistryKeyView clientRegKey, out string tnsAdminValue, out ConfigFileLocationType locationType)
	{
		string environmentVariable;
		EnvironmentVariableTarget variableRealTarget;
		if (_IsDatabaseConfigFile)
		{
			environmentVariable = Environment.GetEnvironmentVariable("TNS_ADMIN", EnvironmentVariableTarget.Machine);
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				locationType = ConfigFileLocationType.TnsAdminMachineEnvironmentVariable;
				tnsAdminValue = environmentVariable;
				return true;
			}
		}
		else if (EnvironmentVariablesUtils.TryGetVariableValue("TNS_ADMIN", out environmentVariable, out variableRealTarget) && !string.IsNullOrEmpty(environmentVariable))
		{
			switch (variableRealTarget)
			{
			case EnvironmentVariableTarget.Process:
				locationType = ConfigFileLocationType.TnsAdminProcessEnvironmentVariable;
				break;
			case EnvironmentVariableTarget.User:
				locationType = ConfigFileLocationType.TnsAdminUserEnvironmentVariable;
				break;
			case EnvironmentVariableTarget.Machine:
				locationType = ConfigFileLocationType.TnsAdminMachineEnvironmentVariable;
				break;
			default:
				throw new InvalidEnumArgumentException("Unknown environment variable target: " + variableRealTarget);
			}
			tnsAdminValue = environmentVariable;
			return true;
		}
		string value = RegistryUtils.GetValue<string>(clientRegKey, "TNS_ADMIN");
		if (!string.IsNullOrEmpty(value))
		{
			locationType = ConfigFileLocationType.TnsAdminRegistryEntry;
			tnsAdminValue = value;
			return true;
		}
		locationType = ConfigFileLocationType.DefaultConfigLocation;
		tnsAdminValue = null;
		return false;
	}
}
