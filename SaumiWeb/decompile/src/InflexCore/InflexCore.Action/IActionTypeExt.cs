namespace InflexCore.Action;

public interface IActionTypeExt
{
	InflexActionGroup Group { get; }

	int Index { get; }

	string LargeGlyphUri { get; }

	string GlyphUri { get; }

	ActionPosition Position { get; }
}
