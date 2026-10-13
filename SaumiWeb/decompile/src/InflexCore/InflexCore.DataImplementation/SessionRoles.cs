using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.Manual, "sys", "session_roles")]
public class SessionRoles
{
	[MapField("role")]
	[PrimaryKey]
	public string RoleName { get; set; }
}
