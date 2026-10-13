using System;
using System.Collections.Generic;
using System.Linq;

namespace InflexCore.Action;

public sealed class ActionCollection : List<InflexAction>
{
	private InflexAction _DefaultAction;

	public InflexAction DefaultAction
	{
		get
		{
			return _DefaultAction;
		}
		set
		{
			if (value == null || Contains(value))
			{
				_DefaultAction = value;
			}
		}
	}

	public InflexAction this[Type actionType]
	{
		get
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					InflexAction current = enumerator.Current;
					if ((object)current.GetType() == actionType)
					{
						return current;
					}
				}
			}
			return this.FirstOrDefault((InflexAction action) => actionType.IsInstanceOfType(action));
		}
	}

	public bool TryAdd(InflexAction item)
	{
		if (item.IsAllowedContext)
		{
			Add(item);
			return true;
		}
		return false;
	}

	public void AddCollection(ActionCollection collection)
	{
		AddRange(collection);
		if (_DefaultAction == null && collection._DefaultAction != null)
		{
			_DefaultAction = collection._DefaultAction;
		}
	}

	public T Action<T>() where T : InflexAction
	{
		return this[typeof(T)] as T;
	}
}
