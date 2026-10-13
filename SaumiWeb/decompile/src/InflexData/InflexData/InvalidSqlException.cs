using System;

namespace InflexData;

public class InvalidSqlException : Exception
{
	public InvalidSqlException(string sql, Exception innerException)
		: base("InvalidSQL:" + Environment.NewLine + innerException.Message + Environment.NewLine + sql, innerException)
	{
	}
}
