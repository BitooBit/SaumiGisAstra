using System;
using System.Reflection;
using System.Xml.Serialization;
using InflexCore.Native.Messages;

namespace InflexCore.Native;

public static class InflexCoreVersion
{
	public static string VersionString
	{
		get
		{
			AssemblyFileVersionAttribute assemblyFileVersionAttribute = (AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(typeof(InflexCoreVersion).Assembly, typeof(AssemblyFileVersionAttribute));
			return assemblyFileVersionAttribute.Version;
		}
	}

	static InflexCoreVersion()
	{
		PrepareSerializers();
	}

	private static void PrepareSerializers()
	{
		Assembly assembly = Assembly.GetAssembly(typeof(MessageArgument));
		if ((object)assembly == null)
		{
			return;
		}
		Type[] types = assembly.GetTypes();
		Type[] array = types;
		foreach (Type type in array)
		{
			if ((object)type.BaseType == typeof(MessageArgument) && !type.IsAbstract)
			{
				XmlSerializer self = new XmlSerializer(type);
				object obj = Activator.CreateInstance(type);
				self.SerializeToString(obj);
			}
		}
	}
}
