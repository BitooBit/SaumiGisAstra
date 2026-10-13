using System;
using System.Runtime.InteropServices;

namespace CommonUtils.ReportUtils;

public static class MessageBoxUtils
{
	[Flags]
	public enum MessageBoxOptions : uint
	{
		Ok = 0u,
		OkCancel = 1u,
		AbortRetryIgnore = 2u,
		YesNoCancel = OkCancel | AbortRetryIgnore,
		YesNo = 4u,
		RetryCancel = OkCancel | YesNo,
		CancelTryContinue = AbortRetryIgnore | YesNo,
		IconHand = 0x10u,
		IconQuestion = 0x20u,
		IconExclamation = IconHand | IconQuestion,
		IconAsterisk = 0x40u,
		UserIcon = 0x80u,
		IconWarning = IconExclamation,
		IconError = IconHand,
		IconInformation = IconAsterisk,
		IconStop = IconHand,
		DefButton1 = 0u,
		DefButton2 = 0x100u,
		DefButton3 = 0x200u,
		DefButton4 = DefButton2 | DefButton3,
		ApplicationModal = 0u,
		SystemModal = 0x1000u,
		TaskModal = 0x2000u,
		Help = 0x4000u,
		NoFocus = 0x8000u,
		SetForeground = 0x10000u,
		DefaultDesktopOnly = 0x20000u,
		Topmost = 0x40000u,
		Right = 0x80000u,
		RTLReading = 0x100000u
	}

	public enum MessageBoxResult : uint
	{
		Ok = 1u,
		Cancel = 2u,
		Abort = 3u,
		Retry = 4u,
		Ignore = 5u,
		Yes = 6u,
		No = 7u,
		Close = 8u,
		Help = 9u,
		TryAgain = 10u,
		Continue = 11u,
		Timeout = 32000u
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern MessageBoxResult MessageBox(IntPtr hWnd, string text, string caption, int options);
}
