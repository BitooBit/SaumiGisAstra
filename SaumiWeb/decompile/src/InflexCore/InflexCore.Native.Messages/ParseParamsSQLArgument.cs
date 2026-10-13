namespace InflexCore.Native.Messages;

public class ParseParamsSQLArgument : MessageArgument
{
	public string SQL { get; set; }

	static ParseParamsSQLArgument()
	{
		MessageHelper.RegisterMessageArgumentType("ECE5B582-10A4-48a3-8EDE-ADC2D384AFE0", typeof(ParseParamsSQLArgument));
	}
}
