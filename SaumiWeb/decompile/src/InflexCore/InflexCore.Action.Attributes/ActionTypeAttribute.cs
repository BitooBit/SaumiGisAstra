using System;

namespace InflexCore.Action.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class ActionTypeAttribute : Attribute
{
	public static int DefaultIndex = -1;

	public string GroupName { get; set; }

	public int Index { get; set; }

	public int GroupOrder { get; set; }

	public string LargeGlyphUri { get; set; }

	public string GlyphUri { get; set; }

	public string GroupGlyphUri { get; set; }

	public ActionPosition Position { get; set; }

	public ActionTypeAttribute()
	{
		Index = DefaultIndex;
		GroupOrder = int.MaxValue;
		Position = ActionPosition.Up;
	}
}
