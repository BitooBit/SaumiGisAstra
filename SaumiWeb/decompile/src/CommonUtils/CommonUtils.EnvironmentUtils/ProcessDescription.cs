using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CommonUtils.EnvironmentUtils;

public class ProcessDescription
{
	private ProcessModule _MainModule;

	private ProcessModuleCollection _LoadedModules;

	private IntPtr? _MainWindowHandle;

	private string _MainWindowTitle;

	public Process DirectProcess { get; private set; }

	public ProcessModule MainModule
	{
		get
		{
			if (_MainModule == null)
			{
				Process directProcess = DirectProcess;
				_MainModule = directProcess.MainModule;
			}
			return _MainModule;
		}
	}

	public ProcessModuleCollection LoadedModules
	{
		get
		{
			if (_LoadedModules == null)
			{
				Process directProcess = DirectProcess;
				_LoadedModules = directProcess.Modules;
			}
			return _LoadedModules;
		}
	}

	public IntPtr MainWindowHandle
	{
		get
		{
			if (!_MainWindowHandle.HasValue)
			{
				Process directProcess = DirectProcess;
				_MainWindowHandle = directProcess.MainWindowHandle;
			}
			return _MainWindowHandle.Value;
		}
	}

	public string MainWindowTitle
	{
		get
		{
			if (_MainWindowTitle == null)
			{
				IntPtr mainWindowHandle = MainWindowHandle;
				if (mainWindowHandle == IntPtr.Zero)
				{
					_MainWindowTitle = string.Empty;
				}
				else
				{
					Process directProcess = DirectProcess;
					_MainWindowTitle = directProcess.MainWindowTitle;
				}
			}
			return _MainWindowTitle;
		}
	}

	public ProcessDescription()
		: this(Process.GetCurrentProcess())
	{
	}

	public ProcessDescription(Process directProcess)
	{
		DirectProcess = directProcess;
	}

	public bool IsModuleLoaded(string moduleName)
	{
		foreach (ProcessModule loadedModule in LoadedModules)
		{
			if (string.Compare(loadedModule.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsModuleLoaded(string moduleName, out string originalModuleName)
	{
		foreach (ProcessModule loadedModule in LoadedModules)
		{
			if (string.Compare(loadedModule.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				originalModuleName = loadedModule.ModuleName;
				return true;
			}
		}
		originalModuleName = null;
		return false;
	}

	public IEnumerable<string> GetAllLoadedModuleNamesFrom(string directoryPath)
	{
		IList<string> list = new List<string>();
		foreach (ProcessModule loadedModule in LoadedModules)
		{
			if (loadedModule.FileName.ToUpper().Contains(directoryPath.ToUpper()))
			{
				list.Add(loadedModule.ModuleName);
			}
		}
		return list;
	}

	public string ToString(bool isFullLoadedModulesInfoRequired)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Process \"" + MainModule.ModuleName + "\" decription: {");
		stringBuilder.AppendLine("Location: " + MainModule.FileName + ";");
		if (MainWindowHandle != IntPtr.Zero)
		{
			stringBuilder.AppendLine("MainWindow: " + MainWindowTitle + " (handle " + MainWindowHandle + ");");
		}
		int count = LoadedModules.Count;
		if (count > 0)
		{
			stringBuilder.AppendLine(count + " loaded modules: {");
			foreach (ProcessModule loadedModule in LoadedModules)
			{
				string moduleName = loadedModule.ModuleName;
				string fileName = loadedModule.FileName;
				FileVersionInfo fileVersionInfo = loadedModule.FileVersionInfo;
				stringBuilder.AppendLine(isFullLoadedModulesInfoRequired ? fileVersionInfo.ToString() : (moduleName + " - " + fileName));
			}
			stringBuilder.AppendLine("}");
		}
		stringBuilder.AppendLine("}");
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		return ToString(isFullLoadedModulesInfoRequired: false);
	}
}
