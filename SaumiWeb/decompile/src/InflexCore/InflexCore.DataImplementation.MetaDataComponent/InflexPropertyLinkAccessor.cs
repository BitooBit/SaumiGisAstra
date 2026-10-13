using System.Collections.Generic;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.MetaDataComponent;

public abstract class InflexPropertyLinkAccessor : AppManager.InflexAccessorBase<InflexPropertyLink, InflexPropertyLinkAccessor>
{
	private const string _SQLText = "select cc.column_name, ft.type_id, ft.subtype subtype_id, d.id dict_id\r\nfrom sys.all_constraints t, sys.all_constraints t2, sys.all_cons_columns cc, <SCHEMA>.fm_tables ft, <SCHEMA>.dictionaries d\r\nwhere t.constraint_type='R' and t.owner=upper(:owner) and t.table_name=upper(:tableName) and t.constraint_name=cc.constraint_name and cc.position=1\r\nand t.r_constraint_name=t2.constraint_name and (t2.constraint_type='P' or t2.constraint_type='U') and t2.owner=t.owner\r\nand ft.table_name(+)=t2.table_name and d.table_name(+)=t2.owner||'.'||t2.table_name";

	[InflexSqlQuery("select cc.column_name, ft.type_id, ft.subtype subtype_id, d.id dict_id\r\nfrom sys.all_constraints t, sys.all_constraints t2, sys.all_cons_columns cc, <SCHEMA>.fm_tables ft, <SCHEMA>.dictionaries d\r\nwhere t.constraint_type='R' and t.owner=upper(:owner) and t.table_name=upper(:tableName) and t.constraint_name=cc.constraint_name and cc.position=1\r\nand t.r_constraint_name=t2.constraint_name and (t2.constraint_type='P' or t2.constraint_type='U') and t2.owner=t.owner\r\nand ft.table_name(+)=t2.table_name and d.table_name(+)=t2.owner||'.'||t2.table_name")]
	public abstract IList<InflexPropertyLink> SelectForTable(string owner, string tableName);
}
