using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CommonUtils.ParseUtils;
using CommonUtils.ReportUtils;
using OracleUtils.Scanner;

namespace OracleUtils.Operation;

public static class SqlPlusUtils
{
	private static string GetDatabaseVersion(OracleClientInstallation clientInstallation, string user = "SYS", string password = "smpwd", bool isSysDba = true)
	{
		if (clientInstallation == null)
		{
			throw new ArgumentNullException("clientInstallation", "Oracle client installation can't be unspecified!");
		}
		if (string.IsNullOrEmpty(clientInstallation.OciDirectoryPath) || !clientInstallation.HasSqlPlusUtility)
		{
			return "UNDEFINED";
		}
		string text = Path.Combine(clientInstallation.OciDirectoryPath, "sqlplus.exe");
		try
		{
			string text2 = Path.Combine(Path.GetTempPath(), "sqlplusrequest.sql");
			string text3 = Path.Combine(Path.GetTempPath(), "sqlplusresult.lst");
			string text4 = Path.Combine(Path.GetTempPath(), "sqlplus.log");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("whenever oserror exit sql.oscode rollback;");
			stringBuilder.AppendLine("whenever sqlerror exit sql.sqlcode rollback;");
			stringBuilder.AppendLine("spool \"" + text3 + "\";");
			stringBuilder.AppendLine("select value from nls_database_parameters where parameter='NLS_RDBMS_VERSION';");
			stringBuilder.AppendLine("spool off;");
			stringBuilder.AppendLine("show errors;");
			stringBuilder.AppendLine("disconnect;");
			stringBuilder.AppendLine("exit;");
			File.WriteAllText(text2, stringBuilder.ToString(), Encoding.Default);
			string text5 = string.Format("{0}/{1}{2} < \"{3}\" > \"{4}\"", new object[5]
			{
				user,
				password,
				isSysDba ? " as sysdba" : string.Empty,
				text2,
				text4
			});
			UtilsLoggerWrapper.Info($"External launching SQLPLUS: {text} {text5}");
			ProcessStartInfo processStartInfo = new ProcessStartInfo(text, text5);
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			ProcessStartInfo startInfo = processStartInfo;
			using (Process process = Process.Start(startInfo))
			{
				if (process != null)
				{
					process.WaitForExit(3000);
					if (process.ExitCode != 0)
					{
						UtilsLoggerWrapper.Error("SQLPLUS process was finished with errors, its exit code:" + process.ExitCode);
					}
				}
			}
			string[] source = File.ReadAllLines(text3);
			string text6 = source.LastOrDefault((string line) => !string.IsNullOrEmpty(line) && !line.StartsWith("SQL>", StringComparison.OrdinalIgnoreCase));
			if (!string.IsNullOrEmpty(text6))
			{
				string text7 = text6.Trim();
				if (text7.TryParseToVersion(out var _))
				{
					return text7;
				}
			}
		}
		catch (Exception ex)
		{
			UtilsLoggerWrapper.Error("Can't get database version. Exception occurred: " + ex);
		}
		return "UNDEFINED";
	}
}
