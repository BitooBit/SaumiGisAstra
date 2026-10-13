using InflexCore.Native.Messages;

namespace ExplorerWindowBase.Messages;

public class InitDataWindowExecuteArgument : MessageArgument
{
	public int Uid { get; set; }

	public int MenuType { get; set; }

	public int[] ItemsID { get; set; }

	static InitDataWindowExecuteArgument()
	{
		MessageHelper.RegisterMessageArgumentType("B172382E-E5D5-48F1-832E-2EE23C8415AF", typeof(InitDataWindowExecuteArgument));
	}
}
