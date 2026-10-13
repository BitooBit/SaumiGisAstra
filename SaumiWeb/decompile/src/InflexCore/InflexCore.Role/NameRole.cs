namespace InflexCore.Role;

public abstract class NameRole
{
	protected AppManager AppManager { get; set; }

	public abstract string RoleName { get; }

	protected NameRole(AppManager appManager)
	{
		AppManager = appManager;
	}
}
