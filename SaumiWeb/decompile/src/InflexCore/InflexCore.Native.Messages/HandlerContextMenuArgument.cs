namespace InflexCore.Native.Messages;

public class HandlerContextMenuArgument : MessageArgument
{
	public int DataType { get; set; }

	static HandlerContextMenuArgument()
	{
		MessageHelper.RegisterMessageArgumentType("B0FCF9AB-BEDE-44c7-BB0F-5B6DC7A07AE6", typeof(HandlerContextMenuArgument));
	}
}
