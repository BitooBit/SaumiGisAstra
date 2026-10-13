using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CommonUtils.ReportUtils;

public class FileLogger
{
	private enum LogMessageType
	{
		Info,
		Warning,
		Error
	}

	public const string DATE_TIME_STRING_FORMAT = "yyyy.MM.dd HH:mm:ss.fff";

	public const long MAX_LOG_FILE_SIZE_DEFAULT_VALUE = 10485760L;

	public const bool NEED_CONSOLE_OUT_DEFAULT_VALUE = true;

	private static long _MaxLogFileSize;

	private static bool _NeedConsoleOut;

	private static string _LogFilePath;

	private static bool _IsInitialized;

	private static Encoding _LogFileEncoding;

	private static readonly Dictionary<LogMessageType, ConsoleColor> _MessageTypeColors;

	public static string LogFilePath => _LogFilePath;

	public static bool IsInitialized => _IsInitialized;

	public static Encoding LogFileEncoding
	{
		get
		{
			return _LogFileEncoding;
		}
		set
		{
			_LogFileEncoding = value;
		}
	}

	static FileLogger()
	{
		_LogFileEncoding = Encoding.Default;
		_MessageTypeColors = new Dictionary<LogMessageType, ConsoleColor>();
		_MessageTypeColors.Add(LogMessageType.Info, ConsoleColor.White);
		_MessageTypeColors.Add(LogMessageType.Warning, ConsoleColor.Yellow);
		_MessageTypeColors.Add(LogMessageType.Error, ConsoleColor.Red);
	}

	public static void Initialize(string logFilePath, long maxLogFileSize = 10485760L, bool needConsoleOut = true)
	{
		if (!_IsInitialized)
		{
			string directoryName = Path.GetDirectoryName(logFilePath);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			_LogFilePath = (Path.IsPathRooted(logFilePath) ? logFilePath : Path.GetFullPath(logFilePath));
			_MaxLogFileSize = maxLogFileSize;
			_NeedConsoleOut = needConsoleOut;
			_IsInitialized = true;
		}
	}

	public static void Info(string message)
	{
		WriteToLog(LogMessageType.Info, message);
	}

	public static void Warning(string message)
	{
		WriteToLog(LogMessageType.Warning, message);
	}

	public static void Error(string message)
	{
		WriteToLog(LogMessageType.Error, message);
	}

	private static void WriteToLog(LogMessageType messageType, string message)
	{
		if (!_IsInitialized)
		{
			return;
		}
		CheckMaxLogFileSize();
		string callerDescription = TraceUtils.GetCallerDescription(typeof(FileLogger));
		string value = string.Format("[{0}] {1} - {2}: {3}", new object[4]
		{
			DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff"),
			messageType,
			callerDescription,
			message
		});
		using (FileStream stream = File.Open(_LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Write))
		{
			using TextWriter textWriter = new StreamWriter(stream, LogFileEncoding);
			textWriter.WriteLine(value);
		}
		if (_NeedConsoleOut)
		{
			Console.ForegroundColor = _MessageTypeColors[messageType];
			Console.WriteLine(value);
			Console.ResetColor();
		}
	}

	private static void CheckMaxLogFileSize()
	{
		if (!File.Exists(_LogFilePath))
		{
			return;
		}
		FileInfo fileInfo = new FileInfo(_LogFilePath);
		long length = fileInfo.Length;
		if (length < _MaxLogFileSize)
		{
			return;
		}
		try
		{
			using (File.Open(_LogFilePath, FileMode.Truncate, FileAccess.Write, FileShare.Write))
			{
			}
		}
		catch (Exception)
		{
		}
	}
}
