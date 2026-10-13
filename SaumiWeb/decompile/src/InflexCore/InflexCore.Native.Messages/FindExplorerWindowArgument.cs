namespace InflexCore.Native.Messages;

public class FindExplorerWindowArgument : FindEditArgument
{
	public bool UseAnalitic { get; set; }

	static FindExplorerWindowArgument()
	{
		MessageHelper.RegisterMessageArgumentType("D246CAE4-A50E-448e-8D21-9BBADCF1EA74", typeof(FindExplorerWindowArgument));
	}
}
