namespace InflexCore.Native.Messages;

public class FindResultArgument : MessageAnswerArgument
{
	public string Sql { get; set; }

	public string Xml { get; set; }

	static FindResultArgument()
	{
		MessageHelper.RegisterMessageArgumentType("8B13B78F-2B76-4904-BB6D-428AD6FA9B27", typeof(FindResultArgument));
	}
}
