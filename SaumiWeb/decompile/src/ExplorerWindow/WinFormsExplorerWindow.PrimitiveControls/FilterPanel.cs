using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using InflexUtils.PrimitiveControls;
using WinFormsExplorerWindow.Properties;

namespace WinFormsExplorerWindow.PrimitiveControls;

public class FilterPanel : UserControl
{
	private IContainer components;

	private TableLayoutPanel _TableLayoutPanel;

	private ImageButton _ClearFilterButton;

	private ImageToggleButton _EnableFilterButton;

	private Panel _FilterTextBoxPanel;

	private Label _DefaultTextLabel;

	private TextBox _FilterTextBox;

	public string FilterText
	{
		get
		{
			return ((Control)_FilterTextBox).Text;
		}
		set
		{
			((Control)_FilterTextBox).Text = value;
		}
	}

	public bool IsFilterEnabled
	{
		get
		{
			return _EnableFilterButton.Checked;
		}
		set
		{
			_EnableFilterButton.Checked = value;
		}
	}

	public event Action<string> FilterTextChanged;

	public event Action<bool> FocusChanged;

	public event Action<bool> FilterEnabledChanged;

	public FilterPanel()
	{
		InitializeComponent();
		((Control)_DefaultTextLabel).Click += DefaultTextLabelOnClick;
		((Control)_FilterTextBox).TextChanged += FilterTextBoxOnTextChanged;
		((Control)_FilterTextBox).GotFocus += FilterTextBoxOnGotFocus;
		((Control)_FilterTextBox).LostFocus += FilterTextBoxOnLostFocus;
		_EnableFilterButton.CheckedChanged += EnableFilterButtonOnCheckedChanged;
		((Control)_ClearFilterButton).Click += ClearFilterButtonOnClick;
	}

