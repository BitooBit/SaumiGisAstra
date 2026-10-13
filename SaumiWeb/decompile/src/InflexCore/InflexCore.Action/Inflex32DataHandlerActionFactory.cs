using InflexCore.Native.Action;
using InflexCore.Native.GW;

namespace InflexCore.Action;

public class Inflex32DataHandlerActionFactory : IActionFactory
{
	private readonly InflexGwManager _GwManager;

	private IDataHandler DataHandler => _GwManager.Inflex32.DataHandler;

	public Inflex32DataHandlerActionFactory(InflexGwManager gwManager)
	{
		_GwManager = gwManager;
	}

	public ActionCollection GetActions(ActionContext context)
	{
		ActionCollection actionCollection = new ActionCollection();
		if (DataHandler != null)
		{
			actionCollection.TryAdd(new NewInflex32ObjectAction(DataHandler, context));
			actionCollection.TryAdd(new ShowInflex32ObjectAction(DataHandler, context));
			actionCollection.TryAdd(new DeleteInflexObjectAction(DataHandler, context));
			if (context.Type != null && context.Type.AssignedReport != null)
			{
				foreach (OuterReference outerReferenceReport in context.Type.AssignedReport.GetOuterReferenceReports())
				{
					OuterReferenceAction outerReferenceAction = new OuterReferenceAction(DataHandler, context);
					outerReferenceAction.DataType = context.Type;
					outerReferenceAction.Report = outerReferenceReport.Report;
					outerReferenceAction.Column = outerReferenceReport.Column;
					OuterReferenceAction item = outerReferenceAction;
					actionCollection.TryAdd(item);
				}
				foreach (IDataHandlerContextMenuItem item2 in DataHandler.GetContextMenu(context.Type))
				{
					actionCollection.TryAdd(item2.CreateAction(context));
				}
			}
		}
		return actionCollection;
	}
}
