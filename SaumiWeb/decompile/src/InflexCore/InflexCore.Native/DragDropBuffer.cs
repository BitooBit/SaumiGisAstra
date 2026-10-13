using System.Linq;

namespace InflexCore.Native;

public static class DragDropBuffer
{
	private static object _Object;

	private static bool _Empty = true;

	public static void Set(object dragObject)
	{
		Clear();
		if (dragObject != null)
		{
			_Empty = false;
			_Object = dragObject;
		}
	}

	public static void SetItemDatas(params ItemData[] itemDatas)
	{
		ItemDataCollection itemDataCollection = null;
		if (itemDatas != null)
		{
			itemDataCollection = new ItemDataCollection();
			foreach (ItemData itemData in itemDatas)
			{
				itemDataCollection.Add(itemData);
			}
		}
		Set(itemDataCollection);
	}

	public static bool Get(out object dropObject)
	{
		dropObject = (_Empty ? null : _Object);
		return !_Empty;
	}

	public static bool GetItemDatas(out ItemData[] itemDatas)
	{
		itemDatas = null;
		if (Get(out var dropObject))
		{
			if (dropObject is ItemDataCollection source)
			{
				itemDatas = source.ToArray();
			}
			else if (dropObject is ItemData itemData)
			{
				itemDatas = new ItemData[1] { itemData };
			}
			else
			{
				itemDatas = dropObject as ItemData[];
			}
			return itemDatas != null;
		}
		return false;
	}

	public static void Clear()
	{
		_Empty = true;
		_Object = null;
	}
}
