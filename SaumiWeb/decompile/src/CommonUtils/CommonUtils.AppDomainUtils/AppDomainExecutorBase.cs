using System;
using System.Reflection;

namespace CommonUtils.AppDomainUtils;

public abstract class AppDomainExecutorBase<TArgument, TResult, TProxy> where TProxy : AppDomainExecutorBase<TArgument, TResult, TProxy>.AppDomainExecutorProxyBase
{
	public abstract class AppDomainExecutorProxyBase : MarshalByRefObject
	{
		public abstract TResult Execute(TArgument argument);
	}

	public TResult Execute(TArgument argument, string appDomainName)
	{
		TResult result = default(TResult);
		Type typeFromHandle = typeof(TProxy);
		Assembly assembly = Assembly.GetAssembly(typeFromHandle);
		if ((object)assembly != null)
		{
			AppDomain appDomain = AppDomain.CreateDomain(appDomainName);
			if (appDomain != null)
			{
				string location = assembly.Location;
				if (location != null)
				{
					TProxy val = appDomain.CreateInstanceFromAndUnwrap(location, typeFromHandle.FullName) as TProxy;
					if (val != null)
					{
						result = val.Execute(argument);
					}
				}
				AppDomain.Unload(appDomain);
			}
		}
		return result;
	}
}
