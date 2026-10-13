using System.Collections.Generic;
using InflexCore.Native;
using InflexCore.Native.Messages;

namespace ExplorerWindowBase.Messages;

public class SynchronizeFromClipboardMessageArgument : MessageArgument
{
	public List<ItemData> Items { get; set; }

	public int ItemCount { get; set; }

	static SynchronizeFromClipboardMessageArgument()
	{
		MessageHelper.RegisterMessageArgumentType("F93BE70F-B766-4A86-B104-0827865EA4E9", typeof(SynchronizeFromClipboardMessageArgument));
	}

	public SynchronizeFromClipboardMessageArgument()
	{
		Items = new List<ItemData>();
	}

	public void Add(ItemData item)
	{
		Items.Add(item);
		ItemCount = Items.Count;
	}

	public ItemData GetItem(int index)
	{
		return Items[index];
	}
}
