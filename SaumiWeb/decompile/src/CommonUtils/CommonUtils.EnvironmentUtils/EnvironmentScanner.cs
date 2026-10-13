using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonUtils.EnvironmentUtils;

public static class EnvironmentScanner
{
	public static string ShortDescription => GetDescription(fullDescription: false);

	public static string FullDescription => GetDescription(fullDescription: true);

	public static AppPlatformBits ApplicationEnvironmentBits => (AppPlatformBits)(IntPtr.Size * 8);

	public static AppPlatformBits SystemEnvironmentBits
	{
		get
		{
			AppPlatformBits appPlatformBits = ApplicationEnvironmentBits;
			if (appPlatformBits == AppPlatformBits.X86)
			{
				Exception exception = null;
				if (TryCheckIsWow64Process(out exception))
				{
					appPlatformBits = AppPlatformBits.X64;
				}
			}
			return appPlatformBits;
		}
	}

	public static bool IsCurrentProcessVirtualized
	{
		get
		{
			if (ApplicationEnvironmentBits == AppPlatformBits.X86)
			{
				return SystemEnvironmentBits == AppPlatformBits.X64;
			}
			return false;
		}
	}

	public static string SystemDescription => OsDescription.CurrentDescription;

	public static bool IsCurrentUserDomainMember
	{
		get
		{
			UserSecurityDescription userSecurityDescription = new UserSecurityDescription();
			return userSecurityDescription.IsDomainMember;
		}
	}

	public static bool HasCurrentUserAdministratorPrivileges
	{
		get
		{
			UserSecurityDescription userSecurityDescription = new UserSecurityDescription();
			return userSecurityDescription.HasAdministratorPrivileges;
		}
	}

	public static string CurrentUserFullDescription
	{
		get
		{
			UserSecurityDescription userSecurityDescription = new UserSecurityDescription();
			return userSecurityDescription.ToString(fullDescription: true);
		}
	}

	public static string CurrentUserShortDescription
	{
		get
		{
			UserSecurityDescription userSecurityDescription = new UserSecurityDescription();
			return userSecurityDescription.ToString(fullDescription: false);
		}
	}

	public static Version ApplicationVersion
	{
		get
		{
			Assembly applicationAssembly = GetApplicationAssembly();
			AssemblyName name = applicationAssembly.GetName();
			return name.Version;
		}
	}

	public static string ApplicationVersionDescription
	{
		get
		{
			Assembly applicationAssembly = GetApplicationAssembly();
			AssemblyName name = applicationAssembly.GetName();
			return name.Name + " v" + name.Version;
		}
	}

	public static string CallingProcessDescription
	{
		get
		{
			ProcessDescription processDescription = new ProcessDescription();
			return processDescription.ToString();
		}
	}

	public static string CallingAssemblyDescription
	{
		get
		{
			AssemblyAttributesDescription assemblyAttributesDescription = new AssemblyAttributesDescription();
			return assemblyAttributesDescription.ToString();
		}
	}

	private static string GetDescription(bool fullDescription)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(ApplicationVersionDescription);
		stringBuilder.AppendLine("OS: " + SystemDescription);
		stringBuilder.AppendLine(string.Concat(new object[5]
		{
			"CLR: v",
			Environment.Version,
			" (",
			ApplicationEnvironmentBits,
			")"
		}));
		stringBuilder.AppendLine("CPU cores count: " + Environment.ProcessorCount);
		stringBuilder.AppendLine(MemoryUtils.Description);
		string tempPath = Path.GetTempPath();
		string persistentVariableActualValue = EnvironmentVariablesUtils.GetPersistentVariableActualValue("TEMP");
		if (string.Compare(tempPath.TrimEnd(new char[2]
		{
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar
		}), persistentVariableActualValue.TrimEnd(new char[2]
		{
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar
		}), StringComparison.OrdinalIgnoreCase) == 0)
		{
			stringBuilder.AppendLine("%temp%: " + tempPath);
		}
		else
		{
			stringBuilder.AppendLine("%temp%(OS): " + tempPath);
			stringBuilder.AppendLine("%temp%(ENV): " + persistentVariableActualValue + (Directory.Exists(persistentVariableActualValue) ? " (exists)" : " (doesn't exist)"));
		}
		stringBuilder.AppendLine("%windir%: " + Environment.SystemDirectory);
		stringBuilder.AppendLine(HostNetworkDescription.Current.ToString());
		stringBuilder.AppendLine(fullDescription ? CurrentUserFullDescription : CurrentUserShortDescription);
		return stringBuilder.ToString();
	}

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetCurrentProcess();

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IsWow64Process([In] IntPtr processHandle, [MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

	private static bool TryCheckIsWow64Process(out Exception exception)
	{
		exception = null;
		bool wow64Process = false;
		try
		{
			IntPtr currentProcess = GetCurrentProcess();
			if (!IsWow64Process(currentProcess, out wow64Process))
			{
				exception = new Win32Exception(Marshal.GetLastWin32Error());
			}
		}
		catch (Exception ex)
		{
			exception = ex;
		}
		return wow64Process;
	}

	private static Assembly GetApplicationAssembly()
	{
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		if ((object)entryAssembly != null)
		{
			return entryAssembly;
		}
		Assembly foreignCallingAssembly = GetForeignCallingAssembly();
		if ((object)foreignCallingAssembly != null)
		{
			return foreignCallingAssembly;
		}
		return Assembly.GetCallingAssembly();
	}

	private static Assembly GetForeignCallingAssembly()
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		StackTrace stackTrace = new StackTrace();
		StackFrame[] frames = stackTrace.GetFrames();
		if (frames == null)
		{
			return null;
		}
		for (int i = 0; i < frames.Length; i++)
		{
			MethodBase method = frames[i].GetMethod();
			Type reflectedType = method.ReflectedType;
			Assembly assembly = reflectedType.Assembly;
			if ((object)assembly != executingAssembly)
			{
				return assembly;
			}
		}
		return null;
	}
}
