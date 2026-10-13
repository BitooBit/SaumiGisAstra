using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinFormsExplorerWindow;

internal class StopedProgressBarFactory : IDisposable
{
	private StatusStrip _StatusBar;

	private HashSet<StatusPanelStopedProgressBar> _Items = new HashSet<StatusPanelStopedProgressBar>();

	private readonly object _DisposeSync = new object();

	public bool IsAllProgressesFinished
	{
		get
		{
			List<StatusPanelStopedProgressBar> list = new List<StatusPanelStopedProgressBar>(_Items);
			foreach (StatusPanelStopedProgressBar item in list)
			{
				if (!item.IsProgressFinished)
				{
					return false;
				}
			}
			return true;
		}
	}

	public StopedProgressBarFactory(StatusStrip statusBar)
	{
		_StatusBar = statusBar;
	}

	public StatusPanelStopedProgressBar CreateStopedProgressBar(string stopQuest)
	{
		StatusPanelStopedProgressBar statusPanelStopedProgressBar = new StatusPanelStopedProgressBar(_StatusBar, stopQuest);
		RegisterProgressBar(statusPanelStopedProgressBar);
		return statusPanelStopedProgressBar;
	}

	public bool TryStopAllProgress()
	{
		List<StatusPanelStopedProgressBar> list = new List<StatusPanelStopedProgressBar>(_Items);
		foreach (StatusPanelStopedProgressBar item in list)
		{
			if (!item.TryStop())
			{
				return false;
			}
		}
		return true;
	}

	private void RegisterProgressBar(StatusPanelStopedProgressBar progressBar)
	{
		progressBar.Disposing += ProgressBarDisposing;
		_Items.Add(progressBar);
	}

	private void UnRegisterProgressBar(StatusPanelStopedProgressBar progressBar)
	{
		progressBar.Disposing -= ProgressBarDisposing;
		_Items.Remove(progressBar);
	}

	private void ProgressBarDisposing(object sender, EventArgs e)
	{
		UnRegisterProgressBar((StatusPanelStopedProgressBar)sender);
	}

	public void Dispose()
	{
		lock (_DisposeSync)
		{
			if (_Items != null && _Items.Count > 0)
			{
				foreach (StatusPanelStopedProgressBar item in _Items)
				{
					item.Disposing -= ProgressBarDisposing;
					item.ShouldStop = true;
					item.Dispose();
				}
				_Items.Clear();
				_Items = null;
			}
			_StatusBar = null;
		}
	}
}
