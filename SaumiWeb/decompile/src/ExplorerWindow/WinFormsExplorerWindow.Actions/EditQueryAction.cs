using System.Windows.Forms;
using InflexCore.Action.Attributes;
using InflexCore.Native;
using InflexCore.UI;
using InflexData;
using InflexData.RestrictionQuery;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ChangeQueryAction), Index = 200, GlyphUri = "EditQuery")]
[AutoInstanceAction]
public class EditQueryAction : ExplorerWindowToolAction
{
	public override string Caption => "Редактировать запрос";

	protected override bool? DirectIsAllowed()
	{
		InflexDataView dataView = base.DirectContext.DataView;
		RestrictionDataQuery restrictionDataQuery = dataView.DataQuery as RestrictionDataQuery;
		RestrictByQuery restrictByQuery = ((restrictionDataQuery == null) ? null : (restrictionDataQuery.Restriction as RestrictByQuery));
		return restrictionDataQuery != null && restrictByQuery != null && (restrictByQuery.IsFullQuery || !string.IsNullOrEmpty(restrictByQuery.RestrictionXMLText)) && !dataView.InExecute() && ((base.DirectContext.Type.Parent != null) ? base.DirectContext.Type.Parent.UseInSearch : base.DirectContext.Type.UseInSearch);
	}

	protected override void ProtectedDirectExecute()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		if (!(base.DirectContext.DataView.DataQuery is RestrictionDataQuery restrictionDataQuery))
		{
			return;
		}
		using FindWindow findWindow = new FindWindow(base.DirectContext.AppManager);
		RestrictByQuery queryRestriction = restrictionDataQuery.Restriction as RestrictByQuery;
		DialogResult val = findWindow.FindAll(queryRestriction);
		if ((int)val == 1)
		{
			base.DirectContext.ExplorerWindow.UpdateDataQuery(findWindow.QueryRestriction);
		}
	}
}
