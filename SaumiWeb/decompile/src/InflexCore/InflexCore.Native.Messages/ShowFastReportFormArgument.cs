namespace InflexCore.Native.Messages;

public class ShowFastReportFormArgument : MessageArgument
{
	public int FormID { get; set; }

	public int[] Items { get; set; }

	public bool Preview { get; set; }

	static ShowFastReportFormArgument()
	{
		MessageHelper.RegisterMessageArgumentType("CDEA0F5B-31E2-429a-A7EE-00C2A90213FC", typeof(ShowFastReportFormArgument));
	}
}
