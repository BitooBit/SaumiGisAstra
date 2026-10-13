using InflexCore.Native.Messages;

namespace ExplorerWindowBase.Messages;

public class BarCodeInflexEventMessageArgument : MessageArgument
{
	public int Handle { get; set; }

	public int ItemType { get; set; }

	public int ItemID { get; set; }

	public int BarCodeAction { get; set; }

	public string Params { get; set; }

	static BarCodeInflexEventMessageArgument()
	{
		MessageHelper.RegisterMessageArgumentType("50B674D5-27BF-44A1-A9FB-1860AB9B82CC", typeof(BarCodeInflexEventMessageArgument));
	}
}
