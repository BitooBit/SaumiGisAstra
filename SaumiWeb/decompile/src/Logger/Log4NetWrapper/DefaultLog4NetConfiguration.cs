using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Xml;

namespace Log4NetWrapper;

public static class DefaultLog4NetConfiguration
{
	private const string DEFAULT_LOG_FILE_NAME = "logfile.log";

	private static Dictionary<string, XmlElement> _DefaultConfigurationsMap = new Dictionary<string, XmlElement>();

	public static XmlElement Log4NetConfigurationStream => GetLog4NetConfigurationStream();

	public static XmlElement GetLog4NetConfigurationStream(string fullLogFilePath)
	{
		if (!_DefaultConfigurationsMap.ContainsKey(fullLogFilePath))
		{
			if (fullLogFilePath == null)
			{
				throw new ArgumentNullException("fullLogFilePath");
			}
			ResourceManager resourceManager = new ResourceManager("Log4NetWrapper.DefaultConfiguration", Assembly.GetExecutingAssembly());
			string format = resourceManager.GetString("DefaultConfigurationRes");
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(string.Format(format, fullLogFilePath));
			_DefaultConfigurationsMap[fullLogFilePath] = xmlDocument.DocumentElement;
		}
		return _DefaultConfigurationsMap[fullLogFilePath];
	}

	public static XmlElement GetLog4NetConfigurationStream()
	{
		return GetLog4NetConfigurationStream("logfile.log");
	}
}
