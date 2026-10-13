using InflexCore;
using InflexCore.Action;
using InflexData;

namespace WinFormsExplorerWindow.Actions;

public class ExplorerWindowContext : ActionContext
{
	public ExplorerWindow ExplorerWindow { get; private set; }

	public InflexDataView DataView
	{
		get
		{
			if (ExplorerWindow != null)
			{
				return ExplorerWindow.DataView;
			}
			return null;
		}
	}

	public AppManager AppManager { get; private set; }

	public ExplorerWindowContext(ExplorerWindow explorerWindow, AppManager appManager)
	{
		ExplorerWindow = explorerWindow;
		AppManager = appManager;
	}

	protected override ActionContext Instance()
	{
		return new ExplorerWindowContext(ExplorerWindow, AppManager);
	}

	protected override void AfterHandleUpdate()
	{
		ExplorerWindow.UpdateAllowedStateTools();
		base.AfterHandleUpdate();
	}

	protected override void DirectDispose()
	{
		AppManager = null;
		ExplorerWindow = null;
		base.DirectDispose();
	}
}
