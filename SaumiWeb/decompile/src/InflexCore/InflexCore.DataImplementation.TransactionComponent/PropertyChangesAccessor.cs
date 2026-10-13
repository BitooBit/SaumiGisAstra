using System.Collections.Generic;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.TransactionComponent;

public abstract class PropertyChangesAccessor : AppManager.InflexAccessorBase<PropertyChangeItem, PropertyChangesAccessor>
{
	[InflexSqlQuery("select * from <T> where action='U' and table_name=upper(:tableName) and id_value=:id order by change_date")]
	public abstract IList<PropertyChangeItem> GetSortedUpdateChanges(string tableName, decimal id);
}
