using System;
using System.Runtime.InteropServices;

namespace CommonUtils.EnvironmentUtils;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ProcessBasicInformation
{
	internal IntPtr ExitStatus;

	internal IntPtr PebBaseAddress;

	internal IntPtr AffinityMask;

	internal IntPtr BasePriority;

	internal UIntPtr UniqueProcessId;

	internal IntPtr InheritedFromUniqueProcessId;

	public static int Size => Marshal.SizeOf(typeof(ProcessBasicInformation));
}
