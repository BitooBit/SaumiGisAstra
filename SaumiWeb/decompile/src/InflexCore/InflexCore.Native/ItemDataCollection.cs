using System.Collections;
using System.Collections.Generic;

namespace InflexCore.Native;

public class ItemDataCollection : IEnumerable<ItemData>, IEnumerable
{
	private List<ItemData> _Data = new List<ItemData>();

	public int Count => _Data.Count;

	public ItemData GetItem(int index)
	{
		return _Data[index];
	}

	public void Add(ItemData itemData)
	{
		_Data.Add(itemData);
	}

	public ItemData First()
	{
		if (_Data.Count <= 0)
		{
			return null;
		}
		return _Data[0];
	}

	public IEnumerator<ItemData> GetEnumerator()
	{
		return _Data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
