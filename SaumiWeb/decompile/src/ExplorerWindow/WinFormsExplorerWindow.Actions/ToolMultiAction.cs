namespace WinFormsExplorerWindow.Actions;

public abstract class ToolMultiAction : ExplorerWindowToolAction
{
	protected override bool? DirectIsAllowed()
	{
		return true;
	}

	protected override void ProtectedDirectExecute()
	{
	}
}
