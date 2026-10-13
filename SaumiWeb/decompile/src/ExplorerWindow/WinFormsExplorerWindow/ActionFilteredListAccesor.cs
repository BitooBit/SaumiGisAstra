using System;
using System.Linq;
using InflexCore;
using InflexCore.Action;
using WinFormsExplorerWindow.Actions;

namespace WinFormsExplorerWindow;

public sealed class ActionFilteredListAccesor : IDisposable, IActionListAccessor
{
	private Func<IInflexID[], ActionCollection> _ActionProvider;

	private Func<ActionCollection, InflexAction> _DefaultResolver;

	private readonly object _DisposeSync = new object();

	private bool _IsDisposed;

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

	public ActionFilteredListAccesor(Func<IInflexID[], ActionCollection> actionProvider)
		: this(actionProvider, (ActionCollection ac) => ac.OfType<ShowInflexObjectToolAction>().FirstOrDefault())
	{
	}

	public ActionFilteredListAccesor(Func<IInflexID[], ActionCollection> actionProvider, Func<ActionCollection, InflexAction> defaultResolver)
	{
		if (actionProvider == null)
		{
			throw new ArgumentNullException("actionProvider");
		}
		_DefaultResolver = defaultResolver;
		_ActionProvider = actionProvider;
	}

	public ActionCollection GetActions(params IInflexID[] ids)
	{
		ActionCollection actionCollection = _ActionProvider(ids);
		if (actionCollection == null || actionCollection.Count == 0)
		{
			return null;
		}
		if (_DefaultResolver != null)
		{
			actionCollection.DefaultAction = _DefaultResolver(actionCollection);
		}
		return actionCollection;
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
				if (disposing)
				{
					_ActionProvider = null;
					_DefaultResolver = null;
				}
				_IsDisposed = true;
			}
		}
	}
}
