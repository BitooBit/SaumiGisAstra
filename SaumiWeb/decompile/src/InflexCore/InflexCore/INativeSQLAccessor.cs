using System.Data;

namespace InflexCore;

public interface INativeSQLAccessor
{
	IDataReader ExecuteSQL(string sql, params NativeSQLParam[] parameters);

	decimal GetCountValues(string sql);
}
