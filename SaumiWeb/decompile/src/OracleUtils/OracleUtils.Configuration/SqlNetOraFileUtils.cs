using System;
using System.IO;
using System.Text;
using CommonUtils.ReportUtils;

namespace OracleUtils.Configuration;

public static class SqlNetOraFileUtils
{
	private const char COMMENT_CHAR = '#';

	private const char EQUALS_CHAR = '=';

	private static bool ProvideFolderExistence(string targetFolder)
	{
		if (Directory.Exists(targetFolder))
		{
			return true;
		}
		try
		{
			UtilsLoggerWrapper.Info($"Directory \"{targetFolder}\" doesn't exists. It will be created.");
			Directory.CreateDirectory(targetFolder);
			UtilsLoggerWrapper.Info($"Directory \"{targetFolder}\" was successfully created.");
			return true;
		}
		catch (Exception arg)
		{
			UtilsLoggerWrapper.Error($"ERROR! Can't create directory \"{targetFolder}\"! Exception occurred: \"{arg}\".");
			return false;
		}
	}

	public static bool SetConfiguration(string path, string parameterName, string parameterValue)
	{
		UtilsLoggerWrapper.Info($"Setting parameter \"{parameterName}\" to value \"{parameterValue}\" at configuration file \"{path}\".");
		if (!File.Exists(path))
		{
			UtilsLoggerWrapper.Info($"Configuration file \"{path}\" doesn't exists");
			if (!ProvideFolderExistence(Path.GetDirectoryName(path)))
			{
				UtilsLoggerWrapper.Error($"Can't provide folder existence for creating specified configuration file \"{path}\"!");
				return false;
			}
			UtilsLoggerWrapper.Info(string.Format("Creating file \"{0}\" and writing setting \"{1} {2} {3}\" to this file", new object[4] { path, parameterName, '=', parameterValue }));
			try
			{
				File.WriteAllText(path, $"{parameterName} {'='} {parameterValue}", Encoding.Default);
				UtilsLoggerWrapper.Info("Above configuration file was successfully created.");
				return true;
			}
			catch (Exception arg)
			{
				UtilsLoggerWrapper.Error($"ERROR! Can't write configuration text to file \"{path}\"! Exception occurred: \"{arg}\"");
				return false;
			}
		}
		UtilsLoggerWrapper.Info($"Configuration file \"{path}\" exists. It will be modified.");
		UtilsLoggerWrapper.Info($"Searching and replacing \"{parameterName}\" value to \"{parameterValue}\".");
		UtilsLoggerWrapper.Info($"Reading all lines from file \"{path}\" and modifying or appending parameter \"{parameterName}\".");
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = File.ReadAllLines(path, Encoding.Default);
		bool flag = false;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!text.StartsWith('#'.ToString()) && text.Contains('='.ToString()))
			{
				int num = text.IndexOf('='.ToString());
				string text2 = text.Substring(0, num).Trim();
				string text3 = text.Substring(num + 1, text.Length - num - 1).Trim();
				if (text2.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
				{
					UtilsLoggerWrapper.Info($"Specified parameter \"{text2}\" is currently set to \"{text3}\".");
					flag = true;
					if (!text3.Equals(parameterValue, StringComparison.OrdinalIgnoreCase))
					{
						stringBuilder.AppendLine($"{'#'}{text}");
						UtilsLoggerWrapper.Info("Previous parameter setting was disabled.");
						stringBuilder.AppendLine($"{parameterName} {'='} {parameterValue}");
						UtilsLoggerWrapper.Info($"Above parameter was set to value \"{parameterValue}\".");
					}
					else
					{
						stringBuilder.AppendLine(text);
						UtilsLoggerWrapper.Info("Above parameter was already set to specified value.");
					}
				}
				else
				{
					stringBuilder.AppendLine(text);
				}
			}
			else
			{
				stringBuilder.AppendLine(text);
			}
		}
		if (!flag)
		{
			UtilsLoggerWrapper.Info($"Parameter \"{parameterName}\" was not found at configuration file.");
			stringBuilder.AppendLine($"{parameterName} {'='} {parameterValue}");
			UtilsLoggerWrapper.Info($"Specified parameter setting \"{parameterName} {'='} {parameterValue}\" was appended to configuration file.");
		}
		UtilsLoggerWrapper.Info($"Rewriting configuration file \"{path}\" with modified content.");
		try
		{
			File.WriteAllText(path, stringBuilder.ToString(), Encoding.Default);
		}
		catch (Exception arg2)
		{
			UtilsLoggerWrapper.Error($"ERROR! Can't write configuration text to file \"{path}\"! Exception occurred: \"{arg2}\".");
			return false;
		}
		UtilsLoggerWrapper.Info("Target configuration file was successfully rewrited.");
		return true;
	}
}
