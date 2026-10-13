using InflexCore.DataImplementation;
using InflexCore.DataImplementation.Accessors;
using InflexCore.Native.Action;
using InflexCore.Native.GW;

namespace InflexCore.Action;

public class FastReportFormActionFactory : IActionFactory
{
	private readonly AppManager _AppManager;

	private InflexGwManager _GwManager;

	private IFastReport FastReport => _GwManager.Inflex32.FastReport;

	public FastReportFormActionFactory(AppManager appManager, InflexGwManager gwManager)
	{
		_AppManager = appManager;
		_GwManager = gwManager;
	}

	public ActionCollection GetActions(ActionContext context)
	{
		ActionCollection actionCollection = new ActionCollection();
		if (FastReport != null && context.Type != null)
		{
			InflexFastReportFormAccessor inflexFastReportFormAccessor = _AppManager.Accessor<InflexFastReportFormAccessor>();
			IInflexReport assignedReport = context.Type.AssignedReport;
			if (assignedReport != null)
			{
				foreach (InflexFastReportForm item in inflexFastReportFormAccessor.SelectByReport(assignedReport.ID))
				{
					ShowFastReportFormAction showFastReportFormAction = new ShowFastReportFormAction(FastReport, context);
					showFastReportFormAction.OneItemForm = item.ItemHandling;
					showFastReportFormAction.FormID = decimal.ToInt32(item.ID);
					showFastReportFormAction.SetCaption(item.Description);
					if (!string.IsNullOrEmpty(item.MenuGroup))
					{
						showFastReportFormAction.SetChildGroup(new InflexActionGroup
						{
							Name = item.MenuGroup
						});
					}
					actionCollection.TryAdd(showFastReportFormAction);
				}
			}
		}
		return actionCollection;
	}
}
