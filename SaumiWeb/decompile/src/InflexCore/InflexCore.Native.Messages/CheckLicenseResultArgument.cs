namespace InflexCore.Native.Messages;

public class CheckLicenseResultArgument : MessageAnswerArgument
{
	public bool Result { get; set; }

	static CheckLicenseResultArgument()
	{
		MessageHelper.RegisterMessageArgumentType("22622C3A-7AAF-44ac-828D-79C549370593", typeof(CheckLicenseResultArgument));
	}
}
