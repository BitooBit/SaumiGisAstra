using System;
using System.Reflection;
using System.Text;

namespace Log4NetWrapper.Information;

public static class AssemblyVersionInfo
{
	public static string CallingAssemblyInfo
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				Assembly callingAssembly = Assembly.GetCallingAssembly();
				stringBuilder.AppendLine("AssemblyInfo:");
				stringBuilder.AppendLine("Assembly:" + callingAssembly.FullName);
				AssemblyTitleAttribute assemblyAttribute = GetAssemblyAttribute<AssemblyTitleAttribute>(callingAssembly);
				if (assemblyAttribute != null)
				{
					stringBuilder.AppendLine("AssemblyTitle:" + assemblyAttribute.Title);
				}
				AssemblyVersionAttribute assemblyAttribute2 = GetAssemblyAttribute<AssemblyVersionAttribute>(callingAssembly);
				if (assemblyAttribute2 != null)
				{
					stringBuilder.AppendLine("AssemblyVersion:" + assemblyAttribute2.Version);
				}
				AssemblyFileVersionAttribute assemblyAttribute3 = GetAssemblyAttribute<AssemblyFileVersionAttribute>(callingAssembly);
				if (assemblyAttribute3 != null)
				{
					stringBuilder.AppendLine("AssemblyFileVersion:" + assemblyAttribute3.Version);
				}
			}
			catch (Exception)
			{
				stringBuilder.AppendLine("Can't get calling assembly info");
			}
			return stringBuilder.ToString();
		}
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
