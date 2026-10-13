namespace InflexCore.Action.ActionMenu;

public class ContextMenuBuilder
{
	public void BuildMenu(INativeContextMenu contextMenu, ActionCollection actions)
	{
		contextMenu.ClearItems();
		InflexActionMenu inflexActionMenu = new InflexActionMenu(actions);
		foreach (InflexActionMenuItem item in inflexActionMenu.Items)
		{
			INativeContextMenuItem nativeContextMenuItem = BuildItem(contextMenu, item);
			if (actions.DefaultAction != null && item.Action == actions.DefaultAction)
			{
				nativeContextMenuItem.IsDefault = true;
			}
			contextMenu.AddItem(nativeContextMenuItem);
		}
	}

	private INativeContextMenuItem BuildItem(INativeContextMenu contextMenu, InflexActionMenuItem menuItem)
	{
		if (menuItem.IsSplitItem)
		{
			return contextMenu.CreateSeparateItem();
		}
		if (menuItem.SubItems != null && menuItem.SubItems.Count > 0)
		{
			INativeContextMenuItem nativeContextMenuItem = contextMenu.CreateParentItem();
			nativeContextMenuItem.Name = menuItem.Name;
			nativeContextMenuItem.Caption = menuItem.Caption;
			nativeContextMenuItem.SetGlyph(menuItem.GlyphSource, 16, 16);
			{
				foreach (InflexActionMenuItem subItem in menuItem.SubItems)
				{
					INativeContextMenuItem nativeContextMenuItem2 = BuildItem(contextMenu, subItem);
					if (nativeContextMenuItem2 != null)
					{
						nativeContextMenuItem.AddItem(nativeContextMenuItem2);
					}
				}
				return nativeContextMenuItem;
			}
		}
		INativeContextMenuItem nativeContextMenuItem3 = contextMenu.CreateItem();
		nativeContextMenuItem3.Name = menuItem.Name;
		nativeContextMenuItem3.Caption = menuItem.Caption;
		nativeContextMenuItem3.Command = menuItem.Action;
		nativeContextMenuItem3.SetGlyph(menuItem.GlyphSource, 16, 16);
		if (menuItem.IsCheckable)
		{
			nativeContextMenuItem3.IsCheckable = true;
			nativeContextMenuItem3.IsChecked = menuItem.IsChecked;
			nativeContextMenuItem3.CheckedChangedHandler = menuItem.CheckedChangedHandler;
		}
		return nativeContextMenuItem3;
	}
}
