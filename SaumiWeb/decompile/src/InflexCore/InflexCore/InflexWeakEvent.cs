using System;
using System.Collections.Generic;

namespace InflexCore;

public abstract class InflexWeakEvent<T> where T : class
{
	private List<WeakReference> _Handlers = new List<WeakReference>();

	public void Add(T handler)
	{
		if (handler != null)
		{
			_Handlers.Add(new WeakReference(handler));
		}
	}

	public void Remove(T handler)
	{
		for (int i = _Handlers.Count - 1; i >= 0; i++)
		{
			if (!(_Handlers[i].Target is T objA))
			{
				_Handlers.RemoveAt(i);
			}
			else if (object.ReferenceEquals(objA, handler))
			{
				_Handlers.RemoveAt(i);
				break;
			}
		}
	}

	public void DoEvent()
	{
		Stack<int> stack = new Stack<int>();
		try
		{
			for (int i = 0; i < _Handlers.Count; i++)
			{
				if (!(_Handlers[i].Target is T handler))
				{
					stack.Push(i);
				}
				else
				{
					DoEventItem(handler);
				}
			}
		}
		finally
		{
			while (stack.Count > 0)
			{
				_Handlers.RemoveAt(stack.Pop());
			}
		}
	}

	protected abstract void DoEventItem(T handler);
}
