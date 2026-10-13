namespace InflexCore.Native.Messages;

public class DeleteItemResultArgument : MessageAnswerArgument
{
	public bool DeleteResult { get; set; }

	static DeleteItemResultArgument()
	{
		MessageHelper.RegisterMessageArgumentType("5BF844C9-C680-4914-89E9-02A929D2AFC7", typeof(DeleteItemResultArgument));
	}
}
