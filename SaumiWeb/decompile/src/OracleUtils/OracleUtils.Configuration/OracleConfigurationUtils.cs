using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.EnterpriseServices.Internal;
using System.IO;
using CommonUtils.EnvironmentUtils;
using CommonUtils.RegistryUtils;
using CommonUtils.ReportUtils;
using OracleUtils.Scanner;

namespace OracleUtils.Configuration;

public static class OracleConfigurationUtils
{
	public static bool SetActiveByOci(OracleClientInstallation oracleClientInstallation)
	{
		string ociDirectoryPath = oracleClientInstallation.OciDirectoryPath;
		if (!string.IsNullOrEmpty(ociDirectoryPath))
		{
			EnvironmentVariablesUtils.PlaceCustomValueFirstInPersistentVariable("PATH", ociDirectoryPath);
			return true;
		}
		return false;
	}

	public static bool FixNlsLangParameter(OracleClientInstallation oracleClientInstallation, string nlsLangParameterValue = "AMERICAN_AMERICA.CL8MSWIN1251")
	{
		string regKeyPath = oracleClientInstallation.RegKeyPath;
		UtilsLoggerWrapper.Info("Try fix NLS_LANG parameter value for Oracle client described by registry key " + regKeyPath);
		Exception ex = RegistryUtils.SetValue(regKeyPath, "NLS_LANG", nlsLangParameterValue, RegistryViewOptions.Registry64, createBackupEntry: true);
		if (ex == null)
		{
			UtilsLoggerWrapper.Info("NLS_LANG parameter value was successfully fixed to required: " + nlsLangParameterValue);
			return true;
		}
		UtilsLoggerWrapper.Error("Can't fix NLS_LANG parameter value! Exception occurred: " + ex);
		return false;
	}

	public static bool ConfigureSqlNetOraFile(OracleClientInstallation oracleClientInstallation, string parameterName, string parameterValue)
	{
		string path = oracleClientInstallation.SqlNetOraFile.Path;
		if (File.Exists(path))
		{
			return SqlNetOraFileUtils.SetConfiguration(path, parameterName, parameterValue);
		}
		return false;
	}

	public static bool ConfigureTnsNamesOraFile(OracleClientInstallation oracleClientInstallation, string configurationName, string host, string port, string serviceName)
	{
		string path = oracleClientInstallation.TnsNamesOraFile.Path;
		if (TnsNamesOraFileUtils.CheckFileExistence(path))
		{
			UtilsLoggerWrapper.Info($"File \"{path}\" already exists. It will be modified.");
			if (TnsNamesOraFileUtils.IsSameConfigurationExactlyExists(path, configurationName, host, port, serviceName))
			{
				TnsNamesOraFileUtils.RemoveConfiguration(path, configurationName, host, port, serviceName);
				UtilsLoggerWrapper.Info($"The exactly same {configurationName}-configuration was removed");
			}
			int num = TnsNamesOraFileUtils.DisableConfigurations(path, configurationName);
			UtilsLoggerWrapper.Info($"Old {configurationName}-configurations count, that were disabled: {num}");
		}
		else
		{
			UtilsLoggerWrapper.Info($"File \"{path}\" doesn't exists. It will be created.");
		}
		TnsNamesOraFileUtils.AppendConfiguration(path, configurationName, host, port, serviceName);
		UtilsLoggerWrapper.Info($"File \"{path}\" was successfully configured.");
		return true;
	}

