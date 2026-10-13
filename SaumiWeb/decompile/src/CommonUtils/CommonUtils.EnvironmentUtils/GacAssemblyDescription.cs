using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CommonUtils.ParseUtils;

namespace CommonUtils.EnvironmentUtils;

public class GacAssemblyDescription
{
	private const string GAC_ASSEMBLY_FILEPATH_REGEX_PATTERN = "(?si).*?\\\\assembly\\\\(GAC_{0,1}\\S{0,4})\\\\(\\S+)\\\\\\S*?_{0,1}(\\d+\\.\\d+\\.\\d+\\.\\d+)_(\\S*)_(\\S+)\\\\\\S+";

	private const string GAC_ASSEMBLY_DESCRIPTION_TEMPLATE = "{0}, Version={1}, Culture={2}, PublicKeyToken={3}{4}";

	private readonly string _FilePath;

	private readonly string _Name;

	private readonly string _VersionString;

	private Version _Version;

	private readonly string _CultureString;

	private CultureInfo _Culture;

	private readonly string _ArchitectureDirectoryName;

	private ProcessorArchitecture? _ProcessorArchitecture;

	private readonly string _PublicKeyTokenString;

	private byte[] _PublicKeyToken;

	public string FilePath => _FilePath;

	public string Name => _Name;

	public bool? IsVersionParsedSuccessfully { get; private set; }

	public Version Version
	{
		get
		{
			if (!IsVersionParsedSuccessfully.HasValue)
			{
				IsVersionParsedSuccessfully = _VersionString.TryParseToVersion(out _Version);
			}
			return _Version;
		}
	}

	public bool? IsCultureParsedSuccessfully { get; private set; }

	public CultureInfo Culture
	{
		get
		{
			if (!IsCultureParsedSuccessfully.HasValue)
			{
				try
				{
					_Culture = (string.IsNullOrEmpty(_CultureString) ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(_CultureString));
					IsCultureParsedSuccessfully = true;
				}
				catch (Exception)
				{
					IsCultureParsedSuccessfully = false;
				}
			}
			return _Culture;
		}
	}

	public ProcessorArchitecture ProcessorArchitecture
	{
		get
		{
			if (!_ProcessorArchitecture.HasValue)
			{
				_ProcessorArchitecture = GetProcessorArchitectureFromDirectoryName(_ArchitectureDirectoryName);
			}
			return _ProcessorArchitecture.Value;
		}
	}

	public string PublicKeyTokenString => _PublicKeyTokenString;

	public byte[] PublicKeyToken
	{
		get
		{
			if (_PublicKeyToken == null)
			{
				_PublicKeyToken = PublicKeyTokenFromString(_PublicKeyTokenString);
			}
			return _PublicKeyToken;
		}
	}

	public string FullName => string.Format("{0}, Version={1}, Culture={2}, PublicKeyToken={3}{4}", new object[5]
	{
		Name,
		Version,
		Culture.Equals(CultureInfo.InvariantCulture) ? "neutral" : Culture.ToString(),
		_PublicKeyTokenString,
		(ProcessorArchitecture != ProcessorArchitecture.None) ? (", processorArchitecture=" + ProcessorArchitecture) : string.Empty
	});

	public GacAssemblyDescription(string assemblyFilePath)
	{
		Match match = Regex.Match(assemblyFilePath, "(?si).*?\\\\assembly\\\\(GAC_{0,1}\\S{0,4})\\\\(\\S+)\\\\\\S*?_{0,1}(\\d+\\.\\d+\\.\\d+\\.\\d+)_(\\S*)_(\\S+)\\\\\\S+");
		if (!match.Success)
		{
			throw new ArgumentException("Specified assembly file path (" + assemblyFilePath + ") doesn't determine GAC assembly", "assemblyFilePath");
		}
		_FilePath = assemblyFilePath;
		_ArchitectureDirectoryName = match.Groups[1].Value;
		_Name = match.Groups[2].Value;
		_VersionString = match.Groups[3].Value;
		_CultureString = match.Groups[4].Value;
		_PublicKeyTokenString = match.Groups[5].Value;
	}

	private byte[] PublicKeyTokenFromString(string publicKeyTokenString)
	{
		int num = publicKeyTokenString.Length / 2;
		byte[] array = new byte[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = Convert.ToByte(new string(new char[2]
			{
				publicKeyTokenString[i * 2],
				publicKeyTokenString[i * 2 + 1]
			}), 16);
		}
		return array;
	}

	private string PublicKeyTokenToString(byte[] publicKeyToken)
	{
		return publicKeyToken.Aggregate(null, (string current, byte b) => current + b.ToString("x2"));
	}

	private ProcessorArchitecture GetProcessorArchitectureFromDirectoryName(string gacDirectoryName)
	{
		if (string.Compare(gacDirectoryName, "GAC_32", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return ProcessorArchitecture.X86;
		}
		if (string.Compare(gacDirectoryName, "GAC_64", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return ProcessorArchitecture.Amd64;
		}
		if (string.Compare(gacDirectoryName, "GAC_MSIL", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return ProcessorArchitecture.MSIL;
		}
		return ProcessorArchitecture.None;
	}

	public override string ToString()
	{
		return FullName;
	}
}
