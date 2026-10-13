using System.Reflection;

namespace CommonUtils.AppDomainUtils;

public class AppDomainAssemblyByPathDescriber : AppDomainAssembliesExecutorBase<string, AssemblyNameDescription, AppDomainAssemblyByPathDescriber.AppDomainAssemblyByPathDescriberProxy>
{
	public class AppDomainAssemblyByPathDescriberProxy : AppDomainAssembliesExecutorProxyBase
	{
		public override AssemblyNameDescription Execute(string assemblySourcePath)
		{
			AssemblyNameDescription result = null;
			Assembly assembly = GetAssembly(assemblySourcePath);
			if ((object)assembly != null)
			{
				result = new AssemblyNameDescription(assembly);
			}
			return result;
		}
	}
}
