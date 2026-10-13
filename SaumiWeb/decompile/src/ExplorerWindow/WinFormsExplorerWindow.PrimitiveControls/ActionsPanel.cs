using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using InflexCore.Action;
using InflexCore.Action.ActionMenu;
using InflexUtils;
using InflexUtils.PrimitiveControls;
using WinFormsExplorerWindow.Properties;

namespace WinFormsExplorerWindow.PrimitiveControls;

public sealed class ActionsPanel : UserControl
{
	private class ActionsPanelToolStripRenderer : ToolStripProfessionalRenderer
	{
		private readonly ToolStrip _ToolsPanel;

		private readonly Color _MenuImageMarginBackground = Color.FromArgb(25, 75, 125);

		private readonly Color _BorderColor = Color.FromArgb(118, 118, 118);

		private readonly Color _DisabledFontColor = ViewResources.InactiveForegroundColor;

		private Color _MenuBackgroundColor = Color.FromArgb(23, 89, 157);

		public Color MenuBackgroundColor
		{
			get
			{
				return _MenuBackgroundColor;
			}
			set
			{
				_MenuBackgroundColor = value;
			}
		}

		public ActionsPanelToolStripRenderer(ToolStrip toolsPanel)
		{
			_ToolsPanel = toolsPanel;
			((ToolStripProfessionalRenderer)this).RoundedEdges = false;
		}

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Expected O, but got Unknown
			if (e.ToolStrip != _ToolsPanel)
			{
				Pen val = new Pen(_BorderColor);
				try
				{
					Rectangle affectedBounds = e.AffectedBounds;
					affectedBounds.Width--;
					affectedBounds.Height--;
					e.Graphics.DrawRectangle(val, affectedBounds);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				SolidBrush val2 = new SolidBrush(MenuBackgroundColor);
				try
				{
					e.Graphics.FillRectangle((Brush)(object)val2, e.ConnectedArea);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
		}

		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			SolidBrush val = new SolidBrush(_MenuImageMarginBackground);
			try
			{
				e.Graphics.FillRectangle((Brush)(object)val, e.AffectedBounds);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}

		protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Expected O, but got Unknown
			ToolStripItem item = e.Item;
			ToolStripDropDownItem val = (ToolStripDropDownItem)(object)((item is ToolStripDropDownItem) ? item : null);
			if (val != null && ((ToolStripItem)val).Pressed && val.HasDropDownItems)
			{
				Rectangle rectangle = new Rectangle(Point.Empty, ((ToolStripItem)val).Size);
				rectangle.Width--;
				rectangle.Height--;
				SolidBrush val2 = new SolidBrush(_MenuImageMarginBackground);
				try
				{
					e.Graphics.FillRectangle((Brush)(object)val2, rectangle);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				Pen val3 = new Pen(_BorderColor);
				try
				{
					e.Graphics.DrawRectangle(val3, rectangle);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Expected O, but got Unknown
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Expected O, but got Unknown
			if (e.Item.CanSelect && e.Item.Enabled && e.Item.Selected)
			{
				Rectangle rectangle = new Rectangle(Point.Empty, e.Item.Size);
				Padding val = default(Padding);
				((Padding)(ref val))._002Ector(2, 0, 1, 0);
				rectangle.X += ((Padding)(ref val)).Left;
				rectangle.Y += ((Padding)(ref val)).Top;
				rectangle.Width -= ((Padding)(ref val)).Horizontal;
				rectangle.Height -= ((Padding)(ref val)).Vertical;
				SolidBrush val2 = new SolidBrush(Color.FromArgb(153, 196, 225, 255));
				try
				{
					e.Graphics.FillRectangle((Brush)(object)val2, rectangle);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				Pen val3 = new Pen(Color.FromArgb(40, 127, 215));
				try
				{
					rectangle.Width--;
					rectangle.Height--;
					e.Graphics.DrawRectangle(val3, rectangle);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
		}

		protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
		{
		}

		protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
		{
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Expected O, but got Unknown
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Expected O, but got Unknown
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected O, but got Unknown
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			if (!(e.Item is ExtendedToolStripSplitButton extendedToolStripSplitButton))
			{
				return;
			}
			Rectangle rectangle = new Rectangle(Point.Empty, ((ToolStripItem)extendedToolStripSplitButton).Size);
			rectangle.Width--;
			rectangle.Height--;
			bool flag = ((ToolStripSplitButton)extendedToolStripSplitButton).DropDownButtonPressed || extendedToolStripSplitButton.IsDropDownButtonSelected;
			if (flag && ((ToolStripDropDownItem)extendedToolStripSplitButton).HasDropDownItems)
			{
				SolidBrush val = new SolidBrush(_MenuImageMarginBackground);
				try
				{
					e.Graphics.FillRectangle((Brush)(object)val, rectangle);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				Pen val2 = new Pen(_BorderColor);
				try
				{
					e.Graphics.DrawRectangle(val2, rectangle);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			if (flag)
			{
				Brush val3 = (Brush)new SolidBrush(_BorderColor);
				try
				{
					e.Graphics.FillRectangle(val3, ((ToolStripSplitButton)extendedToolStripSplitButton).SplitterBounds);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			((ToolStripRenderer)this).DrawArrow(new ToolStripArrowRenderEventArgs(e.Graphics, (ToolStripItem)(object)extendedToolStripSplitButton, ((ToolStripSplitButton)extendedToolStripSplitButton).DropDownButtonBounds, SystemColors.ControlText, (ArrowDirection)17));
		}

		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
		{
			Rectangle imageRectangle = e.ImageRectangle;
			Image val = (Image)(object)Resources.CheckMark;
			if (imageRectangle != Rectangle.Empty && val != null)
			{
				if (!((ToolStripItemRenderEventArgs)e).Item.Enabled)
				{
					val = ToolStripRenderer.CreateDisabledImage(val);
				}
				((ToolStripItemRenderEventArgs)e).Graphics.DrawImage(val, imageRectangle, new Rectangle(Point.Empty, imageRectangle.Size), (GraphicsUnit)2);
			}
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			FontStyle val = (FontStyle)0;
			if (((ToolStripItemRenderEventArgs)e).Item.OwnerItem == null)
			{
				ToolStripItem item = ((ToolStripItemRenderEventArgs)e).Item;
				ToolStripSplitButton val2 = (ToolStripSplitButton)(object)((item is ToolStripSplitButton) ? item : null);
				if (val2 != null)
				{
					if (val2.ButtonPressed || val2.DropDownButtonPressed)
					{
						val = (FontStyle)1;
					}
				}
				else if (((ToolStripItemRenderEventArgs)e).Item.Pressed)
				{
					val = (FontStyle)1;
				}
				if (((ToolStripItemRenderEventArgs)e).Item.Selected)
				{
					val = (FontStyle)(val | 4);
				}
			}
			Font textFont = e.TextFont;
			Font val3 = new Font(textFont.FontFamily, textFont.Size, val);
			try
			{
				e.TextFont = val3;
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			e.TextColor = (((ToolStripItemRenderEventArgs)e).Item.Enabled ? Color.White : _DisabledFontColor);
			InnerRenderItemText(e);
		}

		private void InnerRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Invalid comparison between Unknown and I4
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Expected O, but got Unknown
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Invalid comparison between Unknown and I4
			Graphics graphics = ((ToolStripItemRenderEventArgs)e).Graphics;
			Color textColor = e.TextColor;
			Font textFont = e.TextFont;
			string text = e.Text;
			Rectangle textRectangle = e.TextRectangle;
			TextFormatFlags textFormat = e.TextFormat;
			if ((int)e.TextDirection != 1 && textRectangle.Width > 0 && textRectangle.Height > 0)
			{
				Size size = FlipSize(textRectangle.Size);
				Bitmap val = new Bitmap(size.Width, size.Height, (PixelFormat)925707);
				try
				{
					Graphics val2 = Graphics.FromImage((Image)(object)val);
					try
					{
						val2.TextRenderingHint = (TextRenderingHint)4;
						TextRenderer.DrawText((IDeviceContext)(object)val2, text, textFont, new Rectangle(Point.Empty, size), textColor, textFormat);
						((Image)val).RotateFlip((RotateFlipType)(((int)e.TextDirection == 2) ? 1 : 3));
						graphics.DrawImage((Image)(object)val, textRectangle);
						return;
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			TextRenderer.DrawText((IDeviceContext)(object)graphics, text, textFont, textRectangle, textColor, textFormat);
		}

		private Size FlipSize(Size size)
		{
			int width = size.Width;
			size.Width = size.Height;
			size.Height = width;
			return size;
		}

		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			SolidBrush val = new SolidBrush(MenuBackgroundColor);
			try
			{
				e.Graphics.FillRectangle((Brush)(object)val, e.AffectedBounds);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}

		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
		{
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			e.ArrowColor = Color.White;
			((ToolStripProfessionalRenderer)this).OnRenderArrow(e);
		}
	}

	private IContainer components;

	private TableLayoutPanel _MainPanel;

	private ToolStrip _ToolsPanel;

	private ExtendedToolStripSplitButton _Update;

	private ExtendedToolStripButton _Create;

	private ExtendedToolStripDropDownButton _Save;

	private ExtendedToolStripDropDownButton _Print;

	private ExtendedToolStripDropDownButton _View;

	private ExtendedToolStripDropDownButton _SQL;

	private ExtendedToolStripDropDownButton _User;

	private ExtendedToolStripButton _Map;

	private ExtendedToolStripButton _ShowAnalitic;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripSeparator toolStripSeparator2;

	private ExtendedToolStripButton _StopUpdate;

	private ActionCollection _SaveUserActions;

	private ActionCollection _UserActions;

	private readonly object _DisposeSync = new object();

	public override Color BackColor
	{
		get
		{
			return ((Control)this).BackColor;
		}
		set
		{
			if (_ToolsPanel.Renderer is ActionsPanelToolStripRenderer actionsPanelToolStripRenderer)
			{
				actionsPanelToolStripRenderer.MenuBackgroundColor = value;
			}
			((Control)this).BackColor = value;
		}
	}

	internal ExtendedToolStripSplitButton UpdateControl => _Update;

	internal ExtendedToolStripButton CreateNewControl => _Create;

	internal ExtendedToolStripDropDownButton SaveControl => _Save;

	internal ExtendedToolStripDropDownButton PrintControl => _Print;

	internal ExtendedToolStripDropDownButton ViewControl => _View;

	internal ExtendedToolStripDropDownButton SqlControl => _SQL;

	internal ExtendedToolStripDropDownButton UserControl => _User;

	internal ExtendedToolStripButton MapControl => _Map;

	internal ExtendedToolStripButton AnaliticWindow => _ShowAnalitic;

	internal ExtendedToolStripButton StopUpdate => _StopUpdate;

	public ActionCollection SaveUserActions
	{
		get
		{
			return _SaveUserActions;
		}
		set
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Expected O, but got Unknown
			_SaveUserActions = value;
			if (value == null || ((ArrangedElementCollection)((ToolStripDropDownItem)_Save).DropDownItems).Count <= 0 || value.Count <= 0)
			{
				return;
			}
			((ToolStripDropDownItem)_Save).DropDownItems.Add((ToolStripItem)new ToolStripSeparator());
			ContextMenuStrip val = new ContextMenuStrip();
			ContextMenuBuilder contextMenuBuilder = new ContextMenuBuilder();
			contextMenuBuilder.BuildMenu(new ContextMenuWrapper(val), _SaveUserActions);
			List<ToolStripItem> list = new List<ToolStripItem>();
			foreach (ToolStripItem item2 in (ArrangedElementCollection)((ToolStrip)val).Items)
			{
				ToolStripItem item = item2;
				list.Add(item);
			}
			((ToolStrip)val).Items.Clear();
			((ToolStripDropDownItem)_Save).DropDownItems.AddRange(list.ToArray());
		}
	}

	public ActionCollection UserActions
	{
		get
		{
			return _UserActions;
		}
		set
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected O, but got Unknown
			_UserActions = value;
			if (value == null)
			{
				return;
			}
			ContextMenuStrip val = new ContextMenuStrip();
			ContextMenuBuilder contextMenuBuilder = new ContextMenuBuilder();
			contextMenuBuilder.BuildMenu(new ContextMenuWrapper(val), _UserActions);
			List<ToolStripItem> list = new List<ToolStripItem>();
			foreach (ToolStripItem item2 in (ArrangedElementCollection)((ToolStrip)val).Items)
			{
				ToolStripItem item = item2;
				list.Add(item);
			}
			((ToolStrip)val).Items.Clear();
			((ToolStripDropDownItem)_User).DropDownItems.AddRange(list.ToArray());
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		InnerDispose(disposing);
		((ContainerControl)this).Dispose(disposing);
		GC.SuppressFinalize(this);
	}

	private void InitializeComponent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Expected O, but got Unknown
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_0512: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Expected O, but got Unknown
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0679: Unknown result type (might be due to invalid IL or missing references)
		//IL_0712: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_087f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0901: Unknown result type (might be due to invalid IL or missing references)
		//IL_090b: Expected O, but got Unknown
		_MainPanel = new TableLayoutPanel();
		_ToolsPanel = new ToolStrip();
		toolStripSeparator1 = new ToolStripSeparator();
		_Update = new ExtendedToolStripSplitButton();
		_StopUpdate = new ExtendedToolStripButton();
		_Create = new ExtendedToolStripButton();
		_Save = new ExtendedToolStripDropDownButton();
		_Print = new ExtendedToolStripDropDownButton();
		_View = new ExtendedToolStripDropDownButton();
		_SQL = new ExtendedToolStripDropDownButton();
		_User = new ExtendedToolStripDropDownButton();
		toolStripSeparator2 = new ToolStripSeparator();
		_Map = new ExtendedToolStripButton();
		_ShowAnalitic = new ExtendedToolStripButton();
		((Control)_MainPanel).SuspendLayout();
		((Control)_ToolsPanel).SuspendLayout();
		((Control)this).SuspendLayout();
		_MainPanel.ColumnCount = 1;
		_MainPanel.ColumnStyles.Add(new ColumnStyle((SizeType)2, 100f));
		_MainPanel.ColumnStyles.Add(new ColumnStyle((SizeType)1, 20f));
		_MainPanel.Controls.Add((Control)(object)_ToolsPanel, 0, 0);
		((Control)_MainPanel).Dock = (DockStyle)5;
		((Control)_MainPanel).Location = new Point(0, 0);
		((Control)_MainPanel).Name = "_MainPanel";
		_MainPanel.RowCount = 1;
		_MainPanel.RowStyles.Add(new RowStyle((SizeType)2, 100f));
		((Control)_MainPanel).Size = new Size(806, 27);
		((Control)_MainPanel).TabIndex = 0;
		_ToolsPanel.BackColor = Color.Transparent;
		_ToolsPanel.CanOverflow = false;
		((Control)_ToolsPanel).Dock = (DockStyle)5;
		_ToolsPanel.GripMargin = new Padding(0);
		_ToolsPanel.GripStyle = (ToolStripGripStyle)0;
		_ToolsPanel.Items.AddRange((ToolStripItem[])(object)new ToolStripItem[12]
		{
			(ToolStripItem)toolStripSeparator1,
			(ToolStripItem)_Update,
			(ToolStripItem)_StopUpdate,
			(ToolStripItem)_Create,
			(ToolStripItem)_Save,
			(ToolStripItem)_Print,
			(ToolStripItem)_View,
			(ToolStripItem)_SQL,
			(ToolStripItem)_User,
			(ToolStripItem)toolStripSeparator2,
			(ToolStripItem)_Map,
			(ToolStripItem)_ShowAnalitic
		});
		_ToolsPanel.LayoutStyle = (ToolStripLayoutStyle)1;
		((Control)_ToolsPanel).Location = new Point(0, 0);
		((Control)_ToolsPanel).Name = "_ToolsPanel";
		((Control)_ToolsPanel).Padding = new Padding(0);
		_ToolsPanel.RenderMode = (ToolStripRenderMode)2;
		_ToolsPanel.ShowItemToolTips = false;
		((Control)_ToolsPanel).Size = new Size(806, 27);
		((Control)_ToolsPanel).TabIndex = 0;
		((ToolStripItem)toolStripSeparator1).Name = "toolStripSeparator1";
		((ToolStripItem)toolStripSeparator1).Size = new Size(6, 27);
		((ToolStripItem)toolStripSeparator1).Visible = false;
		((ToolStripSplitButton)_Update).AutoToolTip = false;
		_Update.Command = null;
		_Update.CommandParameter = null;
		((ToolStripItem)_Update).DisplayStyle = (ToolStripItemDisplayStyle)1;
		((ToolStripItem)_Update).ForeColor = Color.White;
		((ToolStripItem)_Update).ImageTransparentColor = Color.Magenta;
		((ToolStripItem)_Update).Name = "_Update";
		((ToolStripItem)_Update).Size = new Size(73, 24);
		((ToolStripItem)_Update).Text = "Обновить";
		((ToolStripButton)_StopUpdate).AutoToolTip = false;
		_StopUpdate.Command = null;
		_StopUpdate.CommandParameter = null;
		((ToolStripItem)_StopUpdate).DisplayStyle = (ToolStripItemDisplayStyle)1;
		((ToolStripItem)_StopUpdate).Enabled = false;
		((ToolStripItem)_StopUpdate).ForeColor = Color.White;
		((ToolStripItem)_StopUpdate).ImageTransparentColor = Color.Magenta;
		((ToolStripItem)_StopUpdate).Name = "_StopUpdate";
		((ToolStripItem)_StopUpdate).Size = new Size(72, 24);
		((ToolStripItem)_StopUpdate).Text = "Остановить";
		((ToolStripItem)_StopUpdate).Visible = false;
		((ToolStripButton)_Create).AutoToolTip = false;
		_Create.Command = null;
		_Create.CommandParameter = null;
		((ToolStripItem)_Create).DisplayStyle = (ToolStripItemDisplayStyle)1;
		((ToolStripItem)_Create).Enabled = false;
		((ToolStripItem)_Create).ForeColor = Color.White;
		((ToolStripItem)_Create).ImageTransparentColor = Color.Magenta;
		((ToolStripItem)_Create).Margin = new Padding(10, 1, 0, 2);
		((ToolStripItem)_Create).Name = "_Create";
		((ToolStripItem)_Create).Size = new Size(54, 24);
		((ToolStripItem)_Create).Text = "Создать";
		((ToolStripItem)_Create).Visible = false;
		((ToolStripDropDownButton)_Save).AutoToolTip = false;
		_Save.Command = null;
		_Save.CommandParameter = null;
		((ToolStripItem)_Save).DisplayStyle = (ToolStripItemDisplayStyle)1;
		((ToolStripItem)_Save).Font = new Font("Tahoma", 8.25f);
		((ToolStripItem)_Save).ForeColor = Color.White;
		((ToolStripItem)_Save).ImageTransparentColor = Color.Magenta;
		((ToolStripItem)_Save).Margin = new Padding(10, 1, 0, 2);
		((ToolStripItem)_Save).Name = "_Save";
		((ToolStripItem)_Save).Size = new Size(75, 24);
		((ToolStripItem)_Save).Text = "Сохранить";
		((ToolStripDropDownButton)_Print).AutoToolTip = false;
		_Print.Command = null;
		_Print.CommandParameter = null;
		((ToolStripItem)_Print).DisplayStyle = (ToolStripItemDisplayStyle)1;
		((ToolStripItem)_Print).ForeColor = Color.White;
		((ToolStripItem)_Print).ImageTransparentColor = Color.Magenta;
		((ToolStripItem)_Print).Margin = new Padding(10, 1, 0, 2);
		((ToolStripItem)_Print).Name = "_Print";
		((ToolStripItem)_Print).Size = new Size(57, 24);
		((ToolStripItem)_Print).Text = "Печать";
		((ToolStripDropDownButton)_View).AutoToolTip = false;
		_View.Command = null;
		_View.CommandParameter = null;
		((ToolStripItem)_View).DisplayStyle = (ToolStripItemDisplayStyle)1;
		((ToolStripItem)_View).ForeColor = Color.White;
		((ToolStripItem)_View).ImageTransparentColor = Color.Magenta;
		((ToolStripItem)_View).Margin = new Padding(10, 1, 0, 2);
		((ToolStripItem)_View).Name = "_View";
		((ToolStripItem)_View).Size = new Size(40, 24);
		((ToolStripItem)_View).Text = "Вид";
		((ToolStripDropDownButton)_SQL).AutoToolTip = false;
		_SQL.Command = null;
		_SQL.CommandParameter = null;
		((ToolStripItem)_SQL).DisplayStyle = (ToolStripItemDisplayStyle)1;
		((ToolStripItem)_SQL).ForeColor = Color.White;
		((ToolStripItem)_SQL).ImageTransparentColor = Color.Magenta;
		((ToolStripItem)_SQL).Margin = new Padding(10, 1, 0, 2);
		((ToolStripItem)_SQL).Name = "_SQL";
		((ToolStripItem)_SQL).Size = new Size(39, 24);
		((ToolStripItem)_SQL).Text = "SQL";
		((ToolStripDropDownButton)_User).AutoToolTip = false;
		_User.Command = null;
		_User.CommandParameter = null;
		((ToolStripItem)_User).DisplayStyle = (ToolStripItemDisplayStyle)1;
		((ToolStripItem)_User).Enabled = false;
		((ToolStripItem)_User).ForeColor = Color.White;
		((ToolStripItem)_User).ImageTransparentColor = Color.Magenta;
		((ToolStripItem)_User).Margin = new Padding(10, 1, 0, 2);
		((ToolStripItem)_User).Name = "_User";
		((ToolStripItem)_User).Size = new Size(82, 24);
		((ToolStripItem)_User).Text = "Дополнение";
		((ToolStripItem)_User).Visible = false;
		((ToolStripItem)toolStripSeparator2).Name = "toolStripSeparator2";
		((ToolStripItem)toolStripSeparator2).Size = new Size(6, 27);
		((ToolStripButton)_Map).AutoToolTip = false;
		_Map.Command = null;
		_Map.CommandParameter = null;
		((ToolStripItem)_Map).DisplayStyle = (ToolStripItemDisplayStyle)1;
		((ToolStripItem)_Map).ForeColor = Color.White;
		((ToolStripItem)_Map).ImageTransparentColor = Color.Magenta;
		((ToolStripItem)_Map).Margin = new Padding(10, 1, 0, 2);
		((ToolStripItem)_Map).Name = "_Map";
		((ToolStripItem)_Map).Size = new Size(42, 24);
		((ToolStripItem)_Map).Text = "Карта";
		((ToolStripItem)_ShowAnalitic).AutoSize = false;
		((ToolStripItem)_ShowAnalitic).BackColor = Color.Transparent;
		_ShowAnalitic.Command = null;
		_ShowAnalitic.CommandParameter = null;
		((ToolStripItem)_ShowAnalitic).Font = new Font("Tahoma", 8.25f);
		((ToolStripItem)_ShowAnalitic).ForeColor = Color.White;
		((ToolStripItem)_ShowAnalitic).Name = "_ShowAnalitic";
		((ToolStripItem)_ShowAnalitic).Size = new Size(80, 24);
		((ToolStripItem)_ShowAnalitic).Text = "Аналитика";
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).AutoSize = true;
		((Control)this).BackColor = Color.Transparent;
		((Control)this).Controls.Add((Control)(object)_MainPanel);
		((Control)this).Name = "ActionsPanel";
		((Control)this).Size = new Size(806, 27);
		((Control)_MainPanel).ResumeLayout(false);
		((Control)_MainPanel).PerformLayout();
		((Control)_ToolsPanel).ResumeLayout(false);
		((Control)_ToolsPanel).PerformLayout();
		((Control)this).ResumeLayout(false);
	}

	public ActionsPanel()
	{
		InitializeComponent();
		_ToolsPanel.Renderer = (ToolStripRenderer)(object)new ActionsPanelToolStripRenderer(_ToolsPanel);
		ConfigureItems();
	}

	private void ConfigureItems()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		_ = ((Control)_ToolsPanel).Font;
		Font val = new Font(new FontFamily("Verdana"), 8f, (FontStyle)1, (GraphicsUnit)3);
		try
		{
			foreach (ToolStripItem item in (ArrangedElementCollection)_ToolsPanel.Items)
			{
				ToolStripItem val2 = item;
				val2.MouseEnter += delegate
				{
					((Control)_ToolsPanel).Cursor = Cursors.Hand;
				};
				val2.MouseLeave += delegate
				{
					((Control)_ToolsPanel).Cursor = Cursors.Default;
				};
				val2.Font = val;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		_User.IsHiddenWhenDisabled = true;
	}

	private void InnerDispose(bool disposing)
	{
		lock (_DisposeSync)
		{
			if (!disposing)
			{
				return;
			}
			ExtendedToolStripDropDownButton user = _User;
			if (user != null)
			{
				if (((ArrangedElementCollection)((ToolStripDropDownItem)user).DropDownItems).Count > 0)
				{
					for (int i = 0; i < ((ArrangedElementCollection)((ToolStripDropDownItem)user).DropDownItems).Count; i++)
					{
						ToolStripItem val = ((ToolStripDropDownItem)user).DropDownItems[i];
						if (!val.IsDisposed)
						{
							((Component)(object)val).Dispose();
						}
					}
					((ToolStripDropDownItem)user).DropDownItems.Clear();
				}
				if (!((ToolStripItem)user).IsDisposed)
				{
					((Component)(object)user).Dispose();
				}
				_User = null;
			}
			ExtendedToolStripButton stopUpdate = _StopUpdate;
			if (stopUpdate != null && !((ToolStripItem)stopUpdate).IsDisposed)
			{
				((Component)(object)stopUpdate).Dispose();
			}
			ExtendedToolStripSplitButton update = _Update;
			if (update != null && !((ToolStripItem)update).IsDisposed)
			{
				((Component)(object)update).Dispose();
			}
		}
	}
}
