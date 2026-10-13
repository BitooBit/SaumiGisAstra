using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace InflexUtils.PrimitiveControls.Popup;

[CLSCompliant(true)]
[ToolboxItem(false)]
public class Popup : ToolStripDropDown
{
	private IContainer components;

	private ToolStripControlHost _host;

	private Control _opener;

	private Popup _ownerPopup;

	private Popup _childPopup;

	private bool _resizableTop;

	private bool _resizableLeft;

	private bool _isChildPopupOpened;

	private bool _resizable;

	private bool _nonInteractive;

	private VisualStyleRenderer _sizeGripRenderer;

	public Control Content { get; private set; }

	public PopupAnimations ShowingAnimation { get; set; }

	public PopupAnimations HidingAnimation { get; set; }

	public int AnimationDuration { get; set; }

	public bool FocusOnOpen { get; set; }

	public bool AcceptAlt { get; set; }

	public bool Resizable
	{
		get
		{
			if (_resizable)
			{
				return !_isChildPopupOpened;
			}
			return false;
		}
		set
		{
			_resizable = value;
		}
	}

	public bool NonInteractive
	{
		get
		{
			return _nonInteractive;
		}
		set
		{
			if (value != _nonInteractive)
			{
				_nonInteractive = value;
				if (((Control)this).IsHandleCreated)
				{
					((Control)this).RecreateHandle();
				}
			}
		}
	}

	public Size MinimumSize { get; set; }

	public Size MaximumSize { get; set; }

