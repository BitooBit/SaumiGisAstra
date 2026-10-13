namespace InflexCore.UI;

public class AnaliticExplorerWindow : ExplorerWindow
{
	public AnaliticExplorerWindow(AppManager appManager)
		: base(appManager)
	{
		base.UseAnalitic = true;
	}
}
