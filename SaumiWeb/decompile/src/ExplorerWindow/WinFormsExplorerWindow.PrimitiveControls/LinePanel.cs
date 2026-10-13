using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsExplorerWindow.PrimitiveControls;

public class LinePanel : Panel
{
	private Color _LineColor = Color.White;

	public Color LineColor
	{
		get
		{
			return _LineColor;
		}
		set
		{
			_LineColor = value;
			((Control)this).Refresh();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		if (e.ClipRectangle.Width > 0)
		{
			PointF pointF = new PointF(0f, 1f);
			PointF pointF2 = new PointF(e.ClipRectangle.Width, 1f);
			LinearGradientBrush val = new LinearGradientBrush(pointF, pointF2, LineColor, Color.Transparent);
			try
			{
				e.Graphics.FillRectangle((Brush)(object)val, new RectangleF(0f, 0f, e.ClipRectangle.Width, 1f));
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		((Control)this).OnPaint(e);
	}
}
