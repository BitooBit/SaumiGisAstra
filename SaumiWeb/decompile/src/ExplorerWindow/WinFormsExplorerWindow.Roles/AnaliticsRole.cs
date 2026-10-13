using InflexCore;
using InflexCore.Role;

namespace WinFormsExplorerWindow.Roles;

public class AnaliticsRole : NameRole
{
	public override string RoleName => base.AppManager.MainSchema.ToUpper() + "_ANALITICS";

	public AnaliticsRole(AppManager appManager)
		: base(appManager)
	{
	}
}
