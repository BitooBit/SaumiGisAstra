using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Xml;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace Log4NetWrapper;

public class Logger
{
	public class MultiRepositorySupport
	{
		public ILog this[string loggerDomainNamespace]
		{
			get
			{
				//IL_004a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0050: Expected O, but got Unknown
				ILoggerRepository val = null;
				ILoggerRepository[] allRepositories = LogManager.GetAllRepositories();
				ILoggerRepository[] array = allRepositories;
				foreach (ILoggerRepository val2 in array)
				{
					if (string.Compare(val2.Name, loggerDomainNamespace, ignoreCase: true) == 0)
					{
						val = val2;
						break;
					}
				}
				if (val == null)
				{
					val = allRepositories[0];
				}
				val.GetCurrentLoggers();
				return (ILog)new LogImpl(val.GetCurrentLoggers()[0]);
			}
		}

		internal MultiRepositorySupport()
		{
		}
	}

	public class Paths
	{
		public static string CallerLocalPath => Path.GetDirectoryName(Assembly.GetCallingAssembly().GetModules()[0].FullyQualifiedName);

		public static string ProvideLogsBaseFolder(string baseFolderPath)
		{
			try
			{
				if (!Directory.Exists(baseFolderPath))
				{
					Directory.CreateDirectory(baseFolderPath);
				}
			}
			catch (Exception)
			{
				return null;
			}
			return baseFolderPath;
		}
	}

	public class Reflection
	{
		public static string CallerNamespace => GetCallerType().Namespace;
	}

	private const string MESSAGE_FOR_RELEASE_ENVIRONMENT = "Debug info is not available. Logging disabled";

	internal const string SAUMI_LOGWRAPPER_DEFAULT_LOGGER = "SaumiLogwrapperDefaultLogger";

	private const char TYPE_NAME_SEPARATOR_CHAR = '.';

