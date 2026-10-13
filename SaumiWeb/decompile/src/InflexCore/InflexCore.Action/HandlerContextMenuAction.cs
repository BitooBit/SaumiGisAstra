using InflexCore.Native;
using InflexCore.Native.Action;

namespace InflexCore.Action;

public class HandlerContextMenuAction : InflexAction, IDataHandlerAction
{
	private readonly IDataHandlerContextMenuItem _MenuItem;

	private string _Caption;

	private DataHandlerActionHelper _HandlerHelper;

	public override string Caption => _Caption;

	public int MenuIndex { get; set; }

	public int ParentMenuIndex { get; set; }

	public int Level { get; set; }

	public HandlerContextMenuAction(IDataHandler dataHandler, ActionContext context, IDataHandlerContextMenuItem menuItem)
	{
		_HandlerHelper = new DataHandlerActionHelper(this, dataHandler, context);
		_MenuItem = menuItem;
	}

	public void SetCaption(string value)
	{
		_Caption = value;
	}

	protected override bool? DirectIsAllowed()
	{
		return base.Context.ActionArgs.ObjectIDs != null && base.Context.ActionArgs.ObjectIDs.Length > 0;
	}

	protected override bool DirectIsAllowedContext()
	{
		return _HandlerHelper.DirectIsAllowedContext();
	}

	protected override void DirectExecute()
	{
		ItemDataCollection itemDataCollection = new ItemDataCollection();
		IInflexID[] objectIDs = base.Context.ActionArgs.ObjectIDs;
		foreach (IInflexID inflexID in objectIDs)
		{
			itemDataCollection.Add(ItemData.Parse(inflexID));
		}
		_HandlerHelper.DataHandler.ExecuteContextMenuItem(_MenuItem, itemDataCollection);
	}

	public bool InnerIsAllowedContext()
	{
		return true;
	}
}
