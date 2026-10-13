using InflexCore.Native.Messages;

namespace ExplorerWindowBase.Messages;

public class DataChangeInflexEventMessageArgument : MessageArgument
{
	public int ItemType { get; set; }

	public int ItemID { get; set; }

	public int ChangeAction { get; set; }

	public int Handle { get; set; }

	static DataChangeInflexEventMessageArgument()
	{
		MessageHelper.RegisterMessageArgumentType("E2BFF9C0-B348-4E97-A5ED-C2477DBC0ED7", typeof(DataChangeInflexEventMessageArgument));
	}
}
