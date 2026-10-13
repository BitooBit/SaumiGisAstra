namespace InflexCore.Action.ActionMenu;

public interface INativeContextMenuItem : INativeMenuItemCollection, ICheckableItem
{
	bool IsDefault { get; set; }

	string Name { get; set; }

	string Caption { get; set; }

	InflexAction Command { get; set; }

	void SetGlyph(string imageSource, int width, int height);
}