	protected override CreateParams CreateParams
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			CreateParams createParams = ((ToolStripDropDown)this).CreateParams;
			createParams.ExStyle |= 0x8000000;
			if (NonInteractive)
			{
				createParams.ExStyle |= 0x800A0;
			}
			return createParams;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (components != null)
			{
				components.Dispose();
			}
			Content = null;
		}
		((ToolStripDropDown)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		components = new Container();
	}

	public Popup(Control content)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		PaintEventHandler val = null;
		((ToolStripDropDown)this)._002Ector();
		Popup popup = this;
		if (content == null)
		{
			throw new ArgumentNullException("content");
		}
		Content = content;
		FocusOnOpen = true;
		AcceptAlt = true;
		ShowingAnimation = PopupAnimations.SystemDefault;
		HidingAnimation = PopupAnimations.None;
		AnimationDuration = 100;
		InitializeComponent();
		((Control)this).AutoSize = false;
		((Control)this).DoubleBuffered = true;
		((Control)this).ResizeRedraw = true;
		_host = new ToolStripControlHost(content);
		ToolStripControlHost host = _host;
		Padding val2 = (((ToolStripItem)_host).Margin = Padding.Empty);
		Padding val3 = (((ToolStripItem)host).Padding = val2);
		Padding padding = (((Control)this).Margin = val3);
		((Control)this).Padding = padding;
		if (NativeMethods.IsRunningOnMono)
		{
			content.Margin = Padding.Empty;
		}
		((Control)this).Size = content.Size;
		if (NativeMethods.IsRunningOnMono)
		{
			((ToolStripItem)_host).Size = content.Size;
		}
		bool tabStop = (content.TabStop = true);
		((ToolStrip)this).TabStop = tabStop;
		content.Location = Point.Empty;
		((ToolStrip)this).Items.Add((ToolStripItem)(object)_host);
		content.SizeChanged += ContentSizeChanged;
		((Component)(object)content).Disposed += delegate
		{
			content = null;
			((Component)(object)popup).Dispose(true);
		};
		content.RegionChanged += delegate
		{
			UpdateRegion();
		};
		Control obj = content;
		if (val == null)
		{
			val = (PaintEventHandler)delegate(object sender, PaintEventArgs e)
			{
				PaintSizeGrip(e);
			};
		}
		obj.Paint += val;
		UpdateRegion();
	}

	private void ContentSizeChanged(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		Control val = (Control)sender;
		((Control)this).ClientSize = new Size(val.Width, val.Height);
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		((ToolStripDropDown)this).OnVisibleChanged(e);
		if (NativeMethods.IsRunningOnMono || (((ToolStripDropDown)this).Visible && ShowingAnimation == PopupAnimations.None) || (!((ToolStripDropDown)this).Visible && HidingAnimation == PopupAnimations.None))
		{
			return;
		}
		NativeMethods.AnimationFlags animationFlags = ((!((ToolStripDropDown)this).Visible) ? NativeMethods.AnimationFlags.Hide : NativeMethods.AnimationFlags.Roll);
		PopupAnimations popupAnimations = (((ToolStripDropDown)this).Visible ? ShowingAnimation : HidingAnimation);
		if (popupAnimations == PopupAnimations.SystemDefault)
		{
			popupAnimations = (SystemInformation.IsMenuAnimationEnabled ? ((!SystemInformation.IsMenuFadeEnabled) ? ((PopupAnimations)(0x40000 | (((ToolStripDropDown)this).Visible ? 4 : 8))) : PopupAnimations.Blend) : PopupAnimations.None);
		}
		if ((popupAnimations & (PopupAnimations.Center | PopupAnimations.Slide | PopupAnimations.Blend | PopupAnimations.Roll)) == 0)
		{
			return;
		}
		if (_resizableTop)
		{
			if ((popupAnimations & PopupAnimations.BottomToTop) != PopupAnimations.None)
			{
				popupAnimations = (popupAnimations & ~PopupAnimations.BottomToTop) | PopupAnimations.TopToBottom;
			}
			else if ((popupAnimations & PopupAnimations.TopToBottom) != PopupAnimations.None)
			{
				popupAnimations = (popupAnimations & ~PopupAnimations.TopToBottom) | PopupAnimations.BottomToTop;
			}
		}
		if (_resizableLeft)
		{
			if ((popupAnimations & PopupAnimations.RightToLeft) != PopupAnimations.None)
			{
				popupAnimations = (popupAnimations & ~PopupAnimations.RightToLeft) | PopupAnimations.LeftToRight;
			}
			else if ((popupAnimations & PopupAnimations.LeftToRight) != PopupAnimations.None)
			{
				popupAnimations = (popupAnimations & ~PopupAnimations.LeftToRight) | PopupAnimations.RightToLeft;
			}
		}
		animationFlags = (NativeMethods.AnimationFlags)((int)animationFlags | (int)((PopupAnimations)0xFFFFF & popupAnimations));
		NativeMethods.SetTopMost((Control)(object)this);
		NativeMethods.AnimateWindow((Control)(object)this, AnimationDuration, animationFlags);
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		if (AcceptAlt && (keyData & 0x40000) == 262144)
		{
			if ((keyData & 0x73) != 115)
			{
				return false;
			}
			((ToolStripDropDown)this).Close();
		}
		bool flag = ((ToolStripDropDown)this).ProcessDialogKey(keyData);
		if (!flag && ((int)keyData == 9 || (int)keyData == 65545))
		{
			bool flag2 = (keyData & 0x10000) == 65536;
			Content.SelectNextControl((Control)null, !flag2, true, true, true);
		}
		return flag;
	}

	protected void UpdateRegion()
	{
		if (((ToolStripDropDown)this).Region != null)
		{
			((ToolStripDropDown)this).Region.Dispose();
			((ToolStripDropDown)this).Region = null;
		}
		if (Content.Region != null)
		{
			((ToolStripDropDown)this).Region = Content.Region.Clone();
		}
	}

	public void Show(Control control)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		Show(control, control.ClientRectangle);
	}

	public void Show(Rectangle area)
	{
		_resizableTop = (_resizableLeft = false);
		Point point = new Point(area.Left, area.Top + area.Height);
		Rectangle workingArea = Screen.FromControl((Control)(object)this).WorkingArea;
		if (point.X + ((Control)this).Size.Width > workingArea.Left + workingArea.Width)
		{
			_resizableLeft = true;
			point.X = workingArea.Left + workingArea.Width - ((Control)this).Size.Width;
		}
		if (point.Y + ((Control)this).Size.Height > workingArea.Top + workingArea.Height)
		{
			_resizableTop = true;
			point.Y -= ((Control)this).Size.Height + area.Height;
		}
		((ToolStripDropDown)this).Show(point, (ToolStripDropDownDirection)3);
	}

	public void Show(Control control, Rectangle area)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		SetOwnerItem(control);
		_resizableTop = (_resizableLeft = false);
		Point point = control.PointToScreen(new Point(area.Left, area.Top + area.Height));
		Rectangle workingArea = Screen.FromControl(control).WorkingArea;
		if (point.X + ((Control)this).Size.Width > workingArea.Left + workingArea.Width)
		{
			_resizableLeft = true;
			point.X = workingArea.Left + workingArea.Width - ((Control)this).Size.Width;
		}
		if (point.Y + ((Control)this).Size.Height > workingArea.Top + workingArea.Height)
		{
			_resizableTop = true;
			point.Y -= ((Control)this).Size.Height + area.Height;
		}
		point = control.PointToClient(point);
		((ToolStripDropDown)this).Show(control, point, (ToolStripDropDownDirection)3);
	}

	private void SetOwnerItem(Control control)
	{
		if (control == null)
		{
			return;
		}
		if (control is Popup)
		{
			Popup popup = (_ownerPopup = control as Popup);
			_ownerPopup._childPopup = this;
			((ToolStripDropDown)this).OwnerItem = ((ToolStrip)popup).Items[0];
			return;
		}
		if (_opener == null)
		{
			_opener = control;
		}
		if (control.Parent != null)
		{
			SetOwnerItem(control.Parent);
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		if (Content != null)
		{
			Content.Size = ((Control)this).Size;
			Content.Location = Point.Empty;
		}
		((Control)this).OnSizeChanged(e);
	}

	protected override void OnLayout(LayoutEventArgs e)
	{
		if (!NativeMethods.IsRunningOnMono)
		{
			((ToolStripDropDown)this).OnLayout(e);
			return;
		}
		Size preferredSize = ((Control)this).GetPreferredSize(Size.Empty);
		if (((Control)this).AutoSize && preferredSize != ((Control)this).Size)
		{
			((Control)this).Size = preferredSize;
		}
		((ToolStrip)this).SetDisplayedItems();
		((ToolStrip)this).OnLayoutCompleted(EventArgs.Empty);
		((Control)this).Invalidate();
	}

	protected override void OnOpening(CancelEventArgs e)
	{
		if (Content.IsDisposed || Content.Disposing)
		{
			e.Cancel = true;
			return;
		}
		UpdateRegion();
		((ToolStripDropDown)this).OnOpening(e);
	}

	protected override void OnOpened(EventArgs e)
	{
		if (_ownerPopup != null)
		{
			_ownerPopup._isChildPopupOpened = true;
		}
		if (FocusOnOpen)
		{
			Content.Focus();
		}
		((ToolStripDropDown)this).OnOpened(e);
	}

	protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
	{
		_opener = null;
		if (_ownerPopup != null)
		{
			_ownerPopup._isChildPopupOpened = false;
		}
		((ToolStripDropDown)this).OnClosed(e);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (!InternalProcessResizing(ref m, contentControl: false))
		{
			((ToolStripDropDown)this).WndProc(ref m);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public bool ProcessResizing(ref Message m)
	{
		return InternalProcessResizing(ref m, contentControl: true);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	private bool InternalProcessResizing(ref Message m, bool contentControl)
	{
		if (((Message)(ref m)).Msg == 134 && ((Message)(ref m)).WParam != IntPtr.Zero && _childPopup != null && ((ToolStripDropDown)_childPopup).Visible)
		{
			((Control)_childPopup).Hide();
		}
		if (!Resizable && !NonInteractive)
		{
			return false;
		}
		if (((Message)(ref m)).Msg == 132)
		{
			return OnNcHitTest(ref m, contentControl);
		}
		if (((Message)(ref m)).Msg == 36)
		{
			return OnGetMinMaxInfo(ref m);
		}
		return false;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	private bool OnGetMinMaxInfo(ref Message m)
	{
		NativeMethods.MINMAXINFO mINMAXINFO = (NativeMethods.MINMAXINFO)Marshal.PtrToStructure(((Message)(ref m)).LParam, typeof(NativeMethods.MINMAXINFO));
		if (!MaximumSize.IsEmpty)
		{
			mINMAXINFO.maxTrackSize = MaximumSize;
		}
		mINMAXINFO.minTrackSize = MinimumSize;
		Marshal.StructureToPtr((object)mINMAXINFO, ((Message)(ref m)).LParam, false);
		return true;
	}

	private bool OnNcHitTest(ref Message m, bool contentControl)
	{
		if (NonInteractive)
		{
			((Message)(ref m)).Result = (IntPtr)(-1);
			return true;
		}
		int x = Cursor.Position.X;
		int y = Cursor.Position.Y;
		Point pt = ((Control)this).PointToClient(new Point(x, y));
		GripBounds gripBounds = new GripBounds(contentControl ? Content.ClientRectangle : ((Control)this).ClientRectangle);
		IntPtr intPtr = new IntPtr(-1);
		if (_resizableTop)
		{
			if (_resizableLeft && gripBounds.TopLeft.Contains(pt))
			{
				((Message)(ref m)).Result = (contentControl ? intPtr : ((IntPtr)13));
				return true;
			}
			if (!_resizableLeft && gripBounds.TopRight.Contains(pt))
			{
				((Message)(ref m)).Result = (contentControl ? intPtr : ((IntPtr)14));
				return true;
			}
			if (gripBounds.Top.Contains(pt))
			{
				((Message)(ref m)).Result = (contentControl ? intPtr : ((IntPtr)12));
				return true;
			}
		}
		else
		{
			if (_resizableLeft && gripBounds.BottomLeft.Contains(pt))
			{
				((Message)(ref m)).Result = (contentControl ? intPtr : ((IntPtr)16));
				return true;
			}
			if (!_resizableLeft && gripBounds.BottomRight.Contains(pt))
			{
				((Message)(ref m)).Result = (contentControl ? intPtr : ((IntPtr)17));
				return true;
			}
			if (gripBounds.Bottom.Contains(pt))
			{
				((Message)(ref m)).Result = (contentControl ? intPtr : ((IntPtr)15));
				return true;
			}
		}
		if (_resizableLeft && gripBounds.Left.Contains(pt))
		{
			((Message)(ref m)).Result = (contentControl ? intPtr : ((IntPtr)10));
			return true;
		}
		if (!_resizableLeft && gripBounds.Right.Contains(pt))
		{
			((Message)(ref m)).Result = (contentControl ? intPtr : ((IntPtr)11));
			return true;
		}
		return false;
	}

	public void PaintSizeGrip(PaintEventArgs e)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		if (e == null || e.Graphics == null || !_resizable)
		{
			return;
		}
		Size clientSize = Content.ClientSize;
		Bitmap val = new Bitmap(16, 16);
		try
		{
			Graphics val2 = Graphics.FromImage((Image)(object)val);
			try
			{
				if (Application.RenderWithVisualStyles)
				{
					if (_sizeGripRenderer == null)
					{
						_sizeGripRenderer = new VisualStyleRenderer(Gripper.Normal);
					}
					_sizeGripRenderer.DrawBackground((IDeviceContext)(object)val2, new Rectangle(0, 0, 16, 16));
				}
				else
				{
					ControlPaint.DrawSizeGrip(val2, Content.BackColor, 0, 0, 16, 16);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			GraphicsState val3 = e.Graphics.Save();
			e.Graphics.ResetTransform();
			if (_resizableTop)
			{
				if (_resizableLeft)
				{
					e.Graphics.RotateTransform(180f);
					e.Graphics.TranslateTransform((float)(-clientSize.Width), (float)(-clientSize.Height));
				}
				else
				{
					e.Graphics.ScaleTransform(1f, -1f);
					e.Graphics.TranslateTransform(0f, (float)(-clientSize.Height));
				}
			}
			else if (_resizableLeft)
			{
				e.Graphics.ScaleTransform(-1f, 1f);
				e.Graphics.TranslateTransform((float)(-clientSize.Width), 0f);
			}
			e.Graphics.DrawImage((Image)(object)val, clientSize.Width - 16, clientSize.Height - 16 + 1, 16, 16);
			e.Graphics.Restore(val3);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
