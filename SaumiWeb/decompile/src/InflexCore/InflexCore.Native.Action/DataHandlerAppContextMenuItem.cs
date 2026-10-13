using InflexCore.Action;
using InflexCore.Native.Messages;

namespace InflexCore.Native.Action;

public class DataHandlerAppContextMenuItem : IDataHandlerContextMenuItem
{
	private readonly IDataHandler _DataHandler;

	public IInflexType Type { get; set; }

	public HandlerContextMenuResultArgument.HandlerContextMenuItem MenuItem { get; set; }

	public DataHandlerAppContextMenuItem(IDataHandler dataHandler, IInflexType type, HandlerContextMenuResultArgument.HandlerContextMenuItem menuItem)
	{
		Type = type;
		MenuItem = menuItem;
		_DataHandler = dataHandler;
	}

	public InflexAction CreateAction(ActionContext context)
	{
		HandlerContextMenuAction handlerContextMenuAction = new HandlerContextMenuAction(_DataHandler, context, this);
		handlerContextMenuAction.MenuIndex = MenuItem.MenuIndex;
		handlerContextMenuAction.ParentMenuIndex = MenuItem.ParentMenuIndex;
		handlerContextMenuAction.Level = MenuItem.Level;
		HandlerContextMenuAction handlerContextMenuAction2 = handlerContextMenuAction;
		string caption = ((MenuItem.Caption == null) ? null : MenuItem.Caption.Replace("&", ""));
		handlerContextMenuAction2.SetCaption(caption);
		return handlerContextMenuAction2;
	}
}
