using System.Reflection;

namespace CommonUtils.AppDomainUtils;

public static class AssemblyCheckUtils
{
	public static bool CheckAssemblyUsingAppDomain(string assemblyFullPath, string appDomainName)
	{
		AppDomainGacChecker appDomainGacChecker = new AppDomainGacChecker();
		return appDomainGacChecker.Execute(assemblyFullPath, appDomainName);
	}

	public static AssemblyNameDescription GetAssemblyDescriptionUsingAppDomain(string assemblyFullPath, string appDomainName)
	{
		AppDomainAssemblyByPathDescriber appDomainAssemblyByPathDescriber = new AppDomainAssemblyByPathDescriber();
		return appDomainAssemblyByPathDescriber.Execute(assemblyFullPath, appDomainName);
	}

	public static AssemblyNameDescription GetAssemblyDescriptionUsingAppDomain(AssemblyName assemblyName, string appDomainName)
	{
		AppDomainAssemblyByNameDescriber appDomainAssemblyByNameDescriber = new AppDomainAssemblyByNameDescriber();
		return appDomainAssemblyByNameDescriber.Execute(assemblyName, appDomainName);
	}
}
