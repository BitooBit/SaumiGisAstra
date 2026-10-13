using CommonUtils.AppDomainUtils;
using CommonUtils.ReportUtils;

namespace OracleUtils.Connection;

public class AppDomainConnectionChecker : AppDomainExecutorBase<string[], ConnectionResult, AppDomainConnectionChecker.AppDomainConnectionCheckerProxy>
{
	public class AppDomainConnectionCheckerProxy : AppDomainExecutorProxyBase
	{
		public override ConnectionResult Execute(string[] connectionCheckerParameters)
		{
			if (connectionCheckerParameters != null && connectionCheckerParameters.Length == 3)
			{
				string connectionString = connectionCheckerParameters[0];
				string odpNetAssemblyPath = connectionCheckerParameters[1];
				string text = connectionCheckerParameters[2];
				if (!string.IsNullOrEmpty(text))
				{
					FileLogger.Initialize(text, 10485760L);
					UtilsLoggerWrapper.SetLoggerProvider(FileLogger.Info, FileLogger.Warning, FileLogger.Error);
				}
				return ConnectionUtils.CheckConnection(connectionString, odpNetAssemblyPath);
			}
			return ConnectionResult.Failed;
		}
	}
}
