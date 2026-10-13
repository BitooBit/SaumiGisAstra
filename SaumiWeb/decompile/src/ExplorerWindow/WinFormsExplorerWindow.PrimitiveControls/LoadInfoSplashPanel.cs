using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InflexCore;
using InflexCore.Action;
using WinFormsExplorerWindow.Actions;
using WinFormsExplorerWindow.Properties;

namespace WinFormsExplorerWindow.PrimitiveControls;

public class LoadInfoSplashPanel : UserControl
{
	private IContainer components;

	private Panel panel1;

	private LinkLabel _StopLink;

	private Label _MessageLabel;

	private PictureBox _AnimationPictureBox;

	private Label _ProgressLabel;

	private StopAction _StopAction;

	private readonly object _DisposeSync = new object();

	public IActionListAccessor ActionsProvider { get; set; }

	private StopAction StopAction
	{
		get
		{
			if (_StopAction == null)
			{
				IActionListAccessor actionsProvider = ActionsProvider;
				if (actionsProvider == null)
				{
					return null;
				}
				ActionCollection actions = actionsProvider.GetActions();
				if (actions == null || actions.Count == 0)
				{
					return null;
				}
				InflexAction inflexAction = actions.FirstOrDefault((InflexAction a) => a is StopAction);
				_StopAction = inflexAction as StopAction;
			}
			return _StopAction;
		}
	}

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
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Expected O, but got Unknown
		panel1 = new Panel();
		_ProgressLabel = new Label();
		_StopLink = new LinkLabel();
		_MessageLabel = new Label();
		_AnimationPictureBox = new PictureBox();
		((Control)panel1).SuspendLayout();
		((ISupportInitialize)_AnimationPictureBox).BeginInit();
		((Control)this).SuspendLayout();
		((Control)panel1).Anchor = (AnchorStyles)0;
		((Control)panel1).BackColor = Color.White;
		((Control)panel1).Controls.Add((Control)(object)_ProgressLabel);
		((Control)panel1).Controls.Add((Control)(object)_StopLink);
		((Control)panel1).Controls.Add((Control)(object)_MessageLabel);
		((Control)panel1).Controls.Add((Control)(object)_AnimationPictureBox);
		((Control)panel1).Location = new Point(37, 40);
		((Control)panel1).Name = "panel1";
		((Control)panel1).Size = new Size(456, 231);
		((Control)panel1).TabIndex = 3;
		((Control)_ProgressLabel).BackColor = Color.Transparent;
		((Control)_ProgressLabel).Font = new Font("Verdana", 7f, (FontStyle)0, (GraphicsUnit)3, (byte)204);
		((Control)_ProgressLabel).ForeColor = Color.Gray;
		((Control)_ProgressLabel).Location = new Point(100, 108);
		((Control)_ProgressLabel).Margin = new Padding(0);
		((Control)_ProgressLabel).Name = "_ProgressLabel";
		((Control)_ProgressLabel).Size = new Size(19, 13);
		((Control)_ProgressLabel).TabIndex = 3;
		_ProgressLabel.TextAlign = (ContentAlignment)32;
		((Control)_StopLink).AutoSize = true;
		((Control)_StopLink).Location = new Point(141, 116);
		((Control)_StopLink).Name = "_StopLink";
		((Control)_StopLink).Size = new Size(94, 13);
		((Control)_StopLink).TabIndex = 2;
		((Label)_StopLink).TabStop = true;
		((Control)_StopLink).Text = "Остановить - Esc";
		((Control)_MessageLabel).AutoSize = true;
		((Control)_MessageLabel).Font = new Font("Verdana", 9f, (FontStyle)1, (GraphicsUnit)3, (byte)204);
		((Control)_MessageLabel).Location = new Point(141, 95);
		((Control)_MessageLabel).Name = "_MessageLabel";
		((Control)_MessageLabel).Size = new Size(230, 14);
		((Control)_MessageLabel).TabIndex = 1;
		((Control)_MessageLabel).Text = "Выполняется загрузка данных...";
		_AnimationPictureBox.Image = (Image)(object)Resources.LoadInfoAnimation;
		((Control)_AnimationPictureBox).Location = new Point(85, 90);
		((Control)_AnimationPictureBox).Name = "pictureBox1";
		((Control)_AnimationPictureBox).Size = new Size(49, 49);
		_AnimationPictureBox.TabIndex = 0;
		_AnimationPictureBox.TabStop = false;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Control)this).BackColor = Color.White;
		((Control)this).Controls.Add((Control)(object)panel1);
		((Control)this).Name = "LoadInfoSplashPanel";
		((Control)this).Size = new Size(530, 311);
		((Control)panel1).ResumeLayout(false);
		((Control)panel1).PerformLayout();
		((ISupportInitialize)_AnimationPictureBox).EndInit();
		((Control)this).ResumeLayout(false);
	}

	public LoadInfoSplashPanel()
	{
		InitializeComponent();
		((Control)_StopLink).Click += OnStopUpdateClick;
	}

	private void OnStopUpdateClick(object sender, EventArgs e)
	{
		StopAction stopAction = StopAction;
		if (stopAction != null && stopAction.IsAllowed == true)
		{
			stopAction.Execute();
		}
	}

	public void SetSplashMessage(string message, bool canStop)
	{
		((Control)_MessageLabel).Text = message;
		((Control)_StopLink).Visible = canStop;
		((Control)_ProgressLabel).Text = string.Empty;
	}

	private void InnerDispose(bool disposing)
	{
		lock (_DisposeSync)
		{
			if (disposing)
			{
				ActionsProvider = null;
				_StopAction = null;
				if (_StopLink != null)
				{
					((Control)_StopLink).Click -= OnStopUpdateClick;
				}
				if (_AnimationPictureBox != null)
				{
					_AnimationPictureBox.Image = null;
				}
			}
		}
	}

	public void UpdateProgressValue(int percent)
	{
		if (percent >= 0 && percent < 100)
		{
			((Control)_ProgressLabel).Text = percent.ToString();
		}
	}
}
