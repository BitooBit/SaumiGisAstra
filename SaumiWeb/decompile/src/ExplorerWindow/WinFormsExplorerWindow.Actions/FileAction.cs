using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(Index = 100)]
public class FileAction : ToolMultiAction
{
	public override string Caption => "Файл";
}
