using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Log4NetWrapper.Information;

public static class NetworkInfo
{
	public static string ComputerFQDN
	{
		get
		{
			string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
			string hostName = Dns.GetHostName();
			string text = "";
			if (!hostName.Contains(domainName))
			{
				return hostName + "." + domainName;
			}
			return hostName;
		}
	}

	public static bool IsDomainMemberComputer => !string.IsNullOrEmpty(IPGlobalProperties.GetIPGlobalProperties().DomainName);

	public static string DomainMembership
	{
		get
		{
			try
			{
				return "Computer is " + (string.IsNullOrEmpty(IPGlobalProperties.GetIPGlobalProperties().DomainName) ? "NOT " : "") + "domain member";
			}
			catch (Exception)
			{
				return "Can't detect computer domain membership";
			}
		}
	}
}
