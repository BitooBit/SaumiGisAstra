using System.Collections;
using System.Collections.Generic;
using InflexCore.MetaData;

namespace InflexCore.DataImplementation.MetaDataComponent;

public class SimpleInflexPropertyCollection(IInflexType ownerType) : InflexPropertyCollection(ownerType), ICollection<IInflexProperty>, IEnumerable<IInflexProperty>, IEnumerable
{
	private HashSet<IInflexProperty> _Data = new HashSet<IInflexProperty>();

	public int Count => _Data.Count;

	public bool IsReadOnly => false;

	public override IEnumerator<IInflexProperty> GetEnumerator()
	{
		return _Data.GetEnumerator();
	}

	public void Add(IInflexProperty item)
	{
		_Data.Add(item);
	}

	public void Clear()
	{
		_Data.Clear();
	}

	public bool Contains(IInflexProperty item)
	{
		return _Data.Contains(item);
	}

	public void CopyTo(IInflexProperty[] array, int arrayIndex)
	{
		_Data.CopyTo(array, arrayIndex);
	}

	public bool Remove(IInflexProperty item)
	{
		return _Data.Remove(item);
	}
}
