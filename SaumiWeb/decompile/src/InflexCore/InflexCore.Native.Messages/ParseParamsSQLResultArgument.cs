namespace InflexCore.Native.Messages;

public class ParseParamsSQLResultArgument : MessageAnswerArgument
{
	public string SQL { get; set; }

	static ParseParamsSQLResultArgument()
	{
		MessageHelper.RegisterMessageArgumentType("1181CB3F-EEE4-4ef3-903E-702653A44851", typeof(ParseParamsSQLResultArgument));
	}
}
