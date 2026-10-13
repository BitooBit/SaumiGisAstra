using InflexCore.Native.Messages;

namespace ExplorerWindowBase.Messages;

public class ShowInflexWindowMessageArgument : MessageArgument
{
	public int InnerHandle { get; set; }

	public int QueryType { get; set; }

	public string RestrictionText { get; set; }

	public string RestrictionXml { get; set; }

	public int RestrictionKey { get; set; }

	public string InfoText { get; set; }

	public string ManualFrom { get; set; }

	public string ManualWhere { get; set; }

	static ShowInflexWindowMessageArgument()
	{
		MessageHelper.RegisterMessageArgumentType("B2F5692D-E0E7-4CDF-8CF3-4704BB790747", typeof(ShowInflexWindowMessageArgument));
	}
}
