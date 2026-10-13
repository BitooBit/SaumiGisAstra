namespace InflexCore.DataImplementation.Reflection;

internal static class SqlStringExtentions
{
	public static string ReplaceFormatChar(this string self, string formatStr, string replaceStr)
	{
		string text = self.ToLower();
		string value = formatStr.ToLower();
		int length = formatStr.Length;
		int length2 = replaceStr.Length;
		int startIndex = 0;
		while (true)
		{
			int num = text.IndexOf(value, startIndex);
			if (num == -1)
			{
				break;
			}
			text = text.Remove(num, length);
			text = text.Insert(num, replaceStr);
			self = self.Remove(num, length);
			self = self.Insert(num, replaceStr);
			startIndex = num + length2;
		}
		return self;
	}
}
