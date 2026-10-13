using System.Collections.Generic;
using System.Linq;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.Accessors;

public abstract class UserPrivilegeObjectAccessor : AppManager.InflexAccessorBase<UserPrivilegeObject, UserPrivilegeObjectAccessor>
{
	private const string SQL_GET_PRIVELEGES_FOR_TABLE = "select * from\r\n          (\r\n            select alls.TABLE_NAME, alls.GRANTEE_NAME, alls.PRIVILEGE, fmt.grantable_by_userman as GRANTABLE_BY_USERMAN from\r\n           ((select atp.table_name TABLE_NAME, atp.grantee GRANTEE_NAME, atp.privilege as PRIVILEGE\r\n           from all_tab_privs atp where atp.table_schema=upper(:schema) union\r\n           select up.table_name, up.grantee_name as GRNAME, up.PRIVILEGE as PRIVILEGE\r\n           from sm.v_user_permissions_for_view up) alls), sm.fm_tables fmt\r\n           where fmt.table_name (+)= alls.TABLE_NAME \r\n           ) \r\n           where TABLE_NAME=upper(:tableName)";

	[InflexSqlQuery("select * from\r\n          (\r\n            select alls.TABLE_NAME, alls.GRANTEE_NAME, alls.PRIVILEGE, fmt.grantable_by_userman as GRANTABLE_BY_USERMAN from\r\n           ((select atp.table_name TABLE_NAME, atp.grantee GRANTEE_NAME, atp.privilege as PRIVILEGE\r\n           from all_tab_privs atp where atp.table_schema=upper(:schema) union\r\n           select up.table_name, up.grantee_name as GRNAME, up.PRIVILEGE as PRIVILEGE\r\n           from sm.v_user_permissions_for_view up) alls), sm.fm_tables fmt\r\n           where fmt.table_name (+)= alls.TABLE_NAME \r\n           ) \r\n           where TABLE_NAME=upper(:tableName)")]
	protected abstract IList<UserPrivilegeObject> FindPrivilegesForTableDirect(string schema, string tableName);

	public IEnumerable<IUserPrivilegeObject> GetPrivilegesForTable(string schema, string tableName)
	{
		return FindPrivilegesForTableDirect(schema, tableName).OfType<IUserPrivilegeObject>();
	}
}
