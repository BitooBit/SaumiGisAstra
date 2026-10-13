using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InflexUtils.PrimitiveControls;

public class DraggablePanel : TableLayoutPanel
{
	private const int WM_NCLBUTTONDOWN = 161;

	private const int HT_CAPTION = 2;

	[DllImport("user32.dll")]
	private static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

	protected override void OnMouseDown(MouseEventArgs e)
	{
		((Control)this).BringToFront();
		((Control)this).Cursor = Cursors.SizeAll;
		ReleaseCapture();
		SendMessage(((Control)this).Handle, 161, 2, 0);
		((Control)this).Cursor = Cursors.Arrow;
		((Control)this).SetBoundsCore(((Control)this).Left, ((Control)this).Top, ((Control)this).Width, ((Control)this).Height, (BoundsSpecified)3);
		((Control)this).OnMouseDown(e);
	}

	protected override void OnLocationChanged(EventArgs e)
	{
		((Control)this).Invalidate(true);
		((Control)this).OnLocationChanged(e);
	}
}
