using System;
using CommonUtils.ReportUtils;
using Microsoft.Win32;

namespace CommonUtils.RegistryUtils;

public static class RegistryUtils
{
	private static RegistryKeyView GetRegistryHiveKey(string regHiveName, RegistryViewOptions registryViewOptions)
	{
		RegistryKeyView registryKeyView = null;
		return regHiveName switch
		{
			"HKEY_LOCAL_MACHINE" => RegistryKeyView.FromRegistryKey(Registry.LocalMachine, registryViewOptions), 
			"HKEY_CURRENT_USER" => RegistryKeyView.FromRegistryKey(Registry.CurrentUser, registryViewOptions), 
			_ => throw new ArgumentException("Not supported registry hive name " + regHiveName + " was specified", "regHiveName"), 
		};
	}

	private static RegistryKeyView GetRegistryHiveKey(RegistryHive regHive, RegistryViewOptions registryViewOptions)
	{
		RegistryKeyView registryKeyView = null;
		return regHive switch
		{
			RegistryHive.LocalMachine => RegistryKeyView.FromRegistryKey(Registry.LocalMachine, registryViewOptions), 
			RegistryHive.CurrentUser => RegistryKeyView.FromRegistryKey(Registry.CurrentUser, registryViewOptions), 
			_ => throw new ArgumentException(string.Concat("Not supported registry hive ", regHive, " was specified"), "regHive"), 
		};
	}

	private static bool TrySplitRegKeyFullPath(string regKeyFullPath, out string regHiveName, out string regKeyName)
	{
		int num = regKeyFullPath.IndexOf('\\');
		if (num == -1)
		{
			regHiveName = null;
			regKeyName = null;
			return false;
		}
		regHiveName = regKeyFullPath.Substring(0, num);
		regKeyName = regKeyFullPath.Substring(num + 1, regKeyFullPath.Length - num - 1);
		return true;
	}

	public static TResult GetValue<TResult>(RegistryHive regHive, string regKeyName, string regEntryName, RegistryViewOptions registryViewOptions)
	{
		return (TResult)GetValue(regHive, regKeyName, regEntryName, registryViewOptions);
	}

	public static TResult GetValue<TResult>(string regKeyFullPath, string regEntryName, RegistryViewOptions registryViewOptions)
	{
		return (TResult)GetValue(regKeyFullPath, regEntryName, registryViewOptions);
	}

	public static TResult GetValue<TResult>(string regHiveName, string regKeyName, string regEntryName, RegistryViewOptions registryViewOptions)
	{
		return (TResult)GetValue(regHiveName, regKeyName, regEntryName, registryViewOptions);
	}

	public static TResult GetValue<TResult>(RegistryKeyView regKey, string regEntryName)
	{
		return (TResult)GetValue(regKey, regEntryName);
	}

	public static object GetValue(RegistryHive regHive, string regKeyName, string regEntryName, RegistryViewOptions registryViewOptions)
	{
		RegistryKeyView registryHiveKey = GetRegistryHiveKey(regHive, registryViewOptions);
		UtilsLoggerWrapper.Info(string.Concat("Registry hive key ", registryHiveKey, " was successfully determined"));
		return GetValue(registryHiveKey, regKeyName, regEntryName);
	}

	public static object GetValue(string regKeyFullPath, string regEntryName, RegistryViewOptions registryViewOptions)
	{
		if (!TrySplitRegKeyFullPath(regKeyFullPath, out var regHiveName, out var regKeyName))
		{
			throw new ArgumentException("Can't determine registry hive and registry key by splitting specified path", "regKeyFullPath");
		}
		return GetValue(regHiveName, regKeyName, regEntryName, registryViewOptions);
	}

	public static object GetValue(string regHiveName, string regKeyName, string regEntryName, RegistryViewOptions registryViewOptions)
	{
		RegistryKeyView registryHiveKey = GetRegistryHiveKey(regHiveName, registryViewOptions);
		UtilsLoggerWrapper.Info(string.Concat("Registry hive key ", registryHiveKey, " was successfully determined"));
		return GetValue(registryHiveKey, regKeyName, regEntryName);
	}

