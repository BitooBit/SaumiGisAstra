namespace InflexCore.Native.Messages;

public class NewItemResultArgument : MessageAnswerArgument
{
	public ItemData ItemData { get; set; }

	static NewItemResultArgument()
	{
		MessageHelper.RegisterMessageArgumentType("ADE0BEAC-A1E5-4572-8EE8-3D847A71031A", typeof(NewItemResultArgument));
	}
}
