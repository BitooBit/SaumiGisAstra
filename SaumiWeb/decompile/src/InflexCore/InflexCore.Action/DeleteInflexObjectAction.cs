using System.Collections.Generic;
using InflexCore.Native;
using InflexCore.Native.Action;

namespace InflexCore.Action;

public class DeleteInflexObjectAction : InflexAction, IDataHandlerAction
{
	private DataHandlerActionHelper _HandlerHelper;

	public override string Caption => "Удалить";

	public DeleteInflexObjectAction(IDataHandler dataHandler, ActionContext context)
	{
		_HandlerHelper = new DataHandlerActionHelper(this, dataHandler, context);
	}

	bool IDataHandlerAction.InnerIsAllowedContext()
	{
		return _HandlerHelper.DataHandler.IsDeleteHandler(base.Context.Type);
	}

	protected override bool? DirectIsAllowed()
	{
		IInflexID[] objectIDs = base.Context.ActionArgs.ObjectIDs;
		return objectIDs != null && objectIDs.Length > 0;
	}

	protected override bool DirectIsAllowedContext()
	{
		return _HandlerHelper.DirectIsAllowedContext();
	}

	protected override void DirectExecute()
	{
		List<IInflexID> list = new List<IInflexID>(base.Context.ActionArgs.ObjectIDs);
		foreach (IInflexID item2 in list)
		{
			ItemData item = ItemData.Parse(item2);
			_HandlerHelper.DataHandler.DeleteItem(item);
		}
	}
}
