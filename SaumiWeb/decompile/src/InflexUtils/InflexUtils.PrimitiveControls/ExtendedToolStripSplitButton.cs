using System;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using InflexUtils.CommandInfrastructure;

namespace InflexUtils.PrimitiveControls;

public class ExtendedToolStripSplitButton : ToolStripSplitButton
{
	private ICommand _Command;

	private bool _IsMouseEnter;

	public bool IsHiddenWhenDisabled { get; set; }

	public bool IsDropDownButtonSelected { get; private set; }

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

	public ExtendedToolStripSplitButton()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		((ToolStripSplitButton)this)._002Ector();
		((ToolStripSplitButton)this).ButtonClick += InternalClick;
		((ToolStripDropDownItem)this).DropDownOpening += ItemsOpening;
		((ToolStripItem)this).MouseEnter += OnExtMouseEnter;
		((ToolStripItem)this).MouseLeave += OnExtMouseLeave;
		((ToolStripItem)this).MouseMove += new MouseEventHandler(OnExtMouseMove);
	}

	private void OnExtMouseMove(object sender, MouseEventArgs e)
	{
		if (_IsMouseEnter)
		{
			IsDropDownButtonSelected = ((ToolStripSplitButton)this).DropDownButtonBounds.Contains(e.Location);
			((ToolStripItem)this).Invalidate();
		}
	}

	private void OnExtMouseLeave(object sender, EventArgs e)
	{
		_IsMouseEnter = false;
		IsDropDownButtonSelected = false;
	}

	private void OnExtMouseEnter(object sender, EventArgs e)
	{
		_IsMouseEnter = true;
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
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		if (disposing)
		{
			((ToolStripSplitButton)this).ButtonClick -= InternalClick;
			((ToolStripDropDownItem)this).DropDownOpening -= ItemsOpening;
			((ToolStripItem)this).MouseEnter -= OnExtMouseEnter;
			((ToolStripItem)this).MouseLeave -= OnExtMouseLeave;
			((ToolStripItem)this).MouseMove -= new MouseEventHandler(OnExtMouseMove);
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
