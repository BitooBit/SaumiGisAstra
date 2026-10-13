using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommonUtils.AppDomainUtils;
using CommonUtils.EnvironmentUtils;
using CommonUtils.RegistryUtils;
using CommonUtils.ReportUtils;
using Microsoft.Win32;
using OracleUtils.Configuration;

namespace OracleUtils.Scanner;

public static class OracleScanner
{
	public static IEnumerable<OracleClientInstallation> GetAllOracleClientInstallations()
	{
		UtilsLoggerWrapper.Info("Environment variables:");
		TraceEnvironmentVariable("TNS_ADMIN");
		TraceEnvironmentVariable("ORACLE_HOME");
		TraceEnvironmentVariable("ORACLE_BASE");
		TraceEnvironmentVariable("ORACLE_SID");
		TraceEnvironmentVariable("PATH");
		UtilsLoggerWrapper.Info("Find all Oracle client installations");
		List<OracleClientInstallation> list = new List<OracleClientInstallation>();
		try
		{
			bool isCurrentProcessVirtualized = EnvironmentScanner.IsCurrentProcessVirtualized;
			UtilsLoggerWrapper.Info("Current process (" + (int)EnvironmentScanner.ApplicationEnvironmentBits + " bits) " + (isCurrentProcessVirtualized ? "is" : "is not") + " virtualized on current OS (" + (int)EnvironmentScanner.SystemEnvironmentBits + " bits)");
			AppPlatformBits systemEnvironmentBits = EnvironmentScanner.SystemEnvironmentBits;
			RegistryKeyView registryKeyView;
			if (isCurrentProcessVirtualized)
			{
				UtilsLoggerWrapper.Info("Try open oracle registry key \"" + OracleConstants.ORACLE_REGKEY_PATH + "\" using 64-bit registry view option, cause process is virtualized");
				registryKeyView = Registry.LocalMachine.OpenSubKey(OracleConstants.ORACLE_REGKEY_PATH, RegistryViewOptions.Registry64, isWritable: false);
				UtilsLoggerWrapper.Info("Above oracle registry key is " + ((registryKeyView != null) ? "opened successfully" : "NULL, it can't be opened"));
			}
			else
			{
				UtilsLoggerWrapper.Info("Try open oracle registry key \"" + OracleConstants.ORACLE_REGKEY_PATH + "\" without special registry view options, cause process is not virtualized");
				registryKeyView = Registry.LocalMachine.OpenSubKey(OracleConstants.ORACLE_REGKEY_PATH, RegistryViewOptions.Default, isWritable: false);
				UtilsLoggerWrapper.Info("Above oracle registry key is " + ((registryKeyView != null) ? "opened successfully" : "NULL, it can't be opened"));
			}
			if (registryKeyView != null)
			{
				IList<OracleClientInstallation> oracleClientInstallationsFromOracleRootRegKey = GetOracleClientInstallationsFromOracleRootRegKey(registryKeyView, systemEnvironmentBits, out var oracleClientOdpNetInstallations);
				IList<OdpNetInstallation> odpNetInstallationsFromOracleRootRegKey = GetOdpNetInstallationsFromOracleRootRegKey(registryKeyView, systemEnvironmentBits);
				int count = oracleClientInstallationsFromOracleRootRegKey.Count;
				UtilsLoggerWrapper.Info("Found " + count + " NOT VIRTUALIZED client installations:");
				for (int i = 0; i < count; i++)
				{
					OracleClientInstallation oracleClientInstallation = oracleClientInstallationsFromOracleRootRegKey[i];
					foreach (OdpNetInstallation item in odpNetInstallationsFromOracleRootRegKey)
					{
						string directoryName = Path.GetDirectoryName(item.UnmanagedLibrariesPath);
						if (!string.IsNullOrEmpty(directoryName) && string.Compare(directoryName.TrimEnd(new char[2]
						{
							Path.DirectorySeparatorChar,
							Path.AltDirectorySeparatorChar
						}), oracleClientInstallation.OracleHomePath.TrimEnd(new char[2]
						{
							Path.DirectorySeparatorChar,
							Path.AltDirectorySeparatorChar
						}), StringComparison.OrdinalIgnoreCase) == 0)
						{
							oracleClientInstallation.RelatedOdpNetInstallations.Add(item);
						}
					}
					UtilsLoggerWrapper.Info(i + 1 + ":");
					UtilsLoggerWrapper.Info(oracleClientInstallation.ToString());
				}
				oracleClientOdpNetInstallations.AddRange(odpNetInstallationsFromOracleRootRegKey);
				list.AddRange(oracleClientInstallationsFromOracleRootRegKey);
			}
			if (systemEnvironmentBits != AppPlatformBits.X86)
			{
				AppPlatformBits platformBits = AppPlatformBits.X86;
				RegistryKeyView registryKeyView2;
				if (isCurrentProcessVirtualized)
				{
					UtilsLoggerWrapper.Info("Try open virtualized oracle registry key \"" + OracleConstants.ORACLE_WOW6432_REGKEY_PATH + "\" using 64-bit registry view option, cause process is virtualized");
					registryKeyView2 = Registry.LocalMachine.OpenSubKey(OracleConstants.ORACLE_WOW6432_REGKEY_PATH, RegistryViewOptions.Registry64, isWritable: false);
					UtilsLoggerWrapper.Info("Above virtualized oracle registry key is " + ((registryKeyView2 != null) ? "opened successfully" : "NULL, it can't be opened"));
				}
				else
				{
					UtilsLoggerWrapper.Info("Try open virtualized oracle registry key \"" + OracleConstants.ORACLE_WOW6432_REGKEY_PATH + "\" without special registry view options, cause process is not virtualized");
					registryKeyView2 = Registry.LocalMachine.OpenSubKey(OracleConstants.ORACLE_WOW6432_REGKEY_PATH, RegistryViewOptions.Default);
					UtilsLoggerWrapper.Info("Above virtualized oracle registry key is " + ((registryKeyView2 != null) ? "opened successfully" : "NULL, it can't be opened"));
				}
				if (registryKeyView2 != null)
				{
					IList<OracleClientInstallation> oracleClientInstallationsFromOracleRootRegKey2 = GetOracleClientInstallationsFromOracleRootRegKey(registryKeyView2, platformBits, out var oracleClientOdpNetInstallations2);
					IList<OdpNetInstallation> odpNetInstallationsFromOracleRootRegKey2 = GetOdpNetInstallationsFromOracleRootRegKey(registryKeyView2, platformBits);
					int count2 = oracleClientInstallationsFromOracleRootRegKey2.Count;
					UtilsLoggerWrapper.Info("Found " + count2 + " VIRTUALIZED client installations:");
					for (int j = 0; j < count2; j++)
					{
						OracleClientInstallation oracleClientInstallation2 = oracleClientInstallationsFromOracleRootRegKey2[j];
						foreach (OdpNetInstallation item2 in odpNetInstallationsFromOracleRootRegKey2)
						{
							string directoryName2 = Path.GetDirectoryName(item2.UnmanagedLibrariesPath);
							if (!string.IsNullOrEmpty(directoryName2) && string.Compare(directoryName2.TrimEnd(new char[2]
							{
								Path.DirectorySeparatorChar,
								Path.AltDirectorySeparatorChar
							}), oracleClientInstallation2.OracleHomePath.TrimEnd(new char[2]
							{
								Path.DirectorySeparatorChar,
								Path.AltDirectorySeparatorChar
							}), StringComparison.OrdinalIgnoreCase) == 0)
							{
								oracleClientInstallation2.RelatedOdpNetInstallations.Add(item2);
							}
						}
						UtilsLoggerWrapper.Info(j + 1 + ":");
						UtilsLoggerWrapper.Info(oracleClientInstallation2.ToString());
					}
					oracleClientOdpNetInstallations2.AddRange(odpNetInstallationsFromOracleRootRegKey2);
					list.AddRange(oracleClientInstallationsFromOracleRootRegKey2);
				}
			}
		}
		catch (Exception ex)
		{
			UtilsLoggerWrapper.Error("Can't find any Oracle client installations. EXCEPTION occurred: " + ex);
		}
		return list;
	}

