using System;

namespace InflexCore.Components;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class SingleComponentAttribute : Attribute
{
	public Type SingleComponent { get; set; }

	public SingleComponentAttribute(Type singleComponent)
	{
		SingleComponent = singleComponent;
	}
}
