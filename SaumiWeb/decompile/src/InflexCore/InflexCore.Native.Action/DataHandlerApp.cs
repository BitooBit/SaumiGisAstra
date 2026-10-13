using System;
using System.Collections.Generic;
using System.Linq;
using InflexCore.Native.GW;
using InflexCore.Native.Messages;
using InflexCore.log;

namespace InflexCore.Native.Action;

public class DataHandlerApp : IDataHandler
{
	private string _DeleteItemHandlerName = "DeleteItem";

	private string _ShowItemHandlerName = "ShowItem";

	private string _NewItemHandlerName = "NewItem";

	public bool IsNewHandler(IInflexType type)
	{
		return IsHandle((type != null && type.TypeID.HasValue) ? decimal.ToInt32(type.TypeID.Value) : 0, _NewItemHandlerName);
	}

	public ItemData NewItem(IInflexType type)
	{
		NewItemArgument newItemArgument = new NewItemArgument();
		newItemArgument.DataType = ((type != null && type.TypeID.HasValue) ? decimal.ToInt32(type.TypeID.Value) : 0);
		newItemArgument.DataSubType = ((type != null && type.SubtypeID.HasValue) ? decimal.ToInt32(type.SubtypeID.Value) : 0);
		NewItemArgument sendArgument = newItemArgument;
		MessageArgument resultArgument = null;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, sendArgument, out resultArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, newItemArgument, out result);", ex);
		}
		if (resultArgument is NewItemResultArgument newItemResultArgument)
		{
			return newItemResultArgument.ItemData;
		}
		return null;
	}

	public bool IsShowItem(ItemData item)
	{
		return IsHandle(item.ObjType, _ShowItemHandlerName);
	}

	public bool IsShowHandler(IInflexType type)
	{
		return IsHandle((type != null && type.TypeID.HasValue) ? decimal.ToInt32(type.TypeID.Value) : 0, _ShowItemHandlerName);
	}

	private bool IsHandle(int dataType, string handleName)
	{
		IsHandleArgument isHandleArgument = new IsHandleArgument();
		isHandleArgument.DataType = dataType;
		isHandleArgument.HandleName = handleName;
		IsHandleArgument sendArgument = isHandleArgument;
		MessageArgument resultArgument = null;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, sendArgument, out resultArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, isHandleArgument, out result);", ex);
		}
		if (resultArgument is IsHandleResultArgument isHandleResultArgument)
		{
			return isHandleResultArgument.IsHandle;
		}
		return false;
	}

	public void ShowItem(ItemData item)
	{
		ShowItemArgument showItemArgument = new ShowItemArgument();
		showItemArgument.ItemData = item;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, showItemArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, showItemArgument);", ex);
		}
	}

	public bool IsDeleteItem(ItemData item)
	{
		return IsHandle(item.ObjType, _DeleteItemHandlerName);
	}

	public bool IsDeleteHandler(IInflexType type)
	{
		return IsHandle((type != null && type.TypeID.HasValue) ? decimal.ToInt32(type.TypeID.Value) : 0, _DeleteItemHandlerName);
	}

	public bool DeleteItem(ItemData item)
	{
		DeleteItemArgument deleteItemArgument = new DeleteItemArgument();
		deleteItemArgument.ItemData = item;
		MessageArgument resultArgument = null;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, deleteItemArgument, out resultArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, deleteItemArgument, out result);", ex);
		}
		if (resultArgument is DeleteItemResultArgument deleteItemResultArgument)
		{
			return deleteItemResultArgument.DeleteResult;
		}
		return false;
	}

	public void ShowOuterReference(IInflexType type, IInflexReport report, IInflexColumn column, ItemData[] items)
	{
		ShowOuterReferenceArgument showOuterReferenceArgument = new ShowOuterReferenceArgument();
		showOuterReferenceArgument.DataType = ((type != null && type.TypeID.HasValue) ? decimal.ToInt32(type.TypeID.Value) : 0);
		showOuterReferenceArgument.ReportID = decimal.ToInt32(report.ID);
		showOuterReferenceArgument.ColumnID = decimal.ToInt32(column.ID);
		showOuterReferenceArgument.Caption = $"{report.Name} ({column.ColumnName})";
		showOuterReferenceArgument.Items = items.Select((ItemData item) => item.ObjID).ToArray();
		ShowOuterReferenceArgument sendArgument = showOuterReferenceArgument;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, sendArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, contextMenuArgument);", ex);
		}
	}

	public IEnumerable<IDataHandlerContextMenuItem> GetContextMenu(IInflexType type)
	{
		List<IDataHandlerContextMenuItem> list = new List<IDataHandlerContextMenuItem>();
		int dataType = ((type != null && type.TypeID.HasValue) ? decimal.ToInt32(type.TypeID.Value) : 0);
		HandlerContextMenuArgument handlerContextMenuArgument = new HandlerContextMenuArgument();
		handlerContextMenuArgument.DataType = dataType;
		MessageArgument resultArgument = null;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, handlerContextMenuArgument, out resultArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, contextMenuArgument, out result);", ex);
		}
		HandlerContextMenuResultArgument handlerContextMenuResultArgument = (HandlerContextMenuResultArgument)resultArgument;
		if (handlerContextMenuResultArgument != null)
		{
			foreach (HandlerContextMenuResultArgument.HandlerContextMenuItem item in handlerContextMenuResultArgument.Menu)
			{
				list.Add(new DataHandlerAppContextMenuItem(this, type, item));
			}
		}
		return list;
	}

	public void ExecuteContextMenuItem(IDataHandlerContextMenuItem menuItem, ItemDataCollection collection)
	{
		if (!(menuItem is DataHandlerAppContextMenuItem dataHandlerAppContextMenuItem))
		{
			return;
		}
		HandlerContextMenuExecuteArgument handlerContextMenuExecuteArgument = new HandlerContextMenuExecuteArgument();
		handlerContextMenuExecuteArgument.DataType = ((dataHandlerAppContextMenuItem.Type != null && dataHandlerAppContextMenuItem.Type.TypeID.HasValue) ? decimal.ToInt32(dataHandlerAppContextMenuItem.Type.TypeID.Value) : 0);
		handlerContextMenuExecuteArgument.ItemDatas = collection;
		handlerContextMenuExecuteArgument.MenuIndex = dataHandlerAppContextMenuItem.MenuItem.MenuIndex;
		handlerContextMenuExecuteArgument.ParentMenuIndex = dataHandlerAppContextMenuItem.MenuItem.ParentMenuIndex;
		handlerContextMenuExecuteArgument.MenuTag = dataHandlerAppContextMenuItem.MenuItem.MenuTag;
		handlerContextMenuExecuteArgument.Level = dataHandlerAppContextMenuItem.MenuItem.Level;
		HandlerContextMenuExecuteArgument sendArgument = handlerContextMenuExecuteArgument;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, sendArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, contextMenuArgument);", ex);
		}
	}
}
