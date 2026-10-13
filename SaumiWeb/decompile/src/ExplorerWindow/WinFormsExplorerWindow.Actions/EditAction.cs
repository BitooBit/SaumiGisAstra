using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(Index = 200)]
public class EditAction : ToolMultiAction
{
	public override string Caption => "Правка";
}
