using System;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using InflexUtils.CommandInfrastructure;

namespace InflexUtils.PrimitiveControls;

public class ExtendedMenuItem : ToolStripMenuItem
{
	private ICommand _Command;

	public bool IsHiddenWhenDisabled { get; set; }

	public ICommand Command
	{
		get
		{
			return _Command;
		}
		set
		{
			if (_Command != value)
			{
				CommandChanged(value);
			}
		}
	}

	public object CommandParameter { get; set; }

	protected override void OnEnabledChanged(EventArgs e)
	{
		((ToolStripItem)this).OnEnabledChanged(e);
		((ToolStripItem)this).Visible = !IsHiddenWhenDisabled || ((ToolStripItem)this).Enabled;
	}

	public ExtendedMenuItem()
	{
		((ToolStripItem)this).Click += InternalClick;
		((ToolStripDropDownItem)this).DropDownOpening += ItemsOpening;
	}

	private void ItemsOpening(object sender, EventArgs e)
	{
		UpdateOpeningChildsState();
	}

	private void InternalClick(object sender, EventArgs eventArgs)
	{
		if (CheckCommandCanExecute())
		{
			Command.Execute(CommandParameter);
		}
	}

	private void CommandChanged(ICommand command)
	{
		if (_Command != null)
		{
			_Command.CanExecuteChanged -= CommandCanExecuteChanged;
		}
		if (command != null)
		{
			command.CanExecuteChanged += CommandCanExecuteChanged;
			_Command = command;
		}
	}

	private void CommandCanExecuteChanged(object sender, EventArgs e)
	{
		UpdateState();
	}

	private void UpdateOpeningChildsState()
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
			if (val is ExtendedMenuItem extendedMenuItem)
			{
				extendedMenuItem.UpdateState();
			}
		}
	}

	internal void UpdateState()
	{
		((ToolStripItem)this).Enabled = CheckCommandEnabled();
	}

	private bool CheckCommandCanExecute()
	{
		if (Command != null)
		{
			return Command.CanExecute(CommandParameter);
		}
		return false;
	}

	private bool CheckCommandEnabled()
	{
		if (Command == null)
		{
			return true;
		}
		return Command.CanExecute(CommandParameter);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			DirectDispose(this);
		}
		((ToolStripMenuItem)this).Dispose(disposing);
	}

	private void DirectDispose(ExtendedMenuItem item)
	{
		((ToolStripItem)item).Click -= InternalClick;
		((ToolStripDropDownItem)item).DropDownOpening -= ItemsOpening;
		((ToolStripItem)item).Image = null;
		if (item.Command != null)
		{
			item.Command.CanExecuteChanged -= CommandCanExecuteChanged;
			item.Command = null;
		}
		if (item.CommandParameter != null)
		{
			item.CommandParameter = null;
		}
		if (((ArrangedElementCollection)((ToolStripDropDownItem)this).DropDownItems).Count <= 0)
		{
			return;
		}
		for (int i = 0; i < ((ArrangedElementCollection)((ToolStripDropDownItem)item).DropDownItems).Count; i++)
		{
			if (((ToolStripDropDownItem)item).DropDownItems[i] is ExtendedMenuItem extendedMenuItem && !((ToolStripItem)extendedMenuItem).IsDisposed)
			{
				item.DirectDispose(extendedMenuItem);
			}
		}
		((ToolStripDropDownItem)item).DropDownItems.Clear();
	}
}
