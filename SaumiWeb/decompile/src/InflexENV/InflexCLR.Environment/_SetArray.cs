using System.Reflection;

namespace InflexCLR.Environment;

public static class _SetArray
{
	public static void SetValue(object obj, string PropertyName, object[] val)
	{
		obj?.GetType().GetField("_" + PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(obj, val);
	}
}
