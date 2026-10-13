using InflexCore;

namespace WinFormsExplorerWindow.Config;

public class GisIntoExplorerWindowConfig
{
	private readonly AppManager _AppManager;

	private int _WorkspaceID;

	private bool _IsReadWorkspaceID;

	private string _WorkspaceIdKey = "MAP_DATA_WINDOW_WORKSPACE";

	public int WorkspaceID
	{
		get
		{
			if (!_IsReadWorkspaceID)
			{
				if (!int.TryParse(_AppManager.UserSettings[_WorkspaceIdKey], out var result) && !int.TryParse(_AppManager.AppSettings[_WorkspaceIdKey], out result))
				{
					result = -1;
				}
				_IsReadWorkspaceID = true;
				_WorkspaceID = result;
			}
			return _WorkspaceID;
		}
		set
		{
			if (WorkspaceID != value)
			{
				_WorkspaceID = value;
				_AppManager.UserSettings[_WorkspaceIdKey] = value.ToString();
			}
		}
	}

	public GisIntoExplorerWindowConfig(AppManager appManager)
	{
		_AppManager = appManager;
	}
}
