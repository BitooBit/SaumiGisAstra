using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 300, Index = 100, GlyphUri = "Stop", LargeGlyphUri = "StopLarge", HotKey = "Escape")]
public class StopAction : ExplorerWindowToolAction
{
	public override bool IsHiddenWhenDisabled => true;

	public override string Caption => "Остановить";

	protected override bool? DirectIsAllowed()
	{
		return base.DirectContext.DataView?.InExecute() ?? false;
	}

	protected override void ProtectedDirectExecute()
	{
		base.DirectContext.DataView.Stop(wait: false);
	}
}
