using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using InflexUtils.PrimitiveControls;
using WinFormsExplorerWindow.Properties;

namespace WinFormsExplorerWindow.PrimitiveControls.WindowTitle;

public class ControlBox : UserControl
{
	private IContainer components;

	private ImageButton _BtnMinimize;

	private ImageButton _BtnMaximize;

	private ImageButton _BtnMaximizeToWindow;

	private ImageButton _BtnClose;

	private readonly object _DisposeSync = new object();

	private Bitmap _BtnCloseActiveImage;

	private Bitmap _BtnCloseInactiveImage;

	private Bitmap _BtnMaximizeActiveImage;

	private Bitmap _BtnMaximizeInactiveImage;

	private Bitmap _BtnMinimizeActiveImage;

	private Bitmap _BtnMinimizeInactiveImage;

	private Bitmap _BtnMaximizeToWindowInactiveImage;

	private Bitmap _BtnMaximizeToWindowActiveImage;

	private ExplorerWindow _AttachedForm;

	private bool _CanClose;

	public ExplorerWindow AttachedForm
	{
		get
		{
			return _AttachedForm;
		}
		set
		{
			ExplorerWindow attachedForm = _AttachedForm;
			if (attachedForm != value)
			{
				if (attachedForm != null)
				{
					attachedForm.WindowStateChanged -= AttachedFormOnWindowStateChanged;
				}
				attachedForm = value;
				if (attachedForm != null)
				{
					attachedForm.WindowStateChanged += AttachedFormOnWindowStateChanged;
				}
				_AttachedForm = attachedForm;
			}
		}
	}

