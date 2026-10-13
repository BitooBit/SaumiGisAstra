using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using CommonUtils.EnvironmentUtils;
using CommonUtils.ReportUtils;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace CommonUtils.RegistryUtils;

public static class RegistryNativeUtils
{
	private const string SAFEREGISTRYHANDLE_TYPE_FULLNAME = "Microsoft.Win32.SafeHandles.SafeRegistryHandle";

	private const uint REGOPENKEYEX_ULOPTIONS_RESERVED_VALUE = 0u;

	private const int REGDELETEKEYEX_RESERVED_VALUE = 0;

	private const int REGOPENKEYEX_ERROR_SUCCESS_RESULT_CODE = 0;

	private const int REGOPENKEYEX_ERROR_FILE_NOT_FOUND_RESULT_CODE = 2;

	private const int REGDELETEKEYEX_ERROR_SUCCESS_RESULT_CODE = 0;

	private const int REGDELETEKEYEX_ERROR_FILE_NOT_FOUND_RESULT_CODE = 2;

	private const string REGISTRYVIEW_TYPE_FULLNAME = "Microsoft.Win32.RegistryView";

	private const string REGISTRYVIEW_DEFAULT_FIELD_NAME = "Default";

	private const int REGISTRYVIEW_DEFAULT_FIELD_VALUE = 0;

	private const string REGISTRYVIEW_REGISTRY64_FIELD_NAME = "Registry64";

	private const int REGISTRYVIEW_REGISTRY64_FIELD_VALUE = 256;

	private const string REGISTRYVIEW_REGISTRY32_FIELD_NAME = "Registry32";

	private const int REGISTRYVIEW_REGISTRY32_FIELD_VALUE = 512;

