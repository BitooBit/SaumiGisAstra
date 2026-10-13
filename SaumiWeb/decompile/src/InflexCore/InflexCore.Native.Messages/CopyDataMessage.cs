using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InflexCore.Native.Messages;

public class CopyDataMessage : IDisposable
{
	[CompilerGenerated]
	private Message _003CMessage_003Ek__BackingField;

	public Message Message
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CMessage_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CMessage_003Ek__BackingField = value;
		}
	}

	public string Data { get; set; }

	public CopyDataStruct CopyDataStruct { get; set; }

	public IntPtr UnmanagedPointer { get; set; }

	public int ID { get; private set; }

	public void Link(MessageArgument argument)
	{
		ID = argument.ID;
	}

	public void Dispose()
	{
		if (UnmanagedPointer != IntPtr.Zero)
		{
			try
			{
				Marshal.FreeHGlobal(UnmanagedPointer);
			}
			finally
			{
				UnmanagedPointer = IntPtr.Zero;
			}
		}
	}
}
