using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace InflexCLR.Environment;

internal static class HookExitSaumi
{
	private delegate IntPtr GetMsgDelegate(int nCode, IntPtr wParam, IntPtr lParam);

	private const int WH_GETMESSAGE = 3;

	private const int HC_ACTION = 0;

	private const int WM_DESTROY = 2;

	private const int WM_QUIT = 18;

	private static readonly List<ISaumiPlugInExitable> plugins = new List<ISaumiPlugInExitable>();

	private static IntPtr pHook = IntPtr.Zero;

	private static IntPtr hwndMain = IntPtr.Zero;

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	private static extern IntPtr GetParent(IntPtr hWnd);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, GetMsgDelegate lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("kernel32.dll")]
	private static extern int GetLastError();

	private static void Apply()
	{
		if (!IntPtr.Zero.Equals((object?)(nint)pHook) || plugins.Count <= 0)
		{
			return;
		}
		if (hwndMain.Equals((object?)(nint)IntPtr.Zero))
		{
			IntPtr hWnd = IntPtr.Zero;
			int num = (int)Registry.GetValue("HKEY_CURRENT_USER\\Software\\XS\\Inflex\\Registered\\SAUMI30", "Handle", 0);
			if (num > 0)
			{
				hWnd = new IntPtr(num);
			}
			if (!hWnd.Equals((object?)(nint)IntPtr.Zero))
			{
				hwndMain = GetParent(hWnd);
				if (hwndMain.Equals((object?)(nint)IntPtr.Zero))
				{
					hwndMain = hWnd;
				}
			}
		}
		if (hwndMain.Equals((object?)(nint)IntPtr.Zero))
		{
			return;
		}
		using Process process = Process.GetCurrentProcess();
		using ProcessModule processModule = process.MainModule;
		pHook = SetWindowsHookEx(3, GetMsgHandler, GetModuleHandle(processModule.ModuleName), (uint)AppDomain.GetCurrentThreadId());
		if (pHook.Equals((object?)(nint)IntPtr.Zero))
		{
			throw new Win32Exception(GetLastError());
		}
	}

	private static IntPtr GetMsgHandler(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode == 0)
		{
			IntPtr intPtr = (IntPtr)Marshal.ReadInt32(lParam, 0);
			uint num = (uint)Marshal.ReadInt32(lParam, Marshal.SizeOf((object)(nint)intPtr));
			if ((intPtr.Equals((object?)(nint)hwndMain) && num == 2) || num == 18)
			{
				Notify();
				UnhookWindowsHookEx(pHook);
			}
		}
		return CallNextHookEx(pHook, nCode, wParam, lParam);
	}

	private static void Notify()
	{
		foreach (ISaumiPlugInExitable plugin in plugins)
		{
			try
			{
				plugin.OnApplicationExit();
			}
			catch (Exception)
			{
			}
		}
	}

	public static void Add(object saumiPlugin)
	{
		if (saumiPlugin is ISaumiPlugInExitable)
		{
			plugins.Add(saumiPlugin as ISaumiPlugInExitable);
			Apply();
		}
	}
}
