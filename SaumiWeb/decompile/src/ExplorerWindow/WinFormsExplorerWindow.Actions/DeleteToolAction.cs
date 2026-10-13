using System.Windows.Forms;
using InflexCore.Action;
using InflexDataGridComponent;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(EditAction), GroupName = "Remove", GroupIndex = 100, Index = 500, GlyphUri = "Delete", LargeGlyphUri = "DeleteLarge", HotKey = "Shift+Del", Position = ActionPosition.Down)]
public class DeleteToolAction : ExplorerWindowToolAction
{
	private DeleteInflexObjectAction _Action;

	public override string Caption => "Удалить из базы";

	public DeleteToolAction(DeleteInflexObjectAction action)
	{
		_Action = action;
		Initialize(action.Context);
	}

	protected override bool? DirectIsAllowed()
	{
		return _Action.IsAllowed;
	}

	protected override void ProtectedDirectExecute()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		DialogResult val = MessageBox.Show("Выбранные объекты навсегда будут удалены из базы данных. Удалить?", "Внимание!!!", (MessageBoxButtons)4, (MessageBoxIcon)48, (MessageBoxDefaultButton)256);
		if ((int)val == 6)
		{
			_Action.Execute();
			base.DirectContext.DataView.RefreshQuery(background: true);
		}
		ExplorerWindow explorerWindow = base.DirectContext.ExplorerWindow;
		if (explorerWindow != null)
		{
			((Form)explorerWindow).Activate();
			InflexDataGrid dataGrid = explorerWindow.DataGrid;
			if (dataGrid != null)
			{
				((Control)dataGrid).Focus();
			}
		}
	}
}
