using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(FileAction), GroupIndex = 500, GlyphUri = "Print", LargeGlyphUri = "PrintLarge")]
public class PrintAction : ToolMultiAction
{
	public override string Caption => "Печать";
}
