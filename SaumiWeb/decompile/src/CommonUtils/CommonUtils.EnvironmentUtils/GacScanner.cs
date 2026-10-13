using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommonUtils.IOUtils;
using CommonUtils.RegistryUtils;
using Microsoft.Win32;

namespace CommonUtils.EnvironmentUtils;

public static class GacScanner
{
	public const string ASSEMBLY_SHORTNAME_FEW_SYMBOLS_WILDCARD = "*";

	public const string ASSEMBLY_SHORTNAME_SINGLE_SYMBOL_WILDCARD = "?";

	private const string MICROSOFT_NET_DIRECTORY_NAME = "Microsoft.NET";

	private const string ASSEMBLY_DIRECTORY_NAME = "assembly";

	private const string CLR_2_NATIVE_IMAGES_DIRECTORY_NAME_PREFIX = "NativeImages";

	private const string MICROSOFT_REGKEY_NAME = "Microsoft";

	private const string FUSION_REGKEY_NAME = "Fusion";

	private const string FUSION_CACHELOCATION_REGENTRY_NAME = "CacheLocation";

	private static readonly string[] CLR_2_TEMP_DIRECTORY_NAMES = new string[2] { "temp", "tmp" };

	private static readonly string FUSION_REGKEY_PATH = "SOFTWARE" + '\\' + "Microsoft" + '\\' + "Fusion";

	public static ReadOnlyCollection<string> GetAssembliesFromGac(GacViewOptions gacViewOptions)
	{
		return GetAssembliesFromGac("*", gacViewOptions);
	}

	public static ReadOnlyCollection<string> GetAssembliesFromGac(string assemblyShortName, GacViewOptions gacViewOptions)
	{
		IList<string> list = new List<string>();
		int major = Environment.Version.Major;
		if ((gacViewOptions == GacViewOptions.Default && (major == 2 || major == 4)) || gacViewOptions == GacViewOptions.Clr2 || gacViewOptions == GacViewOptions.All)
		{
			string gacLocation = GetGacLocation(2);
			if (Directory.Exists(gacLocation))
			{
				string[] directories = Directory.GetDirectories(gacLocation);
				string[] array = directories;
				foreach (string path in array)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(path);
					string name = directoryInfo.Name;
					string[] cLR_2_TEMP_DIRECTORY_NAMES = CLR_2_TEMP_DIRECTORY_NAMES;
					foreach (string strB in cLR_2_TEMP_DIRECTORY_NAMES)
					{
						if (string.Compare(name, strB, StringComparison.OrdinalIgnoreCase) == 0)
						{
							break;
						}
					}
					if (name.StartsWith("NativeImages", StringComparison.OrdinalIgnoreCase))
					{
						break;
					}
					string[] files = Directory.GetFiles(path, assemblyShortName + ".dll", SearchOption.AllDirectories);
					if (files != null)
					{
						string[] array2 = files;
						foreach (string item in array2)
						{
							list.Add(item);
						}
					}
				}
			}
		}
		if ((gacViewOptions == GacViewOptions.Default && major == 4) || gacViewOptions == GacViewOptions.Clr4 || gacViewOptions == GacViewOptions.All)
		{
			string gacLocation2 = GetGacLocation(4);
			if (Directory.Exists(gacLocation2))
			{
				string[] directories2 = Directory.GetDirectories(gacLocation2);
				string[] array3 = directories2;
				foreach (string path2 in array3)
				{
					string[] files2 = Directory.GetFiles(path2, assemblyShortName + ".dll", SearchOption.AllDirectories);
					if (files2 != null)
					{
						string[] array4 = files2;
						foreach (string item2 in array4)
						{
							list.Add(item2);
						}
					}
				}
			}
		}
		return new ReadOnlyCollection<string>(list);
	}

	private static string GetGacLocation(int clrVersion)
	{
		string value = CommonUtils.RegistryUtils.RegistryUtils.GetValue<string>(RegistryHive.LocalMachine, FUSION_REGKEY_PATH, "CacheLocation", RegistryViewOptions.Registry64);
		string path = ((!string.IsNullOrEmpty(value)) ? value : (clrVersion switch
		{
			2 => DiscUtils.GetParentDirectory(Environment.SystemDirectory), 
			4 => Path.Combine(DiscUtils.GetParentDirectory(Environment.SystemDirectory), "Microsoft.NET"), 
			_ => throw new ExecutionEngineException("Can't get GAC location! Invalid CLR version " + clrVersion + " detected!"), 
		}));
		return Path.Combine(path, "assembly");
	}
}
