using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(Index = 400)]
public class LinksAction : ToolMultiAction
{
	public override string Caption => "Связи";
}
