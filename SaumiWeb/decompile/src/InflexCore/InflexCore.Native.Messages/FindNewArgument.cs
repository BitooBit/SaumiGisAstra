namespace InflexCore.Native.Messages;

public class FindNewArgument : MessageArgument
{
	public int ReportID { get; set; }

	public int FindID { get; set; }

	public bool IsAsync { get; set; }

	static FindNewArgument()
	{
		MessageHelper.RegisterMessageArgumentType("F7366D13-98E1-4629-9DEA-410C9D2DAFFA", typeof(FindNewArgument));
	}
}
