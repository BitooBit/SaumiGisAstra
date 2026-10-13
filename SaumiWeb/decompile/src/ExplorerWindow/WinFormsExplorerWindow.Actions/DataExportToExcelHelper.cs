namespace WinFormsExplorerWindow.Actions;

public static class DataExportToExcelHelper
{
	private const int SHEET_NAME_MAX_LENGTH = 31;

	private const string SHEET_NAME_MAX_LENGTH_COMPLETION = "...";

	private const char SHEET_NAME_INVALID_CHAR_REPLACER = '_';

	private static readonly char[] SHEET_NAME_INVALID_CHARS = new char[7] { ':', '\\', '/', '?', '*', '[', ']' };

	public static string GetValidSheetName(string sheetName)
	{
		if (sheetName.Length > 31)
		{
			sheetName = sheetName.Remove(31 - "...".Length) + "...";
		}
		char[] sHEET_NAME_INVALID_CHARS = SHEET_NAME_INVALID_CHARS;
		foreach (char oldChar in sHEET_NAME_INVALID_CHARS)
		{
			sheetName = sheetName.Replace(oldChar, '_');
		}
		return sheetName;
	}
}
