using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.Accessors;

public abstract class InflexDictionaryTypeAccessor : AppManager.InflexAccessorBase<InflexDictionaryType, InflexDictionaryTypeAccessor>
{
	private const string _SQLText = "select cc.column_name from sys.all_constraints c, sys.all_cons_columns cc\r\nwhere c.OWNER=:owner and c.table_name=:tableName and c.constraint_type='P' and c.constraint_name=cc.constraint_name and cc.position=1";

	public string GetPrimaryColumnForTable(string owner, string tableName)
	{
		string text = PrimaryColumnForTable(owner, tableName);
		return text ?? string.Empty;
	}

	[InflexSqlQuery("select cc.column_name from sys.all_constraints c, sys.all_cons_columns cc\r\nwhere c.OWNER=:owner and c.table_name=:tableName and c.constraint_type='P' and c.constraint_name=cc.constraint_name and cc.position=1")]
	protected abstract string PrimaryColumnForTable(string owner, string tableName);
}
