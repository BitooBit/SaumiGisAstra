using System.Collections.Generic;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.Accessors;

public abstract class InflexDictionaryColumnAccessor : AppManager.InflexAccessorBase<InflexDictionaryColumn, InflexDictionaryColumnAccessor>
{
	private const string _SQLText = "select t.column_id, t.owner, t.table_name, t.column_name, cc.comments\r\n, t.table_name||'.'||t.column_name full_column_name\r\n, t.owner||'.'||t.table_name full_table_name\r\nfrom sys.all_tab_columns t, sys.all_col_comments cc\r\nwhere t.OWNER=:owner and t.TABLE_NAME=:tableName and cc.owner=t.OWNER and cc.table_name=t.TABLE_NAME and cc.column_name=t.COLUMN_NAME";

	[InflexSqlQuery("select t.column_id, t.owner, t.table_name, t.column_name, cc.comments\r\n, t.table_name||'.'||t.column_name full_column_name\r\n, t.owner||'.'||t.table_name full_table_name\r\nfrom sys.all_tab_columns t, sys.all_col_comments cc\r\nwhere t.OWNER=:owner and t.TABLE_NAME=:tableName and cc.owner=t.OWNER and cc.table_name=t.TABLE_NAME and cc.column_name=t.COLUMN_NAME")]
	public abstract IList<InflexDictionaryColumn> SelectExplorerColumns(string owner, string tableName);
}
