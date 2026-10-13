namespace InflexCore.Native.Messages;

public class DeleteItemArgument : MessageArgument
{
	public ItemData ItemData { get; set; }

	static DeleteItemArgument()
	{
		MessageHelper.RegisterMessageArgumentType("D3263C6A-933B-486d-ACA1-571356053BC1", typeof(DeleteItemArgument));
	}
}