	public static IEnumerable<OracleClientInstallation> GetAllOracleClientInstallations(out OracleClientInstallation activeOracleClientInstallationByOci)
	{
		IEnumerable<OracleClientInstallation> allOracleClientInstallations = GetAllOracleClientInstallations();
		activeOracleClientInstallationByOci = TryGetActiveClientByOci(allOracleClientInstallations);
		return allOracleClientInstallations;
	}

	private static IList<OracleClientInstallation> GetOracleClientInstallationsFromOracleRootRegKey(RegistryKeyView oracleRootKey, AppPlatformBits platformBits, out List<OdpNetInstallation> oracleClientOdpNetInstallations)
	{
		oracleClientOdpNetInstallations = new List<OdpNetInstallation>();
		IList<OracleClientInstallation> list = new List<OracleClientInstallation>();
		UtilsLoggerWrapper.Info("Get oracle client installations from registry key: \"" + oracleRootKey.Name + "\" (" + oracleRootKey.ValueCount + " values, " + oracleRootKey.SubKeyCount + " subkeys)");
		string[] subKeyNames = oracleRootKey.GetSubKeyNames();
		UtilsLoggerWrapper.Info("Found " + subKeyNames.Length + " real subkeys for above registry key");
		string[] array = subKeyNames;
		foreach (string text in array)
		{
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			UtilsLoggerWrapper.Info("Analyze subkey \"" + text + "\". Try open it.");
			RegistryKeyView registryKeyView = oracleRootKey.OpenSubKey(text);
			if (registryKeyView != null)
			{
				UtilsLoggerWrapper.Info("Above subkey is successfully opened for analyzing");
				string value = RegistryUtils.GetValue<string>(registryKeyView, "ORACLE_HOME");
				if (string.IsNullOrEmpty(value))
				{
					continue;
				}
				UtilsLoggerWrapper.Info("Above subkey contains required parameter \"ORACLE_HOME\". Oracle client key is found.");
				OracleClientInstallation oracleClientInstallation = new OracleClientInstallation(registryKeyView, value, platformBits);
				string[] subKeyNames2 = registryKeyView.GetSubKeyNames();
				if (Enumerable.Contains(subKeyNames2, "ODP.NET"))
				{
					RegistryKeyView registryKeyView2 = registryKeyView.OpenSubKey("ODP.NET");
					if (registryKeyView2 != null)
					{
						string unmanagedLibrariesPath = Path.Combine(value, "bin");
						OdpNetInstallation item = new OdpNetInstallation(registryKeyView2, unmanagedLibrariesPath, platformBits);
						oracleClientInstallation.RelatedOdpNetInstallations.Add(item);
						oracleClientOdpNetInstallations.Add(item);
					}
				}
				list.Add(oracleClientInstallation);
			}
			else
			{
				UtilsLoggerWrapper.Warning("Registry key \"" + oracleRootKey.Name + "\\" + text + "\" is NULL, it can't be opened");
			}
		}
		return list;
	}

