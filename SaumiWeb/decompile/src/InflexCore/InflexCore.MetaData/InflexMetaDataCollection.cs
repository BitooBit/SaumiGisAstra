using System;
using System.Collections;
using System.Collections.Generic;

namespace InflexCore.MetaData;

public class InflexMetaDataCollection : ICollection<IInflexMetaData>, IEnumerable<IInflexMetaData>, IEnumerable
{
	private List<IInflexMetaData> _Data = new List<IInflexMetaData>();

	public int Count => _Data.Count;

	public bool IsReadOnly => false;

	public IEnumerator<IInflexMetaData> GetEnumerator()
	{
		return _Data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(IInflexMetaData item)
	{
		_Data.Add(item);
	}

	public void Clear()
	{
		_Data.Clear();
	}

	public bool Contains(IInflexMetaData item)
	{
		return _Data.Contains(item);
	}

	public void CopyTo(IInflexMetaData[] array, int arrayIndex)
	{
		_Data.CopyTo(array, arrayIndex);
	}

	public bool Remove(IInflexMetaData item)
	{
		return _Data.Remove(item);
	}

	public void Set(IInflexMetaData item)
	{
		Type type = item.GetType();
		for (int num = _Data.Count - 1; num >= 0; num--)
		{
			if ((object)_Data[num].GetType() == type)
			{
				Remove(_Data[num]);
			}
		}
		Add(item);
	}

	public T Get<T>() where T : IInflexMetaData
	{
		return Get<T>(defaultAvailable: false);
	}

	public T Get<T>(bool defaultAvailable) where T : IInflexMetaData
	{
		Type typeFromHandle = typeof(T);
		foreach (IInflexMetaData datum in _Data)
		{
			if ((object)datum.GetType() == typeFromHandle)
			{
				return (T)datum;
			}
		}
		if (defaultAvailable)
		{
			return default(T);
		}
		throw new InflexMetaDataNotFoundException();
	}
}
