using System;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using InflexUtils.PrimitiveControls;
using WinFormsExplorerWindow.Actions;

namespace WinFormsExplorerWindow.PrimitiveControls;

public class InflexMenuItem : ExtendedMenuItem
{
	private ICheckTool _CheckTool;

	public ICheckTool CheckTool
	{
		get
		{
			return _CheckTool;
		}
		set
		{
			SetCheckToolState(value);
		}
	}

	public InflexMenuItem()
	{
		((ToolStripDropDownItem)this).DropDownOpening += InflexMenuItemsOpening;
	}

	private void InflexMenuItemsOpening(object sender, EventArgs e)
	{
		UpdateChildCheckStates();
	}

	private void UpdateChildCheckStates()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		if (!((ToolStripDropDownItem)this).HasDropDownItems)
		{
			return;
		}
		foreach (ToolStripItem item in (ArrangedElementCollection)((ToolStripDropDownItem)this).DropDownItems)
		{
			ToolStripItem val = item;
			if (val is InflexMenuItem inflexMenuItem)
			{
				inflexMenuItem.UpdateCheckState();
			}
		}
	}

	private void UpdateCheckState()
	{
		if (_CheckTool != null)
		{
			SyncCheckState();
		}
	}

	protected override void OnCheckedChanged(EventArgs e)
	{
		if (_CheckTool != null)
		{
			_CheckTool.CheckToolState.IsChecked = ((ToolStripMenuItem)this).Checked;
		}
		((ToolStripMenuItem)this).OnCheckedChanged(e);
	}

	private void SetCheckToolState(ICheckTool value)
	{
		if (_CheckTool != value)
		{
			if (_CheckTool != null)
			{
				_CheckTool.CheckToolState.CheckedChanged -= CheckToolStateChanged;
			}
			_CheckTool = value;
			if (_CheckTool != null)
			{
				_CheckTool.CheckToolState.CheckedChanged += CheckToolStateChanged;
			}
		}
	}

	private void CheckToolStateChanged(object sender, EventArgs e)
	{
		SyncCheckState();
	}

	private void SyncCheckState()
	{
		((ToolStripMenuItem)this).Checked = _CheckTool.CheckToolState.IsChecked;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			((ToolStripDropDownItem)this).DropDownOpening -= InflexMenuItemsOpening;
			if (CheckTool != null)
			{
				CheckTool.CheckToolState.CheckedChanged -= CheckToolStateChanged;
				CheckTool = null;
			}
		}
		base.Dispose(disposing);
	}
}
