using InflexCore.Native;
using InflexCore.Native.Action;

namespace InflexCore.Action;

public class ShowInflex32ObjectAction : ShowInflexObjectAction, IDataHandlerAction
{
	private DataHandlerActionHelper _HandlerHelper;

	public ShowInflex32ObjectAction(IDataHandler dataHandler, ActionContext context)
	{
		_HandlerHelper = new DataHandlerActionHelper(this, dataHandler, context);
	}

	bool IDataHandlerAction.InnerIsAllowedContext()
	{
		return _HandlerHelper.DataHandler.IsShowHandler(base.Context.Type);
	}

	protected override bool? DirectIsAllowed()
	{
		IInflexID[] objectIDs = base.Context.ActionArgs.ObjectIDs;
		return objectIDs != null && objectIDs.Length == 1 && _HandlerHelper.DataHandler.IsShowItem(ItemData.Parse(objectIDs[0]));
	}

	protected override bool DirectIsAllowedContext()
	{
		return _HandlerHelper.DirectIsAllowedContext();
	}

	protected override void DirectExecute()
	{
		_HandlerHelper.DataHandler.ShowItem(ItemData.Parse(base.Context.ActionArgs.ObjectIDs[0]));
	}
}
