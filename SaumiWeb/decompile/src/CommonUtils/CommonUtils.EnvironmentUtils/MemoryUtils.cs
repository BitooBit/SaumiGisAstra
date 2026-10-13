using System.Runtime.InteropServices;

namespace CommonUtils.EnvironmentUtils;

public static class MemoryUtils
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private class MEMORYSTATUSEX
	{
		public uint dwLength;

		public uint dwMemoryLoad;

		public ulong ullTotalPhys;

		public ulong ullAvailPhys;

		public ulong ullTotalPageFile;

		public ulong ullAvailPageFile;

		public ulong ullTotalVirtual;

		public ulong ullAvailVirtual;

		public ulong ullAvailExtendedVirtual;

		public MEMORYSTATUSEX()
		{
			dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
		}
	}

	public static string Description => "RAM bytes: " + GetTotalMemorySize() + " (total), " + GetAvailableMemorySize() + " (available)";

	public static ulong GetTotalMemorySize()
	{
		MEMORYSTATUSEX mEMORYSTATUSEX = new MEMORYSTATUSEX();
		if (GlobalMemoryStatusEx(mEMORYSTATUSEX))
		{
			return mEMORYSTATUSEX.ullTotalPhys;
		}
		return 0uL;
	}

	public static ulong GetAvailableMemorySize()
	{
		MEMORYSTATUSEX mEMORYSTATUSEX = new MEMORYSTATUSEX();
		if (GlobalMemoryStatusEx(mEMORYSTATUSEX))
		{
			return mEMORYSTATUSEX.ullAvailPhys;
		}
		return 0uL;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GlobalMemoryStatusEx([In][Out] MEMORYSTATUSEX lpBuffer);
}
