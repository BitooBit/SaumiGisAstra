namespace InflexCore.Native.Messages;

public class ReportTreeSaveArgument : MessageArgument
{
	public string RestrictionSQL { get; set; }

	public int ReportID { get; set; }

	public string ReportName { get; set; }

	static ReportTreeSaveArgument()
	{
		MessageHelper.RegisterMessageArgumentType("4C46A72A-1570-4685-AC33-3EFCBF023908", typeof(ReportTreeSaveArgument));
	}
}
