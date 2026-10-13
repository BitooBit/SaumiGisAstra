using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CommonUtils.ReportUtils;
using OracleUtils.Connection;

namespace OracleUtils.Operation;

public static class SqlScriptExecutor
{
	public static bool Execute(string odpNetAssemblyPath, string sqlScript, string host, string port, string service, string user, string password, bool sysDbaMode)
	{
		string fullConnectionString = ConnectionUtils.GetFullConnectionString(host, port, service, user, password, sysDbaMode ? ConnectionPrivileges.SYSDBA : ConnectionPrivileges.NORMAL);
		Exception catchedException = null;
		if (!ConnectionUtils.TryOpenConnection(fullConnectionString, odpNetAssemblyPath, out var connection, out catchedException))
		{
			UtilsLoggerWrapper.Error("Can't open connection to Oracle database! Exception occurred: " + catchedException);
			return false;
		}
		bool flag;
		ReadOnlyCollection<DbCommandResult> results;
		using (IDbCommand command = connection.CreateCommand())
		{
			flag = Execute(command, sqlScript, skipErrors: false, out results);
		}
		ConnectionUtils.CloseConnection(connection);
		if (!flag)
		{
			foreach (DbCommandResult item in results)
			{
				if (item.ExecuteException != null)
				{
					UtilsLoggerWrapper.Error(string.Concat("Can't execute SQL script command: \"", item, "\""));
				}
			}
		}
		return flag;
	}

	public static bool ExecuteFromFile(string odpNetAssemblyPath, string sqlFilePath, string host, string port, string service, string user, string password, bool sysDbaMode)
	{
		if (!File.Exists(sqlFilePath))
		{
			UtilsLoggerWrapper.Error("Can't find specified SQL-script file \"" + sqlFilePath + "\"!");
			return false;
		}
		string sqlScript = File.ReadAllText(sqlFilePath, Encoding.Default);
		return Execute(odpNetAssemblyPath, sqlScript, host, port, service, user, password, sysDbaMode);
	}

	public static bool Execute(IDbCommand command, string sqlScript, bool skipErrors, out ReadOnlyCollection<DbCommandResult> results)
	{
		UtilsLoggerWrapper.Info("SQL script executing is started");
		CommandType commandType = command.CommandType;
		sqlScript = Regex.Replace(sqlScript, "/\\*.*?\\*/", string.Empty, RegexOptions.Singleline);
		IList<DbCommandResult> list = new List<DbCommandResult>();
		int num = 0;
		StringReader stringReader = new StringReader(sqlScript);
		bool flag = false;
		int num2 = 0;
		int num3 = 0;
		StringBuilder stringBuilder = new StringBuilder();
		string text;
		while ((text = stringReader.ReadLine()) != null)
		{
			if (text.TrimStart(new char[0]).StartsWith("--"))
			{
				continue;
			}
			stringBuilder.AppendLine(text);
			if (Regex.IsMatch(text, "declare", RegexOptions.IgnoreCase) || Regex.IsMatch(text, "create\\s*?(\\s*?|or replace)\\s*?procedure\\s+?(\\S*?)\\s+?.*?\\s+?is", RegexOptions.IgnoreCase) || Regex.IsMatch(text, "create\\s*?(\\s*?|or replace)\\s*?function\\s+?(\\S*?)\\s+?.*?\\s+?return.*?\\s+?(is|as)", RegexOptions.IgnoreCase))
			{
				flag = true;
			}
			if (Regex.IsMatch(text, "begin", RegexOptions.IgnoreCase) || Regex.IsMatch(text, "create\\s*?(\\s*?|or replace)\\s*?package.*?\\s+?is", RegexOptions.IgnoreCase))
			{
				flag = false;
				num2++;
			}
			if (Regex.IsMatch(text, "execute immediate\\s*?\\(\\s*?'", RegexOptions.IgnoreCase))
			{
				num3++;
			}
			if (num3 > 0 && Regex.IsMatch(text, "'\\s*?\\)\\s*?;", RegexOptions.IgnoreCase))
			{
				num3--;
			}
			if (num2 > 0 && Regex.IsMatch(text, "end(?!\\s+?if\\s*?;|\\s+?loop\\s*?;)(\\s*?|\\s+?\\S*?)\\s*?;", RegexOptions.IgnoreCase))
			{
				num2--;
			}
			string value = ';'.ToString(CultureInfo.InvariantCulture);
			if (num2 != 0 || num3 != 0 || flag || !text.TrimEnd(new char[0]).EndsWith(value))
			{
				continue;
			}
			string text2 = stringBuilder.ToString().Trim(new char[4] { ' ', '\r', '\n', '/' });
			text2 = text2.Replace(Environment.NewLine, '\n'.ToString());
			string text3 = text2.TrimEnd(new char[1] { ';' });
			bool flag2 = text3.Contains(value);
			command.CommandType = CommandType.Text;
			command.CommandText = ((!flag2) ? text3 : text2);
			command.Parameters.Clear();
			DbCommandResult dbCommandResult = new DbCommandResult(command.CommandType, command.CommandText);
			list.Add(dbCommandResult);
			try
			{
				UtilsLoggerWrapper.Info("Executing command \"" + command.CommandText + "\"");
				int num4 = command.ExecuteNonQuery();
				dbCommandResult.SetSuccessResult(num4);
				UtilsLoggerWrapper.Info("Above command was successfully executed. Executing result: \"" + num4 + "\".");
			}
			catch (Exception ex)
			{
				UtilsLoggerWrapper.Error("Can't execute above command! Exception occurred: " + ex);
				dbCommandResult.SetFailureResult(ex);
				num++;
				if (!skipErrors)
				{
					command.CommandType = commandType;
					results = new ReadOnlyCollection<DbCommandResult>(list);
					UtilsLoggerWrapper.Error("SQL script executing will be stopped because of an error");
					return false;
				}
				UtilsLoggerWrapper.Warning("Skip errors flag is specified, that's why SQL script executing will be continued");
			}
			stringBuilder = new StringBuilder();
		}
		command.CommandType = commandType;
		results = new ReadOnlyCollection<DbCommandResult>(list);
		if (num == 0)
		{
			UtilsLoggerWrapper.Info("SQL script executing was completed successfully");
		}
		else
		{
			UtilsLoggerWrapper.Warning("SQL script executing was completed with some errors");
		}
		return num == 0;
	}
}
