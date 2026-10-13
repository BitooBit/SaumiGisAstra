using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using CommonUtils.EnvironmentUtils;

namespace CommonUtils.AppDomainUtils;

public static class AssemblyLoadUtils
{
	public static bool TryFindAppDomainAssemblyByPath(string assemblyFilePath, out Assembly foundAssembly)
	{
		AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFilePath);
		return TryFindAppDomainAssemblyByName(assemblyName, out foundAssembly);
	}

	public static bool TryFindAppDomainAssemblyByName(AssemblyName assemblyName, out Assembly foundAssembly)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			if (string.Compare(assembly.FullName, assemblyName.FullName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				foundAssembly = assembly;
				return true;
			}
		}
		foundAssembly = null;
		return false;
	}

	public static bool TryFindAppDomainAssemblyByShortName(string assemblyShortName, out Assembly foundAssembly)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			if (string.Compare(assembly.GetName().Name, assemblyShortName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				foundAssembly = assembly;
				return true;
			}
		}
		foundAssembly = null;
		return false;
	}

	public static bool TryFindAppDomainAssemblyByType(string typeName, out Assembly foundAssembly)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			Type type = assembly.GetType(typeName);
			if ((object)type != null)
			{
				foundAssembly = assembly;
				return true;
			}
		}
		foundAssembly = null;
		return false;
	}

	public static bool CanLoadAssembly(string filePath)
	{
		PePlatformType pePlatformType = PeHeaderScanner.GetPePlatformType(filePath, out var isManaged, out var clrVersion);
		if (isManaged && clrVersion != null && ((Environment.Version.Major == 2 && clrVersion.Major <= 2) || (Environment.Version.Major == 4 && clrVersion.Major == Environment.Version.Major)))
		{
			AppPlatformBits applicationEnvironmentBits = EnvironmentScanner.ApplicationEnvironmentBits;
			return applicationEnvironmentBits.Matches(pePlatformType);
		}
		return false;
	}

	public static bool CanLoadAssembly(AssemblyName assembly)
	{
		AppPlatformBits applicationEnvironmentBits = EnvironmentScanner.ApplicationEnvironmentBits;
		return applicationEnvironmentBits.Matches(assembly.ProcessorArchitecture);
	}

	public static bool TryLoadAssemblyFromProcessModules(string assemblyShortName, out Assembly loadedAssembly)
	{
		ProcessDescription processDescription = new ProcessDescription();
		ProcessModuleCollection loadedModules = processDescription.LoadedModules;
		foreach (ProcessModule item in loadedModules)
		{
			string moduleName = item.ModuleName;
			if (string.Compare(moduleName, assemblyShortName + ".dll", StringComparison.OrdinalIgnoreCase) == 0)
			{
				string fileName = item.FileName;
				if (CanLoadAssembly(fileName))
				{
					AssemblyName assemblyName = AssemblyName.GetAssemblyName(fileName);
					loadedAssembly = Assembly.Load(assemblyName);
					return true;
				}
			}
		}
		loadedAssembly = null;
		return false;
	}

	public static bool TryLoadAssemblyFromGac(string assemblyShortName, out Assembly loadedAssembly)
	{
		ReadOnlyCollection<string> assembliesFromGac = GacScanner.GetAssembliesFromGac(assemblyShortName, GacViewOptions.Default);
		foreach (string item in assembliesFromGac)
		{
			if (CanLoadAssembly(item))
			{
				AssemblyName assemblyName = AssemblyName.GetAssemblyName(item);
				loadedAssembly = Assembly.Load(assemblyName);
				return true;
			}
		}
		loadedAssembly = null;
		return false;
	}

	public static bool TryLoadAssemblyFromReferences(string requiredAssemblyShortName, out Assembly requiredAssembly, bool shouldSearchReferencesRecursively)
	{
		HashSet<string> checkingAssemblies = new HashSet<string>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly[] array = assemblies;
		foreach (Assembly checkingAssembly in array)
		{
			if (TryLoadAssemblyFromReferences(checkingAssembly, checkingAssemblies, requiredAssemblyShortName, out requiredAssembly, shouldSearchReferencesRecursively))
			{
				return true;
			}
		}
		requiredAssembly = null;
		return false;
	}

	public static bool TryLoadAssemblyFromReferences(Assembly checkingAssembly, string requiredAssemblyShortName, out Assembly requiredAssembly, bool shouldSearchReferencesRecursively)
	{
		return TryLoadAssemblyFromReferences(checkingAssembly, new HashSet<string>(), requiredAssemblyShortName, out requiredAssembly, shouldSearchReferencesRecursively);
	}

	private static bool TryLoadAssemblyFromReferences(Assembly checkingAssembly, HashSet<string> checkingAssemblies, string requiredAssemblyShortName, out Assembly requiredAssembly, bool shouldSearchReferencesRecursively)
	{
		if (checkingAssemblies.Contains(checkingAssembly.FullName))
		{
			requiredAssembly = null;
			return false;
		}
		checkingAssemblies.Add(checkingAssembly.FullName);
		AssemblyName[] referencedAssemblies = checkingAssembly.GetReferencedAssemblies();
		AssemblyName[] array = referencedAssemblies;
		foreach (AssemblyName assemblyName in array)
		{
			if (string.Compare(assemblyName.Name, requiredAssemblyShortName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				requiredAssembly = Assembly.Load(assemblyName);
				return true;
			}
			if (shouldSearchReferencesRecursively && TryLoadAssemblyFromReferences(assemblyName, checkingAssemblies, requiredAssemblyShortName, out requiredAssembly))
			{
				return true;
			}
		}
		requiredAssembly = null;
		return false;
	}

	private static bool TryLoadAssemblyFromReferences(AssemblyName checkingAssembly, HashSet<string> checkingAssemblies, string requiredAssemblyShortName, out Assembly requiredAssembly)
	{
		if (checkingAssemblies.Contains(checkingAssembly.FullName))
		{
			requiredAssembly = null;
			return false;
		}
		checkingAssemblies.Add(checkingAssembly.FullName);
		AssemblyNameDescription assemblyDescriptionUsingAppDomain = AssemblyCheckUtils.GetAssemblyDescriptionUsingAppDomain(checkingAssembly, checkingAssembly.Name + "GetReferencedAssembliesTempDomain");
		AssemblyName[] referencedAssemblies = assemblyDescriptionUsingAppDomain.ReferencedAssemblies;
		AssemblyName[] array = referencedAssemblies;
		foreach (AssemblyName assemblyName in array)
		{
			if (string.Compare(assemblyName.Name, requiredAssemblyShortName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				requiredAssembly = Assembly.Load(assemblyName);
				return true;
			}
			if (TryLoadAssemblyFromReferences(assemblyName, checkingAssemblies, requiredAssemblyShortName, out requiredAssembly))
			{
				return true;
			}
		}
		requiredAssembly = null;
		return false;
	}
}