	[DllImport("advapi32.dll", CharSet = CharSet.Auto)]
	private static extern int RegOpenKeyEx(IntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out IntPtr phkResult);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto)]
	private static extern int RegDeleteKeyEx(IntPtr hKey, string lpSubKey, int samDesired, int reserved);

	public static RegistryKey OpenSubKey(RegistryKey parentKey, string subKeyName, RegistryViewOptions registryViewOptions, bool isWriteable)
	{
		if (TryGetRegistryKeyInfo(parentKey, out var registryKeyHandle, out var registryKeyPermissionCheck))
		{
			IntPtr parentKeyHandleDangerousPointer = registryKeyHandle.DangerousGetHandle();
			return OpenSubKey(parentKeyHandleDangerousPointer, parentKey.Name, registryKeyPermissionCheck, subKeyName, registryViewOptions, isWriteable);
		}
		return null;
	}

	public static RegistryKey OpenSubKey(RegistryHiveKeys registryHiveKey, string subKeyName, RegistryViewOptions registryViewOptions, bool isWriteable)
	{
		IntPtr parentKeyHandleDangerousPointer = new IntPtr((int)registryHiveKey);
		string parentKeyName = registryHiveKey.ToString();
		RegistryKeyPermissionCheck parentKeyPermissionCheck = RegistryKeyPermissionCheck.Default;
		return OpenSubKey(parentKeyHandleDangerousPointer, parentKeyName, parentKeyPermissionCheck, subKeyName, registryViewOptions, isWriteable);
	}

	private static RegistryKey OpenSubKey(IntPtr parentKeyHandleDangerousPointer, string parentKeyName, RegistryKeyPermissionCheck parentKeyPermissionCheck, string subKeyName, RegistryViewOptions registryViewOptions, bool isWriteable)
	{
		string text = "UNDEFINED";
		try
		{
			text = parentKeyName.TrimEnd(new char[1] { '\\' }) + '\\' + subKeyName.TrimStart(new char[1] { '\\' });
			registryViewOptions = AdjustRegistryViewOptions(text, registryViewOptions);
			RegistryAccessRights registryAccessRights = (isWriteable ? RegistryAccessRights.KEY_WRITE : RegistryAccessRights.KEY_READ);
			RegistryAccessRights registryAccessOptions = GetRegistryAccessOptions(registryViewOptions);
			UtilsLoggerWrapper.Info("Try open registry key \"" + text + "\" using unmanaged library advapi32.dll");
			int num = RegOpenKeyEx(parentKeyHandleDangerousPointer, subKeyName, 0u, (int)(registryAccessRights | registryAccessOptions), out var phkResult);
			UtilsLoggerWrapper.Info("Open result code: " + num);
			switch (num)
			{
			case 2:
				UtilsLoggerWrapper.Warning("Specified subkey \"" + subKeyName + "\" for key " + parentKeyName + " doesn't exist. Can't open it.");
				return null;
			default:
				throw new Win32Exception(num, "Can't open registry key using RegOpenKeyEx function! Error code: " + num);
			case 0:
			{
				UtilsLoggerWrapper.Info("Create registry key instance");
				RegistryKeyPermissionCheck registryKeyPermissonCheck = GetRegistryKeyPermissonCheck(parentKeyPermissionCheck, isWriteable);
				RegistryKey registryKey = CreateRegistryKeyFromHandle(phkResult, text, registryKeyPermissonCheck, isWriteable, ownsHandle: false, registryViewOptions);
				UtilsLoggerWrapper.Info("Registry key instance is created - " + registryKey.Name + " (" + registryKey.ValueCount + " values, " + registryKey.SubKeyCount + " subkeys)");
				return registryKey;
			}
			}
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("Can't open specified registry key " + text, innerException);
		}
	}

	public static void DeleteSubKey(RegistryKey parentKey, string subKeyName, RegistryViewOptions registryViewOptions, bool recursive, bool throwOnMissingSubKey)
	{
		if (!TryGetRegistryKeyInfo(parentKey, out var registryKeyHandle, out var _))
		{
			return;
		}
		IntPtr parentKeyHandleDangerousPointer = registryKeyHandle.DangerousGetHandle();
		RegistryKey registryKey = OpenSubKey(parentKey, subKeyName, registryViewOptions, isWriteable: false);
		if (registryKey != null)
		{
			try
			{
				if (registryKey.SubKeyCount > 0)
				{
					if (!recursive)
					{
						throw new InvalidOperationException("Can't delete specified registry subkey, because it has own subkeys");
					}
					string[] subKeyNames = registryKey.GetSubKeyNames();
					string[] array = subKeyNames;
					foreach (string subKeyName2 in array)
					{
						DeleteSubKey(registryKey, subKeyName2, registryViewOptions, recursive, throwOnMissingSubKey);
					}
				}
			}
			finally
			{
				registryKey.Close();
			}
		}
		else if (throwOnMissingSubKey)
		{
			throw new ArgumentException("Can't delete specified registry subkey - it is absent");
		}
		DeleteSubKey(parentKeyHandleDangerousPointer, parentKey.Name, subKeyName, registryViewOptions);
	}

	private static void DeleteSubKey(IntPtr parentKeyHandleDangerousPointer, string parentKeyName, string subKeyName, RegistryViewOptions registryViewOptions)
	{
		string text = "UNDEFINED";
		try
		{
			text = parentKeyName.TrimEnd(new char[1] { '\\' }) + '\\' + subKeyName.TrimStart(new char[1] { '\\' });
			registryViewOptions = AdjustRegistryViewOptions(text, registryViewOptions);
			RegistryAccessRights registryAccessOptions = GetRegistryAccessOptions(registryViewOptions);
			UtilsLoggerWrapper.Info("Try delete registry key \"" + text + "\" using unmanaged library advapi32.dll");
			int num = RegDeleteKeyEx(parentKeyHandleDangerousPointer, subKeyName, (int)registryAccessOptions, 0);
			UtilsLoggerWrapper.Info("Delete result code: " + num);
			switch (num)
			{
			case 2:
				UtilsLoggerWrapper.Warning("Specified subkey \"" + subKeyName + "\" for key " + parentKeyName + " doesn't exist. Can't delete it.");
				break;
			default:
				throw new Win32Exception(num, "Can't delete registry key using RegDeleteKeyEx function! Error code: " + num);
			case 0:
				break;
			}
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("Can't delete specified registry key " + text, innerException);
		}
	}

	private static RegistryViewOptions AdjustRegistryViewOptions(string registryKeyPath, RegistryViewOptions registryViewOptions)
	{
		if (registryViewOptions == RegistryViewOptions.Registry64 && (Environment.OSVersion.Version.Major < 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 1)) && EnvironmentScanner.SystemEnvironmentBits == AppPlatformBits.X64 && IsRegistryKeyUnderWow6432Node(registryKeyPath))
		{
			UtilsLoggerWrapper.Warning("Specified registry key is virtualized under Wow6432Node and current system " + EnvironmentScanner.SystemDescription + " requires 32-bit registry view option for access to above key, that's why specified 64-bit registry view option will be adjusted to 32-bit registry view option");
			return RegistryViewOptions.Registry32;
		}
		return registryViewOptions;
	}

	public static bool IsRegistryKeyUnderWow6432Node(string registryKeyPath)
	{
		string[] source = registryKeyPath.Split(new char[1] { '\\' });
		return source.Any((string part) => string.Compare(part, "Wow6432Node", StringComparison.OrdinalIgnoreCase) == 0);
	}

	public static bool IsRegistryKeyWritable(RegistryKey registryKey)
	{
		Type typeFromHandle = typeof(RegistryKey);
		MethodInfo method = typeFromHandle.GetMethod("IsWritable", BindingFlags.Instance | BindingFlags.NonPublic);
		object obj = method.Invoke(registryKey, null);
		return (bool)obj;
	}

	private static RegistryAccessRights GetRegistryAccessOptions(RegistryViewOptions registryViewOptions)
	{
		return registryViewOptions switch
		{
			RegistryViewOptions.Registry64 => RegistryAccessRights.KEY_WOW64_64KEY, 
			RegistryViewOptions.Registry32 => RegistryAccessRights.KEY_WOW64_32KEY, 
			_ => (RegistryAccessRights)0, 
		};
	}

	private static bool TryGetRegistryKeyInfo(RegistryKey registryKey, out SafeHandle registryKeyHandle, out RegistryKeyPermissionCheck registryKeyPermissionCheck)
	{
		Type typeFromHandle = typeof(RegistryKey);
		FieldInfo field = typeFromHandle.GetField("hkey", BindingFlags.Instance | BindingFlags.NonPublic);
		FieldInfo field2 = typeFromHandle.GetField("checkMode", BindingFlags.Instance | BindingFlags.NonPublic);
		try
		{
			registryKeyHandle = (SafeHandle)field.GetValue(registryKey);
			registryKeyPermissionCheck = (RegistryKeyPermissionCheck)field2.GetValue(registryKey);
			return true;
		}
		catch (Exception)
		{
			registryKeyHandle = null;
			registryKeyPermissionCheck = RegistryKeyPermissionCheck.Default;
			return false;
		}
	}

	private static bool TrySetRegistryKeyInfo(RegistryKey registryKey, string registryKeyName, RegistryKeyPermissionCheck registryKeyPermissionCheck)
	{
		Type typeFromHandle = typeof(RegistryKey);
		FieldInfo field = typeFromHandle.GetField("keyName", BindingFlags.Instance | BindingFlags.NonPublic);
		FieldInfo field2 = typeFromHandle.GetField("checkMode", BindingFlags.Instance | BindingFlags.NonPublic);
		try
		{
			field.SetValue(registryKey, registryKeyName);
			field2.SetValue(registryKey, registryKeyPermissionCheck);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static RegistryKey CreateRegistryKeyFromHandle(IntPtr safeHandleDangerousPointer, string registryKeyName, RegistryKeyPermissionCheck registryKeyPermissionCheck, bool isWriteable, bool ownsHandle, RegistryViewOptions registryViewOptions)
	{
		Assembly assembly = typeof(SafeHandle).Assembly;
		Type type = assembly.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");
		int major = Environment.Version.Major;
		RegistryKey registryKey;
		switch (major)
		{
		case 2:
		{
			SafeHandle safeHandle2 = (SafeHandle)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[2] { safeHandleDangerousPointer, ownsHandle }, null);
			registryKey = (RegistryKey)Activator.CreateInstance(typeof(RegistryKey), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[2] { safeHandle2, isWriteable }, null);
			break;
		}
		case 4:
		{
			string name = registryViewOptions switch
			{
				RegistryViewOptions.Default => "Default", 
				RegistryViewOptions.Registry32 => "Registry32", 
				RegistryViewOptions.Registry64 => "Registry64", 
				_ => throw new NotSupportedException(string.Concat("Specified member of enum RegistryViewOptions \"", registryViewOptions, "\" doesn't supported!")), 
			};
			Type type2 = assembly.GetType("Microsoft.Win32.RegistryView");
			object value = type2.GetField(name, BindingFlags.Static | BindingFlags.Public).GetValue(null);
			SafeHandle safeHandle = (SafeHandle)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public, null, new object[2] { safeHandleDangerousPointer, ownsHandle }, null);
			registryKey = (RegistryKey)Activator.CreateInstance(typeof(RegistryKey), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[3] { safeHandle, isWriteable, value }, null);
			break;
		}
		default:
			throw new ExecutionEngineException("Can't create registry key from handle! Invalid CLR version " + major + " detected!");
		}
		if (!TrySetRegistryKeyInfo(registryKey, registryKeyName, registryKeyPermissionCheck))
		{
			return null;
		}
		return registryKey;
	}

	private static RegistryKeyPermissionCheck GetRegistryKeyPermissonCheck(RegistryKeyPermissionCheck parentKeyCheckMode, bool subkeyWritable)
	{
		if (parentKeyCheckMode == RegistryKeyPermissionCheck.Default)
		{
			return parentKeyCheckMode;
		}
		if (!subkeyWritable)
		{
			return RegistryKeyPermissionCheck.ReadSubTree;
		}
		return RegistryKeyPermissionCheck.ReadWriteSubTree;
	}

	private static RegistryKey CreateRegistryKeyFromHandle(IntPtr hKey, bool writable, bool ownsHandle)
	{
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
		Type type = typeof(SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");
		Type[] types = new Type[2]
		{
			typeof(IntPtr),
			typeof(bool)
		};
		ConstructorInfo constructor = type.GetConstructor(bindingAttr, null, types, null);
		object obj = constructor.Invoke(new object[2] { hKey, ownsHandle });
		Type typeFromHandle = typeof(RegistryKey);
		Type[] types2 = new Type[2]
		{
			type,
			typeof(bool)
		};
		ConstructorInfo constructor2 = typeFromHandle.GetConstructor(bindingAttr, null, types2, null);
		return (RegistryKey)constructor2.Invoke(new object[2] { obj, writable });
	}
}
