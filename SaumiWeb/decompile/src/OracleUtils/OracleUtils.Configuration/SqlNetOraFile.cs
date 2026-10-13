using System;
using CommonUtils.RegistryUtils;

namespace OracleUtils.Configuration;

public class SqlNetOraFile : ConfigFileBase
{
	protected override string FileName => "sqlnet.ora";

	public SqlNetOraFile(RegistryKeyView clientRegKey, string clientOracleHomePath, bool isDatabaseConfigFile)
		: base(clientRegKey, clientOracleHomePath, isDatabaseConfigFile)
	{
	}

	public bool EqualsByPath(SqlNetOraFile sqlNetOraFile)
	{
		return string.Compare(base.Path, sqlNetOraFile.Path, StringComparison.OrdinalIgnoreCase) == 0;
	}
}
