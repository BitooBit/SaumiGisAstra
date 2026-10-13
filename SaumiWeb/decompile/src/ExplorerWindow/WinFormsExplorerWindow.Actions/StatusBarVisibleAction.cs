using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 100, Index = 400)]
[AutoInstanceAction]
public class StatusBarVisibleAction : CheckExplorerWindowToolAction
{
	public override string Caption => "Строка состояния";

	protected override bool? DirectIsAllowed()
	{
		return true;
	}

	protected override void ProtectedDirectExecute()
	{
		base.DirectContext.ExplorerWindow.StatusBarVisible = base.CheckToolState.IsChecked;
	}

	protected override void AfterContextUpdate()
	{
		base.AfterContextUpdate();
	}
}
