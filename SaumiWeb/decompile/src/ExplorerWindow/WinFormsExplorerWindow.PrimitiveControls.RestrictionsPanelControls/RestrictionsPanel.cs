using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using InflexCore;
using InflexCore.log;
using InflexUtils.PrimitiveControls;
using InflexUtils.PrimitiveControls.Popup;
using WinFormsExplorerWindow.Actions;
using WinFormsExplorerWindow.Properties;

namespace WinFormsExplorerWindow.PrimitiveControls.RestrictionsPanelControls;

public class RestrictionsPanel : UserControl
{
	private IContainer components;

	private LinkLabel _AddQueryButton;

	private LinkLabel _SetRestrictionColumnsButton;

	private ImageButton _ClearAllRestrictionColumnsButton;

	private LinkLabel _RestrictionTextShort;

	private TableLayoutPanel _MainTableLayoutPanel;

	private TableLayoutPanel _RestrictionColumnsTableLayoutPanel;

	private TableLayoutPanel _QueryRestrictionTableLayoutPanel;

	private ImageButton _RestoreQueryButton;

	private IActionListAccessor _ActionsAccessor;

	private Label _RestrictionTextFull;

	private Popup _RestrictionFullTextPopup;

	private RestrictionsPopupPanel _SetRestrictionsPanel;

	private Popup _SetRestrictionsPopup;

	private readonly object _DysposeSync;

	private FieldInfo _LabelShowToolTipFieldInfo;

	public LinkLabel RestrictionTextShort => _RestrictionTextShort;

	public Label RestrictionTextFull => _RestrictionTextFull;

	public RestrictionsPopupPanel SetRestrictionsPanel => _SetRestrictionsPanel;

	private RestoreOriginalQueryAction RestoreOriginalQueryAction => _ActionsAccessor.GetActions()?.Action<RestoreOriginalQueryAction>();

	private EditQueryAction EditQueryAction => _ActionsAccessor.GetActions()?.Action<EditQueryAction>();

	private AddQueryAction AddQueryAction => _ActionsAccessor.GetActions()?.Action<AddQueryAction>();

	private FieldInfo LabelShowToolTipFieldInfo
	{
		get
		{
			if ((object)_LabelShowToolTipFieldInfo == null)
			{
				try
				{
					_LabelShowToolTipFieldInfo = typeof(Label).GetField("showToolTip", BindingFlags.Instance | BindingFlags.NonPublic);
				}
				catch (Exception ex)
				{
					FastLog.Loger(this).Warn((object)"LabelShowToolTipFieldInfo property getter exception. Can't get field \"showToolTip\" from type \"Label\".", ex);
				}
			}
			return _LabelShowToolTipFieldInfo;
		}
	}

