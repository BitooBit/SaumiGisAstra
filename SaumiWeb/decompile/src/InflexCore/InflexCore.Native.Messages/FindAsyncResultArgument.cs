namespace InflexCore.Native.Messages;

public class FindAsyncResultArgument : MessageArgument
{
	public int FindID { get; set; }

	public bool FindRes { get; set; }

	public string Sql { get; set; }

	public string Xml { get; set; }

	static FindAsyncResultArgument()
	{
		MessageHelper.RegisterMessageArgumentType("9189FA59-C824-4C6D-8302-7957FBF9DEAA", typeof(FindAsyncResultArgument));
	}
}
