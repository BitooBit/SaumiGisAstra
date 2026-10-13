using System;
using System.Collections.Generic;

namespace CommonUtils.ParseUtils;

public static class StringVersionParseExtension
{
	private static string TrimRedundantVersionDelimiters(string versionString)
	{
		IList<int> list = new List<int>();
		int num = -1;
		while ((num = versionString.IndexOf('.', num + 1)) >= 0)
		{
			list.Add(num);
		}
		if (list.Count > 3)
		{
			return versionString.Remove(list[3]);
		}
		return versionString;
	}

	public static bool TryParseToVersion(this string versionString, out Version version)
	{
		try
		{
			string version2 = TrimRedundantVersionDelimiters(versionString);
			version = new Version(version2);
			return true;
		}
		catch (Exception)
		{
			version = null;
			return false;
		}
	}
}
