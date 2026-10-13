using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace InflexUtils.PrimitiveControls;

public class ImageButton : PictureBox, IButtonControl
{
	private const int WM_KEYDOWN = 256;

	private const int WM_KEYUP = 257;

	private DialogResult _DialogResult;

	private Image _HoverImage;

	private Image _DownImage;

	private Image _NormalImage;

	private bool _IsHover;

	private bool _IsDown;

	private bool _IsDefault;

	private bool _HoldingSpace;

	public DialogResult DialogResult
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _DialogResult;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_DialogResult = value;
		}
	}

	[Category("Appearance")]
	[Description("Image to show when the button is hovered over.")]
	public Image HoverImage
	{
		get
		{
			return _HoverImage;
		}
		set
		{
			_HoverImage = value;
			if (_IsHover)
			{
				Image = value;
			}
		}
	}

	[Description("Image to show when the button is depressed.")]
	[Category("Appearance")]
	public Image DownImage
	{
		get
		{
			return _DownImage;
		}
		set
		{
			_DownImage = value;
			if (_IsDown)
			{
				Image = value;
			}
		}
	}

	[Description("Image to show when the button is not in any other state.")]
	[Category("Appearance")]
	public virtual Image NormalImage
	{
		get
		{
			return _NormalImage;
		}
		set
		{
			_NormalImage = value;
			if (!_IsHover && !_IsDown)
			{
				Image = value;
			}
		}
	}

	public bool IsHover => _IsHover;

	public bool IsDown => _IsDown;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	[Description("The text associated with the control.")]
	[Browsable(true)]
	[Category("Appearance")]
	public override string Text
	{
		get
		{
			return ((PictureBox)this).Text;
		}
		set
		{
			((PictureBox)this).Text = value;
		}
	}

	[Browsable(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	[Category("Appearance")]
	[Description("The font used to display text in the control.")]
	public override Font Font
	{
		get
		{
			return ((PictureBox)this).Font;
		}
		set
		{
			((PictureBox)this).Font = value;
		}
	}

	[Description("Controls how the ImageButton will handle image placement and control sizing.")]
	public PictureBoxSizeMode SizeMode
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return ((PictureBox)this).SizeMode;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((PictureBox)this).SizeMode = value;
		}
	}

	[Description("Controls what type of border the ImageButton should have.")]
	public BorderStyle BorderStyle
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return ((PictureBox)this).BorderStyle;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((PictureBox)this).BorderStyle = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Image Image
	{
		get
		{
			return ((PictureBox)this).Image;
		}
		set
		{
			((PictureBox)this).Image = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ImageLayout BackgroundImageLayout
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return ((Control)this).BackgroundImageLayout;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((Control)this).BackgroundImageLayout = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Image BackgroundImage
	{
		get
		{
			return ((Control)this).BackgroundImage;
		}
		set
		{
			((Control)this).BackgroundImage = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string ImageLocation
	{
		get
		{
			return ((PictureBox)this).ImageLocation;
		}
		set
		{
			((PictureBox)this).ImageLocation = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public Image ErrorImage
	{
		get
		{
			return ((PictureBox)this).ErrorImage;
		}
		set
		{
			((PictureBox)this).ErrorImage = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Image InitialImage
	{
		get
		{
			return ((PictureBox)this).InitialImage;
		}
		set
		{
			((PictureBox)this).InitialImage = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool WaitOnLoad
	{
		get
		{
			return ((PictureBox)this).WaitOnLoad;
		}
		set
		{
			((PictureBox)this).WaitOnLoad = value;
		}
	}

	public void NotifyDefault(bool value)
	{
		_IsDefault = value;
	}

	public void PerformClick()
	{
		((Control)this).OnClick(EventArgs.Empty);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		_IsHover = true;
		if (_IsDown)
		{
			if (DownImage != null && Image != DownImage)
			{
				Image = DownImage;
			}
		}
		else if (HoverImage != null)
		{
			Image = HoverImage;
		}
		else
		{
			Image = NormalImage;
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		_IsHover = false;
		Image = NormalImage;
		((Control)this).OnMouseLeave(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		((Control)this).Focus();
		((Control)this).OnMouseUp((MouseEventArgs)null);
		_IsDown = true;
		if (DownImage != null)
		{
			Image = DownImage;
		}
		((Control)this).OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		_IsDown = false;
		if (_IsHover)
		{
			if (HoverImage != null)
			{
				Image = HoverImage;
			}
		}
		else
		{
			Image = NormalImage;
		}
		((Control)this).OnMouseUp(e);
	}

	public override bool PreProcessMessage(ref Message msg)
	{
		if (((Message)(ref msg)).Msg == 257)
		{
			if (_HoldingSpace)
			{
				if ((int)((Message)(ref msg)).WParam == 32)
				{
					((Control)this).OnMouseUp((MouseEventArgs)null);
					PerformClick();
				}
				else if ((int)((Message)(ref msg)).WParam == 27 || (int)((Message)(ref msg)).WParam == 9)
				{
					_HoldingSpace = false;
					((Control)this).OnMouseUp((MouseEventArgs)null);
				}
			}
			return true;
		}
		if (((Message)(ref msg)).Msg == 256)
		{
			if ((int)((Message)(ref msg)).WParam == 32)
			{
				_HoldingSpace = true;
				((Control)this).OnMouseDown((MouseEventArgs)null);
			}
			else if ((int)((Message)(ref msg)).WParam == 13)
			{
				PerformClick();
			}
			return true;
		}
		return ((Control)this).PreProcessMessage(ref msg);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		_HoldingSpace = false;
		((Control)this).OnMouseUp((MouseEventArgs)null);
		((Control)this).OnLostFocus(e);
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		((PictureBox)this).OnPaint(pe);
		if (!string.IsNullOrEmpty(((Control)this).Text) && pe != null && ((PictureBox)this).Font != null)
		{
			SolidBrush val = new SolidBrush(((PictureBox)this).ForeColor);
			SizeF sizeF = pe.Graphics.MeasureString(((PictureBox)this).Text, ((PictureBox)this).Font);
			PointF pointF = ((((PictureBox)this).Image == null) ? new PointF((float)(((Control)this).Width / 2) - sizeF.Width / 2f, (float)(((Control)this).Height / 2) - sizeF.Height / 2f) : new PointF((float)(((PictureBox)this).Image.Width / 2) - sizeF.Width / 2f, (float)(((PictureBox)this).Image.Height / 2) - sizeF.Height / 2f));
			pe.Graphics.DrawString(((PictureBox)this).Text, ((PictureBox)this).Font, (Brush)(object)val, pointF);
		}
	}

	protected override void OnTextChanged(EventArgs e)
	{
		((Control)this).Refresh();
		((Control)this).OnTextChanged(e);
	}
}
