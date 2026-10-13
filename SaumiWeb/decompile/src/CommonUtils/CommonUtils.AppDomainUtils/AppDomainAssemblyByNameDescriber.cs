using System.Reflection;

namespace CommonUtils.AppDomainUtils;

public class AppDomainAssemblyByNameDescriber : AppDomainAssembliesExecutorBase<AssemblyName, AssemblyNameDescription, AppDomainAssemblyByNameDescriber.AppDomainAssemblyByNameDescriberProxy>
{
	public class AppDomainAssemblyByNameDescriberProxy : AppDomainAssembliesExecutorProxyBase
	{
		public override AssemblyNameDescription Execute(AssemblyName assemblyName)
		{
			AssemblyNameDescription result = null;
			Assembly assembly = GetAssembly(assemblyName);
			if ((object)assembly != null)
			{
				result = new AssemblyNameDescription(assembly);
			}
			return result;
		}
	}
}
