using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.InflexSchema, "user_reportforms")]
public class InflexFastReportForm : SessionCacheObject<InflexFastReportForm>
{
	[PrimaryKey(1)]
	[MapField("id")]
	public decimal ID { get; set; }

	[MapField("description")]
	public string Description { get; set; }

	[MapIgnore]
	public bool IsDefault => DirectIsDefault == 'Y';

	[MapField("is_default")]
	public char DirectIsDefault { get; set; }

	[MapField("shortcut_code")]
	public decimal ShortcutCode { get; set; }

	[MapIgnore]
	public bool ItemHandling => DirectItemHandling == 'Y';

	[MapField("item_handling")]
	public char DirectItemHandling { get; set; }

	[MapField("menu_group")]
	public string MenuGroup { get; set; }

	[MapField("name")]
	public string Name { get; set; }

	protected override bool DirectEquals(InflexFastReportForm obj)
	{
		return ID == obj.ID;
	}

	protected override int DirectGetHashCode()
	{
		return ID.GetHashCode();
	}
}
