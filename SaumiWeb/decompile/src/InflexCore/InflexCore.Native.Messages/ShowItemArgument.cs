namespace InflexCore.Native.Messages;

public class ShowItemArgument : MessageArgument
{
	public ItemData ItemData { get; set; }

	static ShowItemArgument()
	{
		MessageHelper.RegisterMessageArgumentType("EBF8C7A4-9B9F-4b8c-9A5A-D150853790F7", typeof(ShowItemArgument));
	}
}
