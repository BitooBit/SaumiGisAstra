using System;
using System.Drawing;
using System.Windows.Forms;
using WinFormsExplorerWindow.Properties;

namespace WinFormsExplorerWindow;

public class StatusPanelStopedProgressBar : IDisposable
{
	private StatusStrip _StatusPanel;

	private readonly string _StopQuest;

	private ToolStripProgressBar _ProgressBar;

	private ToolStripDropDownButton _ButtonClose;

	private bool _ShouldStop;

	private readonly object _DisposeSync;

	private bool _IsDisposed;

	public bool IsProgressFinished { get; private set; }

	public bool ShouldStop
	{
		get
		{
			return _ShouldStop;
		}
		set
		{
			_ShouldStop = value;
			if (value)
			{
				HideItems();
			}
		}
	}

	public int Value
	{
		get
		{
			if (_ProgressBar != null)
			{
				return _ProgressBar.Value;
			}
			return 0;
		}
		set
		{
			if (_ProgressBar != null)
			{
				_ProgressBar.Value = value;
				Application.DoEvents();
			}
		}
	}

	public event EventHandler Disposing;

	public StatusPanelStopedProgressBar(StatusStrip statusPanel, string stopQuest)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		_DisposeSync = new object();
		base._002Ector();
		_StatusPanel = statusPanel;
		_StopQuest = stopQuest;
		ShouldStop = false;
		ToolStripProgressBar val = new ToolStripProgressBar();
		((ToolStripItem)val).Height = 9;
		val.Value = 0;
		((ToolStripItem)val).Width = 100;
		_ProgressBar = val;
		ToolStripDropDownButton val2 = new ToolStripDropDownButton();
		((ToolStripItem)val2).DisplayStyle = (ToolStripItemDisplayStyle)2;
		((ToolStripItem)val2).BackColor = Color.Transparent;
		((ToolStripItem)val2).AutoSize = false;
		((ToolStripItem)val2).Image = (Image)(object)Resources.CancelExport;
		((ToolStripItem)val2).ImageScaling = (ToolStripItemImageScaling)0;
		val2.ShowDropDownArrow = false;
		((ToolStripItem)val2).Height = 12;
		((ToolStripItem)val2).Width = 12;
		_ButtonClose = val2;
		ToolStripDropDownButton buttonClose = _ButtonClose;
		EventHandler eventHandler = delegate
		{
			TryStop();
		};
		((ToolStripItem)buttonClose).Click += eventHandler;
		ToolStripItem[] array = (ToolStripItem[])(object)new ToolStripItem[2]
		{
			(ToolStripItem)_ProgressBar,
			(ToolStripItem)_ButtonClose
		};
		((ToolStrip)statusPanel).Items.AddRange(array);
	}

	public bool TryStop()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		DialogResult val = MessageBox.Show(_StopQuest, "Внимание!", (MessageBoxButtons)4, (MessageBoxIcon)32);
		if ((int)val == 6)
		{
			ShouldStop = true;
			return true;
		}
		return false;
	}

	public void Free()
	{
		IsProgressFinished = true;
	}

	internal void HideItems()
	{
		if (_ProgressBar != null && _ButtonClose != null && _StatusPanel != null)
		{
			((ToolStripItem)_ProgressBar).Visible = false;
			((ToolStripItem)_ButtonClose).Visible = false;
			((Control)_StatusPanel).Refresh();
		}
	}

	private void OnDisposing()
	{
		Disposing?.Invoke(this, new EventArgs());
	}

	public void Dispose()
	{
		lock (_DisposeSync)
		{
			if (_IsDisposed)
			{
				return;
			}
			OnDisposing();
			if (_StatusPanel != null)
			{
				if (_ProgressBar != null)
				{
					((ToolStripItem)_ProgressBar).Visible = false;
				}
				if (_ButtonClose != null)
				{
					((ToolStripItem)_ButtonClose).Visible = false;
				}
				((ToolStrip)_StatusPanel).Items.Remove((ToolStripItem)(object)_ProgressBar);
				_ProgressBar = null;
				((ToolStrip)_StatusPanel).Items.Remove((ToolStripItem)(object)_ButtonClose);
				_ButtonClose = null;
				_StatusPanel = null;
			}
			_IsDisposed = true;
		}
	}
}
