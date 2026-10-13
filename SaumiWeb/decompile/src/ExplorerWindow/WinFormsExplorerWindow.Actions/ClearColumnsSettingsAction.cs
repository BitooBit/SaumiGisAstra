using System.Windows.Forms;
using InflexCore.Action.Attributes;
using InflexData;
using InflexDataGridComponent;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 300, Index = 300, GlyphUri = "ClearColumnsSettings")]
[AutoInstanceAction]
public class ClearColumnsSettingsAction : ExplorerWindowToolAction
{
	public override string Caption => "Вид и расположение колонок по умолчанию";

	protected override bool? DirectIsAllowed()
	{
		InflexDataView dataView = base.DirectContext.DataView;
		return dataView.DataQuery is IInflexReportBasedDataQuery && !dataView.InExecute();
	}

	protected override void ProtectedDirectExecute()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		DialogResult val = MessageBox.Show("Вы уверены, что хотите сбросить текущее состояние колонок и вернуться к тому набору, который прописан в менеджере отчетов?", Caption, (MessageBoxButtons)4, (MessageBoxIcon)32);
		if ((int)val == 6 && base.DirectContext.DataView.DataQuery is IInflexReportBasedDataQuery inflexReportBasedDataQuery)
		{
			QueryColumnCollectionProvider queryColumnProvider = base.DirectContext.ExplorerWindow.QueryColumnProvider;
			InflexDataGrid dataGrid = base.DirectContext.ExplorerWindow.DataGrid;
			if (queryColumnProvider != null)
			{
				queryColumnProvider.SaveDefaultQueryColumns(inflexReportBasedDataQuery.Report);
				dataGrid?.ResetAllColumnsContexts();
				queryColumnProvider.ResetQueryColumns(inflexReportBasedDataQuery.Report);
			}
			base.DirectContext.DataView.UpdateDataQuery(executeQuery: true, background: true);
		}
	}
}