	public static bool TryInstallOdpNetAssembliesToGac(OdpNetInstallation odpNetInstallation)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		try
		{
			UtilsLoggerWrapper.Info("Try install ODP.NET assemblies to GAC...");
			Publish val = new Publish();
			string odpNetLibraryPath = odpNetInstallation.OdpNetLibraryPath;
			UtilsLoggerWrapper.Info("Try install assembly \"" + odpNetLibraryPath + "\" to GAC");
			val.GacInstall(odpNetLibraryPath);
			UtilsLoggerWrapper.Info("Above assembly was installed to GAC");
			string publisherPoliciesPath = odpNetInstallation.PublisherPoliciesPath;
			if (!string.IsNullOrEmpty(publisherPoliciesPath))
			{
				string[] files = Directory.GetFiles(publisherPoliciesPath);
				string[] array = files;
				foreach (string text in array)
				{
					string fileName = Path.GetFileName(text);
					if (!string.IsNullOrEmpty(fileName) && fileName.StartsWith("Policy.", StringComparison.OrdinalIgnoreCase) && fileName.EndsWith(".Oracle.DataAccess.dll", StringComparison.OrdinalIgnoreCase))
					{
						UtilsLoggerWrapper.Info("Try install assembly \"" + text + "\" to GAC");
						val.GacInstall(text);
						UtilsLoggerWrapper.Info("Above assembly was installed to GAC");
					}
				}
			}
			string resourcesPath = odpNetInstallation.ResourcesPath;
			if (!string.IsNullOrEmpty(resourcesPath))
			{
				string[] directories = Directory.GetDirectories(resourcesPath);
				string[] array2 = directories;
				foreach (string path in array2)
				{
					string text2 = Path.Combine(path, "Oracle.DataAccess.resources.dll");
					if (File.Exists(text2))
					{
						UtilsLoggerWrapper.Info("Try install assembly \"" + text2 + "\" to GAC");
						val.GacInstall(text2);
						UtilsLoggerWrapper.Info("Above assembly was installed to GAC");
					}
				}
			}
			UtilsLoggerWrapper.Info("ODP.NET assemblies were successfully installed to GAC");
			return true;
		}
		catch (Exception ex)
		{
			UtilsLoggerWrapper.Error(string.Concat("Can't install ODP.NET assemblies to GAC. Exception occurred: \"", ex, "\"."));
			return false;
		}
	}

	public static ReadOnlyCollection<string> GetOdpNetAssembliesFromGac(bool mainAssembliesOnly, GacViewOptions gacViewOptions = GacViewOptions.Default)
	{
		if (!mainAssembliesOnly)
		{
			return GacScanner.GetAssembliesFromGac("*Oracle.DataAccess*", gacViewOptions);
		}
		return GacScanner.GetAssembliesFromGac("Oracle.DataAccess", gacViewOptions);
	}

	public static void GetOdpNetAssembliesFromGac(bool mainAssembliesOnly, IEnumerable<OdpNetInstallation> odpNetInstallations, out IDictionary<OdpNetInstallation, IList<string>> relatedOdpNetAssemblies, out IList<string> unrelatedOdpNetAssemblies, GacViewOptions gacViewOptions = GacViewOptions.Default)
	{
		relatedOdpNetAssemblies = new Dictionary<OdpNetInstallation, IList<string>>();
		unrelatedOdpNetAssemblies = new List<string>();
		ReadOnlyCollection<string> odpNetAssembliesFromGac = GetOdpNetAssembliesFromGac(mainAssembliesOnly, gacViewOptions);
		foreach (OdpNetInstallation odpNetInstallation in odpNetInstallations)
		{
			relatedOdpNetAssemblies[odpNetInstallation] = new List<string>();
		}
		foreach (string item in odpNetAssembliesFromGac)
		{
			bool flag = false;
			foreach (KeyValuePair<OdpNetInstallation, IList<string>> relatedOdpNetAssembly in relatedOdpNetAssemblies)
			{
				if (relatedOdpNetAssembly.Key.Contains(item))
				{
					relatedOdpNetAssembly.Value.Add(item);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				unrelatedOdpNetAssemblies.Add(item);
			}
		}
	}

	public static void GetOdpNetAssembliesFromGac(bool mainAssembliesOnly, out IDictionary<OdpNetInstallation, IList<string>> validOdpNetAssemblies, out IList<string> lostOdpNetAssemblies, GacViewOptions gacViewOptions = GacViewOptions.Default)
	{
		IEnumerable<OracleClientInstallation> allOracleClientInstallations = OracleScanner.GetAllOracleClientInstallations();
		List<OdpNetInstallation> list = new List<OdpNetInstallation>();
		foreach (OracleClientInstallation item in allOracleClientInstallations)
		{
			if (item.HasRelatedOdpNetInstallation)
			{
				IList<OdpNetInstallation> relatedOdpNetInstallations = item.RelatedOdpNetInstallations;
				list.AddRange(relatedOdpNetInstallations);
			}
		}
		GetOdpNetAssembliesFromGac(mainAssembliesOnly, list, out validOdpNetAssemblies, out lostOdpNetAssemblies, gacViewOptions);
	}

	public static void RemoveLostOdpNetAssembliesFromGac(bool mainAssembliesOnly, GacViewOptions gacViewOptions = GacViewOptions.Default)
	{
		GetOdpNetAssembliesFromGac(mainAssembliesOnly: false, out var _, out var lostOdpNetAssemblies, gacViewOptions);
		RemoveLostOdpNetAssembliesFromGac(lostOdpNetAssemblies);
	}

	public static void RemoveLostOdpNetAssembliesFromGac(IEnumerable<string> lostOdpNetAssemblies)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		foreach (string lostOdpNetAssembly in lostOdpNetAssemblies)
		{
			UtilsLoggerWrapper.Info("GAC assembly " + lostOdpNetAssembly + " is lost. It will be removed from GAC.");
			Publish publish = new Publish();
			publish.GacRemoveUnambiguously(lostOdpNetAssembly);
			UtilsLoggerWrapper.Info("Above assembly was successfully removed from GAC");
		}
	}

	public static void SetTnsAdminMachineVariable(string tnsAdminDirectoryPath)
	{
		EnvironmentVariablesUtils.SetMachineVariable("TNS_ADMIN", tnsAdminDirectoryPath);
	}

	public static void RemoveTnsAdminMachineVariable()
	{
		EnvironmentVariablesUtils.SetMachineVariable("TNS_ADMIN", null);
	}

	public static void SetOracleHomeMachineVariable(string oracleHomeDirectoryPath)
	{
		EnvironmentVariablesUtils.SetMachineVariable("ORACLE_HOME", oracleHomeDirectoryPath);
	}

	public static void RemoveOracleHomeMachineVariable()
	{
		EnvironmentVariablesUtils.SetMachineVariable("ORACLE_HOME", null);
	}
}
