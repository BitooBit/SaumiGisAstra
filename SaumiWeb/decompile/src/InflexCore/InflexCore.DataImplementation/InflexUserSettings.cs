using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.CurrentUser, "sm_usersettings")]
public class InflexUserSettings : SessionCacheObject<InflexUserSettings>, ISelfKeyGenerator
{
	[PrimaryKey]
	[MapField("key_name")]
	public string Key { get; set; }

	[MapField("value")]
	public string Value { get; set; }

	[MapIgnore]
	public string NewKeyValue { get; set; }

	protected override bool DirectEquals(InflexUserSettings obj)
	{
		return Key == obj.Key;
	}

	protected override int DirectGetHashCode()
	{
		return Key.GetHashCode();
	}

	void ISelfKeyGenerator.NewKey()
	{
		Key = NewKeyValue;
	}
}