	private bool RestrictionTextShortShowToolTip
	{
		get
		{
			FieldInfo labelShowToolTipFieldInfo = LabelShowToolTipFieldInfo;
			if ((object)labelShowToolTipFieldInfo != null)
			{
				object value = labelShowToolTipFieldInfo.GetValue(_RestrictionTextShort);
				return (bool)value;
			}
			return false;
		}
		set
		{
			LabelShowToolTipFieldInfo?.SetValue(_RestrictionTextShort, value);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !((Control)this).IsDisposed)
		{
			if (components != null)
			{
				components.Dispose();
				components = null;
			}
			InnerDispose(disposing);
		}
		((ContainerControl)this).Dispose(disposing);
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
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Expected O, but got Unknown
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Expected O, but got Unknown
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Expected O, but got Unknown
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Expected O, but got Unknown
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Expected O, but got Unknown
		//IL_0544: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Expected O, but got Unknown
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Expected O, but got Unknown
		//IL_05be: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ff: Expected O, but got Unknown
		//IL_067c: Unknown result type (might be due to invalid IL or missing references)
		//IL_072a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0734: Expected O, but got Unknown
		//IL_0746: Unknown result type (might be due to invalid IL or missing references)
		//IL_0750: Expected O, but got Unknown
		//IL_075c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0766: Expected O, but got Unknown
		//IL_07d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_080b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0815: Expected O, but got Unknown
		//IL_088e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0945: Unknown result type (might be due to invalid IL or missing references)
		_SetRestrictionColumnsButton = new LinkLabel();
		_AddQueryButton = new LinkLabel();
		_RestrictionTextShort = new LinkLabel();
		_MainTableLayoutPanel = new TableLayoutPanel();
		_RestrictionColumnsTableLayoutPanel = new TableLayoutPanel();
		_ClearAllRestrictionColumnsButton = new ImageButton();
		_QueryRestrictionTableLayoutPanel = new TableLayoutPanel();
		_RestoreQueryButton = new ImageButton();
		((Control)_MainTableLayoutPanel).SuspendLayout();
		((Control)_RestrictionColumnsTableLayoutPanel).SuspendLayout();
		((ISupportInitialize)_ClearAllRestrictionColumnsButton).BeginInit();
		((Control)_QueryRestrictionTableLayoutPanel).SuspendLayout();
		((ISupportInitialize)_RestoreQueryButton).BeginInit();
		((Control)this).SuspendLayout();
		_SetRestrictionColumnsButton.ActiveLinkColor = Color.FromArgb(224, 224, 224);
		((Control)_SetRestrictionColumnsButton).AutoSize = true;
		((Control)_SetRestrictionColumnsButton).BackColor = Color.Transparent;
		_SetRestrictionColumnsButton.DisabledLinkColor = Color.FromArgb(139, 172, 206);
		((Control)_SetRestrictionColumnsButton).Font = new Font("Verdana", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)204);
		_SetRestrictionColumnsButton.LinkBehavior = (LinkBehavior)2;
		_SetRestrictionColumnsButton.LinkColor = Color.White;
		((Control)_SetRestrictionColumnsButton).Location = new Point(4, 2);
		((Control)_SetRestrictionColumnsButton).Margin = new Padding(4, 2, 0, 2);
		((Control)_SetRestrictionColumnsButton).Name = "_SetRestrictionColumnsButton";
		((Control)_SetRestrictionColumnsButton).Size = new Size(170, 14);
		((Control)_SetRestrictionColumnsButton).TabIndex = 7;
		((Label)_SetRestrictionColumnsButton).TabStop = false;
		((Control)_SetRestrictionColumnsButton).Text = "Установить ограничения";
		_SetRestrictionColumnsButton.VisitedLinkColor = Color.White;
		_AddQueryButton.ActiveLinkColor = Color.FromArgb(224, 224, 224);
		((Control)_AddQueryButton).AutoSize = true;
		((Control)_AddQueryButton).BackColor = Color.Transparent;
		_AddQueryButton.DisabledLinkColor = Color.FromArgb(139, 172, 206);
		((Control)_AddQueryButton).Font = new Font("Verdana", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)204);
		_AddQueryButton.LinkBehavior = (LinkBehavior)2;
		_AddQueryButton.LinkColor = Color.White;
		((Control)_AddQueryButton).Location = new Point(113, 2);
		((Control)_AddQueryButton).Margin = new Padding(4, 2, 4, 2);
		((Control)_AddQueryButton).Name = "_AddQueryButton";
		((Control)_AddQueryButton).Size = new Size(122, 14);
		((Control)_AddQueryButton).TabIndex = 4;
		((Label)_AddQueryButton).TabStop = false;
		((Control)_AddQueryButton).Text = "Добавить данные";
		_AddQueryButton.VisitedLinkColor = Color.White;
		_RestrictionTextShort.ActiveLinkColor = Color.FromArgb(224, 224, 224);
		((Label)_RestrictionTextShort).AutoEllipsis = true;
		((Control)_RestrictionTextShort).AutoSize = true;
		((Control)_RestrictionTextShort).BackColor = Color.Transparent;
		_RestrictionTextShort.DisabledLinkColor = Color.FromArgb(139, 172, 206);
		((Control)_RestrictionTextShort).Font = new Font("Verdana", 9f, (FontStyle)0, (GraphicsUnit)3, (byte)204);
		_RestrictionTextShort.LinkBehavior = (LinkBehavior)1;
		_RestrictionTextShort.LinkColor = Color.FromArgb(255, 255, 180);
		((Control)_RestrictionTextShort).Location = new Point(23, 2);
		((Control)_RestrictionTextShort).Margin = new Padding(4, 2, 4, 2);
		((Control)_RestrictionTextShort).Name = "_RestrictionTextShort";
		((Control)_RestrictionTextShort).Size = new Size(82, 14);
		((Control)_RestrictionTextShort).TabIndex = 5;
		((Label)_RestrictionTextShort).TabStop = false;
		((Control)_RestrictionTextShort).Text = "Все данные";
		_RestrictionTextShort.VisitedLinkColor = Color.White;
		((Control)_MainTableLayoutPanel).AutoSize = true;
		((Panel)_MainTableLayoutPanel).AutoSizeMode = (AutoSizeMode)0;
		_MainTableLayoutPanel.ColumnCount = 2;
		_MainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle((SizeType)2, 100f));
		_MainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
		_MainTableLayoutPanel.Controls.Add((Control)(object)_RestrictionColumnsTableLayoutPanel, 1, 0);
		_MainTableLayoutPanel.Controls.Add((Control)(object)_QueryRestrictionTableLayoutPanel, 0, 0);
		((Control)_MainTableLayoutPanel).Dock = (DockStyle)5;
		((Control)_MainTableLayoutPanel).Location = new Point(0, 0);
		((Control)_MainTableLayoutPanel).Name = "_MainTableLayoutPanel";
		_MainTableLayoutPanel.RowCount = 1;
		_MainTableLayoutPanel.RowStyles.Add(new RowStyle((SizeType)2, 100f));
		((Control)_MainTableLayoutPanel).Size = new Size(432, 19);
		((Control)_MainTableLayoutPanel).TabIndex = 5;
		((Control)_RestrictionColumnsTableLayoutPanel).AutoSize = true;
		((Panel)_RestrictionColumnsTableLayoutPanel).AutoSizeMode = (AutoSizeMode)0;
		((Control)_RestrictionColumnsTableLayoutPanel).BackColor = Color.Transparent;
		_RestrictionColumnsTableLayoutPanel.ColumnCount = 2;
		_RestrictionColumnsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle((SizeType)2, 100f));
		_RestrictionColumnsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
		_RestrictionColumnsTableLayoutPanel.Controls.Add((Control)(object)_SetRestrictionColumnsButton, 0, 0);
		_RestrictionColumnsTableLayoutPanel.Controls.Add((Control)(object)_ClearAllRestrictionColumnsButton, 1, 0);
		((Control)_RestrictionColumnsTableLayoutPanel).Dock = (DockStyle)5;
		((Control)_RestrictionColumnsTableLayoutPanel).Location = new Point(239, 0);
		((Control)_RestrictionColumnsTableLayoutPanel).Margin = new Padding(0);
		((Control)_RestrictionColumnsTableLayoutPanel).Name = "_RestrictionColumnsTableLayoutPanel";
		_RestrictionColumnsTableLayoutPanel.RowCount = 1;
		_RestrictionColumnsTableLayoutPanel.RowStyles.Add(new RowStyle((SizeType)2, 100f));
		((Control)_RestrictionColumnsTableLayoutPanel).Size = new Size(193, 19);
		((Control)_RestrictionColumnsTableLayoutPanel).TabIndex = 0;
		((Control)_ClearAllRestrictionColumnsButton).BackColor = Color.Transparent;
		_ClearAllRestrictionColumnsButton.DialogResult = (DialogResult)0;
		_ClearAllRestrictionColumnsButton.DownImage = (Image)(object)Resources.RemovePressed;
		_ClearAllRestrictionColumnsButton.HoverImage = (Image)(object)Resources.RemoveOver;
		((Control)_ClearAllRestrictionColumnsButton).Location = new Point(174, 0);
		((Control)_ClearAllRestrictionColumnsButton).Margin = new Padding(0);
		((Control)_ClearAllRestrictionColumnsButton).Name = "_ClearAllRestrictionColumnsButton";
		_ClearAllRestrictionColumnsButton.NormalImage = (Image)(object)Resources.Remove;
		((Control)_ClearAllRestrictionColumnsButton).Size = new Size(19, 19);
		_ClearAllRestrictionColumnsButton.SizeMode = (PictureBoxSizeMode)2;
		((PictureBox)_ClearAllRestrictionColumnsButton).TabIndex = 9;
		((PictureBox)_ClearAllRestrictionColumnsButton).TabStop = false;
		((Control)_ClearAllRestrictionColumnsButton).Visible = false;
		((Control)_QueryRestrictionTableLayoutPanel).AutoSize = true;
		((Panel)_QueryRestrictionTableLayoutPanel).AutoSizeMode = (AutoSizeMode)0;
		((Control)_QueryRestrictionTableLayoutPanel).BackColor = Color.Transparent;
		_QueryRestrictionTableLayoutPanel.ColumnCount = 3;
		_QueryRestrictionTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
		_QueryRestrictionTableLayoutPanel.ColumnStyles.Add(new ColumnStyle((SizeType)2, 100f));
		_QueryRestrictionTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
		_QueryRestrictionTableLayoutPanel.Controls.Add((Control)(object)_RestrictionTextShort, 1, 0);
		_QueryRestrictionTableLayoutPanel.Controls.Add((Control)(object)_RestoreQueryButton, 0, 0);
		_QueryRestrictionTableLayoutPanel.Controls.Add((Control)(object)_AddQueryButton, 2, 0);
		((Control)_QueryRestrictionTableLayoutPanel).Dock = (DockStyle)5;
		((Control)_QueryRestrictionTableLayoutPanel).Location = new Point(0, 0);
		((Control)_QueryRestrictionTableLayoutPanel).Margin = new Padding(0);
		((Control)_QueryRestrictionTableLayoutPanel).Name = "_QueryRestrictionTableLayoutPanel";
		_QueryRestrictionTableLayoutPanel.RowCount = 1;
		_QueryRestrictionTableLayoutPanel.RowStyles.Add(new RowStyle((SizeType)2, 100f));
		((Control)_QueryRestrictionTableLayoutPanel).Size = new Size(239, 19);
		((Control)_QueryRestrictionTableLayoutPanel).TabIndex = 6;
		((Control)_RestoreQueryButton).BackColor = Color.Transparent;
		_RestoreQueryButton.DialogResult = (DialogResult)0;
		_RestoreQueryButton.DownImage = (Image)(object)Resources.RestorePressed;
		_RestoreQueryButton.HoverImage = (Image)(object)Resources.RestoreOver;
		((Control)_RestoreQueryButton).Location = new Point(0, 0);
		((Control)_RestoreQueryButton).Margin = new Padding(0);
		((Control)_RestoreQueryButton).Name = "_RestoreQueryButton";
		_RestoreQueryButton.NormalImage = (Image)(object)Resources.Restore;
		((Control)_RestoreQueryButton).Size = new Size(19, 19);
		_RestoreQueryButton.SizeMode = (PictureBoxSizeMode)2;
		((PictureBox)_RestoreQueryButton).TabIndex = 9;
		((PictureBox)_RestoreQueryButton).TabStop = false;
		((Control)_RestoreQueryButton).Visible = false;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).AutoSize = true;
		((UserControl)this).AutoSizeMode = (AutoSizeMode)0;
		((Control)this).BackColor = Color.Transparent;
		((Control)this).Controls.Add((Control)(object)_MainTableLayoutPanel);
		((Control)this).Margin = new Padding(0);
		((Control)this).Name = "RestrictionsPanel";
		((Control)this).Size = new Size(432, 19);
		((Control)_MainTableLayoutPanel).ResumeLayout(false);
		((Control)_MainTableLayoutPanel).PerformLayout();
		((Control)_RestrictionColumnsTableLayoutPanel).ResumeLayout(false);
		((Control)_RestrictionColumnsTableLayoutPanel).PerformLayout();
		((ISupportInitialize)_ClearAllRestrictionColumnsButton).EndInit();
		((Control)_QueryRestrictionTableLayoutPanel).ResumeLayout(false);
		((Control)_QueryRestrictionTableLayoutPanel).PerformLayout();
		((ISupportInitialize)_RestoreQueryButton).EndInit();
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}

	public RestrictionsPanel(IActionListAccessor actionsAccessor)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		_DysposeSync = new object();
		((UserControl)this)._002Ector();
		InitializeComponent();
		Color disabledLinkColor = Color.FromArgb(139, 172, 206);
		_RestrictionTextShort.DisabledLinkColor = disabledLinkColor;
		_SetRestrictionColumnsButton.DisabledLinkColor = disabledLinkColor;
		_AddQueryButton.DisabledLinkColor = disabledLinkColor;
		_ActionsAccessor = actionsAccessor;
		((Control)_RestoreQueryButton).Click += RestoreQueryButtonOnClick;
		_RestrictionTextShort.LinkClicked += new LinkLabelLinkClickedEventHandler(RestrictionTextShortOnLinkClicked);
		((Control)_RestrictionTextShort).Paint += new PaintEventHandler(RestrictionTextShortOnPaint);
		((Control)_RestrictionTextShort).MouseHover += RestrictionTextShortOnMouseHover;
		((Control)_RestrictionTextShort).MouseLeave += RestrictionTextShortOnMouseLeave;
		_AddQueryButton.LinkClicked += new LinkLabelLinkClickedEventHandler(AddQueryButtonOnLinkClicked);
		_SetRestrictionColumnsButton.LinkClicked += new LinkLabelLinkClickedEventHandler(SetRestrictionColumnsButtonOnLinkClicked);
		((Control)_ClearAllRestrictionColumnsButton).Click += ClearAllRestrictionColumnsButtonOnClick;
		((Control)_MainTableLayoutPanel).Paint += new PaintEventHandler(MainTableLayoutPanelOnPaint);
		Label val = new Label();
		((Control)val).AutoSize = true;
		((Control)val).BackColor = Color.FromArgb(22, 82, 142);
		((Control)val).ForeColor = Color.White;
		((Control)val).Padding = new Padding(2, 4, 2, 2);
		_RestrictionTextFull = val;
		Popup popup = new Popup((Control)(object)_RestrictionTextFull);
		((ToolStripDropDown)popup).AutoClose = false;
		((Control)popup).AutoSize = true;
		popup.FocusOnOpen = false;
		_RestrictionFullTextPopup = popup;
		RestrictionsPopupPanel restrictionsPopupPanel = new RestrictionsPopupPanel();
		((Control)restrictionsPopupPanel).AutoSize = true;
		((UserControl)restrictionsPopupPanel).AutoSizeMode = (AutoSizeMode)0;
		_SetRestrictionsPanel = restrictionsPopupPanel;
		_SetRestrictionsPanel.AppliedRestrictionColumnsCollectionChanged += SetRestrictionsPanelOnAppliedRestrictionColumnsCollectionChanged;
		_SetRestrictionsPanel.PopupShouldClosing += SetRestrictionsPanelOnPopupShouldClosing;
		Popup popup2 = new Popup((Control)(object)_SetRestrictionsPanel);
		((ToolStripDropDown)popup2).AutoClose = true;
		((Control)popup2).AutoSize = true;
		popup2.FocusOnOpen = false;
		_SetRestrictionsPopup = popup2;
	}

	public void SetToolTips(ToolTip toolTip)
	{
		toolTip.SetToolTip((Control)(object)_RestoreQueryButton, "Вернуться к первоначальной выборке");
		toolTip.SetToolTip((Control)(object)_ClearAllRestrictionColumnsButton, "Убрать все установленные ограничения");
	}

	public void SetEnabled(bool isEnabled)
	{
		Action action = delegate
		{
			((Control)_RestoreQueryButton).Enabled = isEnabled;
			_AddQueryButton.Links[0].Enabled = isEnabled;
			_RestrictionTextShort.Links[0].Enabled = isEnabled;
			_SetRestrictionColumnsButton.Links[0].Enabled = isEnabled;
			((Control)_ClearAllRestrictionColumnsButton).Enabled = isEnabled;
		};
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)action);
		}
		else
		{
			action();
		}
	}

	public void UpdateActionButtonsAvailability()
	{
		RestoreOriginalQueryAction restoreQueryAction = RestoreOriginalQueryAction;
		if (restoreQueryAction != null)
		{
			if (((Control)_RestoreQueryButton).InvokeRequired)
			{
				((Control)this).Invoke((Delegate)(Action)delegate
				{
					((Control)_RestoreQueryButton).Visible = restoreQueryAction.IsAllowed == true;
				});
			}
			else
			{
				((Control)_RestoreQueryButton).Visible = restoreQueryAction.IsAllowed == true;
			}
		}
		EditQueryAction editQueryAction = EditQueryAction;
		if (editQueryAction != null)
		{
			if (((Control)_RestrictionTextShort).InvokeRequired)
			{
				((Control)this).Invoke((Delegate)(Action)delegate
				{
					_RestrictionTextShort.Links[0].Enabled = editQueryAction.IsAllowed == true;
				});
			}
			else
			{
				_RestrictionTextShort.Links[0].Enabled = editQueryAction.IsAllowed == true;
			}
		}
		AddQueryAction addQueryAction = AddQueryAction;
		if (addQueryAction != null)
		{
			if (((Control)_AddQueryButton).InvokeRequired)
			{
				((Control)this).Invoke((Delegate)(Action)delegate
				{
					_AddQueryButton.Links[0].Enabled = addQueryAction.IsAllowed == true;
				});
			}
			else
			{
				_AddQueryButton.Links[0].Enabled = addQueryAction.IsAllowed == true;
			}
		}
		RestrictionsPopupPanel restrictionsPopupPanel = _SetRestrictionsPanel;
		if (restrictionsPopupPanel == null)
		{
			return;
		}
		if (((Control)_SetRestrictionColumnsButton).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				_SetRestrictionColumnsButton.Links[0].Enabled = restrictionsPopupPanel.CheckBoxesCount > 0;
			});
		}
		else
		{
			_SetRestrictionColumnsButton.Links[0].Enabled = restrictionsPopupPanel.CheckBoxesCount > 0;
		}
	}

	private void MainTableLayoutPanelOnPaint(object sender, PaintEventArgs e)
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
	}

	private void RestrictionTextShortOnPaint(object sender, PaintEventArgs e)
	{
		if (((Control)_RestrictionTextShort).Width < ((Label)_RestrictionTextShort).PreferredWidth)
		{
			RestrictionTextShortShowToolTip = false;
		}
	}

	private void RestrictionTextShortOnMouseHover(object sender, EventArgs e)
	{
		((ToolStripDropDown)_RestrictionFullTextPopup).Show((Control)(object)_RestrictionTextShort, 0, ((Control)_RestrictionTextShort).Height);
	}

	private void RestrictionTextShortOnMouseLeave(object sender, EventArgs e)
	{
		((ToolStripDropDown)_RestrictionFullTextPopup).Close();
	}

	private void RestrictionTextShortOnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		EditQueryAction editQueryAction = EditQueryAction;
		if (editQueryAction.IsAllowed == true)
		{
			editQueryAction.Execute();
		}
	}

	private void RestoreQueryButtonOnClick(object sender, EventArgs e)
	{
		RestoreOriginalQueryAction restoreOriginalQueryAction = RestoreOriginalQueryAction;
		if (restoreOriginalQueryAction.IsAllowed == true)
		{
			restoreOriginalQueryAction.Execute();
		}
	}

	private void AddQueryButtonOnLinkClicked(object sender, EventArgs e)
	{
		AddQueryAction addQueryAction = AddQueryAction;
		if (addQueryAction.IsAllowed == true)
		{
			addQueryAction.Execute();
		}
	}

	private void SetRestrictionColumnsButtonOnLinkClicked(object sender, EventArgs e)
	{
		_SetRestrictionsPanel.RestoreRestrictionColumns();
		((ToolStripDropDown)_SetRestrictionsPopup).Show((Control)(object)_SetRestrictionColumnsButton, 0, ((Control)_SetRestrictionColumnsButton).Height);
	}

	private void ClearAllRestrictionColumnsButtonOnClick(object sender, EventArgs e)
	{
		_SetRestrictionsPanel.UncheckAllRestrictionColumns();
	}

	private void SetRestrictionsPanelOnAppliedRestrictionColumnsCollectionChanged(int restrictionColumnsCount)
	{
		if (restrictionColumnsCount > 0)
		{
			((Control)_SetRestrictionColumnsButton).Text = "Установлены ограничения (" + restrictionColumnsCount + ")";
			((Control)_ClearAllRestrictionColumnsButton).Visible = true;
		}
		else
		{
			((Control)_SetRestrictionColumnsButton).Text = "Установить ограничения";
			((Control)_ClearAllRestrictionColumnsButton).Visible = false;
		}
	}

	private void SetRestrictionsPanelOnPopupShouldClosing()
	{
		((ToolStripDropDown)_SetRestrictionsPopup).Close();
	}

	private void InnerDispose(bool disposing)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		lock (_DysposeSync)
		{
			if (disposing)
			{
				if (_ActionsAccessor != null)
				{
					_ActionsAccessor = null;
				}
				ImageButton restoreQueryButton = _RestoreQueryButton;
				if (restoreQueryButton != null)
				{
					((Control)restoreQueryButton).Click -= RestoreQueryButtonOnClick;
				}
				LinkLabel restrictionTextShort = _RestrictionTextShort;
				if (restrictionTextShort != null)
				{
					restrictionTextShort.LinkClicked -= new LinkLabelLinkClickedEventHandler(RestrictionTextShortOnLinkClicked);
					((Control)restrictionTextShort).Paint -= new PaintEventHandler(RestrictionTextShortOnPaint);
					((Control)restrictionTextShort).MouseHover -= RestrictionTextShortOnMouseHover;
					((Control)restrictionTextShort).MouseLeave -= RestrictionTextShortOnMouseLeave;
				}
				LinkLabel addQueryButton = _AddQueryButton;
				if (addQueryButton != null)
				{
					addQueryButton.LinkClicked -= new LinkLabelLinkClickedEventHandler(AddQueryButtonOnLinkClicked);
				}
				LinkLabel setRestrictionColumnsButton = _SetRestrictionColumnsButton;
				if (setRestrictionColumnsButton != null)
				{
					setRestrictionColumnsButton.LinkClicked -= new LinkLabelLinkClickedEventHandler(SetRestrictionColumnsButtonOnLinkClicked);
				}
				ImageButton clearAllRestrictionColumnsButton = _ClearAllRestrictionColumnsButton;
				if (clearAllRestrictionColumnsButton != null)
				{
					((Control)clearAllRestrictionColumnsButton).Click -= ClearAllRestrictionColumnsButtonOnClick;
				}
				TableLayoutPanel mainTableLayoutPanel = _MainTableLayoutPanel;
				if (mainTableLayoutPanel != null)
				{
					((Control)mainTableLayoutPanel).Paint -= new PaintEventHandler(MainTableLayoutPanelOnPaint);
				}
				((Component)(object)_RestrictionTextFull)?.Dispose();
				_RestrictionTextFull = null;
				((Component)(object)_RestrictionFullTextPopup)?.Dispose();
				_RestrictionFullTextPopup = null;
				RestrictionsPopupPanel setRestrictionsPanel = _SetRestrictionsPanel;
				if (setRestrictionsPanel != null)
				{
					setRestrictionsPanel.AppliedRestrictionColumnsCollectionChanged -= SetRestrictionsPanelOnAppliedRestrictionColumnsCollectionChanged;
					setRestrictionsPanel.PopupShouldClosing -= SetRestrictionsPanelOnPopupShouldClosing;
					((Component)(object)setRestrictionsPanel).Dispose();
				}
				_SetRestrictionsPanel = null;
				((Component)(object)_SetRestrictionsPopup)?.Dispose();
				_SetRestrictionsPopup = null;
			}
		}
	}
}
