using System;
using InflexCore;
using InflexCore.Action;
using WinFormsExplorerWindow.Actions;

namespace WinFormsExplorerWindow.GisComponent;

public sealed class ShowMapHelper : IDisposable
{
	private ExplorerWindow _Window;

	private IActionListAccessor _ActionListAccessor;

	private bool _IsShowMap;

	private int _WorkspaceID = -1;

	private bool _PreviewEnded;

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

	public ShowMapHelper(ExplorerWindow window, IActionListAccessor actionListAccessor)
	{
		_Window = window;
		_ActionListAccessor = actionListAccessor;
	}

	public void ShowMap(int workspaceID)
	{
		ActionCollection actions = _ActionListAccessor.GetActions();
		ShowGisAction showGisAction = actions.Action<ShowGisAction>();
		if (showGisAction != null && showGisAction.IsAllowed != false)
		{
			showGisAction.WorkspaceID = workspaceID;
			showGisAction.Execute();
		}
	}

	public void PreviewShowMapHandler()
	{
		_Window.DataView.EndUpdate += OnShowMapDataViewEndUpdate;
	}

	private void OnShowMapDataViewEndUpdate()
	{
		_Window.DataView.EndUpdate -= OnShowMapDataViewEndUpdate;
		_PreviewEnded = true;
		if (_IsShowMap)
		{
			ShowMap(_WorkspaceID);
		}
	}

	public void InitializePreview(int workspaceID)
	{
		_IsShowMap = true;
		_WorkspaceID = workspaceID;
		if (_PreviewEnded)
		{
			ShowMap(_WorkspaceID);
		}
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
					_ActionListAccessor = null;
					_Window = null;
				}
				_IsDisposed = true;
			}
		}
	}
}
