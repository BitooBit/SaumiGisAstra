using InflexCore.Native.Action;
using InflexCore.Native.GW;

namespace InflexCore.Action;

public class ShowReferenceActionFactory : IActionFactory
{
	private InflexGwManager _GwManager;

	private IDataHandler DataHandler => _GwManager.Inflex32.DataHandler;

	public ShowReferenceActionFactory(InflexGwManager gwManager)
	{
		_GwManager = gwManager;
	}

	public ActionCollection GetActions(ActionContext context)
	{
		ActionCollection actionCollection = new ActionCollection();
		if (DataHandler != null && context.Type != null)
		{
			IInflexReport assignedReport = context.Type.AssignedReport;
			if (assignedReport != null)
			{
				foreach (IInflexColumn explorerColumn in assignedReport.GetExplorerColumns())
				{
					if (explorerColumn.IsOuterReference())
					{
						InflexAction item = new ShowReferenceObjectAction(DataHandler, assignedReport, explorerColumn, context);
						actionCollection.TryAdd(item);
					}
				}
			}
		}
		return actionCollection;
	}
}