	public bool CanClose
	{
		get
		{
			return _CanClose;
		}
		set
		{
			_CanClose = value;
			ImageButton btnClose = _BtnClose;
			if (btnClose != null)
			{
				((Control)btnClose).Enabled = value;
			}
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
	}

	private void InitializeComponent()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		_BtnClose = new ImageButton();
		_BtnMaximizeToWindow = new ImageButton();
		_BtnMaximize = new ImageButton();
		_BtnMinimize = new ImageButton();
		((ISupportInitialize)_BtnClose).BeginInit();
		((ISupportInitialize)_BtnMaximizeToWindow).BeginInit();
		((ISupportInitialize)_BtnMaximize).BeginInit();
		((ISupportInitialize)_BtnMinimize).BeginInit();
		((Control)this).SuspendLayout();
		_BtnClose.DialogResult = (DialogResult)0;
		_BtnClose.DownImage = (Image)(object)Resources.ClosePressed;
		_BtnClose.HoverImage = (Image)(object)Resources.CloseOver;
		((Control)_BtnClose).Location = new Point(47, 0);
		((Control)_BtnClose).Margin = new Padding(0);
		((Control)_BtnClose).Name = "_BtnClose";
		_BtnClose.NormalImage = (Image)(object)Resources.Close;
		((Control)_BtnClose).Size = new Size(42, 18);
		((PictureBox)_BtnClose).TabIndex = 4;
		((PictureBox)_BtnClose).TabStop = false;
		_BtnMaximizeToWindow.DialogResult = (DialogResult)0;
		_BtnMaximizeToWindow.DownImage = (Image)(object)Resources.MaxToWinPressed;
		_BtnMaximizeToWindow.HoverImage = (Image)(object)Resources.MaxToWinOver;
		((Control)_BtnMaximizeToWindow).Location = new Point(24, 0);
		((Control)_BtnMaximizeToWindow).Margin = new Padding(0);
		((Control)_BtnMaximizeToWindow).Name = "_BtnMaximizeToWindow";
		_BtnMaximizeToWindow.NormalImage = (Image)(object)Resources.MaxToWin;
		((Control)_BtnMaximizeToWindow).Size = new Size(24, 18);
		((PictureBox)_BtnMaximizeToWindow).TabIndex = 3;
		((PictureBox)_BtnMaximizeToWindow).TabStop = false;
		_BtnMaximize.DialogResult = (DialogResult)0;
		_BtnMaximize.DownImage = (Image)(object)Resources.MaximizePressed;
		_BtnMaximize.HoverImage = (Image)(object)Resources.MaximizeOver;
		((Control)_BtnMaximize).Location = new Point(24, 0);
		((Control)_BtnMaximize).Margin = new Padding(0);
		((Control)_BtnMaximize).Name = "_BtnMaximize";
		_BtnMaximize.NormalImage = (Image)(object)Resources.Maximize;
		((Control)_BtnMaximize).Size = new Size(24, 18);
		((PictureBox)_BtnMaximize).TabIndex = 1;
		((PictureBox)_BtnMaximize).TabStop = false;
		_BtnMinimize.DialogResult = (DialogResult)0;
		_BtnMinimize.DownImage = (Image)(object)Resources.MinimizePressed;
		_BtnMinimize.HoverImage = (Image)(object)Resources.MinimizeOver;
		((Control)_BtnMinimize).Location = new Point(0, 0);
		((Control)_BtnMinimize).Margin = new Padding(0);
		((Control)_BtnMinimize).Name = "_BtnMinimize";
		_BtnMinimize.NormalImage = (Image)(object)Resources.Minimize;
		((Control)_BtnMinimize).Size = new Size(25, 18);
		((PictureBox)_BtnMinimize).TabIndex = 0;
		((PictureBox)_BtnMinimize).TabStop = false;
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)0;
		((Control)this).AutoSize = true;
		((UserControl)this).AutoSizeMode = (AutoSizeMode)0;
		((Control)this).BackColor = Color.Transparent;
		((Control)this).Controls.Add((Control)(object)_BtnClose);
		((Control)this).Controls.Add((Control)(object)_BtnMaximizeToWindow);
		((Control)this).Controls.Add((Control)(object)_BtnMaximize);
		((Control)this).Controls.Add((Control)(object)_BtnMinimize);
		((Control)this).Margin = new Padding(0);
		((Control)this).Name = "ControlBox";
		((Control)this).Size = new Size(89, 18);
		((ISupportInitialize)_BtnClose).EndInit();
		((ISupportInitialize)_BtnMaximizeToWindow).EndInit();
		((ISupportInitialize)_BtnMaximize).EndInit();
		((ISupportInitialize)_BtnMinimize).EndInit();
		((Control)this).ResumeLayout(false);
	}

	public ControlBox()
	{
		InitializeComponent();
		Image normalImage = _BtnClose.NormalImage;
		_BtnCloseActiveImage = (Bitmap)(object)((normalImage is Bitmap) ? normalImage : null);
		object? obj = Resources.ResourceManager.GetObject("CloseInactive", Resources.Culture);
		_BtnCloseInactiveImage = (Bitmap)((obj is Bitmap) ? obj : null);
		Image normalImage2 = _BtnMaximize.NormalImage;
		_BtnMaximizeActiveImage = (Bitmap)(object)((normalImage2 is Bitmap) ? normalImage2 : null);
		object? obj2 = Resources.ResourceManager.GetObject("MaximizeInactive", Resources.Culture);
		_BtnMaximizeInactiveImage = (Bitmap)((obj2 is Bitmap) ? obj2 : null);
		Image normalImage3 = _BtnMinimize.NormalImage;
		_BtnMinimizeActiveImage = (Bitmap)(object)((normalImage3 is Bitmap) ? normalImage3 : null);
		object? obj3 = Resources.ResourceManager.GetObject("MinimizeInactive", Resources.Culture);
		_BtnMinimizeInactiveImage = (Bitmap)((obj3 is Bitmap) ? obj3 : null);
		Image normalImage4 = _BtnMaximizeToWindow.NormalImage;
		_BtnMaximizeToWindowActiveImage = (Bitmap)(object)((normalImage4 is Bitmap) ? normalImage4 : null);
		object? obj4 = Resources.ResourceManager.GetObject("MaxToWinInactive", Resources.Culture);
		_BtnMaximizeToWindowInactiveImage = (Bitmap)((obj4 is Bitmap) ? obj4 : null);
		ApplyNormalState();
		((Control)_BtnClose).Click += BtnCloseClick;
		((Control)_BtnMinimize).Click += BtnMinimizeClick;
		((Control)_BtnMaximizeToWindow).Click += BtnMaximizeToWindowClick;
		((Control)_BtnMaximize).Click += BtnMaximizeClick;
	}

	public void SetActiveStyle()
	{
		_BtnClose.NormalImage = (Image)(object)_BtnCloseActiveImage;
		_BtnMaximize.NormalImage = (Image)(object)_BtnMaximizeActiveImage;
		_BtnMinimize.NormalImage = (Image)(object)_BtnMinimizeActiveImage;
		_BtnMaximizeToWindow.NormalImage = (Image)(object)_BtnMaximizeToWindowActiveImage;
	}

	public void SetInactiveStyle()
	{
		_BtnClose.NormalImage = (Image)(object)_BtnCloseInactiveImage;
		_BtnMaximize.NormalImage = (Image)(object)_BtnMaximizeInactiveImage;
		_BtnMinimize.NormalImage = (Image)(object)_BtnMinimizeInactiveImage;
		_BtnMaximizeToWindow.NormalImage = (Image)(object)_BtnMaximizeToWindowInactiveImage;
	}

	private void ApplyNormalState()
	{
		((Control)_BtnClose).Visible = true;
		((Control)_BtnMaximize).Visible = true;
		((Control)_BtnMinimize).Visible = true;
		((Control)_BtnMaximizeToWindow).Visible = false;
	}

	private void ApplyMaximizeState()
	{
		((Control)_BtnClose).Visible = true;
		((Control)_BtnMaximize).Visible = false;
		((Control)_BtnMinimize).Visible = true;
		((Control)_BtnMaximizeToWindow).Visible = true;
	}

	private void BtnMaximizeClick(object sender, EventArgs e)
	{
		if (AttachedForm != null)
		{
			((Form)AttachedForm).WindowState = (FormWindowState)2;
		}
	}

	private void BtnMaximizeToWindowClick(object sender, EventArgs e)
	{
		if (AttachedForm != null)
		{
			((Form)AttachedForm).WindowState = (FormWindowState)0;
		}
	}

	private void BtnMinimizeClick(object sender, EventArgs e)
	{
		if (AttachedForm != null)
		{
			((Form)AttachedForm).WindowState = (FormWindowState)1;
		}
	}

	private void BtnCloseClick(object sender, EventArgs e)
	{
		if (AttachedForm != null)
		{
			((Form)AttachedForm).Close();
		}
	}

	private void AttachedFormOnWindowStateChanged(FormWindowState windowState)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected I4, but got Unknown
		switch ((int)windowState)
		{
		case 0:
			ApplyNormalState();
			break;
		case 2:
			ApplyMaximizeState();
			break;
		case 1:
			break;
		}
	}

	private void InnerDispose(bool disposing)
	{
		lock (_DisposeSync)
		{
			if (disposing)
			{
				((Control)_BtnClose).Click -= BtnCloseClick;
				((Control)_BtnMinimize).Click -= BtnMinimizeClick;
				((Control)_BtnMaximizeToWindow).Click -= BtnMaximizeToWindowClick;
				((Control)_BtnMaximize).Click -= BtnMaximizeClick;
				_BtnCloseActiveImage = null;
				_BtnCloseInactiveImage = null;
				_BtnMinimizeActiveImage = null;
				_BtnMinimizeInactiveImage = null;
				_BtnMaximizeToWindowActiveImage = null;
				_BtnMaximizeToWindowInactiveImage = null;
				_BtnMaximizeActiveImage = null;
				_BtnMaximizeInactiveImage = null;
				AttachedForm = null;
			}
		}
	}
}
