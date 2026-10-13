using System;
using System.Text;
using Log4NetWrapper.Information.OS;

namespace Log4NetWrapper.Information;

public static class EnvironmentInfo
{
	public static string Description
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				string osVersionString = OSInfo.OsVersionString;
				stringBuilder.AppendLine("OS: " + ((!string.IsNullOrEmpty(osVersionString)) ? osVersionString : Environment.OSVersion.VersionString));
				stringBuilder.AppendLine("CLR: " + Environment.Version);
				stringBuilder.AppendLine("ProcessorsCount: " + Environment.ProcessorCount);
			}
			catch (Exception)
			{
				stringBuilder.AppendLine("Can't Environment Info");
			}
			return stringBuilder.ToString();
		}
	}
}
