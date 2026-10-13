using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.InflexSchema, "x_sm_messages")]
public class InflexLogMessages
{
	[MapField("msg_time")]
	public string Key { get; set; }

	[MapField("text_message")]
	public string Description { get; set; }

	[MapField("proc_name")]
	public string Type { get; set; }

	[MapField("user_name")]
	public string UserName { get; set; }

	[MapField("msg_critical")]
	public string MsgCritical { get; set; }

	[MapField("msg_type")]
	public string MsgType { get; set; }

	[MapField("msg_kind")]
	public string MsgKind { get; set; }

	[MapField("data_id")]
	public string DataID { get; set; }

	[MapField("type_id")]
	public string TypeID { get; set; }

	[MapField("xml_message")]
	public string XmlMessage { get; set; }

	[MapField("is_solved")]
	public string IsSolved { get; set; }
}
