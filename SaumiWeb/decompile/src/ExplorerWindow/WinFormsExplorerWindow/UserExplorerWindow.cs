using System;
using System.Collections;
using System.Collections.Generic;
using ExplorerWindowBase;
using InflexCore.Action;

namespace WinFormsExplorerWindow;

public class UserExplorerWindow : IUserExplorerWindow, IWin32ExplorerWindow, IDisposable
{
	private class InflexActionList : IList<InflexAction>, ICollection<InflexAction>, IEnumerable<InflexAction>, IEnumerable
	{
		private readonly ActionContext _Context;

		private List<InflexAction> _Data = new List<InflexAction>();

		public int Count => _Data.Count;

		public bool IsReadOnly => false;

		public InflexAction this[int index]
		{
			get
			{
				return _Data[index];
			}
			set
			{
				_Data[index] = value;
			}
		}

		public InflexActionList(ActionContext context)
		{
			_Context = context;
		}

		public IEnumerator<InflexAction> GetEnumerator()
		{
			return _Data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(InflexAction item)
		{
			_Data.Add(item);
			item.Initialize(_Context);
		}

		public void Clear()
		{
			_Data.Clear();
		}

		public bool Contains(InflexAction item)
		{
			return _Data.Contains(item);
		}

		public void CopyTo(InflexAction[] array, int arrayIndex)
		{
			_Data.CopyTo(array, arrayIndex);
		}

		public bool Remove(InflexAction item)
		{
			return _Data.Remove(item);
		}

		public int IndexOf(InflexAction item)
		{
			return _Data.IndexOf(item);
		}

		public void Insert(int index, InflexAction item)
		{
			_Data.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			RemoveAt(index);
		}
	}

	private readonly object _DisposeSync = new object();

	private bool _IsDisposed;

	public int Handle { get; private set; }

	public IList<InflexAction> Actions { get; private set; }

	public IList<InflexAction> ContextActions { get; private set; }

	protected bool IsDisposed
	{
		get
		{
			lock (_DisposeSync)
			{
				return _IsDisposed;
			}
		}
	}

	public UserExplorerWindow(ExplorerWindow explorerWindow, ActionContext context, int handle)
	{
		Handle = handle;
		ContextActions = new InflexActionList(context);
		Actions = new InflexActionList(context);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		lock (_DisposeSync)
		{
			if (!_IsDisposed)
			{
				OnDispose(disposing);
				if (disposing)
				{
					Actions = null;
					ContextActions = null;
				}
				_IsDisposed = true;
			}
		}
	}

	protected virtual void OnDispose(bool disposing)
	{
	}
}
