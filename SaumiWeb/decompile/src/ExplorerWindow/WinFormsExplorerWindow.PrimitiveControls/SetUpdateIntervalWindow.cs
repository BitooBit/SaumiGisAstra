using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WinFormsExplorerWindow.Actions;

namespace WinFormsExplorerWindow.PrimitiveControls;

public class SetUpdateIntervalWindow : Form
{
	private IContainer components;

	private TextBox _TextValue;

	private Label label1;

	private Button _BtnOK;

	private Button _BtnCancel;

	private PeriodicalRefreshAction _RefreshAction;

	public PeriodicalRefreshAction RefreshAction
	{
		get
		{
			return _RefreshAction;
		}
		set
		{
			_RefreshAction = value;
			if (_RefreshAction != null)
			{
				((Control)_TextValue).Text = (_RefreshAction.Interval / 60).ToString();
			}
		}
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
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(SetUpdateIntervalWindow));
		_TextValue = new TextBox();
		label1 = new Label();
		_BtnOK = new Button();
		_BtnCancel = new Button();
		((Control)this).SuspendLayout();
		((Control)_TextValue).Location = new Point(12, 12);
		((Control)_TextValue).Name = "_TextValue";
		((Control)_TextValue).Size = new Size(232, 20);
		((Control)_TextValue).TabIndex = 0;
		((Control)label1).AutoSize = true;
		((Control)label1).Location = new Point(250, 15);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(30, 13);
		((Control)label1).TabIndex = 1;
		((Control)label1).Text = "мин.";
		((Control)_BtnOK).Location = new Point(124, 45);
		((Control)_BtnOK).Name = "_BtnOK";
		((Control)_BtnOK).Size = new Size(75, 23);
		((Control)_BtnOK).TabIndex = 2;
		((Control)_BtnOK).Text = "OK";
		((ButtonBase)_BtnOK).UseVisualStyleBackColor = true;
		((Control)_BtnCancel).Location = new Point(205, 45);
		((Control)_BtnCancel).Name = "_BtnCancel";
		((Control)_BtnCancel).Size = new Size(75, 23);
		((Control)_BtnCancel).TabIndex = 3;
		((Control)_BtnCancel).Text = "Отмена";
		((ButtonBase)_BtnCancel).UseVisualStyleBackColor = true;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(292, 77);
		((Control)this).Controls.Add((Control)(object)_BtnCancel);
		((Control)this).Controls.Add((Control)(object)_BtnOK);
		((Control)this).Controls.Add((Control)(object)label1);
		((Control)this).Controls.Add((Control)(object)_TextValue);
		((Form)this).FormBorderStyle = (FormBorderStyle)5;
		((Form)this).Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
		((Control)this).Name = "SetUpdateIntervalWindow";
		((Form)this).StartPosition = (FormStartPosition)4;
		((Control)this).Text = "Установите интервал обновления";
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}

	public SetUpdateIntervalWindow()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		((Form)this)._002Ector();
		InitializeComponent();
		((Control)_TextValue).KeyDown += new KeyEventHandler(TextValueKeyDown);
		((Control)_BtnOK).Click += BtnOKClick;
		((Control)_BtnCancel).Click += BtnCancelClick;
	}

	private bool CheckValue(string textValue)
	{
		if (string.IsNullOrEmpty(textValue))
		{
			return false;
		}
		try
		{
			int num = Convert.ToInt32(textValue);
			return num > 0;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private void ApplyValue(string textValue)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckValue(textValue))
		{
			MessageBox.Show("Введите корректное значение!", "Ошибка", (MessageBoxButtons)0, (MessageBoxIcon)16);
			return;
		}
		int interval = Convert.ToInt32(textValue) * 60;
		RefreshAction.Interval = interval;
		if (RefreshAction.IsAllowed == true)
		{
			RefreshAction.Execute();
		}
		((Form)this).DialogResult = (DialogResult)1;
	}

	private void TextValueKeyDown(object sender, KeyEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)e.KeyCode == 13)
		{
			ApplyValue(((Control)_TextValue).Text);
		}
	}

	private void BtnOKClick(object sender, EventArgs e)
	{
		ApplyValue(((Control)_TextValue).Text);
	}

	private void BtnCancelClick(object sender, EventArgs e)
	{
		((Form)this).DialogResult = (DialogResult)2;
	}
}
