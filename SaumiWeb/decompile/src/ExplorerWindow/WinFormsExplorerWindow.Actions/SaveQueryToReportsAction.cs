using ExplorerWindowBase.Action;
using InflexCore;
using InflexCore.Action.Attributes;
using InflexCore.Native;
using InflexData.RestrictionQuery;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(FileAction), GroupIndex = 100, Index = 200, GlyphUri = "SaveToReports")]
[AutoInstanceAction]
public class SaveQueryToReportsAction : ExplorerWindowToolAction, IAbstractSaveAction
{
	private class ReportTreeSaveAccesor : AppManager.NativeInflexAccessor
	{
		public ReportTreeSaveAccesor(AppManager appManager)
			: base(appManager)
		{
		}

		public bool IsAllowed()
		{
			return Inflex32.AllowReportTreeSave;
		}

		public void ReportTreeSave(int reportID, string reportName, string restrictionSQL)
		{
			Inflex32.ReportTreeSave(reportID, reportName, restrictionSQL);
		}
	}

	public override string Caption => "Сохранить запрос в меню \"Отчеты\"...";

	protected override bool? DirectIsAllowed()
	{
		return base.DirectContext.DataView.DataQuery is RestrictionDataQuery restrictionDataQuery && restrictionDataQuery.Restriction is RestrictByQuery && !(restrictionDataQuery.Restriction as RestrictByQuery).IsFullQuery && new ReportTreeSaveAccesor(base.DirectContext.AppManager).IsAllowed();
	}

	protected override void ProtectedDirectExecute()
	{
		RestrictionDataQuery restrictionDataQuery = (RestrictionDataQuery)base.DirectContext.DataView.DataQuery;
		int reportID = decimal.ToInt32(restrictionDataQuery.Report.ID);
		string name = restrictionDataQuery.Report.Name;
		string restrictionQueryText = ((RestrictByQuery)restrictionDataQuery.Restriction).RestrictionQueryText;
		new ReportTreeSaveAccesor(base.DirectContext.AppManager).ReportTreeSave(reportID, name, restrictionQueryText);
	}
}
