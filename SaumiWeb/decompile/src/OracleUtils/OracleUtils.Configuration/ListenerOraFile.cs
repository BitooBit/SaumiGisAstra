using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CommonUtils.RegistryUtils;

namespace OracleUtils.Configuration;

public class ListenerOraFile : ConfigFileBase
{
	private const string LISTENER_REGEX_PATTERN = "(?si)(?<=^|\\s+)listener\\s*=.*?\\(\\s*description\\s*=.*?\\(\\s*address\\s*=\\s*\\(\\s*protocol\\s*=\\s*tcp\\s*\\)\\s*\\(\\s*host\\s*=\\s*([^\\s\\)]+)\\s*\\)\\s*\\(\\s*port\\s*=\\s*([^\\s\\)]+)\\s*\\)\\s*\\)\\s*\\)";

	private const string DEFAULTSERVICE_REGEX_PATTERN = "(?si)(?<=^|\\s+)default_service_listener\\s*=\\s*\\(\\s*(\\S+)\\s*\\)";

	public string Host { get; private set; }

	public string Port { get; private set; }

	public int PortNumber
	{
		get
		{
			string port = Port;
			if (int.TryParse(port, out var result))
			{
				return result;
			}
			return -1;
		}
	}

	public string DefaultService { get; private set; }

	protected override string FileName => "listener.ora";

	public ListenerOraFile(RegistryKeyView clientRegKey, string clientOracleHomePath)
		: base(clientRegKey, clientOracleHomePath, isDatabaseConfigFile: true)
	{
		ParseListenerOraFile();
	}

	private void ParseListenerOraFile()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string path = base.Path;
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			return;
		}
		string[] array = File.ReadAllLines(path);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!text.TrimStart(new char[0]).StartsWith('#'.ToString()))
			{
				stringBuilder.AppendLine(text);
			}
		}
		string input = stringBuilder.ToString();
		Regex regex = new Regex("(?si)(?<=^|\\s+)listener\\s*=.*?\\(\\s*description\\s*=.*?\\(\\s*address\\s*=\\s*\\(\\s*protocol\\s*=\\s*tcp\\s*\\)\\s*\\(\\s*host\\s*=\\s*([^\\s\\)]+)\\s*\\)\\s*\\(\\s*port\\s*=\\s*([^\\s\\)]+)\\s*\\)\\s*\\)\\s*\\)");
		Match match = regex.Match(input);
		if (match.Success)
		{
			Host = match.Groups[1].Value;
			Port = match.Groups[2].Value;
		}
		Regex regex2 = new Regex("(?si)(?<=^|\\s+)default_service_listener\\s*=\\s*\\(\\s*(\\S+)\\s*\\)");
		Match match2 = regex2.Match(input);
		if (match2.Success)
		{
			DefaultService = match2.Groups[1].Value;
		}
	}
}
