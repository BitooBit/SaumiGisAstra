using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CommonUtils.RegistryUtils;

namespace OracleUtils.Configuration;

public class TnsNamesOraFile : ConfigFileBase
{
	public const string REGEX = "\\s*(\\S+)\\s*=.*?\\(\\s*description\\s*=.*?\\(\\s*address\\s*=\\s*\\(\\s*protocol\\s*=.*?\\)\\s*\\(\\s*host\\s*=\\s*([^\\s\\)]+)\\s*\\)\\s*\\(\\s*port\\s*=\\s*([^\\s\\)]+)\\s*\\).*?\\(\\s*connect_data\\s*=.*?\\(\\s*service_name\\s*=\\s*([^\\s\\)]+)\\s*\\).*?\\)\\s*\\)";

	private IEnumerable<TnsNamesConfiguration> _Configurations;

	public IEnumerable<TnsNamesConfiguration> Configurations
	{
		get
		{
			if (_Configurations == null)
			{
				_Configurations = GetConfigurations();
			}
			return _Configurations;
		}
	}

	protected override string FileName => "tnsnames.ora";

	public TnsNamesOraFile(RegistryKeyView clientRegKey, string clientOracleHomePath)
		: base(clientRegKey, clientOracleHomePath, isDatabaseConfigFile: false)
	{
	}

	private IEnumerable<TnsNamesConfiguration> GetConfigurations()
	{
		IList<TnsNamesConfiguration> list = new List<TnsNamesConfiguration>();
		string path = base.Path;
		if (!File.Exists(path))
		{
			return list;
		}
		string[] array = File.ReadAllLines(path);
		Regex regex = new Regex("\\s*(\\S+)\\s*=.*?\\(\\s*description\\s*=.*?\\(\\s*address\\s*=\\s*\\(\\s*protocol\\s*=.*?\\)\\s*\\(\\s*host\\s*=\\s*([^\\s\\)]+)\\s*\\)\\s*\\(\\s*port\\s*=\\s*([^\\s\\)]+)\\s*\\).*?\\(\\s*connect_data\\s*=.*?\\(\\s*service_name\\s*=\\s*([^\\s\\)]+)\\s*\\).*?\\)\\s*\\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		string text = null;
		bool isCommented = false;
		for (int i = 0; i < array.Length; i++)
		{
			string text2 = array[i];
			if (string.IsNullOrEmpty(text2.Trim()))
			{
				text = null;
				isCommented = false;
				continue;
			}
			if (text2.TrimStart(new char[0]).StartsWith('#'.ToString()))
			{
				isCommented = true;
				text2 = text2.Replace('#'.ToString(), string.Empty);
			}
			text = text + text2 + Environment.NewLine;
			if (!string.IsNullOrEmpty(text) && regex.IsMatch(text))
			{
				Match match = regex.Match(text);
				string value = match.Groups[1].Value;
				string value2 = match.Groups[2].Value;
				string value3 = match.Groups[3].Value;
				string value4 = match.Groups[4].Value;
				bool isDuplicate = false;
				TnsNamesConfiguration item = new TnsNamesConfiguration(value, value2, value3, value4, isCommented, isDuplicate);
				list.Add(item);
				text = null;
				isCommented = false;
			}
		}
		for (int j = 0; j < list.Count - 1; j++)
		{
			TnsNamesConfiguration tnsNamesConfiguration = list[j];
			string datasourceName = tnsNamesConfiguration.DatasourceName;
			for (int k = j + 1; k < list.Count; k++)
			{
				TnsNamesConfiguration tnsNamesConfiguration2 = list[k];
				if (!tnsNamesConfiguration2.IsCommented && string.Compare(tnsNamesConfiguration2.DatasourceName, datasourceName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					tnsNamesConfiguration.IsDuplicate = true;
					break;
				}
			}
		}
		return list;
	}
}
