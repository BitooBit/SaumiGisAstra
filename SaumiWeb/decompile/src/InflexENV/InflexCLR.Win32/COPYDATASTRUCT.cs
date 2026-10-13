using System;

namespace InflexCLR.Win32;

public struct COPYDATASTRUCT
{
	public IntPtr dwData;

	public int cbData;

	public IntPtr lpData;
}
