using System.Windows.Forms;
using InflexCore;
using InflexCore.Action.Attributes;
using InflexData;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 200, Index = 300, GlyphUri = "SaveColumnsSettings")]
[AutoInstanceAction]
public class SaveColumnsSettingsAction : ExplorerWindowToolAction
{
	public override string Caption => "Запомнить вид и расположение колонок";

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
		DialogResult val = MessageBox.Show("Вы уверены, что хотите сохранить текущий набор колонок и их состояние?", Caption, (MessageBoxButtons)4, (MessageBoxIcon)32);
		if ((int)val != 6)
		{
			return;
		}
		InflexDataView dataView = base.DirectContext.DataView;
		if (dataView != null && dataView.DataQuery is IInflexReportBasedDataQuery inflexReportBasedDataQuery)
		{
			QueryColumnCollectionProvider queryColumnProvider = base.DirectContext.ExplorerWindow.QueryColumnProvider;
			if (queryColumnProvider != null)
			{
				QueryColumnCollection queryColumns = inflexReportBasedDataQuery.QueryColumns;
				queryColumnProvider.SaveQueryColumns(inflexReportBasedDataQuery.Report, queryColumns);
			}
		}
	}
}
