using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(Index = 500)]
[AutoInstanceAction]
public class HelpAction : ToolMultiAction
{
	public override string Caption => "Помощь";
}
