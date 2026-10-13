namespace InflexCore.Action.ActionMenu;

public interface INativeMenuItemCollection
{
	void ClearItems();

	void AddItem(INativeContextMenuItem item);
}
