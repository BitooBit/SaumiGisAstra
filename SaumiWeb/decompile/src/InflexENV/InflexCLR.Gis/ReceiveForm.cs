using System.Runtime.InteropServices;
using System.Windows.Forms;
using InflexCLR.Win32;

namespace InflexCLR.Gis;

internal class ReceiveForm : Form
{
	protected override void WndProc(ref Message m)
	{
		if (((Message)(ref m)).Msg == 74)
		{
			COPYDATASTRUCT cOPYDATASTRUCT = default(COPYDATASTRUCT);
			string mess = Marshal.PtrToStringAnsi(((COPYDATASTRUCT)Marshal.PtrToStructure(((Message)(ref m)).LParam, typeof(COPYDATASTRUCT))).lpData);
			Gis_GW.AddReceiveMessage(mess);
		}
		((Form)this).WndProc(ref m);
	}
}
