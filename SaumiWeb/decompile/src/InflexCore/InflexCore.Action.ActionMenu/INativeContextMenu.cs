namespace InflexCore.Action.ActionMenu;

public interface INativeContextMenu : INativeMenuItemCollection
{
	INativeContextMenuItem CreateSeparateItem();

	INativeContextMenuItem CreateParentItem();

	INativeContextMenuItem CreateItem();
}
