using System.Linq;

namespace CommonUtils.ParseUtils;

public static class ExitCodesParseUtils
{
	public const char SPACE_CHAR = ' ';

	public const char DEFAULT_EXIT_CODES_SEPARATOR = ',';

	private static char _ExitCodesSeparator = ',';

	public static char ExitCodesSeparator
	{
		get
		{
			return _ExitCodesSeparator;
		}
		set
		{
			_ExitCodesSeparator = value;
		}
	}

	public static bool ToIntArray(string exitCodesStringValue, out int[] exitCodes, out string badStringValue)
	{
		string[] array = exitCodesStringValue.Split(new char[1] { ExitCodesSeparator });
		int num = array.Length;
		if (num > 0)
		{
			exitCodes = new int[num];
			badStringValue = null;
			for (int i = 0; i < num; i++)
			{
				string text = array[i];
				if (!int.TryParse(text, out var result))
				{
					exitCodes = null;
					badStringValue = text;
					return false;
				}
				exitCodes[i] = result;
			}
			return true;
		}
		exitCodes = null;
		badStringValue = null;
		return false;
	}

	public static string ToString(int[] exitCodes, bool insertSpaces = true)
	{
		return insertSpaces ? exitCodes.Aggregate(string.Empty, (string current, int successExitCode) => current + ExitCodesSeparator + ' ' + successExitCode).TrimStart(new char[2] { ExitCodesSeparator, ' ' }) : exitCodes.Aggregate(string.Empty, (string current, int successExitCode) => current + ExitCodesSeparator + successExitCode).TrimStart(new char[1] { ExitCodesSeparator });
	}
}
