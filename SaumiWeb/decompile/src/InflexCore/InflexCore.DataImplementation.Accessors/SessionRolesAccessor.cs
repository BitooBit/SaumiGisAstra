using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.Accessors;

public abstract class SessionRolesAccessor : AppManager.InflexAccessorBase<SessionRoles, SessionRolesAccessor>
{
	[InflexSqlQuery("select role from <T>")]
	public abstract string[] AllRoles();
}
