namespace WinFormsExplorerWindow.Actions;

public abstract class CheckExplorerWindowToolAction : ExplorerWindowToolAction, ICheckTool
{
	private CheckToolState _CheckToolState;

	public CheckToolState CheckToolState
	{
		get
		{
			if (_CheckToolState == null)
			{
				_CheckToolState = new CheckToolState();
			}
			return _CheckToolState;
		}
	}
}
