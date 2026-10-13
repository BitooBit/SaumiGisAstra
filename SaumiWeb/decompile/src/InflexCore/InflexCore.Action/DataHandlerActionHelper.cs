using InflexCore.Native.Action;

namespace InflexCore.Action;

public class DataHandlerActionHelper
{
	private readonly IDataHandlerAction _Action;

	public IDataHandler DataHandler { get; set; }

	public DataHandlerActionHelper(IDataHandlerAction action, IDataHandler dataHandler, ActionContext context)
	{
		_Action = action;
		DataHandler = dataHandler;
		action.Initialize(context);
	}

	public bool DirectIsAllowedContext()
	{
		if (_Action.Context.Type == null || DataHandler == null)
		{
			return false;
		}
		return _Action.InnerIsAllowedContext();
	}
}
