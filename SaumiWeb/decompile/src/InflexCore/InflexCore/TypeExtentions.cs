using System;

namespace InflexCore;

internal static class TypeExtentions
{
	public static object Default(this Type self)
	{
		if (!self.IsValueType)
		{
			return null;
		}
		return Activator.CreateInstance(self);
	}
}