	public static PatternLayout DefaultPatternLayout
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			PatternLayout val = new PatternLayout();
			val.ConversionPattern = "%date [%thread] %-5level - %message%newline";
			((LayoutSkeleton)val).ActivateOptions();
			return val;
		}
	}

	public static bool IsDebugEnabled
	{
		get
		{
			ILog logger = GetLogger(resolveAllCallerNamespaces: false);
			if (logger != null)
			{
				return logger.IsDebugEnabled;
			}
			return false;
		}
	}

	public static bool IsInfoEnabled
	{
		get
		{
			ILog logger = GetLogger(resolveAllCallerNamespaces: false);
			if (logger != null)
			{
				return logger.IsInfoEnabled;
			}
			return false;
		}
	}

	public static bool IsWarnEnabled
	{
		get
		{
			ILog logger = GetLogger(resolveAllCallerNamespaces: false);
			if (logger != null)
			{
				return logger.IsWarnEnabled;
			}
			return false;
		}
	}

	public static bool IsErrorEnabled
	{
		get
		{
			ILog logger = GetLogger(resolveAllCallerNamespaces: false);
			if (logger != null)
			{
				return logger.IsErrorEnabled;
			}
			return false;
		}
	}

	public static bool IsFatalEnabled
	{
		get
		{
			ILog logger = GetLogger(resolveAllCallerNamespaces: false);
			if (logger != null)
			{
				return logger.IsFatalEnabled;
			}
			return false;
		}
	}

	public static MultiRepositorySupport Log => new MultiRepositorySupport();

	public static void CreateLogger(string defaultLoggerDomainNamespace, XmlElement xmlElement)
	{
		CreateLogger(isLoggingEnable: true, defaultLoggerDomainNamespace, xmlElement);
	}

	public static void CreateLogger(bool isLoggingEnable, string defaultLoggerDomainNamespace, XmlElement xmlElement)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		ILoggerRepository val = LogManager.CreateRepository(defaultLoggerDomainNamespace);
		Hierarchy val2 = (Hierarchy)val;
		IXmlRepositoryConfigurator val3 = (IXmlRepositoryConfigurator)(object)val2;
		val3.Configure(xmlElement);
		if (!isLoggingEnable)
		{
			val2.Root.Log(Level.Info, (object)"Debug info is not available. Logging disabled", (Exception)null);
			((LoggerRepositorySkeleton)val2).Threshold = Level.Off;
		}
	}

	public static void CreateLogger(string defaultLoggerDomainNamespace, IAppender[] appenders)
	{
		CreateLogger(isLoggingEnable: true, defaultLoggerDomainNamespace, appenders);
	}

	public static void CreateLogger(bool isLoggingEnable, string defaultLoggerDomainNamespace, IAppender[] appenders)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		ILoggerRepository val = LogManager.CreateRepository(defaultLoggerDomainNamespace);
		Hierarchy val2 = (Hierarchy)val;
		ILogger logger = ((LoggerRepositorySkeleton)val2).GetLogger("SaumiLogwrapperDefaultLogger");
		Logger val3 = (Logger)(object)((logger is Logger) ? logger : null);
		foreach (IAppender val4 in appenders)
		{
			val3.AddAppender(val4);
		}
		((LoggerRepositorySkeleton)val2).Configured = true;
		if (!isLoggingEnable)
		{
			val3.Log(Level.Info, (object)"Debug info is not available. Logging disabled", (Exception)null);
			((LoggerRepositorySkeleton)val2).Threshold = Level.Off;
		}
	}

	public static bool TryCreateLogger(string defaultLoggerDomainNamespace, IAppender[] appenders)
	{
		return TryCreateLogger(isLoggingEnable: true, defaultLoggerDomainNamespace, appenders);
	}

	public static bool TryCreateLogger(bool isLoggingEnable, string defaultLoggerDomainNamespace, IAppender[] appenders)
	{
		if (IsRepositoryExists(defaultLoggerDomainNamespace))
		{
			return false;
		}
		CreateLogger(isLoggingEnable, defaultLoggerDomainNamespace, appenders);
		return true;
	}

	private static bool IsRepositoryExists(string checkingRepoName)
	{
		ILoggerRepository[] allRepositories = LogManager.GetAllRepositories();
		if (allRepositories == null || allRepositories.Length == 0)
		{
			return false;
		}
		ILoggerRepository[] array = allRepositories;
		foreach (ILoggerRepository val in array)
		{
			if (string.Compare(checkingRepoName, val.Name, ignoreCase: true) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CloneAppendersBeetweenLogHierarchies(string sourceHierarchyName, string targetHierarchyName)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		if (!IsRepositoryExists(sourceHierarchyName) || !IsRepositoryExists(targetHierarchyName))
		{
			return false;
		}
		ILoggerRepository repository = LogManager.GetRepository(targetHierarchyName);
		if (repository == null)
		{
			return false;
		}
		Hierarchy val = (Hierarchy)repository;
		ILogger logger = ((LoggerRepositorySkeleton)val).GetLogger("SaumiLogwrapperDefaultLogger");
		Logger val2 = (Logger)(object)((logger is Logger) ? logger : null);
		if (val2 == null)
		{
			return false;
		}
		ILoggerRepository repository2 = LogManager.GetRepository(sourceHierarchyName);
		if (repository2 == null)
		{
			return false;
		}
		Hierarchy val3 = (Hierarchy)repository2;
		ILogger logger2 = ((LoggerRepositorySkeleton)val3).GetLogger("SaumiLogwrapperDefaultLogger");
		Logger val4 = (Logger)(object)((logger2 is Logger) ? logger2 : null);
		if (val4 == null)
		{
			return false;
		}
		List<IAppender> list = new List<IAppender>();
		List<IAppender> list2 = new List<IAppender>();
		IAppenderCollectionEnumerator enumerator = val4.Appenders.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				IAppender current = enumerator.Current;
				bool flag = true;
				IAppenderCollectionEnumerator enumerator2 = val2.Appenders.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						IAppender current2 = enumerator2.Current;
						if (IsAppendersEqual(current, current2))
						{
							flag = false;
							list2.Add(current2);
						}
						if (flag)
						{
							list.Add(current);
						}
					}
				}
				finally
				{
					if (enumerator2 is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}
		}
		finally
		{
			if (enumerator is IDisposable disposable2)
			{
				disposable2.Dispose();
			}
		}
		bool flag2 = false;
		while (!flag2)
		{
			flag2 = true;
			IAppenderCollectionEnumerator enumerator3 = val2.Appenders.GetEnumerator();
			try
			{
				while (enumerator3.MoveNext())
				{
					IAppender current3 = enumerator3.Current;
					if (!list2.Contains(current3))
					{
						val2.RemoveAppender(current3);
						flag2 = false;
						break;
					}
				}
			}
			finally
			{
				if (enumerator3 is IDisposable disposable3)
				{
					disposable3.Dispose();
				}
			}
		}
		foreach (IAppender item in list)
		{
			val2.AddAppender(item);
		}
		((LoggerRepositorySkeleton)val).Configured = true;
		return true;
	}

	private static bool IsAppendersEqual(IAppender first, IAppender second)
	{
		if (first == null || second == null)
		{
			return false;
		}
		if (object.ReferenceEquals(first, second))
		{
			return true;
		}
		if ((object)((object)first).GetType() != ((object)second).GetType())
		{
			return false;
		}
		if (string.IsNullOrEmpty(first.Name) || string.IsNullOrEmpty(second.Name))
		{
			return false;
		}
		return string.Compare(first.Name, second.Name, ignoreCase: true) == 0;
	}

	public static void CreateLogger(bool isLoggingEnable, IAppender[] appenders)
	{
		CreateLogger(isLoggingEnable, GetCallerType().Namespace, appenders);
	}

	public static void CreateLogger(IAppender[] appenders)
	{
		CreateLogger(GetCallerType().Namespace, appenders);
	}

	public static IAppender GetDefaultRollingFileAppender(string logFileFullPath)
	{
		return GetDefaultRollingFileAppender(logFileFullPath, Level.Verbose, Level.Error, isCreateNewIfExisting: false);
	}

	public static IAppender GetDefaultRollingFileAppender(string logFileFullPath, bool isCreateNewIfExisting)
	{
		return GetDefaultRollingFileAppender(logFileFullPath, Level.Verbose, Level.Error, isCreateNewIfExisting);
	}

	public static IAppender GetDefaultRollingFileAppender(string logFileFullPath, Level minLogLevel, Level maxLogLevel, bool isCreateNewIfExisting)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		LevelRangeFilter val = new LevelRangeFilter();
		val.LevelMin = minLogLevel;
		val.LevelMax = maxLogLevel;
		return GetDefaultRollingFileAppender(logFileFullPath, (IFilter)(object)val, isCreateNewIfExisting);
	}

	public static IAppender GetDefaultRollingFileAppender(string logFileFullPath, IFilter filter, bool isCreateNewIfExisting)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		RollingFileAppender val = new RollingFileAppender();
		((AppenderSkeleton)val).AddFilter(filter);
		((AppenderSkeleton)val).Layout = (ILayout)(object)DefaultPatternLayout;
		((FileAppender)val).AppendToFile = !isCreateNewIfExisting;
		val.RollingStyle = (RollingMode)1;
		val.MaxSizeRollBackups = 0;
		val.MaximumFileSize = "10Mb";
		val.StaticLogFileName = true;
		((FileAppender)val).File = logFileFullPath;
		((AppenderSkeleton)val).ActivateOptions();
		return (IAppender)(object)val;
	}

	public static IAppender GetDefaultConsoleAppender()
	{
		return GetDefaultConsoleAppender(Level.Verbose, Level.Error);
	}

	public static IAppender GetDefaultConsoleAppender(Level minLogLevel, Level maxLogLevel)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		LevelRangeFilter val = new LevelRangeFilter();
		val.LevelMin = minLogLevel;
		val.LevelMax = maxLogLevel;
		return GetDefaultConsoleAppender((IFilter)(object)val);
	}

	public static IAppender GetDefaultConsoleAppender(IFilter filter)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		ConsoleAppender val = new ConsoleAppender();
		((AppenderSkeleton)val).AddFilter(filter);
		((AppenderSkeleton)val).Layout = (ILayout)(object)DefaultPatternLayout;
		return (IAppender)(object)val;
	}

	private static void TryLog(Action<ILog, string> resolvedLoggerAction, bool resolveAllCallerNamespaces = false)
	{
		string callerDescription = null;
		ILog logger = GetLogger(resolveAllCallerNamespaces, out callerDescription);
		if (logger != null)
		{
			resolvedLoggerAction(logger, callerDescription);
			return;
		}
		ILog defaultLoggerInstance = DefaultLogger.DefaultLoggerInstance;
		ILog[] allConfiguredLoggers = DefaultLogger.AllConfiguredLoggers;
		if (defaultLoggerInstance != null)
		{
			resolvedLoggerAction(defaultLoggerInstance, callerDescription);
		}
		if (allConfiguredLoggers == null || allConfiguredLoggers.Length <= 0)
		{
			return;
		}
		string text = null;
		try
		{
			WindowsIdentity current = WindowsIdentity.GetCurrent();
			if (current != null)
			{
				text = current.Name;
			}
		}
		catch (Exception)
		{
			text = "current user";
		}
		ILog[] array = allConfiguredLoggers;
		foreach (ILog val in array)
		{
			val.Error((object)(((!string.IsNullOrEmpty(callerDescription)) ? (callerDescription + ". ") : "") + "Can't find logger.\nLogger must be configured before using.\nApproppriate log hierarcy not found, see caller namespace.\nLog event forwaded to default log.\nSee " + (text ?? string.Empty) + " local profile app data storage."));
		}
	}

	public static void Debug(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Debug((object)(callerDescription + " " + message));
		});
	}

	public static void Debug(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Debug((object)(callerDescription + " " + message), exception);
		});
	}

	public static void DebugFormat(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.DebugFormat(callerDescription + " " + format, args);
		});
	}

	public static void DebugFormat(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.DebugFormat(provider, callerDescription + " " + format, args);
		});
	}

	public static void Info(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Info((object)(callerDescription + " " + message));
		});
	}

	public static void Info(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Info((object)(callerDescription + " " + message), exception);
		});
	}

	public static void InfoFormat(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.InfoFormat(callerDescription + " " + format, args);
		});
	}

	public static void InfoFormat(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.InfoFormat(provider, callerDescription + " " + format, args);
		});
	}

	public static void Warn(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Warn((object)(callerDescription + " " + message));
		});
	}

	public static void Warn(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Warn((object)(callerDescription + " " + message), exception);
		});
	}

	public static void WarnFormat(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.WarnFormat(callerDescription + " " + format, args);
		});
	}

	public static void WarnFormat(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.WarnFormat(provider, callerDescription + " " + format, args);
		});
	}

	public static void Error(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Error((object)(callerDescription + " " + message));
		});
	}

	public static void Error(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Error((object)(callerDescription + " " + message), exception);
		});
	}

	public static void ErrorFormat(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.ErrorFormat(callerDescription + " " + format, args);
		});
	}

	public static void ErrorFormat(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.ErrorFormat(provider, callerDescription + " " + format, args);
		});
	}

	public static void Fatal(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Fatal((object)(callerDescription + " " + message));
		});
	}

	public static void Fatal(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Fatal((object)(callerDescription + " " + message), exception);
		});
	}

	public static void FatalFormat(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.FatalFormat(callerDescription + " " + format, args);
		});
	}

	public static void FatalFormat(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.FatalFormat(provider, callerDescription + " " + format, args);
		});
	}

	public static void DebugDirect(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Debug(message);
		});
	}

	public static void DebugDirect(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Debug(message, exception);
		});
	}

	public static void DebugFormatDirect(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.DebugFormat(format, args);
		});
	}

	public static void DebugFormatDirect(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.DebugFormat(provider, format, args);
		});
	}

	public static void InfoDirect(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Info(message);
		});
	}

	public static void InfoDirect(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Info(message, exception);
		});
	}

	public static void InfoFormatDirect(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.InfoFormat(format, args);
		});
	}

	public static void InfoFormatDirect(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.InfoFormat(provider, format, args);
		});
	}

	public static void WarnDirect(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Warn(message);
		});
	}

	public static void WarnDirect(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Warn(message, exception);
		});
	}

	public static void WarnFormatDirect(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.WarnFormat(format, args);
		});
	}

	public static void WarnFormatDirect(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.WarnFormat(provider, format, args);
		});
	}

	public static void ErrorDirect(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Error(message);
		});
	}

	public static void ErrorDirect(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Error(message, exception);
		});
	}

	public static void ErrorFormatDirect(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.ErrorFormat(format, args);
		});
	}

	public static void FatalDirect(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Fatal(message);
		});
	}

	public static void FatalDirect(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Fatal(message, exception);
		});
	}

	public static void FatalFormatDirect(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.FatalFormat(format, args);
		});
	}

	public static void FatalFormatDirect(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.FatalFormat(provider, format, args);
		});
	}

	public static void DebugCare(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Debug((object)(callerDescription + " " + message));
		}, resolveAllCallerNamespaces: true);
	}

	public static void DebugCare(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Debug((object)(callerDescription + " " + message), exception);
		}, resolveAllCallerNamespaces: true);
	}

	public static void DebugFormatCare(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.DebugFormat(callerDescription + " " + format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void DebugFormatCare(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.DebugFormat(provider, callerDescription + " " + format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void InfoCare(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Info((object)(callerDescription + " " + message));
		}, resolveAllCallerNamespaces: true);
	}

	public static void InfoCare(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Info((object)(callerDescription + " " + message), exception);
		}, resolveAllCallerNamespaces: true);
	}

	public static void InfoFormatCare(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.InfoFormat(callerDescription + " " + format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void InfoFormatCare(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.InfoFormat(provider, callerDescription + " " + format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void WarnCare(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Warn((object)(callerDescription + " " + message));
		}, resolveAllCallerNamespaces: true);
	}

	public static void WarnCare(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Warn((object)(callerDescription + " " + message), exception);
		}, resolveAllCallerNamespaces: true);
	}

	public static void WarnFormatCare(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.WarnFormat(callerDescription + " " + format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void WarnFormatCare(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.WarnFormat(provider, callerDescription + " " + format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void ErrorCare(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Error((object)(callerDescription + " " + message));
		}, resolveAllCallerNamespaces: true);
	}

	public static void ErrorCare(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Error((object)(callerDescription + " " + message), exception);
		}, resolveAllCallerNamespaces: true);
	}

	public static void ErrorFormatCare(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.ErrorFormat(callerDescription + " " + format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void ErrorFormatCare(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.ErrorFormat(provider, callerDescription + " " + format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void FatalCare(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Fatal((object)(callerDescription + " " + message));
		}, resolveAllCallerNamespaces: true);
	}

	public static void FatalCare(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Fatal((object)(callerDescription + " " + message), exception);
		}, resolveAllCallerNamespaces: true);
	}

	public static void FatalFormatCare(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.FatalFormat(callerDescription + " " + format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void FatalFormatCare(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.FatalFormat(provider, callerDescription + " " + format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void DebugDirectCare(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Debug(message);
		}, resolveAllCallerNamespaces: true);
	}

	public static void DebugDirectCare(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Debug(message, exception);
		}, resolveAllCallerNamespaces: true);
	}

	public static void DebugFormatDirectCare(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.DebugFormat(format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void DebugFormatDirectCare(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.DebugFormat(provider, format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void InfoDirectCare(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Info(message);
		}, resolveAllCallerNamespaces: true);
	}

	public static void InfoDirectCare(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Info(message, exception);
		}, resolveAllCallerNamespaces: true);
	}

	public static void InfoFormatDirectCare(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.InfoFormat(format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void InfoFormatDirectCare(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.InfoFormat(provider, format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void WarnDirectCare(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Warn(message);
		}, resolveAllCallerNamespaces: true);
	}

	public static void WarnDirectCare(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Warn(message, exception);
		}, resolveAllCallerNamespaces: true);
	}

	public static void WarnFormatDirectCare(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.WarnFormat(format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void WarnFormatDirectCare(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.WarnFormat(provider, format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void ErrorDirectCare(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Error(message);
		}, resolveAllCallerNamespaces: true);
	}

	public static void ErrorDirectCare(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Error(message, exception);
		}, resolveAllCallerNamespaces: true);
	}

	public static void ErrorFormatDirectCare(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.ErrorFormat(format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void FatalDirectCare(object message)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Fatal(message);
		}, resolveAllCallerNamespaces: true);
	}

	public static void FatalDirectCare(object message, Exception exception)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.Fatal(message, exception);
		}, resolveAllCallerNamespaces: true);
	}

	public static void FatalFormatDirectCare(string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.FatalFormat(format, args);
		}, resolveAllCallerNamespaces: true);
	}

	public static void FatalFormatDirectCare(IFormatProvider provider, string format, params object[] args)
	{
		TryLog(delegate(ILog iLog, string callerDescription)
		{
			iLog.FatalFormat(provider, format, args);
		}, resolveAllCallerNamespaces: true);
	}

	private static ILog GetLogger(bool resolveAllCallerNamespaces)
	{
		string callerDescription = null;
		return GetLogger(resolveAllCallerNamespaces, out callerDescription);
	}

	private static ILog GetLogger(bool resolveAllCallerNamespaces, out string callerDescription)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		callerDescription = string.Empty;
		Type callerType;
		MethodBase callerMethod;
		if (resolveAllCallerNamespaces)
		{
			GetLoggerCallerInfo(out callerType, out callerMethod, out var callerNamespaces);
			foreach (string item in callerNamespaces)
			{
				ILoggerRepository callerAppropriateRepository = GetCallerAppropriateRepository(item);
				if (callerAppropriateRepository != null)
				{
					return (ILog)new LogImpl(callerAppropriateRepository.GetLogger("SaumiLogwrapperDefaultLogger"));
				}
			}
		}
		else
		{
			GetLoggerCallerInfo(out callerType, out callerMethod);
			ILoggerRepository callerAppropriateRepository2 = GetCallerAppropriateRepository(callerType.Namespace);
			if (callerAppropriateRepository2 != null)
			{
				return (ILog)new LogImpl(callerAppropriateRepository2.GetLogger("SaumiLogwrapperDefaultLogger"));
			}
		}
		callerDescription = callerType.FullName + '.' + callerMethod.Name;
		return null;
	}

	private static ILoggerRepository GetCallerAppropriateRepository(string callerNamespace)
	{
		ILoggerRepository[] allRepositories = LogManager.GetAllRepositories();
		if (allRepositories == null || allRepositories.Length == 0)
		{
			return null;
		}
		string[] array = callerNamespace.Split(new char[1] { '.' });
		List<KeyValuePair<int, ILoggerRepository>> list = new List<KeyValuePair<int, ILoggerRepository>>();
		ILoggerRepository[] array2 = allRepositories;
		foreach (ILoggerRepository val in array2)
		{
			if (callerNamespace.StartsWith(val.Name, ignoreCase: true, CultureInfo.CurrentCulture))
			{
				return val;
			}
			string[] array3 = val.Name.Split(new char[1] { '.' });
			int num = -1;
			for (int j = 0; j < array.Length && j < array3.Length && array[j].StartsWith(array3[j], ignoreCase: true, CultureInfo.CurrentCulture); j++)
			{
				num = j;
			}
			if (num >= 0)
			{
				list.Add(new KeyValuePair<int, ILoggerRepository>(num, val));
			}
		}
		if (list.Count > 0)
		{
			return (from repoDescr in list
				orderby repoDescr.Key descending
				select repoDescr.Value).First();
		}
		return null;
	}

	private static Type GetCallerType()
	{
		Type callerType = null;
		MethodBase callerMethod = null;
		GetLoggerCallerInfo(out callerType, out callerMethod);
		return callerType;
	}

	private static void GetLoggerCallerInfo(out Type callerType, out MethodBase callerMethod)
	{
		if (!TryGetCallerInfo(typeof(Logger), out callerType, out callerMethod))
		{
			throw new InvalidOperationException("Logical error. Can't find Logger callers.");
		}
	}

	private static void GetLoggerCallerInfo(out Type callerType, out MethodBase callerMethod, out IEnumerable<string> callerNamespaces)
	{
		if (!TryGetCallerInfo(typeof(Logger), out callerType, out callerMethod, out callerNamespaces))
		{
			throw new InvalidOperationException("Logical error. Can't find Logger callers.");
		}
	}

	private static bool TryGetCallerInfo(Type calledType, out Type callerType, out MethodBase callerMethod)
	{
		callerMethod = null;
		callerType = null;
		StackTrace stackTrace = new StackTrace();
		StackFrame[] frames = stackTrace.GetFrames();
		if (frames == null)
		{
			return false;
		}
		bool flag = false;
		for (int i = 0; i < frames.Length; i++)
		{
			MethodBase method = frames[i].GetMethod();
			Type reflectedType = method.ReflectedType;
			if ((object)reflectedType == calledType || (reflectedType.IsNested && (object)reflectedType.DeclaringType == calledType))
			{
				flag = true;
			}
			else if (flag)
			{
				callerMethod = method;
				callerType = reflectedType;
				return true;
			}
		}
		return false;
	}

	private static bool TryGetCallerInfo(Type calledType, out Type callerType, out MethodBase callerMethod, out IEnumerable<string> callerNamespaces)
	{
		bool skipDuplicates = true;
		bool result = TryGetCallerInfo(calledType, out var callerTypes, out var callerMethods, out callerNamespaces, skipDuplicates);
		callerMethod = callerMethods.FirstOrDefault();
		callerType = callerTypes.FirstOrDefault();
		return result;
	}

	private static bool TryGetCallerInfo(Type calledType, out IEnumerable<Type> callerTypes, out IEnumerable<MethodBase> callerMethods, out IEnumerable<string> callerNamespaces, bool skipDuplicates)
	{
		IList<MethodBase> list = new List<MethodBase>();
		IList<Type> list2 = new List<Type>();
		IList<string> list3 = new List<string>();
		StackTrace stackTrace = new StackTrace();
		StackFrame[] frames = stackTrace.GetFrames();
		if (frames == null)
		{
			callerMethods = list;
			callerTypes = list2;
			callerNamespaces = list3;
			return false;
		}
		bool flag = false;
		Type type = calledType;
		string strB = null;
		for (int i = 0; i < frames.Length; i++)
		{
			MethodBase method = frames[i].GetMethod();
			Type reflectedType = method.ReflectedType;
			string frameNamespace = reflectedType.Namespace;
			if ((object)reflectedType == calledType || (reflectedType.IsNested && (object)reflectedType.DeclaringType == calledType))
			{
				flag = true;
			}
			else
			{
				if (!flag)
				{
					continue;
				}
				list.Add(method);
				if ((object)reflectedType != type && (!reflectedType.IsNested || (object)reflectedType.DeclaringType != type) && (!skipDuplicates || !list2.Contains(reflectedType)))
				{
					list2.Add(reflectedType);
					type = reflectedType;
					if (string.Compare(frameNamespace, strB, StringComparison.CurrentCultureIgnoreCase) != 0 && (!skipDuplicates || !list3.Any((string ns) => string.Compare(frameNamespace, ns, StringComparison.CurrentCultureIgnoreCase) == 0)))
					{
						list3.Add(frameNamespace);
						strB = frameNamespace;
					}
				}
			}
		}
		callerMethods = list;
		callerTypes = list2;
		callerNamespaces = list3;
		return true;
	}

	private static bool TryGetCallerInfoObsolete(Type calledType, out Type callerType, out MethodBase callerMethod, out IEnumerable<string> callerNamespaces)
	{
		IList<string> list = new List<string>();
		callerType = null;
		callerMethod = null;
		StackTrace stackTrace = new StackTrace();
		StackFrame[] frames = stackTrace.GetFrames();
		if (frames == null)
		{
			callerNamespaces = list;
			return false;
		}
		bool flag = false;
		Type type = calledType;
		string strB = null;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < frames.Length; i++)
		{
			MethodBase method = frames[i].GetMethod();
			Type reflectedType = method.ReflectedType;
			string text = reflectedType.Namespace;
			if (!flag)
			{
				bool flag4 = (object)reflectedType == type || (reflectedType.IsNested && (object)reflectedType.DeclaringType == type);
				if (!flag4 && flag2)
				{
					callerType = reflectedType;
					callerMethod = method;
					type = reflectedType;
					flag = true;
					list.Add(text);
					strB = text;
					flag3 = true;
					continue;
				}
				flag2 = flag4;
			}
			if (flag)
			{
				bool flag5 = string.Compare(text, strB, StringComparison.CurrentCultureIgnoreCase) == 0;
				if (!flag5 && flag3)
				{
					list.Add(text);
					strB = text;
				}
				else
				{
					flag3 = flag5;
				}
			}
		}
		callerNamespaces = list;
		return true;
	}

	[Obsolete("Use method GetLoggerCallerInfo() instead")]
	private static void GetCallerInfo(out Type callerType, out MethodBase callerMethod)
	{
		StackTrace stackTrace = new StackTrace();
		StackFrame[] frames = stackTrace.GetFrames();
		bool flag = false;
		Type typeFromHandle = typeof(Logger);
		for (int i = 0; i < frames.Length; i++)
		{
			Type reflectedType = frames[i].GetMethod().ReflectedType;
			bool flag2 = (object)reflectedType == typeFromHandle || (reflectedType.IsNested && (object)reflectedType.DeclaringType == typeFromHandle);
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
		throw new InvalidOperationException("Logical error");
	}

	public static ILog GetLogger(string loggerDomainNamespace, string loggerName)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		ILoggerRepository val = null;
		ILoggerRepository[] allRepositories = LogManager.GetAllRepositories();
		ILoggerRepository[] array = allRepositories;
		foreach (ILoggerRepository val2 in array)
		{
			if (string.Compare(val2.Name, loggerDomainNamespace, ignoreCase: true) == 0)
			{
				val = val2;
				break;
			}
		}
		if (val == null)
		{
			val = allRepositories[0];
		}
		if (loggerName != null)
		{
			ILogger logger = val.GetLogger(loggerName);
			return (ILog)new LogImpl(logger);
		}
		return (ILog)new LogImpl(val.GetCurrentLoggers()[0]);
	}

	public static ILog GetLogger(string loggerDomainNamespace)
	{
		return GetLogger(loggerDomainNamespace, null);
	}
}
