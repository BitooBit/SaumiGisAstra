using System.Linq;
using InflexCore.Action.Attributes;
using InflexData;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 100, Index = 700)]
public class SummaryVisibleAction : CheckExplorerWindowToolAction
{
	public override string Caption => "Итоговые суммы";

	protected override bool? DirectIsAllowed()
	{
		return base.DirectContext.DataView.DataQuery.Columns.Any((InflexDataColumn column) => column != null && ((IGridColumn)column).TotalSummMode != TotalSummModeEnum.None);
	}

	protected override void ProtectedDirectExecute()
	{
		base.DirectContext.ExplorerWindow.TotalSummaryPanelVisibility = base.CheckToolState.IsChecked;
	}

	protected override void AfterContextUpdate()
	{
		base.AfterContextUpdate();
		base.CheckToolState.IsChecked = base.DirectContext.ExplorerWindow.TotalSummaryPanelVisibility;
	}
}
