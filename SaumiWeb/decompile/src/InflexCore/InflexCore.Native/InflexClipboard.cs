using System;
using System.Collections.Generic;
using InflexCore.Native.GW;
using InflexCore.Native.Messages;

namespace InflexCore.Native;

public static class InflexClipboard
{
	private static List<ItemData> _Items = new List<ItemData>();

	public static int ItemsCount => _Items.Count;

	public static event System.Action SynchronizeEvent;

	public static event System.Action Updated;

	public static void Clear()
	{
		_Items.Clear();
		OnUpdated();
	}

	public static void Copy(ItemData itemData)
	{
		List<ItemData> list = new List<ItemData>(_Items.ToArray());
		list.Add(itemData);
		List<ItemData> list2 = list;
		CopyArray(list2.ToArray());
	}

	public static void CopyArray(ItemData[] itemDatas)
	{
		_Items.Clear();
		foreach (ItemData item in itemDatas)
		{
			_Items.Add(item);
		}
		OnUpdated();
	}

	public static ItemData GetItem(int index)
	{
		return _Items[index];
	}

	public static void Synchronize()
	{
		System.Action synchronizeEvent = SynchronizeEvent;
		if (synchronizeEvent != null)
		{
			synchronizeEvent();
		}
		else
		{
			InflexGwManager.TrySendMessage(new SynchronizeClipboardArgument());
		}
	}

	private static void OnUpdated()
	{
		Updated?.Invoke();
	}
}
