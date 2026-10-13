using System;
using System.Reflection;
using System.Text;

namespace CommonUtils.AppDomainUtils;

[Serializable]
public class AssemblyNameDescription
{
	public AssemblyName AssemblyName { get; private set; }

	public string Location { get; private set; }

	public bool GlobalAssemblyCache { get; private set; }

	public AssemblyName[] ReferencedAssemblies { get; private set; }

	public ImageFileMachine Machine { get; private set; }

	public PortableExecutableKinds PeKind { get; private set; }

	public AssemblyNameDescription(Assembly assembly)
	{
		AssemblyName = assembly.GetName();
		Location = assembly.Location;
		GlobalAssemblyCache = assembly.GlobalAssemblyCache;
		ReferencedAssemblies = assembly.GetReferencedAssemblies();
		assembly.ManifestModule.GetPEKind(out var peKind, out var machine);
		PeKind = peKind;
		Machine = machine;
	}

	public string ToString(bool showReferencedAssemblies)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Concat("AssemblyName: ", AssemblyName, ";"));
		stringBuilder.AppendLine("Location: " + Location + ";");
		stringBuilder.AppendLine("GlobalAssemblyCache: " + GlobalAssemblyCache + ";");
		stringBuilder.AppendLine(string.Concat("Machine: ", Machine, ";"));
		stringBuilder.AppendLine(string.Concat("PeKind: ", PeKind, ";"));
		if (showReferencedAssemblies)
		{
			if (ReferencedAssemblies == null || ReferencedAssemblies.Length == 0)
			{
				stringBuilder.AppendLine("Referenced assemblies were NOT specified;");
			}
			else
			{
				stringBuilder.AppendLine("Referenced assemblies: {");
				for (int i = 0; i < ReferencedAssemblies.Length; i++)
				{
					stringBuilder.AppendLine(i + 1 + ": " + ReferencedAssemblies[i]);
				}
				stringBuilder.AppendLine("}");
			}
		}
		else
		{
			stringBuilder.AppendLine("Referenced assemblies count: " + ReferencedAssemblies.Length + ";");
		}
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		return ToString(showReferencedAssemblies: false);
	}
}
