using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonUtils.IOUtils;

public static class IniUtils
{
	private const int DEFAULT_VALUE_LENGTH = 255;

	[DllImport("kernel32")]
	private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

	[DllImport("kernel32")]
	private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

	private static void CheckRequiredFileExistense(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"File {filePath} doesn't exist!");
		}
	}

	public static bool IniWriteValue(string iniFilePath, string sectionName, string keyName, string keyValue, bool throwOnNonexistentFile = false)
	{
		if (throwOnNonexistentFile)
		{
			CheckRequiredFileExistense(iniFilePath);
		}
		return WritePrivateProfileString(sectionName, keyName, keyValue, iniFilePath);
	}

	public static string IniReadValue(string iniFilePath, string sectionName, string keyName, bool throwOnNonexistentFile = false)
	{
		if (throwOnNonexistentFile)
		{
			CheckRequiredFileExistense(iniFilePath);
		}
		for (int num = 255; num < int.MaxValue; num *= 2)
		{
			StringBuilder stringBuilder = new StringBuilder(num);
			int privateProfileString = GetPrivateProfileString(sectionName, keyName, string.Empty, stringBuilder, num, iniFilePath);
			if (privateProfileString < num - 1)
			{
				return stringBuilder.ToString();
			}
		}
		return string.Empty;
	}

	public static bool IniReadValue(string iniFilePath, string sectionName, string keyName, string defaultValue, out string keyResultValue)
	{
		string text = IniReadValue(iniFilePath, sectionName, keyName);
		if (string.IsNullOrEmpty(text))
		{
			keyResultValue = defaultValue;
			return false;
		}
		keyResultValue = text;
		return true;
	}
}
