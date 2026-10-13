using InflexCore.Native.Messages;

namespace ExplorerWindowBase.Messages;

public class InitDataWindowArgument : MessageArgument
{
	public int DataType { get; set; }

	static InitDataWindowArgument()
	{
		MessageHelper.RegisterMessageArgumentType("A04CA19B-0ED6-43AD-BB52-19B963E6B9D0", typeof(InitDataWindowArgument));
	}
}
