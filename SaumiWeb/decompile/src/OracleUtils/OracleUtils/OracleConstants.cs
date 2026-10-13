using System.IO;

namespace OracleUtils;

public static class OracleConstants
{
	public const string ODPNET_ASSEMBLY_SHORTNAME = "Oracle.DataAccess";

	public const string ORACLECONNECTION_TYPE_FULLNAME = "Oracle.DataAccess.Client.OracleConnection";

	public const string ORACLECOMMAND_TYPE_FULLNAME = "Oracle.DataAccess.Client.OracleCommand";

	public const string ORACLEEXCEPTION_TYPE_FULLNAME = "Oracle.DataAccess.Client.OracleException";

	public const string SYSTEMDATA_ORACLECLIENT_ASSEMBLY_SHORTNAME = "System.Data.OracleClient";

	public const string SYSTEMDATA_ORACLECONNECTION_TYPE_FULLNAME = "System.Data.OracleClient.OracleConnection";

	public const string SYSTEMDATA_ORACLECOMMAND_TYPE_FULLNAME = "System.Data.OracleClient.OracleCommand";

	public const string SYSTEMDATA_ORACLEEXCEPTION_TYPE_FULLNAME = "System.Data.OracleClient.OracleException";

	public const int LOGON_DENIED_ORACLE_EXCEPTION_NUMBER = 1017;

	public const string ORACLE_HOME_BIN_DIR_NAME = "bin";

	public const string ORACLE_HOME_ODPNET_DIR_NAME = "ODP.NET";

	public const string ODPNET_BIN_DIR_NAME = "bin";

	public const string ODPNET_RESOURCES_DIR_NAME = "Resources";

	public const string ODPNET_PUBLISHERPOLICY_DIR_NAME = "PublisherPolicy";

	public const string ENVIRONMENT_TNSADMIN_VARIABLE = "TNS_ADMIN";

	public const string ENVIRONMENT_ORACLEHOME_VARIABLE = "ORACLE_HOME";

	public const string ENVIRONMENT_ORACLEBASE_VARIABLE = "ORACLE_BASE";

	public const string ENVIRONMENT_ORACLESID_VARIABLE = "ORACLE_SID";

	public const string ORACLE_REGKEY_NAME = "ORACLE";

	public const string ORACLEHOME_REGENTRY_NAME = "ORACLE_HOME";

	public const string ORACLESID_REGENTRY_NAME = "ORACLE_SID";

	public const string ORACLEHOMENAME_REGENTRY_NAME = "ORACLE_HOME_NAME";

	public const string NLSLANG_REGENTRY_NAME = "NLS_LANG";

	public const string NLSLANG_REGENTRY_VALUE = "AMERICAN_AMERICA.CL8MSWIN1251";

	public const string TNSADMIN_REGENTRY_NAME = "TNS_ADMIN";

	public const string ODPNET_REGKEY_NAME = "ODP.NET";

	public const string ODPNET_DLLPATH_REGENTRY_NAME = "DllPath";

	public const string ODPNET_VERSION_REGENTRY_NAME = "Version";

	public const string TNSNAMES_CONFIG_FILE_NAME = "tnsnames.ora";

	public const string SQLNET_CONFIG_FILE_NAME = "sqlnet.ora";

	public const string LISTENER_CONFIG_FILE_NAME = "listener.ora";

	public const string OCI_LIBRARY_FILE_NAME = "oci.dll";

	public const string ODPNET_LIBRARY_FILE_NAME = "Oracle.DataAccess.dll";

	public const string ODPNET_RESOURCES_LIBRARY_FILE_NAME = "Oracle.DataAccess.resources.dll";

	public const string ODPNET_POLICIES_FILENAMES_STARTCOMMONPART = "Policy.";

	public const string ODPNET_POLICIES_FILENAMES_ENDCOMMONPART = ".Oracle.DataAccess.dll";

	public const string ODPNET_FILENAMES_COMMONPART = "Oracle.DataAccess";

	public const string ODPNET_9_POLICY_LIBRARY_FILE_NAME = "Policy.9.2.Oracle.DataAccess.dll";

	public const string ODPNET_10_POLICY_LIBRARY_FILE_NAME = "Policy.10.1.Oracle.DataAccess.dll";

	public const string ODPASPNET_LIBRARY_FILE_NAME = "Oracle.Web.dll";

	public const string ODPASPNET_RESOURCES_LIBRARY_FILE_NAME = "Oracle.Web.resources.dll";

	public const string ODPASPNET_POLICIES_FILENAMES_STARTCOMMONPART = "Policy.";

	public const string ODPASPNET_POLICIES_FILENAMES_ENDCOMMONPART = ".Oracle.Web.dll";

	public const string ODPASPNET_FILENAMES_COMMONPART = "Oracle.Web";

	public const string ORACLE_RDBMS_KERNEL_FILE_NAME = "oracle.exe";

	public const string ORACLE_TNS_LISTENER_FILE_NAME = "tnslsnr.exe";

	public const string SQLPLUS_UTILITY_FILE_NAME = "sqlplus.exe";

	public const string EXPORT_UTILITY_FILE_NAME = "exp.exe";

	public const string IMPORT_UTILITY_FILE_NAME = "imp.exe";

	public const string TNSPING_UTILITY_FILE_NAME = "tnsping.exe";

	public const string LSNRCTL_UTILITY_FILE_NAME = "lsnrctl.exe";

	public const string SQLNET_DIAGSIGHANDLERENABLED_PARAMETER = "DIAG_SIGHANDLER_ENABLED";

	public const string SQLNET_AUTHENTICATIONSERVICES_PARAMETER = "SQLNET.AUTHENTICATION_SERVICES";

	public const string SQLNET_AUTHENTICATIONSERVICES_NONE_VALUE = "(NONE)";

	public const string PARAMETER_TRUE_VALUE = "TRUE";

	public const string PARAMETER_FALSE_VALUE = "FALSE";

	public const string XE_CLIENT_HOME_NAME = "XEClient";

	public const string XE_SERVER_HOME_NAME = "XE";

	public const string SYS_USERNAME = "SYS";

	public const string DEFAULT_XE_DATASOURCE = "XE";

	public const string XE_SERVICE_NAME = "XE";

	public const string DEFAULT_TNSPORT = "1521";

	public const string DEFAULT_MTSPORT = "2030";

	public const string DEFAULT_HTTPPORT = "8080";

	public static readonly string ORACLE_HOME_CONFIG_DIR_NAME = "NETWORK" + Path.DirectorySeparatorChar + "ADMIN";

	public static readonly string ORACLE_REGKEY_PATH = "SOFTWARE" + '\\' + "ORACLE";

	public static readonly string ORACLE_WOW6432_REGKEY_PATH = "SOFTWARE" + '\\' + "Wow6432Node" + '\\' + "ORACLE";

	public static readonly string[] ODPNET_RESOURCES_LOCALIZATIONS = new string[9] { "de", "es", "fr", "it", "ja", "ko", "pt-BR", "zh-CHS", "zh-CHT" };

	public static readonly string[] ODPASPNET_RESOURCES_LOCALIZATIONS = new string[9] { "de", "es", "fr", "it", "ja", "ko", "pt-BR", "zh-CHS", "zh-CHT" };

	public static readonly string XE_CLIENT_REGKEY_NAME = "KEY_XEClient";

	public static readonly string XE_SERVER_REGKEY_NAME = "KEY_XE";
}
