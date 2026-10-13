using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsExplorerWindow.PrimitiveControls.WindowTitle;

public class WindowHeaderControl : UserControl
{
	private IContainer components;

	private ControlBox _ControlBox;

	private LinePanel _HeaderLinePanel;

	private TableLayoutPanel _TableLayoutPanel;

	private Label _Label;

	private readonly object _DisposeSync;

	public override string Text
	{
		get
		{
			return ((Control)_Label).Text;
		}
		set
		{
			((Control)_Label).Text = value;
		}
	}

	public ControlBox ControlBox => _ControlBox;

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
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Expected O, but got Unknown
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		_TableLayoutPanel = new TableLayoutPanel();
		_Label = new Label();
		_ControlBox = new ControlBox();
		_HeaderLinePanel = new LinePanel();
		((Control)_TableLayoutPanel).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)_TableLayoutPanel).Anchor = (AnchorStyles)13;
		_TableLayoutPanel.ColumnCount = 2;
		_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle((SizeType)2, 100f));
		_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
		_TableLayoutPanel.Controls.Add((Control)(object)_ControlBox, 1, 0);
		_TableLayoutPanel.Controls.Add((Control)(object)_Label, 0, 0);
		((Control)_TableLayoutPanel).Location = new Point(0, 0);
		((Control)_TableLayoutPanel).Margin = new Padding(0);
		((Control)_TableLayoutPanel).Name = "_TableLayoutPanel";
		_TableLayoutPanel.RowCount = 1;
		_TableLayoutPanel.RowStyles.Add(new RowStyle((SizeType)2, 100f));
		_TableLayoutPanel.RowStyles.Add(new RowStyle((SizeType)1, 27f));
		((Control)_TableLayoutPanel).Size = new Size(695, 20);
		((Control)_TableLayoutPanel).TabIndex = 3;
		((Panel)_TableLayoutPanel).TabStop = false;
		_Label.AutoEllipsis = true;
		((Control)_Label).AutoSize = true;
		((Control)_Label).BackColor = Color.Transparent;
		((Control)_Label).Font = new Font("Verdana", 11.25f, (FontStyle)1, (GraphicsUnit)3, (byte)204);
		((Control)_Label).ForeColor = Color.White;
		((Control)_Label).Location = new Point(5, 1);
		((Control)_Label).Margin = new Padding(5, 1, 0, 0);
		((Control)_Label).Name = "_Label";
		((Control)_Label).Size = new Size(75, 18);
		((Control)_Label).TabIndex = 2;
		((Control)_Label).Text = "Данные";
		((Control)_ControlBox).Anchor = (AnchorStyles)9;
		_ControlBox.AttachedForm = null;
		((Control)_ControlBox).AutoSize = true;
		((UserControl)_ControlBox).AutoSizeMode = (AutoSizeMode)0;
		((Control)_ControlBox).BackColor = Color.Transparent;
		((Control)_ControlBox).Location = new Point(601, 0);
		((Control)_ControlBox).Margin = new Padding(0, 0, 5, 0);
		((Control)_ControlBox).Name = "_ControlBox";
		((Control)_ControlBox).Size = new Size(89, 18);
		((Control)_ControlBox).TabIndex = 1;
		((Control)_ControlBox).TabStop = false;
		((Control)_HeaderLinePanel).Anchor = (AnchorStyles)13;
		_HeaderLinePanel.LineColor = Color.White;
		((Control)_HeaderLinePanel).Location = new Point(0, 22);
		((Control)_HeaderLinePanel).Margin = new Padding(0);
		((Control)_HeaderLinePanel).Name = "_HeaderLinePanel";
		((Control)_HeaderLinePanel).Size = new Size(650, 1);
		((Control)_HeaderLinePanel).TabIndex = 2;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = Color.Transparent;
		((Control)this).Controls.Add((Control)(object)_TableLayoutPanel);
		((Control)this).Controls.Add((Control)(object)_HeaderLinePanel);
		((Control)this).Margin = new Padding(0);
		((Control)this).Name = "WindowHeaderControl";
		((Control)this).Size = new Size(695, 27);
		((Control)_TableLayoutPanel).ResumeLayout(false);
		((Control)_TableLayoutPanel).PerformLayout();
		((Control)this).ResumeLayout(false);
	}

	public WindowHeaderControl()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		_DisposeSync = new object();
		((UserControl)this)._002Ector();
		InitializeComponent();
		((Control)_Label).MouseDown += new MouseEventHandler(LabelOnMouseDown);
		((Control)_TableLayoutPanel).MouseDown += new MouseEventHandler(TableLayoutPanelOnMouseDown);
	}

	private void LabelOnMouseDown(object sender, MouseEventArgs e)
	{
		((Control)this).OnMouseDown(e);
	}

	private void TableLayoutPanelOnMouseDown(object sender, MouseEventArgs e)
	{
		((Control)this).OnMouseDown(e);
	}

	internal void ApplyStyle(WindowStyle style)
	{
		if (style != null)
		{
			((Control)_Label).ForeColor = style.Foreground;
			((Control)_Label).Refresh();
			_HeaderLinePanel.LineColor = style.Foreground;
		}
	}

	internal void SetControlBoxStyle(bool isActive)
	{
		if (isActive)
		{
			ControlBox.SetActiveStyle();
		}
		else
		{
			ControlBox.SetInactiveStyle();
		}
	}

	private void InnerDispose(bool disposing)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		lock (_DisposeSync)
		{
			if (disposing && !((Control)this).IsDisposed)
			{
				Label label = _Label;
				if (label != null)
				{
					((Control)label).MouseDown -= new MouseEventHandler(LabelOnMouseDown);
				}
				TableLayoutPanel tableLayoutPanel = _TableLayoutPanel;
				if (tableLayoutPanel != null)
				{
					((Control)tableLayoutPanel).MouseDown -= new MouseEventHandler(TableLayoutPanelOnMouseDown);
				}
				ControlBox controlBox = _ControlBox;
				if (controlBox != null && !((Control)controlBox).IsDisposed)
				{
					((Component)(object)controlBox).Dispose();
				}
			}
		}
	}
}
