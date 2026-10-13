using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.InflexSchema, "sm_settings")]
public class InflexSettings : SessionCacheObject<InflexSettings>
{
	[PrimaryKey]
	[MapField("key_name")]
	public string Key { get; set; }

	[MapField("key_description")]
	public string Description { get; set; }

	[MapField("key_type")]
	public string Type { get; set; }

	[MapField("value")]
	public string Value { get; set; }

	protected override bool DirectEquals(InflexSettings obj)
	{
		return Key == obj.Key;
	}

	protected override int DirectGetHashCode()
	{
		return Key.GetHashCode();
	}
}
