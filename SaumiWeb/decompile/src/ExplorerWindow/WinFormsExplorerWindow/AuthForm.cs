using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsExplorerWindow;

internal class AuthForm : Form
{
	private IContainer components;

	private Label label1;

	private TextBox _User;

	private TextBox _Database;

	private Label label2;

	private TextBox _Password;

	private Label label3;

	private TextBox _ReportId;

	private Label label4;

	private Button _BtnOk;

	internal int ReportId { get; private set; }

	internal string Database { get; private set; }

	internal string Password { get; private set; }

	internal string UserName { get; private set; }

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
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		label1 = new Label();
		_User = new TextBox();
		_Database = new TextBox();
		label2 = new Label();
		_Password = new TextBox();
		label3 = new Label();
		_ReportId = new TextBox();
		label4 = new Label();
		_BtnOk = new Button();
		((Control)this).SuspendLayout();
		((Control)label1).AutoSize = true;
		((Control)label1).Location = new Point(12, 18);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(32, 13);
		((Control)label1).TabIndex = 0;
		((Control)label1).Text = "User:";
		((Control)_User).Location = new Point(71, 15);
		((Control)_User).Name = "_User";
		((Control)_User).Size = new Size(214, 20);
		((Control)_User).TabIndex = 1;
		((Control)_Database).Location = new Point(71, 67);
		((Control)_Database).Name = "_Database";
		((Control)_Database).Size = new Size(214, 20);
		((Control)_Database).TabIndex = 3;
		((Control)label2).AutoSize = true;
		((Control)label2).Location = new Point(12, 70);
		((Control)label2).Name = "label2";
		((Control)label2).Size = new Size(53, 13);
		((Control)label2).TabIndex = 2;
		((Control)label2).Text = "Database";
		((Control)_Password).Location = new Point(71, 41);
		((Control)_Password).Name = "_Password";
		((Control)_Password).Size = new Size(214, 20);
		((Control)_Password).TabIndex = 2;
		_Password.UseSystemPasswordChar = true;
		((Control)label3).AutoSize = true;
		((Control)label3).Location = new Point(12, 44);
		((Control)label3).Name = "label3";
		((Control)label3).Size = new Size(53, 13);
		((Control)label3).TabIndex = 4;
		((Control)label3).Text = "Password";
		((Control)_ReportId).Location = new Point(71, 93);
		((Control)_ReportId).Name = "_ReportId";
		((Control)_ReportId).Size = new Size(214, 20);
		((Control)_ReportId).TabIndex = 4;
		((Control)label4).AutoSize = true;
		((Control)label4).Location = new Point(12, 96);
		((Control)label4).Name = "label4";
		((Control)label4).Size = new Size(53, 13);
		((Control)label4).TabIndex = 6;
		((Control)label4).Text = "Report ID";
		((Control)_BtnOk).Location = new Point(210, 119);
		((Control)_BtnOk).Name = "_BtnOk";
		((Control)_BtnOk).Size = new Size(75, 23);
		((Control)_BtnOk).TabIndex = 5;
		((Control)_BtnOk).Text = "OK";
		((ButtonBase)_BtnOk).UseVisualStyleBackColor = true;
		((Control)_BtnOk).Click += BtnOkClick;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(292, 148);
		((Control)this).Controls.Add((Control)(object)_BtnOk);
		((Control)this).Controls.Add((Control)(object)_ReportId);
		((Control)this).Controls.Add((Control)(object)label4);
		((Control)this).Controls.Add((Control)(object)_Password);
		((Control)this).Controls.Add((Control)(object)label3);
		((Control)this).Controls.Add((Control)(object)_Database);
		((Control)this).Controls.Add((Control)(object)label2);
		((Control)this).Controls.Add((Control)(object)_User);
		((Control)this).Controls.Add((Control)(object)label1);
		((Form)this).FormBorderStyle = (FormBorderStyle)5;
		((Control)this).Name = "AuthForm";
		((Control)this).Text = "Авторизация";
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}

	public AuthForm()
	{
		InitializeComponent();
	}

	private void BtnOkClick(object sender, EventArgs e)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		int result = -1;
		bool flag = int.TryParse(((Control)_ReportId).Text, out result);
		if (string.IsNullOrEmpty(((Control)_Database).Text.Trim()) || string.IsNullOrEmpty(((Control)_User).Text.Trim()))
		{
			MessageBox.Show("Проверьте корректность введенный данных", "Ошибка", (MessageBoxButtons)0, (MessageBoxIcon)16);
			return;
		}
		if (!flag)
		{
			MessageBox.Show("Проверьте правильность идентификатора отчета", "Ошибка", (MessageBoxButtons)0, (MessageBoxIcon)16);
			return;
		}
		UserName = ((Control)_User).Text.Trim();
		Password = ((Control)_Password).Text;
		Database = ((Control)_Database).Text;
		ReportId = result;
		((Form)this).DialogResult = (DialogResult)1;
	}
}
