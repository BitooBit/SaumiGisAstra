namespace InflexCore.Native.Messages;

public class IsHandleResultArgument : MessageAnswerArgument
{
	public bool IsHandle { get; set; }

	static IsHandleResultArgument()
	{
		MessageHelper.RegisterMessageArgumentType("FE159D19-B11A-4f8c-87E6-DE2FAE23701E", typeof(IsHandleResultArgument));
	}
}
