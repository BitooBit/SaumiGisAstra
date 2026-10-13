using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsExplorerWindow;

public class ShowQueryTextForm : Form
{
	private IContainer components;

	private Button _CloseButton;

	private TextBox _QueryTextBox;

	private TableLayoutPanel _TableLayoutPanel;

	public ShowQueryTextForm(string queryText)
	{
		InitializeComponent();
		string text = char.ConvertFromUtf32(10);
		string text2 = char.ConvertFromUtf32(13);
		((Control)_QueryTextBox).Text = queryText.Replace(text2, string.Empty).Replace(text, text2 + text);
		((Control)_CloseButton).Click += CloseButtonOnClick;
	}

	private void CloseButtonOnClick(object sender, EventArgs e)
	{
		((Form)this).Close();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((Form)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Expected O, but got Unknown
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Expected O, but got Unknown
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Expected O, but got Unknown
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ShowQueryTextForm));
		_CloseButton = new Button();
		_QueryTextBox = new TextBox();
		_TableLayoutPanel = new TableLayoutPanel();
		((Control)_TableLayoutPanel).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)_CloseButton).Location = new Point(164, 286);
		((Control)_CloseButton).Name = "_CloseButton";
		((Control)_CloseButton).Size = new Size(105, 23);
		((Control)_CloseButton).TabIndex = 0;
		((Control)_CloseButton).Text = "Ok";
		((ButtonBase)_CloseButton).UseVisualStyleBackColor = true;
		((Control)_QueryTextBox).BackColor = Color.White;
		_TableLayoutPanel.SetColumnSpan((Control)(object)_QueryTextBox, 3);
		((Control)_QueryTextBox).Dock = (DockStyle)5;
		((Control)_QueryTextBox).Location = new Point(10, 3);
		((Control)_QueryTextBox).Margin = new Padding(10, 3, 10, 0);
		((TextBoxBase)_QueryTextBox).Multiline = true;
		((Control)_QueryTextBox).Name = "_QueryTextBox";
		((TextBoxBase)_QueryTextBox).ReadOnly = true;
		_QueryTextBox.ScrollBars = (ScrollBars)2;
		((Control)_QueryTextBox).Size = new Size(414, 280);
		((Control)_QueryTextBox).TabIndex = 5;
		_TableLayoutPanel.ColumnCount = 3;
		_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle((SizeType)2, 50f));
		_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
		_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle((SizeType)2, 50f));
		_TableLayoutPanel.Controls.Add((Control)(object)_CloseButton, 1, 1);
		_TableLayoutPanel.Controls.Add((Control)(object)_QueryTextBox, 0, 0);
		((Control)_TableLayoutPanel).Dock = (DockStyle)5;
		((Control)_TableLayoutPanel).Location = new Point(0, 0);
		((Control)_TableLayoutPanel).Name = "_TableLayoutPanel";
		_TableLayoutPanel.RowCount = 2;
		_TableLayoutPanel.RowStyles.Add(new RowStyle((SizeType)2, 100f));
		_TableLayoutPanel.RowStyles.Add(new RowStyle());
		((Control)_TableLayoutPanel).Size = new Size(434, 312);
		((Control)_TableLayoutPanel).TabIndex = 6;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(434, 312);
		((Control)this).Controls.Add((Control)(object)_TableLayoutPanel);
		((Form)this).Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
		((Form)this).MaximizeBox = false;
		((Form)this).MinimizeBox = false;
		((Control)this).Name = "ShowQueryTextForm";
		((Control)this).Text = "Текст запроса";
		((Control)_TableLayoutPanel).ResumeLayout(false);
		((Control)_TableLayoutPanel).PerformLayout();
		((Control)this).ResumeLayout(false);
	}
}
