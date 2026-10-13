using System;
using System.Windows.Forms;
using InflexUtils.CommandInfrastructure;

namespace InflexUtils.PrimitiveControls;

public class ExtendedToolStripButton : ToolStripButton
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

	public ExtendedToolStripButton()
	{
		((ToolStripItem)this).Click += InternalClick;
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
			((ToolStripItem)this).Click -= InternalClick;
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
		((ToolStripItem)this).Dispose(disposing);
	}
}
