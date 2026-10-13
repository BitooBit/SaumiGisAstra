namespace CommonUtils.AppDomainUtils;

public class AppDomainGacChecker : AppDomainAssembliesExecutorBase<string, bool, AppDomainGacChecker.AppDomainGacCheckerProxy>
{
	public class AppDomainGacCheckerProxy : AppDomainAssembliesExecutorProxyBase
	{
		public override bool Execute(string assemblySourcePath)
		{
			return GetAssembly(assemblySourcePath)?.GlobalAssemblyCache ?? false;
		}
	}
}
