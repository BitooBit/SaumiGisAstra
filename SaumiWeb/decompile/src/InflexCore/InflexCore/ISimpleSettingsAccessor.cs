namespace InflexCore;

public interface ISimpleSettingsAccessor
{
	string this[string key] { get; set; }

	bool ExistsKey(string key);
}
