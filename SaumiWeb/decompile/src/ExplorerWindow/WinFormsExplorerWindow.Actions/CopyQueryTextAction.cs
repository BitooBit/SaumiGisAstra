using System.Windows.Forms;
using InflexCore.Action.Attributes;
using InflexData;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 100, Index = 501, GlyphUri = "SQLToClipboard")]
[AutoInstanceAction]
public class CopyQueryTextAction : ExplorerWindowToolAction
{
	public override string Caption => "Скопировать запрос в буфер обмена";

	protected override bool? DirectIsAllowed()
	{
		return base.DirectContext.DataView.DataQuery is ISqlQuery;
	}

	protected override void ProtectedDirectExecute()
	{
		if (base.DirectContext.DataView.DataQuery is ISqlQuery sqlQuery)
		{
			Clipboard.SetText(sqlQuery.QuerySqlText);
		}
	}
}
