using System;
using System.IO;
using CommonUtils.IOUtils;

namespace WinFormsExplorerWindow;

public class ExplorerWindowOptions
{
	private const string SAUMI_INI_NAME = "saumi.ini";

	private const string SAUMI_USER_PARAMS = "saumi_user_params.ini";

	private const string EXP_WINDOW_SECTION = "ExplorerWindow";

	private const string SAUMI_NAME = "Saumi";

	private const string COLUMN_HEADER_WORD_WRAP_PARAM = "ColumnHeaderWordWrap";

	private const string CELL_VALUE_WORD_WRAP_PARAM = "CellValueWordWrap";

	private string _CurrentConfigFilePath;

	private string UserAppDataSaumiIniPath => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Saumi"), "saumi_user_params.ini");

	private string AppDirSaumiIniPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saumi.ini");

	public bool ColumnHeaderWordWrap { get; private set; }

	public bool CellValueWordWrap { get; private set; }

	public ExplorerWindowOptions()
	{
		FillDefaultValues();
		if (CheckConfigFileExistense())
		{
			ReadValues();
		}
	}

	private void FillDefaultValues()
	{
		CellValueWordWrap = false;
		ColumnHeaderWordWrap = false;
	}

	private void ReadValues()
	{
		try
		{
			string value = IniUtils.IniReadValue(_CurrentConfigFilePath, "ExplorerWindow", "ColumnHeaderWordWrap");
			string value2 = IniUtils.IniReadValue(_CurrentConfigFilePath, "ExplorerWindow", "CellValueWordWrap");
			if (!string.IsNullOrEmpty(value))
			{
				ColumnHeaderWordWrap = Convert.ToInt32(value) == 1;
			}
			if (!string.IsNullOrEmpty(value2))
			{
				CellValueWordWrap = Convert.ToInt32(value2) == 1;
			}
		}
		catch (Exception)
		{
			FillDefaultValues();
		}
	}

	private bool CheckConfigFileExistense()
	{
		if (File.Exists(UserAppDataSaumiIniPath))
		{
			_CurrentConfigFilePath = UserAppDataSaumiIniPath;
			return true;
		}
		if (File.Exists(AppDirSaumiIniPath))
		{
			_CurrentConfigFilePath = AppDirSaumiIniPath;
			return true;
		}
		return false;
	}
}
