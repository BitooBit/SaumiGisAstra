using System;
using System.Runtime.InteropServices;

namespace InflexCore.Native.Messages;

[Serializable]
[ComVisible(true)]
public abstract class MessageArgument
{
	private static int _LastID;

	public int ID { get; set; }

	private static int NextID()
	{
		_LastID++;
		return _LastID;
	}

	public MessageArgument()
	{
		ID = NextID();
	}
}
