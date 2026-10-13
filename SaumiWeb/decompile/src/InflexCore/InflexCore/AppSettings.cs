using System.Linq;
using InflexCore.DataImplementation;
using InflexCore.DataImplementation.Accessors;
using InflexCore.DataImplementation.Reflection;
using InflexCore.log;

namespace InflexCore;

public class AppSettings : ISimpleSettingsAccessor
{
	private readonly AppManager _AppManager;

	private readonly LazyListLoader<InflexSettings, InflexSettingsAccessor> _Settings;

	public string this[string key]
	{
		get
		{
			if (!string.IsNullOrEmpty(key))
			{
				return GetSettingValueByKey(key);
			}
			return string.Empty;
		}
		set
		{
			InflexSettings settingByKey = GetSettingByKey(key);
			if (settingByKey != null)
			{
				settingByKey.Value = value;
				_AppManager.Accessor<InflexSettingsAccessor>().SaveAndFlush(settingByKey);
			}
			else
			{
				FastLog.Loger(this).Info((object)"Save settings error: AppSettings key not found.");
			}
		}
	}

	public AppSettings(AppManager appManager)
	{
		_AppManager = appManager;
		_Settings = new LazyListLoader<InflexSettings, InflexSettingsAccessor>(_AppManager);
	}

	private string GetSettingValueByKey(string key)
	{
		return (from settings in _Settings.List(_Settings.Accessor.SelectAllAsList)
			where settings.Key == key
			select settings.Value).FirstOrDefault();
	}

	private InflexSettings GetSettingByKey(string key)
	{
		return (from settings in _Settings.List(_Settings.Accessor.SelectAllAsList)
			where settings.Key == key
			select settings).FirstOrDefault();
	}

	public bool ExistsKey(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			return GetSettingByKey(key) != null;
		}
		return false;
	}
}
