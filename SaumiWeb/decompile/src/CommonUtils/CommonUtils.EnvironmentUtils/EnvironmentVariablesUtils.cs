using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CommonUtils.ReportUtils;

namespace CommonUtils.EnvironmentUtils;

public static class EnvironmentVariablesUtils
{
	public const char VARIABLE_VALUES_SEPARATOR = ';';

	public const string PATH_VARIABLE_NAME = "PATH";

	public static void PrintVariableValue(string variableName)
	{
		string environmentVariable = Environment.GetEnvironmentVariable(variableName);
		string environmentVariable2 = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process);
		string environmentVariable3 = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);
		string environmentVariable4 = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);
		UtilsLoggerWrapper.Info($"{variableName} =\n{environmentVariable}");
		UtilsLoggerWrapper.Info($"{variableName} per {EnvironmentVariableTarget.Process} =\n{environmentVariable2}");
		UtilsLoggerWrapper.Info($"{variableName} per {EnvironmentVariableTarget.User} =\n{environmentVariable3}");
		UtilsLoggerWrapper.Info($"{variableName} per {EnvironmentVariableTarget.Machine} =\n{environmentVariable4}");
	}

	public static string GetPersistentVariableActualValue(string variableName)
	{
		string environmentVariable = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);
		string environmentVariable2 = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);
		if (string.Compare(variableName, "PATH", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return $"{environmentVariable2}{((string.IsNullOrEmpty(environmentVariable2) || string.IsNullOrEmpty(environmentVariable)) ? string.Empty : ';'.ToString(CultureInfo.InvariantCulture))}{environmentVariable}";
		}
		if (environmentVariable != null)
		{
			return environmentVariable;
		}
		return environmentVariable2;
	}

	public static bool IsPersistentVariableValueChanged(string variableName)
	{
		string environmentVariable = Environment.GetEnvironmentVariable(variableName);
		string persistentVariableActualValue = GetPersistentVariableActualValue(variableName);
		if (string.IsNullOrEmpty(environmentVariable) || !environmentVariable.Equals(persistentVariableActualValue, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	public static void UpdatePersistentVariableValue(string variableName)
	{
		if (IsPersistentVariableValueChanged(variableName))
		{
			string persistentVariableActualValue = GetPersistentVariableActualValue(variableName);
			Environment.SetEnvironmentVariable(variableName, persistentVariableActualValue);
		}
	}

	public static bool TryGetVariableValue(string variableName, out string variableValue, out EnvironmentVariableTarget variableRealTarget)
	{
		string environmentVariable = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process);
		string environmentVariable2 = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);
		string environmentVariable3 = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);
		if (environmentVariable != null)
		{
			variableRealTarget = EnvironmentVariableTarget.Process;
			if (string.Compare(environmentVariable, environmentVariable2, StringComparison.OrdinalIgnoreCase) == 0)
			{
				variableRealTarget = EnvironmentVariableTarget.User;
			}
			if (string.Compare(environmentVariable, environmentVariable3, StringComparison.OrdinalIgnoreCase) == 0)
			{
				variableRealTarget = EnvironmentVariableTarget.Machine;
			}
			variableValue = environmentVariable;
			return true;
		}
		if (!string.IsNullOrEmpty(environmentVariable2))
		{
			variableRealTarget = EnvironmentVariableTarget.User;
			variableValue = environmentVariable2;
			return true;
		}
		if (!string.IsNullOrEmpty(environmentVariable3))
		{
			variableRealTarget = EnvironmentVariableTarget.Machine;
			variableValue = environmentVariable3;
			return true;
		}
		variableValue = null;
		variableRealTarget = EnvironmentVariableTarget.Process;
		return false;
	}

	public static void SetProcessVariable(string variableName, string variableValue)
	{
		Environment.SetEnvironmentVariable(variableName, variableValue, EnvironmentVariableTarget.Process);
	}

	public static void SetUserVariable(string variableName, string variableValue, bool shouldUpdateProcessVariable = true)
	{
		Environment.SetEnvironmentVariable(variableName, variableValue, EnvironmentVariableTarget.User);
		if (shouldUpdateProcessVariable)
		{
			UpdatePersistentVariableValue(variableName);
		}
	}

	public static void SetMachineVariable(string variableName, string variableValue, bool shouldUpdateProcessVariable = true)
	{
		Environment.SetEnvironmentVariable(variableName, variableValue, EnvironmentVariableTarget.Machine);
		if (shouldUpdateProcessVariable)
		{
			UpdatePersistentVariableValue(variableName);
		}
	}

	public static Process CreateChildProcessUpdatingPersistentVariable(string filename, string args, string variableName)
	{
		Process process = new Process();
		process.StartInfo.FileName = filename;
		process.StartInfo.Arguments = args;
		Process process2 = process;
		if (IsPersistentVariableValueChanged(variableName))
		{
			string persistentVariableActualValue = GetPersistentVariableActualValue(variableName);
			process2.StartInfo.UseShellExecute = false;
			process2.StartInfo.EnvironmentVariables[variableName] = persistentVariableActualValue;
		}
		return process2;
	}

	public static bool ContainsPersistentVariableCustomValue(string variableName, string customValue)
	{
		string persistentVariableActualValue = GetPersistentVariableActualValue(variableName);
		if (persistentVariableActualValue != null && persistentVariableActualValue.IndexOf(customValue, StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return true;
		}
		return false;
	}

	public static int GetPersistentVariableCustomValuePosition(string variableName, string customValue)
	{
		if (!string.IsNullOrEmpty(customValue))
		{
			string persistentVariableActualValue = GetPersistentVariableActualValue(variableName);
			if (persistentVariableActualValue != null)
			{
				int num = persistentVariableActualValue.IndexOf(customValue, StringComparison.OrdinalIgnoreCase);
				if (num >= 0)
				{
					int num2 = 0;
					int num3 = persistentVariableActualValue.IndexOf(';', 0, num);
					while (num3 >= 0 && num3 < num)
					{
						int num4 = num3 + 1;
						int count = num - num4;
						num3 = persistentVariableActualValue.IndexOf(';', num4, count);
						num2++;
					}
					return num2 + 1;
				}
			}
		}
		return -1;
	}

	public static void PlaceCustomValueFirstInPersistentVariable(string variableName, string customValue)
	{
		string text = customValue.Trim().TrimEnd(new char[3]
		{
			';',
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar
		}).TrimEnd(new char[0]);
		string persistentVariableActualValue = GetPersistentVariableActualValue(variableName);
		List<string> list = new List<string>(persistentVariableActualValue.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries));
		for (int i = 0; i < list.Count; i++)
		{
			string strA = list[i].Trim().TrimEnd(new char[2]
			{
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar
			}).TrimEnd(new char[0]);
			if (string.Compare(strA, text, StringComparison.OrdinalIgnoreCase) == 0)
			{
				list.RemoveAt(i);
			}
		}
		list.Insert(0, text);
		string variableValue = list.Aggregate(string.Empty, (string current, string subValue) => current + subValue + ';').TrimEnd(new char[1] { ';' });
		SetMachineVariable(variableName, variableValue);
	}
}
