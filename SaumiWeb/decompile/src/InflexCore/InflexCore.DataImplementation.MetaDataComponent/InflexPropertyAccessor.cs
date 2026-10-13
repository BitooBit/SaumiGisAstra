using System.Collections.Generic;
using System.Data;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.MetaDataComponent;

public abstract class InflexPropertyAccessor : AppManager.InflexAccessorBase<InflexProperty, InflexPropertyAccessor>
{
	[InflexSqlQuery("select t.*, c.history history from <T> t, <SCHEMA>.fm_v_tab_columns c\r\nwhere t.owner = upper(:owner) and t.table_name = upper(:tableName) and t.column_name=c.column_name(+) and t.table_name=c.table_name(+)")]
	public abstract IList<InflexProperty> SelectProperties(string owner, string tableName);

	public IList<string> GetTableChecks(string owner, string tableName)
	{
		List<string> list = new List<string>();
		using IDataReader dataReader = base.AppManager.NativeSQLAccessor.ExecuteSQL("select search_condition from sys.all_constraints t where t.owner=:owner and t.table_name=:tableName and t.constraint_type='C'", new NativeSQLParam("owner", owner.ToUpper()), new NativeSQLParam("tableName", tableName.ToUpper()));
		while (dataReader.Read())
		{
			list.Add(dataReader.GetString(0));
		}
		return list;
	}
}
