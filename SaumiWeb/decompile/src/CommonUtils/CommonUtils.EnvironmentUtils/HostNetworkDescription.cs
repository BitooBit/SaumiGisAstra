using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using CommonUtils.ReportUtils;

namespace CommonUtils.EnvironmentUtils;

public class HostNetworkDescription
{
	private static class NetApiNativeUtils
	{
		private enum NET_API_STATUS : uint
		{
			NERR_Success = 0u,
			NERR_InvalidComputer = 2351u,
			NERR_NotPrimary = 2226u,
			NERR_SpeGroupOp = 2234u,
			NERR_LastAdmin = 2452u,
			NERR_BadPassword = 2203u,
			NERR_PasswordTooShort = 2245u,
			NERR_UserNotFound = 2221u,
			ERROR_ACCESS_DENIED = 5u,
			ERROR_NOT_ENOUGH_MEMORY = 8u,
			ERROR_INVALID_PARAMETER = 87u,
			ERROR_INVALID_NAME = 123u,
			ERROR_INVALID_LEVEL = 124u,
			ERROR_MORE_DATA = 234u,
			ERROR_SESSION_CREDENTIAL_CONFLICT = 1219u
		}

		private enum NetJoinStatus
		{
			NetSetupUnknownStatus,
			NetSetupUnjoined,
			NetSetupWorkgroupName,
			NetSetupDomainName
		}

		private const int ErrorSuccess = 0;

		[DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern int NetGetJoinInformation(string server, out IntPtr domain, out NetJoinStatus status);

		[DllImport("Netapi32.dll")]
		private static extern int NetApiBufferFree(IntPtr buffer);

		public static string GetJoinedDomain(string host)
		{
			string text = null;
			IntPtr domain = IntPtr.Zero;
			try
			{
				if (NetGetJoinInformation(host, out domain, out var status) == 0 && status == NetJoinStatus.NetSetupDomainName)
				{
					text = Marshal.PtrToStringAuto(domain);
				}
			}
			finally
			{
				if (domain != IntPtr.Zero)
				{
					NetApiBufferFree(domain);
				}
			}
			return text ?? string.Empty;
		}
	}

	public const char DOMAIN_NAME_DELIMITER_CHAR = '.';

	private string _HostName;

	private string _DomainName;

	private IPAddress[] _HostAddresses;

	private IEnumerable<PhysicalAddress> _HostMacAddresses;

	private IDictionary<PhysicalAddress, IList<IPAddress>> _HostInterfaces;

	public static HostNetworkDescription Current => new HostNetworkDescription();

	public string HostName => _HostName ?? (_HostName = Dns.GetHostName());

	public string DomainName => _DomainName ?? (_DomainName = IPGlobalProperties.GetIPGlobalProperties().DomainName);

	public bool IsDomainMember => !string.IsNullOrEmpty(DomainName);

	public string FQDN
	{
		get
		{
			string hostName = HostName;
			string domainName = DomainName;
			if (hostName.Contains(domainName))
			{
				return hostName;
			}
			return hostName + "." + domainName;
		}
	}

	public IPAddress[] HostAddresses => _HostAddresses ?? (_HostAddresses = GetHostAddresses(HostName));

	public IEnumerable<PhysicalAddress> HostMacAddresses
	{
		get
		{
			if (_HostMacAddresses == null)
			{
				IList<PhysicalAddress> list = new List<PhysicalAddress>();
				NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				NetworkInterface[] array = allNetworkInterfaces;
				foreach (NetworkInterface networkInterface in array)
				{
					if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback && networkInterface.OperationalStatus == OperationalStatus.Up)
					{
						PhysicalAddress physicalAddress = networkInterface.GetPhysicalAddress();
						list.Add(physicalAddress);
					}
				}
				_HostMacAddresses = list;
			}
			return _HostMacAddresses;
		}
	}

	public IDictionary<PhysicalAddress, IList<IPAddress>> HostInterfaces
	{
		get
		{
			if (_HostInterfaces == null)
			{
				IDictionary<PhysicalAddress, IList<IPAddress>> dictionary = new Dictionary<PhysicalAddress, IList<IPAddress>>();
				NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				NetworkInterface[] array = allNetworkInterfaces;
				foreach (NetworkInterface networkInterface in array)
				{
					if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback || networkInterface.OperationalStatus != OperationalStatus.Up)
					{
						continue;
					}
					PhysicalAddress physicalAddress = networkInterface.GetPhysicalAddress();
					IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
					dictionary[physicalAddress] = new List<IPAddress>();
					foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
					{
						dictionary[physicalAddress].Add(unicastAddress.Address);
					}
				}
				_HostInterfaces = dictionary;
			}
			return _HostInterfaces;
		}
	}

	public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
	{
		try
		{
			UtilsLoggerWrapper.Info("Try get IP-addresses for host " + hostNameOrAddress + " through its IPHostEntry");
			IPHostEntry hostEntry = Dns.GetHostEntry(hostNameOrAddress);
			string hostName = hostEntry.HostName;
			IPHostEntry hostEntry2 = Dns.GetHostEntry(hostName);
			IPAddress[] addressList = hostEntry2.AddressList;
			UtilsLoggerWrapper.Info("IP-addresses were successfully obtained");
			return addressList;
		}
		catch (SocketException ex)
		{
			UtilsLoggerWrapper.Warning("Can't get IPHostEntry for specified host. SocketException occurred: " + ex);
			UtilsLoggerWrapper.Info("Retrieve IP-addresses using Dns.GetHostAddresses() method");
			IPAddress[] hostAddresses = Dns.GetHostAddresses(hostNameOrAddress);
			UtilsLoggerWrapper.Info("IP-addresses were obtained with warnings");
			return hostAddresses;
		}
	}

	protected HostNetworkDescription()
	{
	}

	public override string ToString()
	{
		try
		{
			string text = HostInterfaces.Aggregate(string.Empty, (string result, KeyValuePair<PhysicalAddress, IList<IPAddress>> networkInterface) => string.Concat(new object[6]
			{
				result,
				"MAC: ",
				networkInterface.Key,
				" {IP: ",
				networkInterface.Value.Aggregate(string.Empty, (string addressesResult, IPAddress address) => string.Concat(addressesResult, address, "; ")).TrimEnd(new char[2] { ';', ' ' }),
				"}; "
			})).TrimEnd(new char[2] { ';', ' ' });
			return "Host " + FQDN + " [" + text + "] " + (IsDomainMember ? "IS " : "is NOT ") + "domain member.";
		}
		catch (Exception ex)
		{
			UtilsLoggerWrapper.Error("Can't get computer network information. Exception occurred: " + ex);
			return "UNDEFINED";
		}
	}

	public static string GetDomainName(string hostNameOrAddress)
	{
		return NetApiNativeUtils.GetJoinedDomain(hostNameOrAddress);
	}
}