	private static object GetValue(RegistryKeyView regHiveKey, string regKeyName, string regEntryName)
	{
		RegistryKeyView registryKeyView = null;
		try
		{
			registryKeyView = regHiveKey.OpenSubKey(regKeyName);
			if (registryKeyView == null)
			{
				UtilsLoggerWrapper.Error("Can't open registry key " + regKeyName + ". NULL value will be returned.");
				return null;
			}
			UtilsLoggerWrapper.Info(string.Concat(new object[4] { registryKeyView, " was successfully opened. Its registry entry ", regEntryName, " value will be returned." }));
			return GetValue(registryKeyView, regEntryName);
		}
		finally
		{
			registryKeyView?.Close();
			regHiveKey?.Close();
		}
	}

	public static object GetValue(RegistryKeyView regKey, string regEntryName)
	{
		if (regKey == null)
		{
			throw new ArgumentNullException("regKey", "Target registry key can't be null!");
		}
		return regKey.GetValue(regEntryName);
	}

	public static Exception SetValue(RegistryHive regHive, string regKeyName, string regEntryName, object regEntryValue, RegistryViewOptions registryViewOptions, bool createBackupEntry = false)
	{
		RegistryKeyView registryHiveKey = GetRegistryHiveKey(regHive, registryViewOptions);
		UtilsLoggerWrapper.Info(string.Concat("Registry hive key ", registryHiveKey, " was successfully determined"));
		return SetValue(registryHiveKey, regKeyName, regEntryName, regEntryValue, createBackupEntry);
	}

	public static Exception SetValue(string regKeyFullPath, string regEntryName, object regEntryValue, RegistryViewOptions registryViewOptions, bool createBackupEntry = false)
	{
		if (!TrySplitRegKeyFullPath(regKeyFullPath, out var regHiveName, out var regKeyName))
		{
			return new ArgumentException("Can't determine registry hive and registry key by splitting specified path", "regKeyFullPath");
		}
		return SetValue(regHiveName, regKeyName, regEntryName, regEntryValue, registryViewOptions, createBackupEntry);
	}

	public static Exception SetValue(string regHiveName, string regKeyName, string regEntryName, object regEntryValue, RegistryViewOptions registryViewOptions, bool createBackupEntry = false)
	{
		RegistryKeyView registryHiveKey = GetRegistryHiveKey(regHiveName, registryViewOptions);
		UtilsLoggerWrapper.Info(string.Concat("Registry hive key ", registryHiveKey, " was successfully determined"));
		return SetValue(registryHiveKey, regKeyName, regEntryName, regEntryValue, createBackupEntry);
	}

	private static Exception SetValue(RegistryKeyView regHiveKey, string regKeyName, string regEntryName, object regEntryValue, bool createBackupEntry)
	{
		RegistryKeyView registryKeyView = null;
		try
		{
			registryKeyView = regHiveKey.OpenSubKey(regKeyName, writable: true);
			UtilsLoggerWrapper.Info(string.Concat(registryKeyView, " was successfully opened as writable"));
			Exception result = SetValue(registryKeyView, regEntryName, regEntryValue, createBackupEntry);
			UtilsLoggerWrapper.Info(string.Concat(new object[4] { "Value ", regEntryValue, " was successfully set for registry entry ", regEntryName }));
			return result;
		}
		catch (Exception result2)
		{
			return result2;
		}
		finally
		{
			registryKeyView?.Close();
			regHiveKey?.Close();
		}
	}

	public static Exception SetValue(RegistryKeyView regKey, string regEntryName, object regEntryValue, bool createBackupEntry = false)
	{
		if (regKey == null)
		{
			return new ArgumentNullException("regKey", "Target registry key can't be null!");
		}
		try
		{
			UtilsLoggerWrapper.Info(string.Concat(new object[5] { regKey, " entry ", regEntryName, " should be set to specified value ", regEntryValue }));
			if (!createBackupEntry)
			{
				regKey.SetValue(regEntryName, regEntryValue);
				UtilsLoggerWrapper.Info("Specified value was successfully set for specified entry");
			}
			else
			{
				UtilsLoggerWrapper.Info("Backup registry entry should be created, appropriate flag was specified");
				object value = GetValue(regKey, regEntryName);
				if (value == null)
				{
					UtilsLoggerWrapper.Info("Specified registry entry currently doesn't exist. Shouldn't create backup registry entry.");
					regKey.SetValue(regEntryName, regEntryValue);
					UtilsLoggerWrapper.Info("Specified value was successfully set for specified entry");
				}
				else
				{
					UtilsLoggerWrapper.Info("Current registry entry value is " + value);
					if (value.Equals(regEntryValue))
					{
						UtilsLoggerWrapper.Info(string.Concat("New specified registry entry value ", regEntryValue, " matches to current. Shouldn't make any changes."));
					}
					else
					{
						UtilsLoggerWrapper.Info(string.Concat("New specified registry entry value ", regEntryValue, " doesn't match to current. Specified registry entry will be changed, that's why backup registry entry should be created."));
						string text = regEntryName + "_BAK";
						object value2 = GetValue(regKey, text);
						if (value2 != null)
						{
							UtilsLoggerWrapper.Error("Can't create backup registry entry! Registry entry " + text + " already exists, currently it has value " + value2);
						}
						else
						{
							regKey.SetValue(text, value);
							UtilsLoggerWrapper.Info("Backup registry entry " + text + " was successfully created");
						}
						regKey.SetValue(regEntryName, regEntryValue);
						UtilsLoggerWrapper.Info("Specified value was successfully set for specified entry");
					}
				}
			}
		}
		catch (Exception ex)
		{
			UtilsLoggerWrapper.Error("Can't set specified value for specified entry! Exception occurred: " + ex);
			return ex;
		}
		return null;
	}

