using Microsoft.Win32;

namespace CommonUtils.RegistryUtils;

public static class RegistryKeyExtension
{
	public static RegistryKeyView OpenSubKey(this RegistryKey parentKey, string subKeyName, RegistryViewOptions registryViewOptions)
	{
		return parentKey.OpenSubKey(subKeyName, registryViewOptions, isWritable: false);
	}

	public static RegistryKeyView OpenSubKey(this RegistryKey parentKey, string subKeyName, RegistryViewOptions registryViewOptions, bool isWritable)
	{
		return RegistryKeyView.OpenSubKey(parentKey, subKeyName, registryViewOptions, isWritable);
	}

	public static string ToSafeString(this RegistryKey registryKey)
	{
		return "Registry key " + ((registryKey != null) ? $"\"{registryKey.Name}\" ({registryKey.SubKeyCount} subkeys, {registryKey.ValueCount} values)" : "is NULL");
	}
}
