using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using InflexCore;

namespace InflexUtils.ParamsResolver;

public class ParamsSqlResolverForm : Form
{
	private class ExtTextBox : TextBox
	{
		public object DataContext { get; set; }
	}

	private IContainer components;

	private TableLayoutPanel tableLayoutPanel1;

	private Label label1;

	private Panel _ButtonsPanel;

	private Button button1;

	private Button button2;

	private TableLayoutPanel _ParamsPanel;

	protected override void OnSizeChanged(EventArgs e)
	{
		((Control)_ParamsPanel).Refresh();
		((Control)this).OnSizeChanged(e);
	}

	public ParamsSqlResolverForm(IEnumerable<ParamsSqlHandler.SqlVariable> variables)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Expected O, but got Unknown
		((Form)this)._002Ector();
		InitializeComponent();
		_ParamsPanel.RowCount = 0;
		((TableLayoutStyleCollection)_ParamsPanel.RowStyles).Clear();
		int num = -1;
		foreach (ParamsSqlHandler.SqlVariable variable in variables)
		{
			if (string.IsNullOrEmpty(variable.Caption))
			{
				continue;
			}
			num++;
			TableLayoutPanel paramsPanel = _ParamsPanel;
			TableLayoutRowStyleCollection rowStyles = paramsPanel.RowStyles;
			RowStyle val = new RowStyle();
			((TableLayoutStyle)val).SizeType = (SizeType)0;
			rowStyles.Add(val);
			Label val2 = new Label();
			((Control)val2).AutoSize = true;
			((Control)val2).Tag = variable;
			((Control)val2).Text = variable.Caption;
			((Control)val2).MaximumSize = new Size(150, int.MaxValue);
			((Control)val2).Anchor = (AnchorStyles)4;
			((ControlCollection)paramsPanel.Controls).Add((Control)(object)val2);
			paramsPanel.SetColumn((Control)(object)val2, 0);
			paramsPanel.SetRow((Control)(object)val2, num);
			Control val3 = null;
			if (variable.IsDate)
			{
				DateTimePicker val4 = new DateTimePicker();
				((Control)val4).Tag = variable;
				val4.Format = (DateTimePickerFormat)2;
				DateTimePicker val5 = val4;
				val5.ValueChanged += OnSelectedDateChanged;
				DateTime result;
				if (string.IsNullOrEmpty(variable.DefaultValue))
				{
					val5.Value = DateTime.Today;
				}
				else if (DateTime.TryParse(variable.DefaultValue, out result))
				{
					val5.Value = result.Date;
				}
				((Control)val5).MaximumSize = new Size(85, 21);
				val3 = (Control)(object)val5;
			}
			if (variable.IsInteger || variable.IsNumber || variable.IsString)
			{
				ExtTextBox extTextBox = new ExtTextBox
				{
					DataContext = variable
				};
				((Control)extTextBox).TextChanged += OnTextChanged;
				if (!string.IsNullOrEmpty(variable.DefaultValue))
				{
					((Control)extTextBox).Text = variable.DefaultValue;
				}
				val3 = (Control)(object)extTextBox;
			}
			if (variable.IsList)
			{
				ComboBox val6 = new ComboBox();
				((Control)val6).Tag = variable;
				ComboBox val7 = val6;
				((ListControl)val7).SelectedValueChanged += OnSelectionChanged;
				foreach (ParamsSqlHandler.ListValue listValue in variable.ListValues)
				{
					int selectedIndex = val7.Items.Add((object)listValue);
					if (!string.IsNullOrEmpty(variable.DefaultValue) && variable.DefaultValue == listValue.Value)
					{
						((ListControl)val7).SelectedIndex = selectedIndex;
					}
				}
				val3 = (Control)(object)val7;
			}
			if (val3 != null)
			{
				val3.AutoSize = true;
				val3.Anchor = (AnchorStyles)4;
			}
			((ControlCollection)paramsPanel.Controls).Add(val3);
			paramsPanel.SetColumn(val3, 1);
			paramsPanel.SetRow(val3, num);
		}
	}

	private void OnTextChanged(object sender, EventArgs eventArgs)
	{
		ExtTextBox extTextBox = (ExtTextBox)sender;
		ParamsSqlHandler.SqlVariable sqlVariable = (ParamsSqlHandler.SqlVariable)extTextBox.DataContext;
		int selectionStart = ((TextBoxBase)extTextBox).SelectionStart;
		if (sqlVariable.IsInteger)
		{
			if (!int.TryParse(((Control)extTextBox).Text, out var result))
			{
				if (((Control)extTextBox).Tag is string && int.TryParse((string)((Control)extTextBox).Tag, out result))
				{
					((Control)extTextBox).Text = (string)((Control)extTextBox).Tag;
					((TextBoxBase)extTextBox).SelectionStart = selectionStart;
				}
				else
				{
					((Control)extTextBox).Text = 0.ToString();
					((TextBoxBase)extTextBox).SelectionStart = selectionStart;
				}
				return;
			}
			((Control)extTextBox).Tag = ((Control)extTextBox).Text;
		}
		if (sqlVariable.IsNumber)
		{
			if (!decimal.TryParse(((Control)extTextBox).Text, out var result2))
			{
				if (((Control)extTextBox).Tag is string && decimal.TryParse((string)((Control)extTextBox).Tag, out result2))
				{
					((Control)extTextBox).Text = (string)((Control)extTextBox).Tag;
					((TextBoxBase)extTextBox).SelectionStart = selectionStart;
				}
				else
				{
					((Control)extTextBox).Text = 0m.ToString();
					((TextBoxBase)extTextBox).SelectionStart = selectionStart;
				}
				return;
			}
			((Control)extTextBox).Tag = ((Control)extTextBox).Text;
		}
		sqlVariable.Value.Value = ((Control)extTextBox).Text;
	}

	private void OnSelectionChanged(object sender, EventArgs eventArgs)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		ComboBox val = (ComboBox)sender;
		ParamsSqlHandler.SqlVariable sqlVariable = (ParamsSqlHandler.SqlVariable)((Control)val).Tag;
		if (val.SelectedItem is ParamsSqlHandler.ListValue listValue)
		{
			sqlVariable.Value.Value = listValue.Value;
		}
	}

	private void OnSelectedDateChanged(object sender, EventArgs eventArgs)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		DateTimePicker val = (DateTimePicker)sender;
		ParamsSqlHandler.SqlVariable sqlVariable = (ParamsSqlHandler.SqlVariable)((Control)val).Tag;
		sqlVariable.Value.Value = val.Value.ToShortDateString();
	}

	private void OnOkButtonClick(object sender, EventArgs e)
	{
		((Form)this).DialogResult = (DialogResult)1;
	}

	private void OnCancelButtonClick(object sender, EventArgs e)
	{
		((Form)this).DialogResult = (DialogResult)2;
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
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected O, but got Unknown
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Expected O, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Expected O, but got Unknown
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Expected O, but got Unknown
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Expected O, but got Unknown
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Expected O, but got Unknown
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Expected O, but got Unknown
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0536: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ParamsSqlResolverForm));
		tableLayoutPanel1 = new TableLayoutPanel();
		label1 = new Label();
		_ButtonsPanel = new Panel();
		button2 = new Button();
		button1 = new Button();
		_ParamsPanel = new TableLayoutPanel();
		((Control)tableLayoutPanel1).SuspendLayout();
		((Control)_ButtonsPanel).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)tableLayoutPanel1).AutoSize = true;
		((Panel)tableLayoutPanel1).AutoSizeMode = (AutoSizeMode)0;
		tableLayoutPanel1.ColumnCount = 1;
		tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle((SizeType)2, 100f));
		tableLayoutPanel1.Controls.Add((Control)(object)label1, 0, 0);
		tableLayoutPanel1.Controls.Add((Control)(object)_ButtonsPanel, 0, 2);
		tableLayoutPanel1.Controls.Add((Control)(object)_ParamsPanel, 0, 1);
		((Control)tableLayoutPanel1).Dock = (DockStyle)5;
		((Control)tableLayoutPanel1).Location = new Point(0, 0);
		((Control)tableLayoutPanel1).Name = "tableLayoutPanel1";
		tableLayoutPanel1.RowCount = 3;
		tableLayoutPanel1.RowStyles.Add(new RowStyle((SizeType)1, 30f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle((SizeType)2, 100f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle((SizeType)1, 45f));
		((Control)tableLayoutPanel1).Size = new Size(249, 255);
		((Control)tableLayoutPanel1).TabIndex = 0;
		((Control)label1).Anchor = (AnchorStyles)4;
		((Control)label1).AutoSize = true;
		((Control)label1).Font = new Font("Microsoft Sans Serif", 8.25f, (FontStyle)1, (GraphicsUnit)3, (byte)204);
		((Control)label1).Location = new Point(10, 8);
		((Control)label1).Margin = new Padding(10, 0, 10, 0);
		((Control)label1).Name = "label1";
		((Control)label1).Size = new Size(186, 13);
		((Control)label1).TabIndex = 0;
		((Control)label1).Text = "Укажите параметры запроса:";
		((Control)_ButtonsPanel).AutoSize = true;
		_ButtonsPanel.AutoSizeMode = (AutoSizeMode)0;
		((Control)_ButtonsPanel).Controls.Add((Control)(object)button2);
		((Control)_ButtonsPanel).Controls.Add((Control)(object)button1);
		((Control)_ButtonsPanel).Dock = (DockStyle)5;
		((Control)_ButtonsPanel).Location = new Point(3, 213);
		((Control)_ButtonsPanel).Name = "_ButtonsPanel";
		((Control)_ButtonsPanel).Size = new Size(243, 39);
		((Control)_ButtonsPanel).TabIndex = 1;
		((Control)button2).Anchor = (AnchorStyles)9;
		((Control)button2).Location = new Point(78, 8);
		((Control)button2).Name = "button2";
		((Control)button2).RightToLeft = (RightToLeft)0;
		((Control)button2).Size = new Size(75, 23);
		((Control)button2).TabIndex = 1;
		((Control)button2).Text = "ОК";
		((ButtonBase)button2).UseVisualStyleBackColor = true;
		((Control)button2).Click += OnOkButtonClick;
		((Control)button1).Anchor = (AnchorStyles)9;
		((Control)button1).Location = new Point(159, 8);
		((Control)button1).Name = "button1";
		((Control)button1).RightToLeft = (RightToLeft)0;
		((Control)button1).Size = new Size(75, 23);
		((Control)button1).TabIndex = 0;
		((Control)button1).Text = "Отмена";
		((ButtonBase)button1).UseVisualStyleBackColor = true;
		((Control)button1).Click += OnCancelButtonClick;
		((ScrollableControl)_ParamsPanel).AutoScroll = true;
		((Control)_ParamsPanel).AutoSize = true;
		((Panel)_ParamsPanel).AutoSizeMode = (AutoSizeMode)0;
		_ParamsPanel.ColumnCount = 2;
		_ParamsPanel.ColumnStyles.Add(new ColumnStyle());
		_ParamsPanel.ColumnStyles.Add(new ColumnStyle());
		((Control)_ParamsPanel).Dock = (DockStyle)1;
		((Control)_ParamsPanel).Location = new Point(3, 33);
		((Control)_ParamsPanel).Name = "_ParamsPanel";
		_ParamsPanel.RowCount = 1;
		_ParamsPanel.RowStyles.Add(new RowStyle((SizeType)2, 100f));
		((Control)_ParamsPanel).Size = new Size(243, 0);
		((Control)_ParamsPanel).TabIndex = 2;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(249, 255);
		((Control)this).Controls.Add((Control)(object)tableLayoutPanel1);
		((Form)this).Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
		((Form)this).MaximizeBox = false;
		((Form)this).MinimizeBox = false;
		((Control)this).Name = "ParamsSqlResolverForm";
		((Form)this).StartPosition = (FormStartPosition)1;
		((Control)this).Text = "Параметры запроса";
		((Control)tableLayoutPanel1).ResumeLayout(false);
		((Control)tableLayoutPanel1).PerformLayout();
		((Control)_ButtonsPanel).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
