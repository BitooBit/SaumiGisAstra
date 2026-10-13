using CommonUtils.AppDomainUtils;
using CommonUtils.ReportUtils;

namespace OracleUtils.Connection;

public class AppDomainOciConnectionChecker : AppDomainExecutorBase<string[], ConnectionResult, AppDomainOciConnectionChecker.AppDomainOciConnectionCheckerProxy>
{
	public class AppDomainOciConnectionCheckerProxy : AppDomainExecutorProxyBase
	{
		public override ConnectionResult Execute(string[] connectionCheckerParameters)
		{
			if (connectionCheckerParameters != null && connectionCheckerParameters.Length == 3)
			{
				string connectionString = connectionCheckerParameters[0];
				string ociLibraryPath = connectionCheckerParameters[1];
				string text = connectionCheckerParameters[2];
				if (!string.IsNullOrEmpty(text))
				{
					FileLogger.Initialize(text, 10485760L);
					UtilsLoggerWrapper.SetLoggerProvider(FileLogger.Info, FileLogger.Warning, FileLogger.Error);
				}
				return ConnectionUtils.CheckOciConnection(connectionString, ociLibraryPath);
			}
			return ConnectionResult.Failed;
		}
	}
}
