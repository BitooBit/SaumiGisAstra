using System;
using System.Diagnostics;
using System.Reflection;

namespace CommonUtils.ReportUtils;

public static class UtilsLoggerWrapper
{
	private class LoggerProvider
	{
		public Action<string> InfoLoggerAction { get; private set; }

		public Action<string> WarningLoggerAction { get; private set; }

		public Action<string> ErrorLoggerAction { get; private set; }

		public bool UseMessageTypeStringPrefix { get; private set; }

		public LoggerProvider(Action<string> infoLoggerAction, Action<string> warningLoggerAction, Action<string> errorLoggerAction, bool useMessageTypeStringPrefix)
		{
			InfoLoggerAction = infoLoggerAction;
			WarningLoggerAction = warningLoggerAction;
			ErrorLoggerAction = errorLoggerAction;
			UseMessageTypeStringPrefix = useMessageTypeStringPrefix;
		}
	}

	private static LoggerProvider _LoggerProvider;

	public static bool IsLoggerProviderSet => _LoggerProvider != null;

	public static void SetLoggerProvider(Action<string> loggerAction)
	{
		_LoggerProvider = new LoggerProvider(loggerAction, loggerAction, loggerAction, useMessageTypeStringPrefix: true);
	}

	public static void SetLoggerProvider(Action<string> infoLoggerAction, Action<string> warningLoggerAction, Action<string> errorLoggerAction)
	{
		_LoggerProvider = new LoggerProvider(infoLoggerAction, warningLoggerAction, errorLoggerAction, useMessageTypeStringPrefix: false);
	}

	public static void ResetLoggerProvider()
	{
		_LoggerProvider = null;
	}

	public static bool Info(string message)
	{
		if (!IsLoggerProviderSet)
		{
			return false;
		}
		string callerDescription = TraceUtils.GetCallerDescription(typeof(UtilsLoggerWrapper));
		_LoggerProvider.InfoLoggerAction((_LoggerProvider.UseMessageTypeStringPrefix ? "[INFO]:" : string.Empty) + "[" + callerDescription + "] " + message);
		return true;
	}

	public static bool Warning(string message)
	{
		if (!IsLoggerProviderSet)
		{
			return false;
		}
		string callerDescription = TraceUtils.GetCallerDescription(typeof(UtilsLoggerWrapper));
		_LoggerProvider.WarningLoggerAction((_LoggerProvider.UseMessageTypeStringPrefix ? "[WARNING]:" : string.Empty) + "[" + callerDescription + "] " + message);
		return true;
	}

	public static bool Error(string message)
	{
		if (!IsLoggerProviderSet)
		{
			return false;
		}
		string callerDescription = TraceUtils.GetCallerDescription(typeof(UtilsLoggerWrapper));
		_LoggerProvider.ErrorLoggerAction((_LoggerProvider.UseMessageTypeStringPrefix ? "[ERROR]:" : string.Empty) + "[" + callerDescription + "] " + message);
		return true;
	}

	private static bool TryGetTypeByNameFromCurrentAppDomain(string typeName, out Type type)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			Type type2 = assembly.GetType(typeName);
			if ((object)type2 != null)
			{
				type = type2;
				return true;
			}
		}
		type = null;
		return false;
	}

	private static Type GetCallerType(string loggerTypeName)
	{
		Type callerType = null;
		MethodBase callerMethod = null;
		GetCallerInfo(loggerTypeName, out callerType, out callerMethod);
		return callerType;
	}

	private static void GetCallerInfo(string loggerTypeName, out Type callerType, out MethodBase callerMethod)
	{
		if (!TryGetTypeByNameFromCurrentAppDomain(loggerTypeName, out var type) || (object)type == null)
		{
			callerType = null;
			callerMethod = null;
			return;
		}
		StackFrame[] frames = new StackTrace().GetFrames();
		bool flag = false;
		for (int i = 0; i < frames.Length; i++)
		{
			Type reflectedType = frames[i].GetMethod().ReflectedType;
			bool flag2 = (object)reflectedType == type || (reflectedType.IsNested && (object)reflectedType.DeclaringType == type);
			if (!flag2 || !flag)
			{
				if (!flag2 || flag)
				{
					callerMethod = frames[i].GetMethod();
					callerType = callerMethod.ReflectedType;
					return;
				}
				flag = true;
			}
		}
		throw new InvalidOperationException("Can't get caller info. Logical error is obtained.");
	}

	public static string GetCallerNamespace(string loggerTypeName)
	{
		return GetCallerType(loggerTypeName).Namespace;
	}
}
