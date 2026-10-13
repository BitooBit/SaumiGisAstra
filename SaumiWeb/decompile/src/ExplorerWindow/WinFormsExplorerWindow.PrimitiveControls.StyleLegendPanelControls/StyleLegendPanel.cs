using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using InflexCore;
using InflexDataGridComponent;
using InflexUtils.PrimitiveControls.Popup;
using WinFormsExplorerWindow.Properties;

namespace WinFormsExplorerWindow.PrimitiveControls.StyleLegendPanelControls;

public class StyleLegendPanel : UserControl
{
	private IList<IStyleLegendItem> _ItemsSource;

	private Popup _PopupPath;

	private StyleLegendPanelPopup _PopupContent;

	private List<Image> _PaintColors;

	private readonly object _DisposeSync;

	private IContainer components;

	private Panel _MainPanel;

	public IList<IStyleLegendItem> ItemsSource
	{
		get
		{
			return _ItemsSource;
		}
		set
		{
			if (_ItemsSource != value)
			{
				_ItemsSource = value;
				ApplyItemsSource();
				_PopupContent.ItemsSource = value;
				((Control)_MainPanel).Refresh();
			}
		}
	}

	public StyleLegendPanel()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		_PaintColors = new List<Image>();
		_DisposeSync = new object();
		((UserControl)this)._002Ector();
		InitializeComponent();
		((Control)_MainPanel).MouseHover += MainPanelOnMouseEnter;
		((Control)_MainPanel).MouseLeave += MainPanelOnMouseLeave;
		((Control)_MainPanel).Paint += new PaintEventHandler(MainPanelOnPaint);
		_PopupContent = new StyleLegendPanelPopup();
		Popup popup = new Popup((Control)(object)_PopupContent);
		((Control)popup).AutoSize = true;
		((ToolStripDropDown)popup).AutoClose = false;
		popup.FocusOnOpen = false;
		popup.ShowingAnimation = PopupAnimations.Blend;
		popup.HidingAnimation = PopupAnimations.Blend;
		_PopupPath = popup;
	}

	private void MainPanelOnPaint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.CompositingQuality = (CompositingQuality)2;
		graphics.InterpolationMode = (InterpolationMode)2;
		for (int i = 0; i < _PaintColors.Count; i++)
		{
			int num = ((Control)this).Width - 20 - 20 * i;
			int num2 = 2;
			int num3 = 15;
			int num4 = 15;
			graphics.DrawImage(_PaintColors[i], num, num2, num3, num4);
		}
	}

	private void MainPanelOnMouseEnter(object sender, EventArgs e)
	{
		if (CanShowPopup())
		{
			((ToolStripDropDown)_PopupPath).Show((Control)(object)_MainPanel, -(((Control)_PopupPath).Width - ((Control)this).Width), ((Control)this).Height);
		}
	}

	private void MainPanelOnMouseLeave(object sender, EventArgs e)
	{
		((ToolStripDropDown)_PopupPath).Close();
	}

	private bool CanShowPopup()
	{
		if (ItemsSource != null)
		{
			return ItemsSource.Count > 0;
		}
		return false;
	}

	private void ApplyItemsSource()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		int count = ItemsSource.Count;
		_PaintColors.Clear();
		for (int i = 0; i < count && i <= 2; i++)
		{
			IStyleLegendItem styleLegendItem = ItemsSource[i];
			Color foreground = RowIndicatorConverter.Convert((int)styleLegendItem.Value).Foreground;
			object obj = Resources.ResourceManager.GetObject("Legend_" + foreground.Name);
			if (obj != null)
			{
				_PaintColors.Add((Image)obj);
			}
		}
		((Control)this).Width = _PaintColors.Count * 20;
	}

	private void InnerDispose(bool disposing)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		lock (_DisposeSync)
		{
			if (disposing)
			{
				StyleLegendPanelPopup popupContent = _PopupContent;
				if (popupContent != null && !((Control)popupContent).IsDisposed)
				{
					((Component)(object)popupContent).Dispose();
					_PopupContent = null;
				}
				Popup popupPath = _PopupPath;
				if (popupPath != null && !((Control)popupPath).IsDisposed)
				{
					((Component)(object)popupPath).Dispose();
				}
				_PopupPath = null;
				if (_ItemsSource != null)
				{
					_ItemsSource.Clear();
					_ItemsSource = null;
				}
				if (_PaintColors != null)
				{
					_PaintColors.Clear();
					_PaintColors = null;
				}
				if (_MainPanel != null)
				{
					((Control)_MainPanel).MouseHover -= MainPanelOnMouseEnter;
					((Control)_MainPanel).MouseLeave -= MainPanelOnMouseLeave;
					((Control)_MainPanel).Paint -= new PaintEventHandler(MainPanelOnPaint);
					((Control)_MainPanel).Controls.Clear();
				}
			}
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
	}

	private void InitializeComponent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		_MainPanel = new Panel();
		((Control)this).SuspendLayout();
		((Control)_MainPanel).BackColor = Color.Transparent;
		((Control)_MainPanel).Dock = (DockStyle)5;
		((Control)_MainPanel).Location = new Point(0, 0);
		((Control)_MainPanel).Margin = new Padding(0);
		((Control)_MainPanel).Name = "_MainPanel";
		((Control)_MainPanel).Size = new Size(20, 20);
		((Control)_MainPanel).TabIndex = 0;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = Color.Transparent;
		((Control)this).Controls.Add((Control)(object)_MainPanel);
		((Control)this).Name = "StyleLegendPanel";
		((Control)this).Size = new Size(20, 20);
		((Control)this).ResumeLayout(false);
	}
}
