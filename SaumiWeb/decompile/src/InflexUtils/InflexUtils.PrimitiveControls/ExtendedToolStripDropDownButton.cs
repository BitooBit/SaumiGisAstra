using System;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using InflexUtils.CommandInfrastructure;

namespace InflexUtils.PrimitiveControls;

public class ExtendedToolStripDropDownButton : ToolStripDropDownButton
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

	public ExtendedToolStripDropDownButton()
	{
		((ToolStripDropDownItem)this).DropDownOpening += ItemsOpening;
	}

	private void ItemsOpening(object sender, EventArgs e)
	{
		UpdateOpeningChildsState();
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

	private bool CheckCommandCanExecute()
	{
		if (Command != null)
		{
			return Command.CanExecute(CommandParameter);
		}
		return false;
	}

	private void UpdateState()
	{
		((ToolStripItem)this).Enabled = CheckCommandEnabled();
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
			((ToolStripDropDownItem)this).DropDownOpening -= ItemsOpening;
			if (Command != null)
			{
				Command.CanExecuteChanged -= CommandCanExecuteChanged;
				Command = null;
			}
			if (CommandParameter != null)
			{
				CommandParameter = null;
			}
		}
		((ToolStripDropDownItem)this).Dispose(disposing);
	}
}
