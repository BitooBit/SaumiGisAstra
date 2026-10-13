using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsExplorerWindow;

internal class StatusBarRenderer : ToolStripSystemRenderer
{
	private readonly Color _MenuImageMarginBackground = Color.FromArgb(25, 75, 125);

	private readonly Color _BorderColor = Color.FromArgb(118, 118, 118);

	protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		ToolStripItem item = e.Item;
		ToolStripDropDownItem val = (ToolStripDropDownItem)(object)((item is ToolStripDropDownItem) ? item : null);
		if (val == null)
		{
			return;
		}
		Rectangle rectangle = new Rectangle(Point.Empty, ((ToolStripItem)val).Size);
		rectangle.Width--;
		rectangle.Height--;
		if (((ToolStripItem)val).Pressed)
		{
			SolidBrush val2 = new SolidBrush(_MenuImageMarginBackground);
			try
			{
				e.Graphics.FillRectangle((Brush)(object)val2, rectangle);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		if (((ToolStripItem)val).Selected)
		{
			Pen val3 = new Pen(_BorderColor);
			try
			{
				e.Graphics.DrawRectangle(val3, rectangle);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
	}
}
