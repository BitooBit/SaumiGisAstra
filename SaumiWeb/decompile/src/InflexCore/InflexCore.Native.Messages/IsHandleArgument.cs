namespace InflexCore.Native.Messages;

public class IsHandleArgument : MessageArgument
{
	public int DataType { get; set; }

	public string HandleName { get; set; }

	static IsHandleArgument()
	{
		MessageHelper.RegisterMessageArgumentType("2FC7053B-9F8A-434d-A4CC-2992FA991D08", typeof(IsHandleArgument));
	}
}
