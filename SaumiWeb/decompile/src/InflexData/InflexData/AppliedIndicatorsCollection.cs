using System.Collections;
using System.Collections.Generic;

namespace InflexData;

public class AppliedIndicatorsCollection : IEnumerable<int>, IEnumerable
{
	private readonly HashSet<int> _Elements = new HashSet<int>();

	public void Add(int indicatorValue)
	{
		if (!_Elements.Contains(indicatorValue))
		{
			_Elements.Add(indicatorValue);
		}
	}

	public void Remove(int indicatorValue)
	{
		_Elements.Remove(indicatorValue);
	}

	public void Clear()
	{
		_Elements.Clear();
	}

	public bool Contains(int indicatorValue)
	{
		return _Elements.Contains(indicatorValue);
	}

	public IEnumerator<int> GetEnumerator()
	{
		return _Elements.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
