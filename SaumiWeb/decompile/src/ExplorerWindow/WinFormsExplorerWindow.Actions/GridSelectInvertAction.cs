using System.Data;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using InflexCore;
using InflexCore.Action.Attributes;
using InflexDataGridComponent;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(EditAction), GroupIndex = 200, Index = 200, GroupOrder = 500, HotKey = "Ctrl+Shift+I", GroupName = "Выделение")]
[DataContextAction]
public class GridSelectInvertAction : ExplorerWindowAsyncToolAction
{
	public override string Caption => "Обратить выделение";

	protected override bool? DirectIsAllowed()
	{
		IInflexID[] objectIDs = base.Context.ActionArgs.ObjectIDs;
		if (objectIDs != null && objectIDs.Length > 0)
		{
			return null;
		}
		return false;
	}

	protected override void ProtectedDirectExecuteAsync()
	{
		ExplorerWindow explorerWindow = base.DirectContext.ExplorerWindow;
		InflexDataGrid dataGrid = explorerWindow.DataGrid;
		BaseView mainView = ((GridControl)dataGrid).MainView;
		GridView gridView = (GridView)(object)((mainView is GridView) ? mainView : null);
		InvertSelection(explorerWindow, gridView);
	}

	private void InvertSelection(ExplorerWindow window, GridView gridView)
	{
		if (gridView == null)
		{
			return;
		}
		try
		{
			((BaseView)gridView).BeginSelection();
			for (int i = 0; i < ((BaseView)gridView).DataRowCount; i++)
			{
				int visibleRowHandle = ((ColumnView)gridView).GetVisibleRowHandle(i);
				((ColumnView)gridView).InvertRowSelection(visibleRowHandle);
			}
		}
		finally
		{
			((BaseView)gridView).EndSelection();
		}
	}

	private bool ContainsDataRowInSelection(int[] selectedHandles, DataRow row)
	{
		foreach (int num in selectedHandles)
		{
			object row2 = ((GridControl)base.DirectContext.ExplorerWindow.DataGrid).MainView.GetRow(num);
			if (row2 != null)
			{
				DataRowView dataRowView = (DataRowView)row2;
				if (dataRowView.Row == row)
				{
					return true;
				}
			}
		}
		return false;
	}
}
