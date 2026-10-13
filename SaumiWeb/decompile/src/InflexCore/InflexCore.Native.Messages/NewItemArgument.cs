namespace InflexCore.Native.Messages;

public class NewItemArgument : MessageArgument
{
	public decimal DataSubType { get; set; }

	public decimal DataType { get; set; }

	static NewItemArgument()
	{
		MessageHelper.RegisterMessageArgumentType("CCF36AB6-3365-4295-AAB8-6828677A5311", typeof(NewItemArgument));
	}
}
