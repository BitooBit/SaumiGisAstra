using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(FileAction), GroupIndex = 100, Index = 400)]
public class SaveQueryResultAction : ToolMultiAction
{
	public override string Caption => "Сохранить результат";
}
