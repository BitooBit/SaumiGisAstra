using System;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using InflexCore.Action;
using InflexCore.Action.ActionMenu;
using InflexUtils.PrimitiveControls;

namespace InflexUtils;

public class ContextMenuWrapper : InflexActionExecuter, INativeContextMenu, INativeContextMenuItem, INativeMenuItemCollection, ICheckableItem
{
	private readonly ContextMenuStrip _ContextMenu;

	private readonly ResourceManager _ResourceManager;

	private readonly ExtendedMenuItem _MenuItem;

	private readonly ToolStripItem _Control;

	private bool _IsCheckable;

	private bool _IsChecked;

	public bool IsDefault
	{
		get
		{
			if (_MenuItem != null)
			{
				return ((ToolStripItem)_MenuItem).Font.Bold;
			}
			throw new InvalidOperationException();
		}
		set
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected O, but got Unknown
			if (_MenuItem != null)
			{
				Font font = ((ToolStripItem)_MenuItem).Font;
				Font font2 = new Font(font.FontFamily, font.Size, (FontStyle)(value ? 1 : 0));
				((ToolStripItem)_MenuItem).Font = font2;
			}
		}
	}

	public string Name
	{
		get
		{
			if (_MenuItem != null)
			{
				return ((ToolStripItem)_MenuItem).Name;
			}
			throw new InvalidOperationException();
		}
		set
		{
			if (_MenuItem != null)
			{
				((ToolStripItem)_MenuItem).Name = value;
			}
		}
	}

	public string Caption
	{
		get
		{
			if (_MenuItem != null)
			{
				return ((ToolStripItem)_MenuItem).Text;
			}
			throw new InvalidOperationException();
		}
		set
		{
			if (_MenuItem != null)
			{
				((ToolStripItem)_MenuItem).Text = value;
			}
		}
	}

	public InflexAction Command
	{
		get
		{
			if (_MenuItem != null)
			{
				return (InflexAction)_MenuItem.CommandParameter;
			}
			throw new InvalidOperationException();
		}
		set
		{
			if (_MenuItem != null)
			{
				_MenuItem.CommandParameter = value;
				_MenuItem.Command = base.ExecuteActionCommand;
				_MenuItem.IsHiddenWhenDisabled = value?.IsHiddenWhenDisabled ?? false;
			}
		}
	}

	public bool IsCheckable
	{
		get
		{
			return _IsCheckable;
		}
		set
		{
			if (_IsCheckable != value)
			{
				_IsCheckable = value;
				if (_IsCheckable)
				{
					_ = _MenuItem;
				}
				OnPropertyChanged("IsCheckable");
			}
		}
	}

	public bool IsChecked
	{
		get
		{
			return _IsChecked;
		}
		set
		{
			if (_IsChecked != value)
			{
				_IsChecked = value;
				if (CheckedChangedHandler != null)
				{
					CheckedChangedHandler(_IsChecked);
				}
				OnPropertyChanged("IsChecked");
			}
		}
	}

	public Action<bool> CheckedChangedHandler { get; set; }

	public ContextMenuWrapper(ContextMenuStrip contextMenu, ResourceManager resourceManager = null)
	{
		_ContextMenu = contextMenu;
		_ResourceManager = resourceManager;
	}

	private ContextMenuWrapper(ToolStripSeparator control, ResourceManager resourceManager)
	{
		_Control = (ToolStripItem)(object)control;
		_ResourceManager = resourceManager;
	}

	private ContextMenuWrapper(ExtendedMenuItem menuItem, ResourceManager resourceManager)
	{
		_MenuItem = menuItem;
		_ResourceManager = resourceManager;
		_Control = (ToolStripItem)(object)menuItem;
	}

	public void ClearItems()
	{
		if (_ContextMenu != null)
		{
			((ToolStrip)_ContextMenu).Items.Clear();
		}
		if (_MenuItem != null)
		{
			((ToolStripDropDownItem)_MenuItem).DropDownItems.Clear();
		}
	}

	public void AddItem(INativeContextMenuItem item)
	{
		ContextMenuWrapper contextMenuWrapper = (ContextMenuWrapper)item;
		if (_ContextMenu != null)
		{
			((ToolStrip)_ContextMenu).Items.Add(contextMenuWrapper._Control);
		}
		if (_MenuItem != null)
		{
			((ToolStripDropDownItem)_MenuItem).DropDownItems.Add(contextMenuWrapper._Control);
		}
	}

	public INativeContextMenuItem CreateSeparateItem()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		return new ContextMenuWrapper(new ToolStripSeparator(), _ResourceManager);
	}

	public INativeContextMenuItem CreateParentItem()
	{
		return CreateItem();
	}

	public INativeContextMenuItem CreateItem()
	{
		return new ContextMenuWrapper(new ExtendedMenuItem(), _ResourceManager);
	}

	public void SetGlyph(string imageSource, int width, int height)
	{
		if (_MenuItem != null && _ResourceManager != null && !string.IsNullOrEmpty(imageSource))
		{
			ExtendedMenuItem menuItem = _MenuItem;
			object? obj = _ResourceManager.GetObject(imageSource);
			((ToolStripItem)menuItem).Image = (Image)((obj is Image) ? obj : null);
		}
	}
}
