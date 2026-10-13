using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace InflexUtils.ExceptionBox;

public class ExceptionBoxForm : Form
{
	private IContainer components;

	private TableLayoutPanel tableLayoutPanel1;

	private TextBox _ErrorText;

	private Button _BtnClose;

	private readonly string _FullMessage;

	private ToolTip _ToolTip;

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
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ExceptionBoxForm));
		tableLayoutPanel1 = new TableLayoutPanel();
		_ErrorText = new TextBox();
		_BtnClose = new Button();
		((Control)tableLayoutPanel1).SuspendLayout();
		((Control)this).SuspendLayout();
		tableLayoutPanel1.ColumnCount = 1;
		tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle((SizeType)2, 50f));
		tableLayoutPanel1.Controls.Add((Control)(object)_ErrorText, 0, 0);
		tableLayoutPanel1.Controls.Add((Control)(object)_BtnClose, 0, 1);
		((Control)tableLayoutPanel1).Dock = (DockStyle)5;
		((Control)tableLayoutPanel1).Location = new Point(0, 0);
		((Control)tableLayoutPanel1).Name = "tableLayoutPanel1";
		tableLayoutPanel1.RowCount = 2;
		tableLayoutPanel1.RowStyles.Add(new RowStyle((SizeType)2, 73.33334f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle((SizeType)2, 26.66667f));
		((Control)tableLayoutPanel1).Size = new Size(383, 129);
		((Control)tableLayoutPanel1).TabIndex = 0;
		((Control)_ErrorText).Anchor = (AnchorStyles)15;
		((Control)_ErrorText).BackColor = Color.LightYellow;
		((Control)_ErrorText).Location = new Point(3, 3);
		((TextBoxBase)_ErrorText).Multiline = true;
		((Control)_ErrorText).Name = "_ErrorText";
		((TextBoxBase)_ErrorText).ReadOnly = true;
		((Control)_ErrorText).Size = new Size(377, 88);
		((Control)_ErrorText).TabIndex = 0;
		((Control)_BtnClose).Anchor = (AnchorStyles)0;
		((Control)_BtnClose).Location = new Point(154, 100);
		((Control)_BtnClose).Name = "_BtnClose";
		((Control)_BtnClose).Size = new Size(75, 23);
		((Control)_BtnClose).TabIndex = 1;
		((Control)_BtnClose).Text = "Закрыть";
		((ButtonBase)_BtnClose).UseVisualStyleBackColor = true;
		((Control)_BtnClose).Click += OnExitClick;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(383, 129);
		((Control)this).Controls.Add((Control)(object)tableLayoutPanel1);
		((Form)this).FormBorderStyle = (FormBorderStyle)3;
		((Form)this).Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
		((Form)this).MaximizeBox = false;
		((Form)this).MinimizeBox = false;
		((Control)this).Name = "ExceptionBoxForm";
		((Form)this).StartPosition = (FormStartPosition)1;
		((Control)this).Text = "Ошибка";
		((Form)this).TopMost = true;
		((Control)tableLayoutPanel1).ResumeLayout(false);
		((Control)tableLayoutPanel1).PerformLayout();
		((Control)this).ResumeLayout(false);
	}

	public ExceptionBoxForm(object exception)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((Form)this)._002Ector();
		InitializeComponent();
		_ToolTip = new ToolTip();
		_ToolTip.SetToolTip((Control)(object)_ErrorText, "(Ctr+C) Скопировать полный текст ошибки.");
		Exception ex = exception as Exception;
		if (ex == null)
		{
			((Control)_ErrorText).Text = "Сбой в приложении.";
			_FullMessage = ((exception == null) ? "Тип не известен." : exception.GetType().FullName);
			return;
		}
		((Control)_ErrorText).Text = ex.Message;
		while (ex != null)
		{
			string fullMessage = _FullMessage;
			_FullMessage = fullMessage + "Mess: " + ex.Message + Environment.NewLine + "Stack: " + ex.StackTrace + Environment.NewLine + Environment.NewLine;
			ex = ex.InnerException;
		}
	}

	private void OnExitClick(object sender, EventArgs e)
	{
		((Form)this).Close();
	}

	protected override void OnClosed(EventArgs e)
	{
		if (_ToolTip != null)
		{
			_ToolTip.RemoveAll();
			_ToolTip = null;
		}
		((Form)this).OnClosed(e);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if ((int)keyData == 131139)
		{
			Clipboard.SetText(_FullMessage);
			return true;
		}
		return ((Form)this).ProcessCmdKey(ref msg, keyData);
	}
}
