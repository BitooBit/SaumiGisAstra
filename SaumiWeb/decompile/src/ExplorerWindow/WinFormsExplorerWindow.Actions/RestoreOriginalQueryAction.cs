using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ChangeQueryAction), Index = 100)]
[AutoInstanceAction]
public class RestoreOriginalQueryAction : ExplorerWindowToolAction
{
	public override string Caption => "Восстановить исходный запрос";

	protected override bool? DirectIsAllowed()
	{
		return base.DirectContext.ExplorerWindow.IsDataQueryChanged;
	}

	protected override void ProtectedDirectExecute()
	{
		base.DirectContext.ExplorerWindow.RestoreOriginalDataQuery();
	}
}
