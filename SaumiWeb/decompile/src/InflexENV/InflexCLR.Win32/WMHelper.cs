using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InflexCLR.Win32;

public static class WMHelper
{
	public const int WM_COPYDATA = 74;

	[DllImport("user32.dll")]
	private static extern int SendMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

	[DllImport("user32.dll")]
	private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

	public static int Send(ref Message msg)
	{
		return SendMessage(((Message)(ref msg)).HWnd, ((Message)(ref msg)).Msg, ((Message)(ref msg)).WParam, ((Message)(ref msg)).LParam);
	}

	public static Message CreateCopyDataMessage(IntPtr FromHandle, IntPtr ToHandle, string Text, int dwData, out IntPtr ptr)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		COPYDATASTRUCT cOPYDATASTRUCT = default(COPYDATASTRUCT);
		Text += "\0";
		cOPYDATASTRUCT.dwData = (IntPtr)dwData;
		cOPYDATASTRUCT.lpData = Marshal.StringToHGlobalAnsi(Text);
		cOPYDATASTRUCT.cbData = Text.Length;
		ptr = Marshal.AllocHGlobal(Marshal.SizeOf((object)cOPYDATASTRUCT));
		Marshal.StructureToPtr((object)cOPYDATASTRUCT, ptr, false);
		return Message.Create(ToHandle, 74, FromHandle, ptr);
	}
}