	public void SetToolTips(ToolTip toolTip)
	{
		toolTip.SetToolTip((Control)(object)_ClearFilterButton, "Отобразить все строки выборки");
		toolTip.SetToolTip((Control)(object)_EnableFilterButton, "Отображать только те строки выборки, которые содержат заданный набор символов");
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		Rectangle rectangle = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
		Pen val = new Pen(Color.FromArgb(99, 144, 188));
		try
		{
			e.Graphics.DrawRectangle(val, rectangle);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		((Control)this).OnPaint(e);
	}

	private void FireFilterTextChanged()
	{
		FilterTextChanged?.Invoke(FilterText);
	}

	private void FilterTextBoxOnTextChanged(object sender, EventArgs e)
	{
		if (!((Control)_FilterTextBox).Focused)
		{
			((TextBoxBase)_FilterTextBox).SelectionStart = ((Control)_FilterTextBox).Text.Length;
			((TextBoxBase)_FilterTextBox).SelectionLength = 0;
		}
		((Control)_FilterTextBox).Focus();
		ImageToggleButton enableFilterButton = _EnableFilterButton;
		bool flag = (((Control)_ClearFilterButton).Visible = !string.IsNullOrEmpty(((Control)_FilterTextBox).Text));
		enableFilterButton.Checked = flag;
		FireFilterTextChanged();
	}

	private void FireFocusChanged(bool isFocused)
	{
		FocusChanged?.Invoke(isFocused);
	}

	private void FilterTextBoxOnLostFocus(object sender, EventArgs e)
	{
		((Control)_DefaultTextLabel).Visible = string.IsNullOrEmpty(FilterText);
		FireFocusChanged(isFocused: false);
	}

	private void FilterTextBoxOnGotFocus(object sender, EventArgs e)
	{
		((Control)_DefaultTextLabel).Visible = false;
		FireFocusChanged(isFocused: true);
	}

	private void DefaultTextLabelOnClick(object sender, EventArgs e)
	{
		((Control)_FilterTextBox).Focus();
	}

	private void FireFilterEnabledChanged()
	{
		FilterEnabledChanged?.Invoke(IsFilterEnabled);
	}

	private void EnableFilterButtonOnCheckedChanged(bool isChecked)
	{
		if (isChecked)
		{
			((Control)_FilterTextBox).Focus();
		}
		FireFilterEnabledChanged();
	}

	private void ClearFilterButtonOnClick(object sender, EventArgs e)
	{
		((TextBoxBase)_FilterTextBox).Clear();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((ContainerControl)this).Dispose(disposing);
		GC.SuppressFinalize(this);
	}

	private void InitializeComponent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Expected O, but got Unknown
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Expected O, but got Unknown
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Expected O, but got Unknown
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		_TableLayoutPanel = new TableLayoutPanel();
		_ClearFilterButton = new ImageButton();
		_EnableFilterButton = new ImageToggleButton();
		_FilterTextBoxPanel = new Panel();
		_DefaultTextLabel = new Label();
		_FilterTextBox = new TextBox();
		((Control)_TableLayoutPanel).SuspendLayout();
		((ISupportInitialize)_ClearFilterButton).BeginInit();
		((ISupportInitialize)_EnableFilterButton).BeginInit();
		((Control)_FilterTextBoxPanel).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)_TableLayoutPanel).AutoSize = true;
		((Panel)_TableLayoutPanel).AutoSizeMode = (AutoSizeMode)0;
		((Control)_TableLayoutPanel).BackColor = Color.White;
		_TableLayoutPanel.ColumnCount = 3;
		_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle((SizeType)1, 20f));
		_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle((SizeType)2, 100f));
		_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle((SizeType)1, 19f));
		_TableLayoutPanel.Controls.Add((Control)(object)_ClearFilterButton, 2, 0);
		_TableLayoutPanel.Controls.Add((Control)(object)_EnableFilterButton, 0, 0);
		_TableLayoutPanel.Controls.Add((Control)(object)_FilterTextBoxPanel, 1, 0);
		((Control)_TableLayoutPanel).Dock = (DockStyle)5;
		((Control)_TableLayoutPanel).Location = new Point(1, 1);
		((Control)_TableLayoutPanel).Margin = new Padding(0);
		((Control)_TableLayoutPanel).Name = "_TableLayoutPanel";
		_TableLayoutPanel.RowCount = 1;
		_TableLayoutPanel.RowStyles.Add(new RowStyle((SizeType)1, 19f));
		((Control)_TableLayoutPanel).Size = new Size(163, 19);
		((Control)_TableLayoutPanel).TabIndex = 0;
		_ClearFilterButton.DialogResult = (DialogResult)0;
		_ClearFilterButton.DownImage = (Image)(object)Resources.ClearPressed;
		_ClearFilterButton.HoverImage = (Image)(object)Resources.ClearOver;
		((Control)_ClearFilterButton).Location = new Point(144, 0);
		((Control)_ClearFilterButton).Margin = new Padding(0);
		((Control)_ClearFilterButton).Name = "_ClearFilterButton";
		_ClearFilterButton.NormalImage = (Image)(object)Resources.Clear;
		((Control)_ClearFilterButton).Size = new Size(19, 19);
		_ClearFilterButton.SizeMode = (PictureBoxSizeMode)2;
		((PictureBox)_ClearFilterButton).TabIndex = 3;
		((PictureBox)_ClearFilterButton).TabStop = false;
		((Control)_ClearFilterButton).Visible = false;
		_EnableFilterButton.Checked = false;
		_EnableFilterButton.DialogResult = (DialogResult)0;
		_EnableFilterButton.DownImage = (Image)(object)Resources.SearchPressed;
		_EnableFilterButton.HoverImage = (Image)(object)Resources.SearchOver;
		((Control)_EnableFilterButton).Location = new Point(0, 0);
		((Control)_EnableFilterButton).Margin = new Padding(0);
		((Control)_EnableFilterButton).Name = "_EnableFilterButton";
		_EnableFilterButton.NormalImage = (Image)(object)Resources.Search;
		((Control)_EnableFilterButton).Size = new Size(20, 19);
		_EnableFilterButton.SizeMode = (PictureBoxSizeMode)2;
		((PictureBox)_EnableFilterButton).TabIndex = 4;
		((PictureBox)_EnableFilterButton).TabStop = false;
		((Control)_FilterTextBoxPanel).AutoSize = true;
		_FilterTextBoxPanel.AutoSizeMode = (AutoSizeMode)0;
		((Control)_FilterTextBoxPanel).Controls.Add((Control)(object)_DefaultTextLabel);
		((Control)_FilterTextBoxPanel).Controls.Add((Control)(object)_FilterTextBox);
		((Control)_FilterTextBoxPanel).Location = new Point(20, 0);
		((Control)_FilterTextBoxPanel).Margin = new Padding(0);
		((Control)_FilterTextBoxPanel).Name = "_FilterTextBoxPanel";
		((Control)_FilterTextBoxPanel).Size = new Size(124, 19);
		((Control)_FilterTextBoxPanel).TabIndex = 5;
		((Control)_DefaultTextLabel).AutoSize = true;
		((Control)_DefaultTextLabel).BackColor = Color.Transparent;
		((Control)_DefaultTextLabel).Font = new Font("Verdana", 8.5f, (FontStyle)2, (GraphicsUnit)3, (byte)204);
		((Control)_DefaultTextLabel).ForeColor = Color.Gray;
		((Control)_DefaultTextLabel).Location = new Point(0, 2);
		((Control)_DefaultTextLabel).Margin = new Padding(2);
		((Control)_DefaultTextLabel).Name = "_DefaultTextLabel";
		((Control)_DefaultTextLabel).Size = new Size(108, 14);
		((Control)_DefaultTextLabel).TabIndex = 5;
		((Control)_DefaultTextLabel).Text = "Поиск в данных";
		((Control)_FilterTextBox).BackColor = Color.White;
		((TextBoxBase)_FilterTextBox).BorderStyle = (BorderStyle)0;
		((Control)_FilterTextBox).Font = new Font("Verdana", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)204);
		((Control)_FilterTextBox).Location = new Point(2, 2);
		((Control)_FilterTextBox).Margin = new Padding(2);
		((Control)_FilterTextBox).MinimumSize = new Size(120, 16);
		((Control)_FilterTextBox).Name = "_FilterTextBox";
		((Control)_FilterTextBox).Size = new Size(120, 15);
		((Control)_FilterTextBox).TabIndex = 4;
		((TextBoxBase)_FilterTextBox).WordWrap = false;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).AutoSize = true;
		((UserControl)this).AutoSizeMode = (AutoSizeMode)0;
		((Control)this).BackColor = Color.Transparent;
		((Control)this).Controls.Add((Control)(object)_TableLayoutPanel);
		((Control)this).Margin = new Padding(0);
		((Control)this).Name = "FilterPanel";
		((Control)this).Padding = new Padding(1);
		((Control)this).Size = new Size(165, 21);
		((Control)_TableLayoutPanel).ResumeLayout(false);
		((Control)_TableLayoutPanel).PerformLayout();
		((ISupportInitialize)_ClearFilterButton).EndInit();
		((ISupportInitialize)_EnableFilterButton).EndInit();
		((Control)_FilterTextBoxPanel).ResumeLayout(false);
		((Control)_FilterTextBoxPanel).PerformLayout();
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
