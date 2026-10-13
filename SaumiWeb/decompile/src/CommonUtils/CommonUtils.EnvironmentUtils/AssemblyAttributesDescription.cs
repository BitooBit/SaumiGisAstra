using System;
using System.Reflection;
using System.Text;
using CommonUtils.ReportUtils;

namespace CommonUtils.EnvironmentUtils;

public class AssemblyAttributesDescription
{
	public Assembly Assembly { get; private set; }

	public string FullName => Assembly.FullName;

	public AssemblyTitleAttribute Title => GetAssemblyAttribute<AssemblyTitleAttribute>(Assembly);

	public AssemblyVersionAttribute Version => GetAssemblyAttribute<AssemblyVersionAttribute>(Assembly);

	public AssemblyFileVersionAttribute FileVersion => GetAssemblyAttribute<AssemblyFileVersionAttribute>(Assembly);

	public AssemblyAttributesDescription()
		: this(Assembly.GetCallingAssembly())
	{
	}

	public AssemblyAttributesDescription(Assembly assembly)
	{
		Assembly = assembly;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			stringBuilder.AppendLine("Assembly " + FullName + " attributes description:{");
			AssemblyTitleAttribute title = Title;
			if (title != null)
			{
				stringBuilder.AppendLine("AssemblyTitle: " + title);
			}
			AssemblyVersionAttribute version = Version;
			if (version != null)
			{
				stringBuilder.AppendLine("AssemblyVersion: " + version);
			}
			AssemblyFileVersionAttribute fileVersion = FileVersion;
			if (fileVersion != null)
			{
				stringBuilder.AppendLine("AssemblyFileVersion: " + fileVersion.Version);
			}
			stringBuilder.AppendLine("}");
		}
		catch (Exception ex)
		{
			UtilsLoggerWrapper.Error("Can't get assembly info! Exception occurred: " + ex);
		}
		return stringBuilder.ToString();
	}

	private static T GetAssemblyAttribute<T>(ICustomAttributeProvider assembly) where T : Attribute
	{
		object[] customAttributes = assembly.GetCustomAttributes(typeof(T), inherit: true);
		if (customAttributes != null && customAttributes.Length > 0)
		{
			return (T)customAttributes[0];
		}
		return null;
	}
}
