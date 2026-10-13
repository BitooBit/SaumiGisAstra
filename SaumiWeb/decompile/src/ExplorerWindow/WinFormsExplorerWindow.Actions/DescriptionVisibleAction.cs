using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 100, Index = 600)]
[AutoInstanceAction]
public class DescriptionVisibleAction : CheckExplorerWindowToolAction
{
	public override string Caption => "Комментарий запроса";

	protected override bool? DirectIsAllowed()
	{
		return !string.IsNullOrEmpty(base.DirectContext.ExplorerWindow.DescriptionText);
	}

	protected override void ProtectedDirectExecute()
	{
	}

	protected override void AfterContextUpdate()
	{
		base.AfterContextUpdate();
	}
}
