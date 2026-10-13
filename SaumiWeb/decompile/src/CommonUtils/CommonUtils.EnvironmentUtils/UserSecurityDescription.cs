using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using CommonUtils.ReportUtils;

namespace CommonUtils.EnvironmentUtils;

public class UserSecurityDescription
{
	public WindowsIdentity User { get; private set; }

	public string UserName
	{
		get
		{
			WindowsIdentity user = User;
			if (user == null)
			{
				return "UNDEFINED";
			}
			return user.Name;
		}
	}

	public bool IsDomainMember
	{
		get
		{
			string domainName = HostNetworkDescription.Current.DomainName;
			if (string.IsNullOrEmpty(domainName))
			{
				return false;
			}
			int num = domainName.IndexOf('.');
			string text = ((num >= 0) ? domainName.Substring(0, num) : domainName);
			return UserName.StartsWith(text + "\\", StringComparison.OrdinalIgnoreCase);
		}
	}

	public bool HasAdministratorPrivileges
	{
		get
		{
			WindowsIdentity user = User;
			if (user != null)
			{
				WindowsPrincipal windowsPrincipal = new WindowsPrincipal(user);
				return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
			}
			return false;
		}
	}

	public IdentityReferenceCollection UserGroups => User?.Groups;

	public UserSecurityDescription()
		: this(WindowsIdentity.GetCurrent())
	{
	}

	public UserSecurityDescription(WindowsIdentity userIdentity)
	{
		User = userIdentity;
	}

	public string GetFileEffectiveRightsDescription(string fileFullPath)
	{
		Dictionary<string, int> effectiveRightsOnFile = GetEffectiveRightsOnFile(fileFullPath);
		if (effectiveRightsOnFile == null || effectiveRightsOnFile.Count <= 0)
		{
			return "Can't detect rights";
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, int> item in effectiveRightsOnFile)
		{
			stringBuilder.AppendLine(item.Key);
		}
		return stringBuilder.ToString();
	}

	public Dictionary<string, int> GetEffectiveRightsOnFile(string fileFullPath)
	{
		FileSystemRights? rightsOnFile = GetRightsOnFile(fileFullPath);
		if (!rightsOnFile.HasValue)
		{
			return null;
		}
		return ExtractRights(rightsOnFile.Value);
	}

	public string GetDirectoryEffectiveRightsDescription(string directoryFullPath)
	{
		Dictionary<string, int> effectiveRightsOnDirectory = GetEffectiveRightsOnDirectory(directoryFullPath);
		if (effectiveRightsOnDirectory == null || effectiveRightsOnDirectory.Count <= 0)
		{
			return "Can't detect rights";
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, int> item in effectiveRightsOnDirectory)
		{
			stringBuilder.AppendLine(item.Key);
		}
		return stringBuilder.ToString();
	}

	public Dictionary<string, int> GetEffectiveRightsOnDirectory(string directoryFullPath)
	{
		FileSystemRights? rightsOnDirectory = GetRightsOnDirectory(directoryFullPath);
		if (!rightsOnDirectory.HasValue)
		{
			return null;
		}
		return ExtractRights(rightsOnDirectory.Value);
	}

	public bool CheckAccessForDirectory(string directoryFullPath, FileSystemRights expectedRights)
	{
		return (GetRightsOnDirectory(directoryFullPath) & expectedRights) == expectedRights;
	}

	public bool CheckAccessForFile(string fileFullPath, FileSystemRights expectedRights)
	{
		return (GetRightsOnFile(fileFullPath) & expectedRights) == expectedRights;
	}

	public string ToString(bool fullDescription)
	{
		if (!fullDescription)
		{
			return (IsDomainMember ? "Domain" : "Local") + " user " + UserName + (HasAdministratorPrivileges ? " HAS" : " has NOT") + " administrator privileges";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("User " + UserName + " description: {");
		stringBuilder.AppendLine("User " + (IsDomainMember ? "IS" : "is NOT") + " domain member;");
		stringBuilder.AppendLine("User " + (HasAdministratorPrivileges ? "HAS" : "has NOT") + " administrator privileges;");
		IdentityReferenceCollection userGroups = UserGroups;
		if (userGroups != null)
		{
			int count = userGroups.Count;
			if (count > 0)
			{
				stringBuilder.AppendLine("User is a member of " + count + " groups: {");
				foreach (IdentityReference item in userGroups)
				{
					string text = item.Value;
					try
					{
						if (item is SecurityIdentifier)
						{
							NTAccount nTAccount = item.Translate(typeof(NTAccount)) as NTAccount;
							if (nTAccount != null)
							{
								text = text + " = " + nTAccount;
							}
						}
						else if (item is NTAccount)
						{
							text = item.ToString();
						}
					}
					catch (Exception ex)
					{
						UtilsLoggerWrapper.Error("Can't get extended info for group: " + text + ". Exception occurred: " + ex);
					}
					stringBuilder.AppendLine(text);
				}
				stringBuilder.AppendLine("}");
			}
		}
		else
		{
			stringBuilder.AppendLine("User is a not member of any groups;");
		}
		stringBuilder.AppendLine("}");
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		return ToString(fullDescription: true);
	}

	private static Dictionary<string, int> ExtractRights(FileSystemRights rights)
	{
		return (from name in Enum.GetNames(typeof(FileSystemRights))
			let val = (int)Enum.Parse(typeof(FileSystemRights), name)
			let enumVal = (FileSystemRights)val
			where (enumVal & rights) == enumVal
			select new { val, name }).ToDictionary(i => i.name, i => i.val);
	}

	private FileSystemRights GetRights(AuthorizationRuleCollection fileSystemObjectRules)
	{
		WindowsIdentity user = User;
		IEnumerable<AuthorizationRule> enumerable = from AuthorizationRule rule in fileSystemObjectRules
			where user.User.Equals(rule.IdentityReference) || user.Groups.Contains(rule.IdentityReference)
			select rule;
		FileSystemRights fileSystemRights = (FileSystemRights)0;
		FileSystemRights fileSystemRights2 = (FileSystemRights)0;
		foreach (FileSystemAccessRule item in enumerable)
		{
			if (item.AccessControlType.Equals(AccessControlType.Deny))
			{
				fileSystemRights |= item.FileSystemRights;
			}
			else if (item.AccessControlType.Equals(AccessControlType.Allow))
			{
				fileSystemRights2 |= item.FileSystemRights;
			}
		}
		return fileSystemRights2 & ~fileSystemRights;
	}

	private FileSystemRights? GetRightsOnFile(string fileFullPath)
	{
		try
		{
			if (!File.Exists(fileFullPath))
			{
				return null;
			}
			AuthorizationRuleCollection accessRules = File.GetAccessControl(fileFullPath).GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier));
			return GetRights(accessRules);
		}
		catch (Exception)
		{
			return null;
		}
	}

	private FileSystemRights? GetRightsOnDirectory(string directoryFullPath)
	{
		try
		{
			if (!Directory.Exists(directoryFullPath))
			{
				return null;
			}
			AuthorizationRuleCollection accessRules = Directory.GetAccessControl(directoryFullPath).GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier));
			return GetRights(accessRules);
		}
		catch (Exception)
		{
			return null;
		}
	}
}