	public static Exception RemoveSubKey(RegistryKeyView regKey, string subKeyName)
	{
		if (regKey == null)
		{
			return new ArgumentNullException("regKey", "Target RegistryKey can't be null!");
		}
		try
		{
			regKey.DeleteSubKey(subKeyName, throwOnMissingSubKey: true);
		}
		catch (Exception result)
		{
			return result;
		}
		return null;
	}

	public static Exception RemoveEntry(string regKeyFullPath, string regEntryName, RegistryViewOptions registryViewOptions)
	{
		if (!TrySplitRegKeyFullPath(regKeyFullPath, out var regHiveName, out var regKeyName))
		{
			return new ArgumentException("Can't determine registry hive and registry key by splitting specified path", "regKeyFullPath");
		}
		RegistryKeyView registryKeyView = null;
		RegistryKeyView registryKeyView2 = null;
		try
		{
			registryKeyView = GetRegistryHiveKey(regHiveName, registryViewOptions);
			UtilsLoggerWrapper.Info(string.Concat("Hive ", registryKeyView, " was successfully determined"));
			registryKeyView2 = registryKeyView.OpenSubKey(regKeyName, writable: true);
			UtilsLoggerWrapper.Info(string.Concat(registryKeyView2, " was successfully opened as writable"));
			return RemoveEntry(registryKeyView2, regEntryName);
		}
		catch (Exception result)
		{
			return result;
		}
		finally
		{
			registryKeyView2?.Close();
			registryKeyView?.Close();
		}
	}

	public static Exception RemoveEntry(RegistryKeyView regKey, string regEntryName)
	{
		if (regKey == null)
		{
			return new ArgumentNullException("regKey", "Target RegistryKey can't be null!");
		}
		try
		{
			regKey.DeleteValue(regEntryName, throwOnMissingValue: true);
			UtilsLoggerWrapper.Info("Entry " + regEntryName + " was successfully removed from " + regKey);
		}
		catch (Exception result)
		{
			return result;
		}
		return null;
	}

	public static RegistryKeyView GetRegistryKeyByAbsolutePath(string regKeyFullPath, bool isWritable, RegistryViewOptions registryViewOptions)
	{
		if (!TrySplitRegKeyFullPath(regKeyFullPath, out var regHiveName, out var regKeyName) || string.IsNullOrEmpty(regHiveName))
		{
			return null;
		}
		RegistryKeyView registryKeyView = null;
		foreach (object value in Enum.GetValues(typeof(RegistryHive)))
		{
			RegistryHive regHive = (RegistryHive)value;
			try
			{
				registryKeyView = GetRegistryHiveKey(regHive, registryViewOptions);
			}
			catch (Exception)
			{
			}
		}
		if (registryKeyView == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(regKeyName))
		{
			return registryKeyView;
		}
		RegistryKeyView result = null;
		try
		{
			result = registryKeyView.OpenSubKey(regKeyName, isWritable);
		}
		catch (Exception)
		{
		}
		return result;
	}

	public static string GetRegistryKeyShortName(RegistryKey registryKey)
	{
		return GetRegistryKeyShortName(registryKey.Name);
	}

	public static string GetRegistryKeyShortName(string registryKeyFullPath)
	{
		registryKeyFullPath = registryKeyFullPath.TrimEnd(new char[1] { '\\' });
		registryKeyFullPath = registryKeyFullPath.Remove(0, registryKeyFullPath.LastIndexOf('\\') + 1);
		return registryKeyFullPath;
	}
}
