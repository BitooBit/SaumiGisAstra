namespace OracleUtils.Configuration;

public enum ConfigFileLocationType
{
	DefaultConfigLocation,
	OracleHomeMachineEnvironmentVariable,
	OracleHomeUserEnvironmentVariable,
	OracleHomeProcessEnvironmentVariable,
	TnsAdminRegistryEntry,
	TnsAdminMachineEnvironmentVariable,
	TnsAdminUserEnvironmentVariable,
	TnsAdminProcessEnvironmentVariable
}
