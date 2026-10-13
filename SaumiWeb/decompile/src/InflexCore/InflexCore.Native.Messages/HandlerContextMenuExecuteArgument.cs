namespace InflexCore.Native.Messages;

public class HandlerContextMenuExecuteArgument : MessageArgument
{
	public int DataType { get; set; }

	public int MenuIndex { get; set; }

	public int ParentMenuIndex { get; set; }

	public int MenuTag { get; set; }

	public ItemDataCollection ItemDatas { get; set; }

	public int Level { get; set; }

	static HandlerContextMenuExecuteArgument()
	{
		MessageHelper.RegisterMessageArgumentType("9023A33C-11CA-4b1c-AA28-9CDCED02A7BC", typeof(HandlerContextMenuExecuteArgument));
	}
}
