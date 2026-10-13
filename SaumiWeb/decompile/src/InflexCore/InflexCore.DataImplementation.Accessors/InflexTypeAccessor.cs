using System.Collections.Generic;
using BLToolkit.DataAccess;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.Accessors;

public abstract class InflexTypeAccessor : AppManager.InflexAccessorBase<InflexType, InflexTypeAccessor>
{
	[SqlQuery("select COMMENTS from sys.all_tab_comments where owner=upper(:schema) and table_name=upper(:tableName)")]
	public abstract string GetCommentForTable(string schema, string tableName);

	public InflexType GeneralSelectByType(params object[] keys)
	{
		return SelectByType((decimal)keys[0], (decimal)keys[1]);
	}

	[InflexSqlQuery("select * from <T> where type_id = :typeID and subtype = :subTypeID")]
	public abstract InflexType SelectByType(decimal typeID, decimal subTypeID);

	[InflexSqlQuery("select * from <T> where id = :id")]
	public abstract InflexType SelectParent(decimal id);

	[InflexSqlQuery("select * from <T> where table_r = :id")]
	public abstract IList<InflexType> SelectSubtypes(decimal id);
}
