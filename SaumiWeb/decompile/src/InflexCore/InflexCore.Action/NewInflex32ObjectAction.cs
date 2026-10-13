using InflexCore.Native.Action;

namespace InflexCore.Action;

public class NewInflex32ObjectAction : NewInflexObjectAction, IDataHandlerAction
{
	private DataHandlerActionHelper _HandlerHelper;

	public NewInflex32ObjectAction(IDataHandler dataHandler, ActionContext context)
	{
		_HandlerHelper = new DataHandlerActionHelper(this, dataHandler, context);
	}

	bool IDataHandlerAction.InnerIsAllowedContext()
	{
		return _HandlerHelper.DataHandler.IsNewHandler(base.Context.Type);
	}

	protected override bool? DirectIsAllowed()
	{
		return true;
	}

	protected override bool DirectIsAllowedContext()
	{
		return _HandlerHelper.DirectIsAllowedContext();
	}

	protected override void DirectExecute()
	{
		base.NewItemData = _HandlerHelper.DataHandler.NewItem(base.Context.Type);
	}
}
