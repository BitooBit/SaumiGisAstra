using System;
using System.Collections;
using System.Collections.Generic;

namespace InflexCore;

public sealed class SetInitializeTypes : IEnumerable<Type>, IEnumerable
{
	private List<Type> _Data = new List<Type>();

	public void Set(Type type)
	{
		if (!_Data.Contains(type))
		{
			_Data.Add(type);
		}
	}

	public void Set(params Type[] types)
	{
		foreach (Type type in types)
		{
			Set(type);
		}
	}

	public IEnumerator<Type> GetEnumerator()
	{
		return _Data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
