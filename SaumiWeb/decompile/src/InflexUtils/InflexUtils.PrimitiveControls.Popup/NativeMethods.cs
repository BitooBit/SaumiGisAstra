using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;

namespace InflexUtils.PrimitiveControls.Popup;

internal static class NativeMethods
{
	[Flags]
	internal enum AnimationFlags
	{
		Roll = 0,
		HorizontalPositive = 1,
		HorizontalNegative = 2,
		VerticalPositive = 4,
		VerticalNegative = 8,
		Center = 0x10,
		Hide = 0x10000,
		Activate = 0x20000,
		Slide = 0x40000,
		Blend = 0x80000,
		Mask = 0xFFFFF
	}

	internal struct MINMAXINFO
	{
		public Point reserved;

		public Size maxSize;

		public Point maxPosition;

		public Size minTrackSize;

		public Size maxTrackSize;
	}

	internal const int WM_NCHITTEST = 132;

	internal const int WM_NCACTIVATE = 134;

	internal const int WS_EX_TRANSPARENT = 32;

	internal const int WS_EX_TOOLWINDOW = 128;

	internal const int WS_EX_LAYERED = 524288;

	internal const int WS_EX_NOACTIVATE = 134217728;

	internal const int HTTRANSPARENT = -1;

	internal const int HTLEFT = 10;

	internal const int HTRIGHT = 11;

	internal const int HTTOP = 12;

	internal const int HTTOPLEFT = 13;

	internal const int HTTOPRIGHT = 14;

	internal const int HTBOTTOM = 15;

	internal const int HTBOTTOMLEFT = 16;

	internal const int HTBOTTOMRIGHT = 17;

	internal const int WM_PRINT = 791;

	internal const int WM_USER = 1024;

	internal const int WM_REFLECT = 8192;

	internal const int WM_COMMAND = 273;

	internal const int CBN_DROPDOWN = 7;

	internal const int WM_GETMINMAXINFO = 36;

	private static HandleRef HWND_TOPMOST = new HandleRef(null, new IntPtr(-1));

	private static bool? _isRunningOnMono;

	public static bool IsRunningOnMono
	{
		get
		{
			if (!_isRunningOnMono.HasValue)
			{
				_isRunningOnMono = (object)Type.GetType("Mono.Runtime") != null;
			}
			return _isRunningOnMono.Value;
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	[SuppressUnmanagedCodeSecurity]
	private static extern int AnimateWindow(HandleRef windowHandle, int time, AnimationFlags flags);

	internal static void AnimateWindow(Control control, int time, AnimationFlags flags)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		try
		{
			SecurityPermission val = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
			((CodeAccessPermission)val).Demand();
			AnimateWindow(new HandleRef(control, control.Handle), time, flags);
		}
		catch (SecurityException)
		{
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	[SuppressUnmanagedCodeSecurity]
	private static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

	internal static void SetTopMost(Control control)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		try
		{
			SecurityPermission val = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
			((CodeAccessPermission)val).Demand();
			SetWindowPos(new HandleRef(control, control.Handle), HWND_TOPMOST, 0, 0, 0, 0, 19);
		}
		catch (SecurityException)
		{
		}
	}

	internal static int HIWORD(int n)
	{
		return (short)((n >> 16) & 0xFFFF);
	}

	internal static int HIWORD(IntPtr n)
	{
		return HIWORD((int)(long)n);
	}

	internal static int LOWORD(int n)
	{
		return (short)(n & 0xFFFF);
	}

	internal static int LOWORD(IntPtr n)
	{
		return LOWORD((int)(long)n);
	}
}
