using System;
using System.Reflection;

namespace CommonUtils.AppDomainUtils;

public abstract class AppDomainAssembliesExecutorBase<TArgument, TResult, TProxy> : AppDomainExecutorBase<TArgument, TResult, TProxy> where TProxy : AppDomainAssembliesExecutorBase<TArgument, TResult, TProxy>.AppDomainAssembliesExecutorProxyBase
{
	public abstract class AppDomainAssembliesExecutorProxyBase : AppDomainExecutorProxyBase
	{
		protected Assembly GetAssembly(string assemblySourcePath)
		{
			Assembly assembly = null;
			try
			{
				AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblySourcePath);
				return Assembly.Load(assemblyName);
			}
			catch (Exception)
			{
				return null;
			}
		}

		protected Assembly GetAssembly(AssemblyName assemblyName)
		{
			Assembly assembly = null;
			try
			{
				return Assembly.Load(assemblyName);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
