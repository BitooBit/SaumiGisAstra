using InflexCore;
using InflexCore.Role;

namespace WinFormsExplorerWindow.Roles;

public class AllUsersRole : NameRole
{
	public override string RoleName => "SM_ALLUSERS";

	public AllUsersRole(AppManager appManager)
		: base(appManager)
	{
	}
}
