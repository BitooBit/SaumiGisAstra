namespace InflexCore.Native.Messages;

public class ShowOuterReferenceArgument : MessageArgument
{
	public int ReportID { get; set; }

	public int ColumnID { get; set; }

	public int[] Items { get; set; }

	public string Caption { get; set; }

	public int DataType { get; set; }

	static ShowOuterReferenceArgument()
	{
		MessageHelper.RegisterMessageArgumentType("930238DA-EB02-4045-B4BB-F2CF932F2382", typeof(ShowOuterReferenceArgument));
	}
}
