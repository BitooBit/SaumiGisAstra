using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.InflexSchema, "v_user_permissions_for_view")]
public class UserPrivilegeObject : IUserPrivilegeObject
{
	[MapField("grantee_name")]
	public string GranteeName { get; set; }

	[MapField("table_name")]
	public string TableName { get; set; }

	[MapField("privilege")]
	public string Privelege { get; set; }

	[MapField("grantable_by_userman")]
	[MapValue(true, "Y")]
	[MapValue(false, "N")]
	public bool? GrantableByUserman { get; set; }
}