	private static void TraceEnvironmentVariable(string variableName)
	{
		bool flag = EnvironmentVariablesUtils.TryGetVariableValue(variableName, out var variableValue, out var variableRealTarget);
		UtilsLoggerWrapper.Info(variableName + (flag ? string.Concat(new object[4] { " (", variableRealTarget, "): ", variableValue }) : " is NOT set"));
	}

	private static IList<OdpNetInstallation> GetOdpNetInstallationsFromOracleRootRegKey(RegistryKeyView oracleRootKey, AppPlatformBits platformBits)
	{
		IList<OdpNetInstallation> list = new List<OdpNetInstallation>();
		UtilsLoggerWrapper.Info("Get ODP.NET installations from registry key: " + oracleRootKey.Name);
		string[] subKeyNames = oracleRootKey.GetSubKeyNames();
		if (Enumerable.Contains(subKeyNames, "ODP.NET"))
		{
			RegistryKeyView registryKeyView = oracleRootKey.OpenSubKey("ODP.NET");
			if (registryKeyView != null)
			{
				string[] subKeyNames2 = registryKeyView.GetSubKeyNames();
				string[] array = subKeyNames2;
				foreach (string name in array)
				{
					RegistryKeyView registryKeyView2 = registryKeyView.OpenSubKey(name);
					if (registryKeyView2 != null)
					{
						OdpNetInstallation item = new OdpNetInstallation(registryKeyView2, platformBits);
						list.Add(item);
					}
				}
			}
		}
		return list;
	}

