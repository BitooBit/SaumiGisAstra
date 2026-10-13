using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CommonUtils.EnvironmentUtils;

public static class ProcessUtils
{
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr LoadLibrary(string lpFileName);

	[DllImport("kernel32")]
	private static extern IntPtr GetModuleHandle(string module);

	[DllImport("kernel32")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool FreeLibrary(IntPtr handle);

	[DllImport("ntdll.dll", SetLastError = true)]
	private static extern int NtQueryInformationProcess(IntPtr hProcess, ProcessInfoClass pic, ref ProcessBasicInformation pbi, int cb, out int pSize);

	public static bool IsModuleLoadedToCurrentProcess(string moduleName)
	{
		ProcessDescription processDescription = new ProcessDescription();
		return processDescription.IsModuleLoaded(moduleName);
	}

	public static bool IsModuleLoadedToCurrentProcess(string moduleName, out string originalModuleName)
	{
		ProcessDescription processDescription = new ProcessDescription();
		return processDescription.IsModuleLoaded(moduleName, out originalModuleName);
	}

	public static IEnumerable<string> GetAllLoadedModuleNamesToCurrentProcessFrom(string directoryPath)
	{
		ProcessDescription processDescription = new ProcessDescription();
		return processDescription.GetAllLoadedModuleNamesFrom(directoryPath);
	}

	public static bool UnloadModuleFromCurrentProcess(string moduleName)
	{
		bool result = false;
		IntPtr moduleHandle;
		while ((moduleHandle = GetModuleHandle(moduleName)) != IntPtr.Zero)
		{
			result = FreeLibrary(moduleHandle);
		}
		return result;
	}

	public static IntPtr LoadModuleToCurrentProcess(string moduleFullPath)
	{
		return LoadLibrary(moduleFullPath);
	}

	public static bool TryGetParentProcess(out Process parentProcess, out Exception exception)
	{
		Process currentProcess = Process.GetCurrentProcess();
		return TryGetParentProcess(currentProcess.Handle, out parentProcess, out exception);
	}

	public static bool TryGetParentProcess(int childProcessId, out Process parentProcess, out Exception exception)
	{
		Process processById = Process.GetProcessById(childProcessId);
		return TryGetParentProcess(processById.Handle, out parentProcess, out exception);
	}

	public static bool TryGetParentProcess(IntPtr childProcessHandle, out Process parentProcess, out Exception exception)
	{
		ProcessBasicInformation pbi = default(ProcessBasicInformation);
		int num = NtQueryInformationProcess(childProcessHandle, ProcessInfoClass.ProcessBasicInformation, ref pbi, Marshal.SizeOf((object)pbi), out var _);
		if (num != 0)
		{
			parentProcess = null;
			exception = new Win32Exception(num);
			return false;
		}
		try
		{
			int processId = pbi.InheritedFromUniqueProcessId.ToInt32();
			parentProcess = Process.GetProcessById(processId);
			exception = null;
			return true;
		}
		catch (Exception ex)
		{
			parentProcess = null;
			exception = ex;
			return false;
		}
	}
}
