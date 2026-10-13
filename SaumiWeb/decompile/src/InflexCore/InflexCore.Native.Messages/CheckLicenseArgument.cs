namespace InflexCore.Native.Messages;

public class CheckLicenseArgument : MessageArgument
{
	public int License { get; set; }

	static CheckLicenseArgument()
	{
		MessageHelper.RegisterMessageArgumentType("FBBCE406-6266-4203-A38A-0321CEF47459", typeof(CheckLicenseArgument));
	}
}
