using System.Collections.Generic;
using InflexCore.DataImplementation.Accessors;

namespace InflexCore.DataImplementation;

public class UserSettings : ISimpleSettingsAccessor
{
	private readonly InflexUserSettingsAccessor _Accessor;

	private Dictionary<string, InflexUserSettings> _Values;

	public string this[string key]
	{
		get
		{
			string key2 = (string.IsNullOrEmpty(key) ? "" : key.ToUpper());
			FillValues();
			if (!_Values.TryGetValue(key2, out var value))
			{
				return string.Empty;
			}
			return value.Value;
		}
		set
		{
			string text = (string.IsNullOrEmpty(key) ? "" : key.ToUpper());
			FillValues();
			if (!_Values.TryGetValue(text, out var value2))
			{
				InflexUserSettings inflexUserSettings = new InflexUserSettings();
				inflexUserSettings.NewKeyValue = text;
				value2 = inflexUserSettings;
			}
			value2.Value = value;
			_Accessor.Save(value2);
			_Values.Remove(text);
			_Values.Add(text, value2);
		}
	}

	public UserSettings(InflexUserSettingsAccessor accessor)
	{
		_Accessor = accessor;
	}

	public bool ExistsKey(string key)
	{
		string key2 = (string.IsNullOrEmpty(key) ? "" : key.ToUpper());
		FillValues();
		return _Values.ContainsKey(key2);
	}

	private void FillValues()
	{
		if (_Values != null)
		{
			return;
		}
		_Values = new Dictionary<string, InflexUserSettings>();
		foreach (InflexUserSettings item in _Accessor.SelectAllAsList())
		{
			_Values.Add(item.Key, item);
		}
	}
}
