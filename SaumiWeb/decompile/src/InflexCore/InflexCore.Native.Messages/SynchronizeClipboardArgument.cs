namespace InflexCore.Native.Messages;

public class SynchronizeClipboardArgument : MessageArgument
{
	static SynchronizeClipboardArgument()
	{
		MessageHelper.RegisterMessageArgumentType("837B9013-5CFB-428d-9579-29E9A585DDF7", typeof(SynchronizeClipboardArgument));
	}
}
