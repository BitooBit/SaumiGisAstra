using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(Index = 300)]
[AutoInstanceAction]
public class ViewAction : ToolMultiAction
{
	public override string Caption => "Вид";
}
