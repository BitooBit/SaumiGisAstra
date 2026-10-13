using System;
using System.EnterpriseServices.Internal;
using System.Reflection;

namespace CommonUtils.EnvironmentUtils;

public static class PublishGacExtension
{
	public static void GacRemoveUnambiguously(this Publish publish, string gacAssemblyFilePath)
	{
		GacAssemblyDescription gacAssemblyDescription = new GacAssemblyDescription(gacAssemblyFilePath);
		publish.GacRemoveByDescription(gacAssemblyDescription);
	}

	public static void GacRemoveByDescription(this Publish publish, GacAssemblyDescription gacAssemblyDescription)
	{
		publish.GacRemoveByFullName(gacAssemblyDescription.FullName);
	}

	public static void GacRemoveByFullName(this Publish publish, string assemblyFullName)
	{
		Type typeFromHandle = typeof(Publish);
		MethodInfo method = typeof(Publish).GetMethod("CreateAssemblyCache", BindingFlags.Static | BindingFlags.NonPublic);
		object[] array = new object[2] { null, 0u };
		method.Invoke(publish, array);
		object obj = array[0];
		Type type = typeFromHandle.Assembly.GetType("System.EnterpriseServices.Internal.IAssemblyCache");
		MethodInfo method2 = type.GetMethod("UninstallAssembly", BindingFlags.Instance | BindingFlags.Public);
		object[] array2 = new object[4]
		{
			0u,
			assemblyFullName,
			(IntPtr)0,
			null
		};
		method2.Invoke(obj, array2);
		_ = array2[3];
	}
}
