using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CommonUtils.EnvironmentUtils;
using OracleUtils.Scanner;

namespace OracleUtils.Configuration;

public class TnsNamesConfiguration
{
	public string DatasourceName { get; private set; }

	public string DatabaseHost { get; private set; }

	public IPAddress DatabaseHostIpAddress
	{
		get
		{
			string databaseHost = DatabaseHost;
			IPAddress[] hostAddresses = HostNetworkDescription.GetHostAddresses(databaseHost);
			if (hostAddresses != null)
			{
				return hostAddresses.FirstOrDefault((IPAddress address) => address.AddressFamily == AddressFamily.InterNetwork);
			}
			return IPAddress.None;
		}
	}

	public bool IsDatabaseLocatedAtCurrentHost
	{
		get
		{
			IPAddress[] hostAddresses = HostNetworkDescription.GetHostAddresses(Dns.GetHostName());
			IPAddress[] hostAddresses2 = HostNetworkDescription.GetHostAddresses(DatabaseHost);
			IEnumerable<IPAddress> source = hostAddresses.Intersect(hostAddresses2);
			return source.Any();
		}
	}

	public string DatabasePort { get; private set; }

	public int DatabasePortNumber
	{
		get
		{
			string databasePort = DatabasePort;
			if (int.TryParse(databasePort, out var result))
			{
				return result;
			}
			return -1;
		}
	}

	public string DatabaseServiceName { get; private set; }

	public bool IsCommented { get; private set; }

	public bool IsDuplicate { get; internal set; }

	public bool IsActive
	{
		get
		{
			if (!IsDuplicate)
			{
				return !IsCommented;
			}
			return false;
		}
	}

	public bool TryGetRelatedDatabaseInstallation(out OracleClientInstallation databaseInstallation)
	{
		if (IsDatabaseLocatedAtCurrentHost)
		{
			IEnumerable<OracleClientInstallation> allOracleClientInstallations = OracleScanner.GetAllOracleClientInstallations();
			foreach (OracleClientInstallation item in allOracleClientInstallations)
			{
				if (!item.IsDatabase)
				{
					continue;
				}
				ListenerOraFile listenerOraFile = item.ListenerOraFile;
				if (listenerOraFile.Exists)
				{
					IPAddress[] hostAddresses = HostNetworkDescription.GetHostAddresses(listenerOraFile.Host);
					IPAddress[] hostAddresses2 = HostNetworkDescription.GetHostAddresses(DatabaseHost);
					IEnumerable<IPAddress> source = hostAddresses.Intersect(hostAddresses2);
					if (source.Any() && DatabasePortNumber == listenerOraFile.PortNumber)
					{
						databaseInstallation = item;
						return true;
					}
				}
			}
		}
		databaseInstallation = null;
		return false;
	}

	public TnsNamesConfiguration(string datasourceName, string databaseHost, string databasePort, string databaseServiceName, bool isCommented, bool isDuplicate)
	{
		DatasourceName = datasourceName;
		DatabaseHost = databaseHost;
		DatabasePort = databasePort;
		DatabaseServiceName = databaseServiceName;
		IsCommented = isCommented;
		IsDuplicate = isDuplicate;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"{DatasourceName} =");
		stringBuilder.AppendLine("  (DESCRIPTION =");
		stringBuilder.AppendLine("    (ADDRESS_LIST =");
		stringBuilder.AppendLine($"      (ADDRESS = (PROTOCOL = TCP)(HOST = {DatabaseHost})(PORT = {DatabasePort}))");
		stringBuilder.AppendLine("    )");
		stringBuilder.AppendLine("    (CONNECT_DATA =");
		stringBuilder.AppendLine($"     (SERVICE_NAME = {DatabaseServiceName})");
		stringBuilder.AppendLine("    )");
		stringBuilder.AppendLine("  )");
		return stringBuilder.ToString();
	}
}
