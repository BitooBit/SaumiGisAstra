using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(FileAction), GroupIndex = 100, Index = 100, GlyphUri = "EditQuery", LargeGlyphUri = "EditQueryLarge")]
[AutoInstanceAction]
public class ChangeQueryAction : ToolMultiAction
{
	public override string Caption => "Изменить запрос";
}
