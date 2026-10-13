using System;
using CommonUtils.EnvironmentUtils;
using Microsoft.Win32;

namespace CommonUtils.RegistryUtils;

public class RegistryKeyView : MarshalByRefObject, IDisposable
{
	private RegistryKey _Key;

	private readonly RegistryViewOptions _ViewOptions;

	private readonly bool _IsWritable;

	public bool IsWritable => _IsWritable;

	public RegistryViewOptions ViewOptions => _ViewOptions;

	public string Name => _Key.Name;

	public int SubKeyCount => _Key.SubKeyCount;

	public int ValueCount => _Key.ValueCount;

	public static RegistryKeyView OpenSubKey(RegistryKey registryKey, string subKeyName, RegistryViewOptions registryViewOptions)
	{
		return OpenSubKey(registryKey, subKeyName, registryViewOptions, isWritable: false);
	}

	public static RegistryKeyView OpenSubKey(RegistryKey registryKey, string subKeyName, RegistryViewOptions registryViewOptions, bool isWritable)
	{
		RegistryKey registryKey2 = RegistryNativeUtils.OpenSubKey(registryKey, subKeyName, registryViewOptions, isWritable);
		if (registryKey2 != null)
		{
			return new RegistryKeyView(registryKey2, isWritable, registryViewOptions);
		}
		return null;
	}

	internal static RegistryKeyView FromRegistryKey(RegistryKey registryKey, RegistryViewOptions registryViewOptions)
	{
		bool isWritable = RegistryNativeUtils.IsRegistryKeyWritable(registryKey);
		return new RegistryKeyView(registryKey, isWritable, registryViewOptions);
	}

	private RegistryKeyView(RegistryKey key, bool isWritable, RegistryViewOptions viewOptions)
	{
		_Key = key;
		_IsWritable = isWritable;
		_ViewOptions = viewOptions;
	}

	public bool IsUnderWow6432Node(string subKeyName)
	{
		string registryKeyPath = Name.TrimEnd(new char[1] { '\\' }) + '\\' + subKeyName.TrimStart(new char[1] { '\\' });
		return RegistryNativeUtils.IsRegistryKeyUnderWow6432Node(registryKeyPath);
	}

	public RegistryKeyView OpenSubKey(string name)
	{
		RegistryKey registryKey = (((Environment.OSVersion.Version.Major >= 6 && (Environment.OSVersion.Version.Major != 6 || Environment.OSVersion.Version.Minor >= 1)) || EnvironmentScanner.SystemEnvironmentBits != AppPlatformBits.X64 || ViewOptions != RegistryViewOptions.Registry64 || IsUnderWow6432Node(name)) ? _Key.OpenSubKey(name) : RegistryNativeUtils.OpenSubKey(_Key, name, ViewOptions, isWriteable: false));
		if (registryKey != null)
		{
			return new RegistryKeyView(registryKey, isWritable: false, ViewOptions);
		}
		return null;
	}

	public RegistryKeyView OpenSubKey(string name, bool writable)
	{
		RegistryKey registryKey = (((Environment.OSVersion.Version.Major >= 6 && (Environment.OSVersion.Version.Major != 6 || Environment.OSVersion.Version.Minor >= 1)) || EnvironmentScanner.SystemEnvironmentBits != AppPlatformBits.X64 || ViewOptions != RegistryViewOptions.Registry64 || IsUnderWow6432Node(name)) ? _Key.OpenSubKey(name, writable) : RegistryNativeUtils.OpenSubKey(_Key, name, ViewOptions, writable));
		if (registryKey != null)
		{
			return new RegistryKeyView(registryKey, writable, ViewOptions);
		}
		return null;
	}

	public void DeleteSubKey(string subkey)
	{
		if ((Environment.OSVersion.Version.Major < 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 1)) && EnvironmentScanner.SystemEnvironmentBits == AppPlatformBits.X64 && ViewOptions == RegistryViewOptions.Registry64 && !IsUnderWow6432Node(subkey))
		{
			RegistryNativeUtils.DeleteSubKey(_Key, subkey, ViewOptions, recursive: false, throwOnMissingSubKey: true);
		}
		else
		{
			_Key.DeleteSubKey(subkey);
		}
	}

	public void DeleteSubKey(string subkey, bool throwOnMissingSubKey)
	{
		if ((Environment.OSVersion.Version.Major < 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 1)) && EnvironmentScanner.SystemEnvironmentBits == AppPlatformBits.X64 && ViewOptions == RegistryViewOptions.Registry64 && !IsUnderWow6432Node(subkey))
		{
			RegistryNativeUtils.DeleteSubKey(_Key, subkey, ViewOptions, recursive: false, throwOnMissingSubKey);
		}
		else
		{
			_Key.DeleteSubKey(subkey, throwOnMissingSubKey);
		}
	}

	public void DeleteSubKeyTree(string subkey)
	{
		if ((Environment.OSVersion.Version.Major < 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 1)) && EnvironmentScanner.SystemEnvironmentBits == AppPlatformBits.X64 && ViewOptions == RegistryViewOptions.Registry64 && !IsUnderWow6432Node(subkey))
		{
			RegistryNativeUtils.DeleteSubKey(_Key, subkey, ViewOptions, recursive: true, throwOnMissingSubKey: true);
		}
		else
		{
			_Key.DeleteSubKeyTree(subkey);
		}
	}

	public string[] GetSubKeyNames()
	{
		return _Key.GetSubKeyNames();
	}

	public string[] GetValueNames()
	{
		return _Key.GetValueNames();
	}

	public object GetValue(string name)
	{
		return _Key.GetValue(name);
	}

	public object GetValue(string name, object defaultValue)
	{
		return _Key.GetValue(name, defaultValue);
	}

	public object GetValue(string name, object defaultValue, RegistryValueOptions options)
	{
		return _Key.GetValue(name, defaultValue, options);
	}

	public RegistryValueKind GetValueKind(string name)
	{
		return _Key.GetValueKind(name);
	}

	public void SetValue(string name, object value)
	{
		_Key.SetValue(name, value);
	}

	public void SetValue(string name, object value, RegistryValueKind valueKind)
	{
		_Key.SetValue(name, value, valueKind);
	}

	public void DeleteValue(string name)
	{
		_Key.DeleteValue(name);
	}

	public void DeleteValue(string name, bool throwOnMissingValue)
	{
		_Key.DeleteValue(name, throwOnMissingValue);
	}

	public void Flush()
	{
		_Key.Flush();
	}

	public void Close()
	{
		_Key.Close();
	}

	public void Dispose()
	{
		((IDisposable)_Key).Dispose();
		_Key = null;
	}

	public override string ToString()
	{
		RegistryKey key = _Key;
		return "Registry key " + ((key != null) ? $"\"{key.Name}\" ({key.SubKeyCount} subkeys, {key.ValueCount} values)" : "is NULL");
	}
}
