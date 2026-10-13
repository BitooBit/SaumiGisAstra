using InflexCore.Action.Attributes;
using InflexCore.Native;
using InflexCore.UI;
using InflexData.RestrictionQuery;
using WinFormsExplorerWindow.Roles;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 400, Index = 100)]
[AutoInstanceAction]
public class ShowAnaliticWindowAction : ExplorerWindowToolAction
{
	public override string Caption => "Аналитика";

	protected override bool? DirectIsAllowed()
	{
		RestrictionDataQuery restrictionDataQuery = base.DirectContext.DataView.DataQuery as RestrictionDataQuery;
		RestrictByQuery restrictByQuery = ((restrictionDataQuery == null) ? null : (restrictionDataQuery.Restriction as RestrictByQuery));
		return restrictionDataQuery != null && restrictByQuery != null && base.DirectContext.AppManager.RoleEnabled<AnaliticsRole>() && (restrictByQuery.IsFullQuery || !string.IsNullOrEmpty(restrictByQuery.RestrictionXMLText));
	}

	protected override void ProtectedDirectExecute()
	{
		RestrictByQuery queryRestriction = ((!(base.DirectContext.DataView.DataQuery is RestrictionDataQuery restrictionDataQuery)) ? null : (restrictionDataQuery.Restriction as RestrictByQuery));
		using AnaliticExplorerWindow analiticExplorerWindow = new AnaliticExplorerWindow(base.DirectContext.AppManager);
		analiticExplorerWindow.Find(queryRestriction);
	}
}
