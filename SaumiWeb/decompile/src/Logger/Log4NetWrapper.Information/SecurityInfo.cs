using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace Log4NetWrapper.Information;

public static class SecurityInfo
{
	public static string UserSecuritySettingsDescription
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				WindowsIdentity current = WindowsIdentity.GetCurrent();
				if (current != null)
				{
					stringBuilder.AppendLine($"User identity = {current.Name}");
					WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
					stringBuilder.AppendLine(string.Format("Current identity {0} administrator privileges", windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator) ? "HAS" : "has NOT"));
					IdentityReferenceCollection groups = current.Groups;
					if (groups != null && groups.Count > 0)
					{
						stringBuilder.AppendLine("User is member of groups: ");
						foreach (IdentityReference item in groups)
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
							catch (Exception)
							{
								stringBuilder.AppendLine("Can't get extended info for group: " + text);
							}
							stringBuilder.AppendLine(text);
						}
					}
					else
					{
						stringBuilder.AppendLine("Can't detect user groups");
					}
				}
			}
			catch (Exception)
			{
				stringBuilder.AppendLine("Can't detect user security info");
			}
			return stringBuilder.ToString();
		}
	}

	private static Dictionary<string, int> ExtractRights(FileSystemRights rights)
	{
		return (from name in Enum.GetNames(typeof(FileSystemRights))
			let val = (int)Enum.Parse(typeof(FileSystemRights), name)
			let enumVal = (FileSystemRights)val
			where (enumVal & rights) == enumVal
			select new { val, name }).ToDictionary(i => i.name, i => i.val);
	}

	private static FileSystemRights GetRights(WindowsIdentity user, AuthorizationRuleCollection fileSystemObjectRules)
	{
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

	private static FileSystemRights? GetRightsOnFile(WindowsIdentity user, string fileFullPath)
	{
		try
		{
			if (!File.Exists(fileFullPath))
			{
				return null;
			}
			AuthorizationRuleCollection accessRules = File.GetAccessControl(fileFullPath).GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier));
			return GetRights(user, accessRules);
		}
		catch (Exception)
		{
			return null;
		}
	}

	private static FileSystemRights? GetRightsOnDirectory(WindowsIdentity user, string directoryFullPath)
	{
		try
		{
			if (!Directory.Exists(directoryFullPath))
			{
				return null;
			}
			AuthorizationRuleCollection accessRules = Directory.GetAccessControl(directoryFullPath).GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier));
			return GetRights(user, accessRules);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static string GetFileEffectiveRightsDescription(string fileFullPath)
	{
		WindowsIdentity windowsIdentity = null;
		try
		{
			windowsIdentity = WindowsIdentity.GetCurrent();
		}
		catch (Exception)
		{
		}
		if (windowsIdentity == null)
		{
			return "Can't detect rights";
		}
		Dictionary<string, int> effectiveRightsOnFile = GetEffectiveRightsOnFile(windowsIdentity, fileFullPath);
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

	public static Dictionary<string, int> GetEffectiveRightsOnFile(WindowsIdentity user, string fileFullPath)
	{
		FileSystemRights? rightsOnFile = GetRightsOnFile(user, fileFullPath);
		if (!rightsOnFile.HasValue)
		{
			return null;
		}
		return ExtractRights(rightsOnFile.Value);
	}

	public static string GetDirectoryEffectiveRightsDescription(string directoryFullPath)
	{
		WindowsIdentity windowsIdentity = null;
		try
		{
			windowsIdentity = WindowsIdentity.GetCurrent();
		}
		catch (Exception)
		{
		}
		if (windowsIdentity == null)
		{
			return "Can't detect rights";
		}
		Dictionary<string, int> effectiveRightsOnDirectory = GetEffectiveRightsOnDirectory(windowsIdentity, directoryFullPath);
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

	public static Dictionary<string, int> GetEffectiveRightsOnDirectory(WindowsIdentity user, string directoryFullPath)
	{
		FileSystemRights? rightsOnDirectory = GetRightsOnDirectory(user, directoryFullPath);
		if (!rightsOnDirectory.HasValue)
		{
			return null;
		}
		return ExtractRights(rightsOnDirectory.Value);
	}

	public static bool CheckAccessForDirectory(WindowsIdentity user, string directoryFullPath, FileSystemRights expectedRights)
	{
		return (GetRightsOnDirectory(user, directoryFullPath) & expectedRights) == expectedRights;
	}

	public static bool CheckAccessForFile(WindowsIdentity user, string fileFullPath, FileSystemRights expectedRights)
	{
		return (GetRightsOnFile(user, fileFullPath) & expectedRights) == expectedRights;
	}
}
