using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InflexCore;

namespace WinFormsExplorerWindow.PrimitiveControls.RestrictionsPanelControls;

public class RestrictionsPopupPanel : UserControl
{
	private readonly object _DisposeSync;

	private IContainer components;

	private Label _CaptionLabel;

	private Panel _ButtonsPanel;

	private Panel _CheckBoxesPanel;

	private LinkLabel _CancelButton;

	private LinkLabel _ApplyButton;

	private List<RestrictionColumnCheckBox> RestrictColumnsCheckBoxes { get; set; }

	public int CheckBoxesCount => RestrictColumnsCheckBoxes.Count;

	private IInflexColumn[] SelectedRestrictionColumns => (from restrictionColumn in RestrictColumnsCheckBoxes
		where ((CheckBox)restrictionColumn).Checked
		select restrictionColumn.Column).ToArray();

	private bool IsRestrictionColumnsChanged => RestrictColumnsCheckBoxes.Any((RestrictionColumnCheckBox restrictionColumn) => restrictionColumn.IsCheckedChanged);

	public event Action<int> AppliedRestrictionColumnsCollectionChanged;

	public event Action<IInflexColumn[]> RestrictionColumnsSelectionChanged;

	public event Action PopupShouldClosing;

	public RestrictionsPopupPanel()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		_DisposeSync = new object();
		((UserControl)this)._002Ector();
		InitializeComponent();
		RestrictColumnsCheckBoxes = new List<RestrictionColumnCheckBox>();
		_ApplyButton.LinkClicked += new LinkLabelLinkClickedEventHandler(ApplyButtonOnLinkClicked);
		_CancelButton.LinkClicked += new LinkLabelLinkClickedEventHandler(CancelButtonOnLinkClicked);
	}

	public void AddRestrictionColumn(IInflexColumn column, bool isChecked)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).SuspendLayout();
		RestrictionColumnCheckBox restrictionColumnCheckBox = new RestrictionColumnCheckBox(column, isChecked);
		((Control)restrictionColumnCheckBox).AutoSize = true;
		((Control)restrictionColumnCheckBox).Margin = new Padding(2);
		((Control)restrictionColumnCheckBox).BackColor = Color.FromArgb(22, 82, 142);
		((Control)restrictionColumnCheckBox).ForeColor = Color.White;
		((Control)restrictionColumnCheckBox).TabStop = false;
		RestrictionColumnCheckBox restrictionColumnCheckBox2 = restrictionColumnCheckBox;
		((Control)restrictionColumnCheckBox2).Left = 10;
		RestrictionColumnCheckBox restrictionColumnCheckBox3 = RestrictColumnsCheckBoxes.LastOrDefault();
		if (restrictionColumnCheckBox3 != null)
		{
			((Control)restrictionColumnCheckBox2).Top = ((Control)restrictionColumnCheckBox3).Top + ((Control)restrictionColumnCheckBox3).Height + 2;
		}
		((Control)_CheckBoxesPanel).Controls.Add((Control)(object)restrictionColumnCheckBox2);
		((Control)this).ResumeLayout();
		RestrictColumnsCheckBoxes.Add(restrictionColumnCheckBox2);
		FireAppliedRestrictionColumnsCollectionChanged(SelectedRestrictionColumns.Length);
	}

	public void ClearRestrictionColumns()
	{
		((Control)this).SuspendLayout();
		((Control)_CheckBoxesPanel).Controls.Clear();
		((Control)this).ResumeLayout();
		RestrictColumnsCheckBoxes.Clear();
		FireAppliedRestrictionColumnsCollectionChanged(0);
	}

	private void FireAppliedRestrictionColumnsCollectionChanged(int columnsCount)
	{
		AppliedRestrictionColumnsCollectionChanged?.Invoke(columnsCount);
	}

	private void FireRestrictionColumnsSelectionChanged(IInflexColumn[] columns)
	{
		RestrictionColumnsSelectionChanged?.Invoke(columns);
	}

	private void FirePopupShouldClosing()
	{
		PopupShouldClosing?.Invoke();
	}

	public void UncheckRestrictionColumn(IInflexColumn column)
	{
		if (RestrictColumnsCheckBoxes == null)
		{
			return;
		}
		foreach (RestrictionColumnCheckBox restrictColumnsCheckBox in RestrictColumnsCheckBoxes)
		{
			if (restrictColumnsCheckBox.Column == column)
			{
				((CheckBox)restrictColumnsCheckBox).Checked = false;
				break;
			}
		}
		FireRestrictionColumnsSelectionChanged(SelectedRestrictionColumns);
	}

	public void UncheckAllRestrictionColumns()
	{
		if (RestrictColumnsCheckBoxes == null)
		{
			return;
		}
		foreach (RestrictionColumnCheckBox restrictColumnsCheckBox in RestrictColumnsCheckBoxes)
		{
			((CheckBox)restrictColumnsCheckBox).Checked = false;
		}
		FireRestrictionColumnsSelectionChanged(SelectedRestrictionColumns);
	}

	public void RestoreRestrictionColumns()
	{
		if (RestrictColumnsCheckBoxes == null || !IsRestrictionColumnsChanged)
		{
			return;
		}
		foreach (RestrictionColumnCheckBox restrictColumnsCheckBox in RestrictColumnsCheckBoxes)
		{
			restrictColumnsCheckBox.RestoreIsChecked();
		}
	}

	private void ApplyButtonOnLinkClicked(object sender, EventArgs e)
	{
		FirePopupShouldClosing();
		if (RestrictColumnsCheckBoxes == null || !IsRestrictionColumnsChanged)
		{
			return;
		}
		foreach (RestrictionColumnCheckBox restrictColumnsCheckBox in RestrictColumnsCheckBoxes)
		{
			restrictColumnsCheckBox.KeepIsChecked();
		}
		FireRestrictionColumnsSelectionChanged(SelectedRestrictionColumns);
	}

	private void CancelButtonOnLinkClicked(object sender, EventArgs e)
	{
		FirePopupShouldClosing();
		RestoreRestrictionColumns();
	}

	private void InnerDispose(bool disposing)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		lock (_DisposeSync)
		{
			if (!disposing)
			{
				return;
			}
			List<RestrictionColumnCheckBox> restrictColumnsCheckBoxes = RestrictColumnsCheckBoxes;
			if (restrictColumnsCheckBoxes != null)
			{
				foreach (RestrictionColumnCheckBox item in restrictColumnsCheckBoxes)
				{
					((Component)(object)item).Dispose();
				}
				restrictColumnsCheckBoxes.Clear();
			}
			RestrictColumnsCheckBoxes = null;
			LinkLabel applyButton = _ApplyButton;
			if (applyButton != null)
			{
				applyButton.LinkClicked -= new LinkLabelLinkClickedEventHandler(ApplyButtonOnLinkClicked);
			}
			LinkLabel cancelButton = _CancelButton;
			if (cancelButton != null)
			{
				cancelButton.LinkClicked -= new LinkLabelLinkClickedEventHandler(CancelButtonOnLinkClicked);
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
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Expected O, but got Unknown
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Expected O, but got Unknown
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		_CaptionLabel = new Label();
		_ButtonsPanel = new Panel();
		_CancelButton = new LinkLabel();
		_ApplyButton = new LinkLabel();
		_CheckBoxesPanel = new Panel();
		((Control)_ButtonsPanel).SuspendLayout();
		((Control)this).SuspendLayout();
		((Control)_CaptionLabel).AutoSize = true;
		((Control)_CaptionLabel).Font = new Font("Verdana", 8.25f, (FontStyle)0, (GraphicsUnit)3, (byte)204);
		((Control)_CaptionLabel).ForeColor = Color.White;
		((Control)_CaptionLabel).Location = new Point(3, 5);
		((Control)_CaptionLabel).Margin = new Padding(3);
		((Control)_CaptionLabel).Name = "_CaptionLabel";
		((Control)_CaptionLabel).Size = new Size(147, 13);
		((Control)_CaptionLabel).TabIndex = 2;
		((Control)_CaptionLabel).Text = "Установка ограничений";
		_CaptionLabel.UseMnemonic = false;
		_ButtonsPanel.AutoSizeMode = (AutoSizeMode)0;
		((Control)_ButtonsPanel).Controls.Add((Control)(object)_CancelButton);
		((Control)_ButtonsPanel).Controls.Add((Control)(object)_ApplyButton);
		((Control)_ButtonsPanel).Dock = (DockStyle)2;
		((Control)_ButtonsPanel).Location = new Point(0, 25);
		((Control)_ButtonsPanel).Margin = new Padding(0);
		((Control)_ButtonsPanel).Name = "_ButtonsPanel";
		((Control)_ButtonsPanel).Size = new Size(153, 30);
		((Control)_ButtonsPanel).TabIndex = 3;
		_CancelButton.ActiveLinkColor = Color.FromArgb(224, 224, 224);
		((Control)_CancelButton).Anchor = (AnchorStyles)10;
		((Control)_CancelButton).AutoSize = true;
		((Control)_CancelButton).BackColor = Color.Transparent;
		((Control)_CancelButton).Font = new Font("Verdana", 8.25f, (FontStyle)0, (GraphicsUnit)3, (byte)204);
		_CancelButton.LinkBehavior = (LinkBehavior)2;
		_CancelButton.LinkColor = Color.White;
		((Control)_CancelButton).Location = new Point(18, 8);
		((Control)_CancelButton).Margin = new Padding(4, 2, 4, 2);
		((Control)_CancelButton).Name = "_CancelButton";
		((Control)_CancelButton).Size = new Size(49, 13);
		((Control)_CancelButton).TabIndex = 5;
		((Label)_CancelButton).TabStop = true;
		((Control)_CancelButton).Text = "Отмена";
		_CancelButton.VisitedLinkColor = Color.White;
		_ApplyButton.ActiveLinkColor = Color.FromArgb(224, 224, 224);
		((Control)_ApplyButton).Anchor = (AnchorStyles)10;
		((Control)_ApplyButton).AutoSize = true;
		((Control)_ApplyButton).BackColor = Color.Transparent;
		((Control)_ApplyButton).Font = new Font("Verdana", 8.25f, (FontStyle)0, (GraphicsUnit)3, (byte)204);
		_ApplyButton.LinkBehavior = (LinkBehavior)2;
		_ApplyButton.LinkColor = Color.White;
		((Control)_ApplyButton).Location = new Point(79, 8);
		((Control)_ApplyButton).Margin = new Padding(4, 2, 4, 2);
		((Control)_ApplyButton).Name = "_ApplyButton";
		((Control)_ApplyButton).Size = new Size(69, 13);
		((Control)_ApplyButton).TabIndex = 6;
		((Label)_ApplyButton).TabStop = true;
		((Control)_ApplyButton).Text = "Применить";
		_ApplyButton.VisitedLinkColor = Color.White;
		((Control)_CheckBoxesPanel).AutoSize = true;
		_CheckBoxesPanel.AutoSizeMode = (AutoSizeMode)0;
		((Control)_CheckBoxesPanel).Location = new Point(0, 25);
		((Control)_CheckBoxesPanel).Margin = new Padding(3, 3, 3, 30);
		((Control)_CheckBoxesPanel).Name = "_CheckBoxesPanel";
		((Control)_CheckBoxesPanel).Size = new Size(0, 0);
		((Control)_CheckBoxesPanel).TabIndex = 4;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).AutoSize = true;
		((UserControl)this).AutoSizeMode = (AutoSizeMode)0;
		((Control)this).BackColor = Color.FromArgb(22, 82, 142);
		((Control)this).Controls.Add((Control)(object)_CheckBoxesPanel);
		((Control)this).Controls.Add((Control)(object)_CaptionLabel);
		((Control)this).Controls.Add((Control)(object)_ButtonsPanel);
		((Control)this).Margin = new Padding(0);
		((Control)this).MinimumSize = new Size(144, 50);
		((Control)this).Name = "RestrictionsPopupPanel";
		((Control)this).Size = new Size(153, 55);
		((Control)_ButtonsPanel).ResumeLayout(false);
		((Control)_ButtonsPanel).PerformLayout();
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
