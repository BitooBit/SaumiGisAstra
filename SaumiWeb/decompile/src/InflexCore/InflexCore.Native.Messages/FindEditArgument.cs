namespace InflexCore.Native.Messages;

public class FindEditArgument : FindNewArgument
{
	public string Xml { get; set; }

	static FindEditArgument()
	{
		MessageHelper.RegisterMessageArgumentType("010DFE09-8E8C-4d58-B56C-1D37D00948CA", typeof(FindEditArgument));
	}
}
