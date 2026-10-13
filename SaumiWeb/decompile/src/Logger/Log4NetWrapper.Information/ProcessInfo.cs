using System;
using System.Diagnostics;
using System.Text;

namespace Log4NetWrapper.Information;

public static class ProcessInfo
{
	public static string CallingProcessDescription
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				Process currentProcess = Process.GetCurrentProcess();
				ProcessModule mainModule = currentProcess.MainModule;
				string text = ((currentProcess.MainWindowHandle != IntPtr.Zero) ? currentProcess.MainWindowTitle : null);
				stringBuilder.AppendLine("Process: " + mainModule.ModuleName + "\n\tlocation: " + mainModule.FileName + ((text != null) ? ("\n\ttitle:" + text) : ""));
				ProcessModuleCollection modules = currentProcess.Modules;
				if (modules != null && modules.Count > 0)
				{
					stringBuilder.AppendLine("Process loaded modules:");
					foreach (ProcessModule module in currentProcess.Modules)
					{
						string moduleName = mainModule.ModuleName;
						string fileName = mainModule.FileName;
						FileVersionInfo fileVersionInfo = module.FileVersionInfo;
						stringBuilder.AppendLine("Module: " + ((fileVersionInfo != null) ? fileVersionInfo.ToString() : (moduleName + "\n\tlocation: " + fileName)));
					}
				}
			}
			catch (Exception)
			{
				stringBuilder.AppendLine("Can't get info about calling process");
			}
			return stringBuilder.ToString();
		}
	}
}
