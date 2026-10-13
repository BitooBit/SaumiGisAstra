using System.Windows.Forms;
using InflexCore.Action.Attributes;
using InflexData;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 100, Index = 500, GlyphUri = "ViewSQL")]
public class ShowQueryTextAction : ExplorerWindowToolAction
{
	public override string Caption => "Показать текст запроса...";

	protected override bool? DirectIsAllowed()
	{
		return base.DirectContext.DataView.DataQuery is ISqlQuery;
	}

	protected override void ProtectedDirectExecute()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (base.DirectContext.DataView.DataQuery is ISqlQuery sqlQuery)
		{
			ShowQueryTextForm showQueryTextForm = new ShowQueryTextForm(sqlQuery.QuerySqlText);
			((Form)showQueryTextForm).ShowDialog();
		}
	}
}
