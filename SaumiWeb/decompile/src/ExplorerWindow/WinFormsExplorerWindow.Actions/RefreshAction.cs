using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 200, Index = 100, GlyphUri = "Refresh", LargeGlyphUri = "RefreshLarge", HotKey = "F5")]
public class RefreshAction : ExplorerWindowToolAction
{
	public override bool IsHiddenWhenDisabled => true;

	public override string Caption => "Обновить";

	protected override bool? DirectIsAllowed()
	{
		return base.DirectContext.DataView != null && !base.DirectContext.DataView.InExecute();
	}

	protected override void ProtectedDirectExecute()
	{
		base.DirectContext.DataView.RefreshQuery(background: true);
	}
}
