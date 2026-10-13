using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(EditAction), GroupIndex = 200, Index = 100, HotKey = "Ctrl+A")]
public class GridSelectAllAction : ExplorerWindowAsyncToolAction
{
	public override string Caption => "Выделить всё";

	protected override bool? DirectIsAllowed()
	{
		GridView dataGridView = base.DirectContext.ExplorerWindow.DataGridView;
		return dataGridView != null && ((BaseView)dataGridView).DataRowCount > 0;
	}

	protected override void ProtectedDirectExecuteAsync()
	{
		GridView dataGridView = base.DirectContext.ExplorerWindow.DataGridView;
		if (dataGridView != null)
		{
			dataGridView.ExpandAllGroups();
			((ColumnView)dataGridView).SelectAll();
		}
	}
}
