using System;

namespace InflexCore.MetaData;

public class NotInflexObjectPropertyException : Exception
{
	public IInflexProperty Property { get; set; }

	public NotInflexObjectPropertyException(IInflexProperty property)
	{
		Property = property;
	}
}
