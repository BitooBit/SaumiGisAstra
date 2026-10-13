using System;
using System.Windows.Forms;

namespace InflexCore.Native.Messages;

public class InflexPing
{
	private Random _Random = new Random();

	public bool Ping(int handle)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Message m = Message.Create((IntPtr)handle, MessageHelper.IWM_PING, (IntPtr)_Random.Next(), (IntPtr)_Random.Next());
		int num = CalcPingCode(ref m);
		int num2 = MessageHelper.SendWindowMessage(ref m);
		return num2 == num;
	}

	private int CalcPingCode(ref Message m)
	{
		return ((Message)(ref m)).WParam.ToInt32() ^ ((Message)(ref m)).LParam.ToInt32();
	}
}
