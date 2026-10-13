using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using InflexCore;
using InflexDataGridComponent;
using WinFormsExplorerWindow.Properties;

namespace WinFormsExplorerWindow.PrimitiveControls.StyleLegendPanelControls;

public class StyleLegendPanelPopup : UserControl
{
	private IContainer components;

	private TableLayoutPanel _MainPanel;

	private IList<IStyleLegendItem> _ItemsSource;

	private readonly Dictionary<string, RowIndicatorStyle> _IndicatorStyles = new Dictionary<string, RowIndicatorStyle>();

	private readonly List<Label> _Labels = new List<Label>();

	private readonly List<PictureBox> _PictureBoxes = new List<PictureBox>();

	private readonly object _DisposedSync = new object();

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
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !((Control)this).IsDisposed)
		{
			if (components != null)
			{
				components.Dispose();
			}
			InnerDispose(disposing);
		}
		((ContainerControl)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Expected O, but got Unknown
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		_MainPanel = new TableLayoutPanel();
		((Control)this).SuspendLayout();
		((Control)_MainPanel).AutoSize = true;
		((Panel)_MainPanel).AutoSizeMode = (AutoSizeMode)0;
		((Control)_MainPanel).BackColor = Color.Transparent;
		_MainPanel.ColumnCount = 2;
		_MainPanel.ColumnStyles.Add(new ColumnStyle());
		_MainPanel.ColumnStyles.Add(new ColumnStyle((SizeType)2, 100f));
		((Control)_MainPanel).Dock = (DockStyle)5;
		((Control)_MainPanel).Location = new Point(0, 0);
		((Control)_MainPanel).Margin = new Padding(0);
		((Control)_MainPanel).Name = "_MainPanel";
		((Control)_MainPanel).Padding = new Padding(7, 5, 7, 5);
		_MainPanel.RowCount = 1;
		_MainPanel.RowStyles.Add(new RowStyle((SizeType)1, 20f));
		((Control)_MainPanel).Size = new Size(14, 30);
		((Control)_MainPanel).TabIndex = 0;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).AutoSize = true;
		((UserControl)this).AutoSizeMode = (AutoSizeMode)0;
		((Control)this).BackColor = Color.FromArgb(232, 246, 254);
		((Control)this).Controls.Add((Control)(object)_MainPanel);
		((Control)this).Margin = new Padding(0);
		((Control)this).Name = "StyleLegendPanelPopup";
		((Control)this).Size = new Size(14, 30);
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}

	public StyleLegendPanelPopup()
	{
		InitializeComponent();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		Rectangle rectangle = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
		int num = 5;
		Pen val = new Pen(Color.FromArgb(22, 82, 142), (float)num);
		try
		{
			e.Graphics.DrawRectangle(val, rectangle);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		Pen val2 = new Pen(Color.FromArgb(118, 118, 118));
		try
		{
			e.Graphics.DrawRectangle(val2, rectangle);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		((Control)this).OnPaint(e);
	}

	private void ApplyItemsSource()
	{
		int count = ItemsSource.Count;
		_IndicatorStyles.Clear();
		for (int i = 0; i < count; i++)
		{
			IStyleLegendItem styleLegendItem = ItemsSource[i];
			if (!_IndicatorStyles.ContainsKey(styleLegendItem.Info))
			{
				_IndicatorStyles.Add(styleLegendItem.Info, RowIndicatorConverter.Convert((int)styleLegendItem.Value));
			}
		}
		BuildPanel();
	}

	private void BuildPanel()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		((Control)_MainPanel).SuspendLayout();
		((ControlCollection)_MainPanel.Controls).Clear();
		((TableLayoutStyleCollection)_MainPanel.RowStyles).Clear();
		DisposePictureBoxes();
		DisposeLabels();
		int num = -1;
		foreach (KeyValuePair<string, RowIndicatorStyle> indicatorStyle in _IndicatorStyles)
		{
			num++;
			_MainPanel.RowStyles.Add(new RowStyle((SizeType)0));
			Color foreground = indicatorStyle.Value.Foreground;
			object obj = Resources.ResourceManager.GetObject("Legend_" + foreground.Name);
			if (obj != null)
			{
				Image val = (Image)obj;
				PictureBox val2 = new PictureBox();
				((Control)val2).Anchor = (AnchorStyles)4;
				val2.Image = val;
				((Control)val2).Width = val.Width;
				((Control)val2).Height = val.Height;
				PictureBox val3 = val2;
				val = null;
				_PictureBoxes.Add(val3);
				_MainPanel.SetColumn((Control)(object)val3, 0);
				_MainPanel.SetRow((Control)(object)val3, num);
				((ControlCollection)_MainPanel.Controls).Add((Control)(object)val3);
			}
			Label val4 = new Label();
			((Control)val4).AutoSize = true;
			((Control)val4).MaximumSize = new Size(230, int.MaxValue);
			((Control)val4).ForeColor = indicatorStyle.Value.Foreground;
			((Control)val4).Margin = new Padding(0, 5, 0, 2);
			((Control)val4).Text = indicatorStyle.Key;
			Label val5 = val4;
			((Control)val5).Font = new Font(((Control)val5).Font.FontFamily, ((Control)val5).Font.Size, indicatorStyle.Value.FontStyle, ((Control)val5).Font.Unit);
			_Labels.Add(val5);
			_MainPanel.SetColumn((Control)(object)val5, 1);
			_MainPanel.SetRow((Control)(object)val5, num);
			((ControlCollection)_MainPanel.Controls).Add((Control)(object)val5);
		}
		((Control)_MainPanel).ResumeLayout();
	}

	private void DisposePictureBoxes()
	{
		foreach (PictureBox pictureBox in _PictureBoxes)
		{
			if (pictureBox.Image != null)
			{
				pictureBox.Image.Dispose();
			}
			if (!((Control)pictureBox).IsDisposed)
			{
				((Component)(object)pictureBox).Dispose();
			}
		}
		_PictureBoxes.Clear();
	}

	private void DisposeLabels()
	{
		foreach (Label label in _Labels)
		{
			((Control)label).Font.Dispose();
			((Component)(object)label).Dispose();
		}
		_Labels.Clear();
	}

	private void InnerDispose(bool disposing)
	{
		lock (_DisposedSync)
		{
			if (disposing)
			{
				DisposePictureBoxes();
				DisposeLabels();
			}
		}
	}
}
