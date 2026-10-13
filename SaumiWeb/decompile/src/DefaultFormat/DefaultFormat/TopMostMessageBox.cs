using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DefaultFormat;

public static class TopMostMessageBox
{
	public static DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		Form val = new Form();
		val.Size = new Size(1, 1);
		val.ShowInTaskbar = false;
		((Control)val).Visible = false;
		val.StartPosition = (FormStartPosition)0;
		Rectangle virtualScreen = SystemInformation.VirtualScreen;
		val.Location = new Point(virtualScreen.Bottom + 10, virtualScreen.Right + 10);
		((Control)val).Show();
		((Control)val).Focus();
		((Control)val).BringToFront();
		val.TopMost = true;
		DialogResult result = MessageBox.Show((IWin32Window)(object)val, message, title, buttons, icon);
		((Component)(object)val).Dispose();
		return result;
	}
}
