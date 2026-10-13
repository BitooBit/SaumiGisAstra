using System.Windows.Forms;
using InflexCore.Action.Attributes;
using InflexCore.Native;
using InflexCore.UI;
using InflexData;
using InflexData.RestrictionQuery;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ChangeQueryAction), Index = 100, GlyphUri = "AddQuery")]
[AutoInstanceAction]
public class AddQueryAction : ExplorerWindowToolAction
{
	public override string Caption => "Добавить данные в запрос";

	protected override bool? DirectIsAllowed()
	{
		InflexDataView dataView = base.DirectContext.DataView;
		RestrictByQuery restrictByQuery = ((!(dataView.DataQuery is RestrictionDataQuery restrictionDataQuery)) ? null : (restrictionDataQuery.Restriction as RestrictByQuery));
		return restrictByQuery != null && !restrictByQuery.IsFullQuery && !dataView.InExecute() && ((base.DirectContext.Type.Parent != null) ? base.DirectContext.Type.Parent.UseInSearch : base.DirectContext.Type.UseInSearch);
	}

	protected override void ProtectedDirectExecute()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		using FindWindow findWindow = new FindWindow(base.DirectContext.AppManager);
		IInflexReportBasedDataQuery inflexReportBasedDataQuery = (IInflexReportBasedDataQuery)base.DirectContext.DataView.DataQuery;
		DialogResult val = findWindow.FindAll(inflexReportBasedDataQuery.Report);
		if ((int)val == 1 && findWindow.QueryRestriction is RestrictByQuery restrictByQuery)
		{
			RestrictionDataQuery restrictionDataQuery = (RestrictionDataQuery)base.DirectContext.DataView.DataQuery;
			RestrictByQuery restrictByQuery2 = (RestrictByQuery)restrictionDataQuery.Restriction;
			RestrictByQuery restrictByQuery3 = restrictByQuery2.AddRestrictionQuery(restrictByQuery.Report, restrictByQuery.RestrictionQuery, restrictByQuery.RestrictionXML);
			if (restrictByQuery3 != null)
			{
				base.DirectContext.ExplorerWindow.UpdateDataQuery(restrictByQuery3);
			}
		}
	}
}
