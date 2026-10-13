using System.ComponentModel;
using System.Data.Common;
using System.Data.OracleClient;
using InflexCLR.Environment;

namespace InflexCLR.Oracle;

public static class QuerySQL
{
	public static TableRows Query(string sql)
	{
		OracleConnection connection = OracleProvider.GetConnection();
		return Query(connection, sql);
	}

	public static TableRows Query(OracleConnection connection, string sql)
	{
		OracleCommand val = connection.CreateCommand();
		((DbCommand)(object)val).CommandText = sql;
		((Component)(object)val).Dispose();
		OracleDataReader reader = val.ExecuteReader();
		return new TableRows(reader);
	}
}
