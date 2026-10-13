using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using InflexCore;
using WinFormsExplorerWindow.InflexReportTree;

namespace WinFormsExplorerWindow.PrimitiveControls;

public class InflexReportTreeWindow : Form
{
	private IContainer components;

	private TableLayoutPanel _MainPanel;

	private Panel _ControlsPanel;

	private Button _BtnCancel;

	private Button _BtnOk;

	private CheckBox _RememberAsDefault;

	private QueryColumnCollection _ModifiedQueryColumnCollection;

	private QueryColumnCollection _ResultQueryColumnCollection;

	private QueryColumnCollection _SavedQueryColumnCollection;

	private WinFormsExplorerWindow.InflexReportTree.InflexReportTree _Tree;

	private readonly object _DisposeSync = new object();

	public QueryColumnCollection ResultQueryColumnCollection => _ResultQueryColumnCollection;

	public bool RememberAsDefault
	{
		get
		{
			return _RememberAsDefault.Checked;
		}
		set
		{
			_RememberAsDefault.Checked = value;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		InnerDispose(disposing);
		((Form)this).Dispose(disposing);
		GC.SuppressFinalize(this);
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
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected O, but got Unknown
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(InflexReportTreeWindow));
		_MainPanel = new TableLayoutPanel();
		_ControlsPanel = new Panel();
		_RememberAsDefault = new CheckBox();
		_BtnCancel = new Button();
		_BtnOk = new Button();
		((Control)_MainPanel).SuspendLayout();
		((Control)_ControlsPanel).SuspendLayout();
		((Control)this).SuspendLayout();
		_MainPanel.ColumnCount = 1;
		_MainPanel.ColumnStyles.Add(new ColumnStyle());
		_MainPanel.Controls.Add((Control)(object)_ControlsPanel, 0, 1);
		((Control)_MainPanel).Dock = (DockStyle)5;
		((Control)_MainPanel).Location = new Point(0, 0);
		((Control)_MainPanel).Name = "_MainPanel";
		_MainPanel.RowCount = 2;
		_MainPanel.RowStyles.Add(new RowStyle((SizeType)2, 100f));
		_MainPanel.RowStyles.Add(new RowStyle());
		((Control)_MainPanel).Size = new Size(299, 395);
		((Control)_MainPanel).TabIndex = 0;
		((Control)_ControlsPanel).AutoSize = true;
		_ControlsPanel.AutoSizeMode = (AutoSizeMode)0;
		((Control)_ControlsPanel).Controls.Add((Control)(object)_RememberAsDefault);
		((Control)_ControlsPanel).Controls.Add((Control)(object)_BtnCancel);
		((Control)_ControlsPanel).Controls.Add((Control)(object)_BtnOk);
		((Control)_ControlsPanel).Dock = (DockStyle)5;
		((Control)_ControlsPanel).Location = new Point(0, 332);
		((Control)_ControlsPanel).Margin = new Padding(0);
		((Control)_ControlsPanel).Name = "_ControlsPanel";
		((Control)_ControlsPanel).Padding = new Padding(0, 0, 0, 3);
		((Control)_ControlsPanel).Size = new Size(299, 63);
		((Control)_ControlsPanel).TabIndex = 1;
		((Control)_RememberAsDefault).AutoSize = true;
		((Control)_RememberAsDefault).Location = new Point(29, 9);
		((Control)_RememberAsDefault).Name = "_RememberAsDefault";
		((Control)_RememberAsDefault).Size = new Size(234, 17);
		((Control)_RememberAsDefault).TabIndex = 2;
		((Control)_RememberAsDefault).Text = "Запомнить вид и расположение колонок";
		((ButtonBase)_RememberAsDefault).UseVisualStyleBackColor = true;
		_BtnCancel.DialogResult = (DialogResult)2;
		((Control)_BtnCancel).Location = new Point(153, 34);
		((Control)_BtnCancel).Name = "_BtnCancel";
		((Control)_BtnCancel).Size = new Size(75, 23);
		((Control)_BtnCancel).TabIndex = 1;
		((Control)_BtnCancel).Text = "Отмена";
		((ButtonBase)_BtnCancel).UseVisualStyleBackColor = true;
		_BtnOk.DialogResult = (DialogResult)1;
		((Control)_BtnOk).Location = new Point(69, 34);
		((Control)_BtnOk).Name = "_BtnOk";
		((Control)_BtnOk).Size = new Size(75, 23);
		((Control)_BtnOk).TabIndex = 0;
		((Control)_BtnOk).Text = "OK";
		((ButtonBase)_BtnOk).UseVisualStyleBackColor = true;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(299, 395);
		((Control)this).Controls.Add((Control)(object)_MainPanel);
		((Form)this).Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
		((Form)this).MaximizeBox = false;
		((Form)this).MinimizeBox = false;
		((Control)this).Name = "InflexReportTreeWindow";
		((Form)this).StartPosition = (FormStartPosition)4;
		((Control)this).Text = "Выбрать колонки";
		((Control)_MainPanel).ResumeLayout(false);
		((Control)_MainPanel).PerformLayout();
		((Control)_ControlsPanel).ResumeLayout(false);
		((Control)_ControlsPanel).PerformLayout();
		((Control)this).ResumeLayout(false);
	}

	public InflexReportTreeWindow()
	{
		InitializeComponent();
		_Tree = new WinFormsExplorerWindow.InflexReportTree.InflexReportTree();
		((Control)_Tree).Dock = (DockStyle)5;
		((ControlCollection)_MainPanel.Controls).Add((Control)(object)_Tree);
		_MainPanel.SetRow((Control)(object)_Tree, 0);
	}

	protected override void OnLoad(EventArgs e)
	{
		if (_Tree != null)
		{
			((Control)_Tree).Select();
		}
		((Form)this).OnLoad(e);
	}

	public bool? ShowWindow(IInflexReport report, QueryColumnCollection queryColumns)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		QueryColumnCollection columnCollection = SaveCurrentQueryColumnCollection(queryColumns);
		InitTree(report, ref columnCollection);
		DialogResult val = ((Form)this).ShowDialog();
		bool? flag = (int)val == 1;
		SetResultQueryColumnCollection(flag);
		return flag;
	}

	private void SetResultQueryColumnCollection(bool? result)
	{
		if (result == true)
		{
			_ResultQueryColumnCollection = _ModifiedQueryColumnCollection;
		}
		else if (result == false)
		{
			_ResultQueryColumnCollection = _SavedQueryColumnCollection;
		}
	}

	private QueryColumnCollection SaveCurrentQueryColumnCollection(QueryColumnCollection columnCollection)
	{
		_ModifiedQueryColumnCollection = new QueryColumnCollection();
		if (columnCollection != null)
		{
			_ModifiedQueryColumnCollection.AddRange(columnCollection);
			_SavedQueryColumnCollection = columnCollection;
		}
		return _ModifiedQueryColumnCollection;
	}

	private void InitTree(IInflexReport report, ref QueryColumnCollection columnCollection)
	{
		_Tree.ReportTreeSource = new InflexReportTreeSource(report, columnCollection);
	}

	private void InnerDispose(bool disposing)
	{
		lock (_DisposeSync)
		{
			if (!disposing)
			{
				return;
			}
			_ModifiedQueryColumnCollection = null;
			_ResultQueryColumnCollection = null;
			_SavedQueryColumnCollection = null;
			if (_Tree != null)
			{
				_Tree.ReportTreeSource = null;
				if (_MainPanel != null)
				{
					((ControlCollection)_MainPanel.Controls).Remove((Control)(object)_Tree);
				}
				_Tree = null;
			}
		}
	}
}
