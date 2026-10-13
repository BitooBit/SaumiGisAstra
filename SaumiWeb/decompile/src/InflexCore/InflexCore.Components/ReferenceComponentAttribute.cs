using System;

namespace InflexCore.Components;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class ReferenceComponentAttribute : Attribute
{
	public Type ReferenceComponent { get; set; }

	public ReferenceComponentAttribute(Type referenceComponent)
	{
		ReferenceComponent = referenceComponent;
	}
}
