using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace Log4NetWrapper;

internal class DefaultLogger
{
	private const string SAUMI_DATA_BASE_FOLDER_NAME_USERSPACE = "Saumi";

	private const string SAUMI_LOGS_BASE_FOLDER_NAME_USERSPACE = "Logs";

	private const string DEFAULT_LOGGER_REPOSITORY = "Log2NetWrapperDefaultLoggerRepository_8C492A24-06C0-4BB8-BE2C-D13A8A932F6A";

	private const string DEFAULT_LOGGER_NAME = "Log2NetWrapperDefaultLoggerName_86E27522-9842-49EB-BEBC-3A91F352D908";

	private const string DEFAULT_LOG_FILE_NAME = "defaultlog2netwrapper.log";

	private static ILog _DefaultLoggerInstance;

	private static string SaumiDataFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Saumi");

	public static string SaumiLogsFolderPath => Path.Combine(SaumiDataFolderPath, "Logs");

	public static ILog DefaultLoggerInstance => _DefaultLoggerInstance;

	public static ILog[] AllConfiguredLoggers
	{
		get
		{
			ILoggerRepository[] allRepositories = LogManager.GetAllRepositories();
			if (allRepositories == null || allRepositories.Length == 0)
			{
				return null;
			}
			return (ILog[])(object)((IEnumerable<ILoggerRepository>)allRepositories).Select((Func<ILoggerRepository, LogImpl>)delegate(ILoggerRepository repo)
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				return new LogImpl(repo.GetLogger("SaumiLogwrapperDefaultLogger"));
			}).ToArray();
		}
	}

	static DefaultLogger()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		string text = null;
		try
		{
			Process currentProcess = Process.GetCurrentProcess();
			if (currentProcess.MainModule != null)
			{
				text = Path.GetFileNameWithoutExtension(currentProcess.MainModule.FileName) + ".log";
			}
		}
		catch (Exception)
		{
		}
		ILoggerRepository val = LogManager.CreateRepository("Log2NetWrapperDefaultLoggerRepository_8C492A24-06C0-4BB8-BE2C-D13A8A932F6A");
		Hierarchy val2 = (Hierarchy)val;
		ILogger logger = ((LoggerRepositorySkeleton)val2).GetLogger("Log2NetWrapperDefaultLoggerName_86E27522-9842-49EB-BEBC-3A91F352D908");
		Logger val3 = (Logger)(object)((logger is Logger) ? logger : null);
		if (string.IsNullOrEmpty(text))
		{
			text = "defaultlog2netwrapper.log";
		}
		Logger.Paths.ProvideLogsBaseFolder(SaumiLogsFolderPath);
		string logFileFullPath = Path.Combine(Logger.Paths.ProvideLogsBaseFolder(SaumiLogsFolderPath), text);
		IAppender defaultRollingFileAppender = Logger.GetDefaultRollingFileAppender(logFileFullPath, isCreateNewIfExisting: false);
		val3.AddAppender(defaultRollingFileAppender);
		((LoggerRepositorySkeleton)val2).Configured = true;
		_DefaultLoggerInstance = (ILog)new LogImpl((ILogger)(object)val3);
	}
}
