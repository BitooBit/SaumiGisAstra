using System;
using System.Runtime.InteropServices;
using System.Security;

namespace CommonUtils.SecureUtils;

public static class SecureStringExtension
{
	public unsafe static SecureString ConvertToSecureString(this string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		fixed (char* value2 = value)
		{
			SecureString secureString = new SecureString(value2, value.Length);
			secureString.MakeReadOnly();
			return secureString;
		}
	}

	public static string ConvertToUnsecureString(this SecureString value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = Marshal.SecureStringToGlobalAllocUnicode(value);
			return Marshal.PtrToStringUni(intPtr);
		}
		finally
		{
			Marshal.ZeroFreeGlobalAllocUnicode(intPtr);
		}
	}
}