	public static OracleClientInstallation TryGetActiveClientByOci(IEnumerable<OracleClientInstallation> oracleClientInstallations)
	{
		OracleClientInstallation[] array = oracleClientInstallations.ToArray();
		int num = int.MaxValue;
		int num2 = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IsOciDirectorySpecifiedAtEnvironmentPathVariable)
			{
				int ociDirectoryPositionAtEnvironmentPathVariable = array[i].OciDirectoryPositionAtEnvironmentPathVariable;
				if (ociDirectoryPositionAtEnvironmentPathVariable > -1 && ociDirectoryPositionAtEnvironmentPathVariable < num)
				{
					num = ociDirectoryPositionAtEnvironmentPathVariable;
					num2 = i;
				}
			}
		}
		if (num2 <= -1)
		{
			return null;
		}
		return array[num2];
	}

	public static OracleClientInstallation TryGetActiveClientByOdpNet(IEnumerable<OracleClientInstallation> oracleClientInstallations)
	{
		if (!AssemblyLoadUtils.TryFindAppDomainAssemblyByShortName("Oracle.DataAccess", out var foundAssembly) || (object)foundAssembly == null)
		{
			UtilsLoggerWrapper.Info("Oracle.DataAccess assembly wasn't still loaded to current AppDomain. Try find and load it from referenced assemblies for entry assembly.");
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			if ((object)entryAssembly != null && AssemblyLoadUtils.TryLoadAssemblyFromReferences("Oracle.DataAccess", out foundAssembly, shouldSearchReferencesRecursively: false) && (object)foundAssembly != null)
			{
				UtilsLoggerWrapper.Info("Assembly \"" + foundAssembly.FullName + "\" was just loaded to current AppDomain");
			}
		}
		if ((object)foundAssembly != null)
		{
			foreach (OracleClientInstallation oracleClientInstallation in oracleClientInstallations)
			{
				foreach (OdpNetInstallation relatedOdpNetInstallation in oracleClientInstallation.RelatedOdpNetInstallations)
				{
					AssemblyName assemblyName = null;
					try
					{
						assemblyName = AssemblyName.GetAssemblyName(relatedOdpNetInstallation.OdpNetLibraryPath);
					}
					catch (Exception)
					{
						continue;
					}
					if (string.Compare(foundAssembly.FullName, assemblyName.FullName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return oracleClientInstallation;
					}
				}
			}
		}
		else
		{
			UtilsLoggerWrapper.Warning("Can't get active Oracle client by ODP.NET, cause ODP.NET assembly wasn't loaded to current AppDomain and wasn't specified by any referenced assemblies for entry assembly.");
		}
		return null;
	}

	public static OracleClientInstallation GetCurrentActiveOdpNetClient()
	{
		IEnumerable<OracleClientInstallation> allOracleClientInstallations = GetAllOracleClientInstallations();
		if (!allOracleClientInstallations.Any())
		{
			UtilsLoggerWrapper.Warning("Can't get active Oracle client by ODP.NET, cause there are no any Oracle client found.");
			return null;
		}
		return TryGetActiveClientByOdpNet(allOracleClientInstallations);
	}

	public static TnsNamesConfiguration GetClientTnsNamesConfiguration(OracleClientInstallation client, string datasourceName)
	{
		TnsNamesOraFile tnsNamesOraFile = client.TnsNamesOraFile;
		return tnsNamesOraFile.Configurations.FirstOrDefault((TnsNamesConfiguration c) => !c.IsCommented && !c.IsDuplicate && string.Compare(c.DatasourceName, datasourceName, StringComparison.OrdinalIgnoreCase) == 0);
	}
}
