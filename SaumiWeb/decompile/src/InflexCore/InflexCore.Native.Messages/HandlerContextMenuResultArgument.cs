using System.Collections.Generic;

namespace InflexCore.Native.Messages;

public class HandlerContextMenuResultArgument : MessageAnswerArgument
{
	public class HandlerContextMenuItem
	{
		public int ParentMenuIndex { get; set; }

		public int MenuIndex { get; set; }

		public string Caption { get; set; }

		public int MenuTag { get; set; }

		public int Level { get; set; }
	}

	private List<HandlerContextMenuItem> _Menu = new List<HandlerContextMenuItem>();

	public List<HandlerContextMenuItem> Menu => _Menu;

	static HandlerContextMenuResultArgument()
	{
		MessageHelper.RegisterMessageArgumentType("6F9FD7C3-9694-4943-AB21-61CC7DD2D5D3", typeof(HandlerContextMenuResultArgument));
	}

	public void AddMenu(string caption, int menuIndex, int parentMenuIndex, int menuTag, int level)
	{
		HandlerContextMenuItem handlerContextMenuItem = new HandlerContextMenuItem();
		handlerContextMenuItem.Caption = caption;
		handlerContextMenuItem.MenuIndex = menuIndex;
		handlerContextMenuItem.ParentMenuIndex = parentMenuIndex;
		handlerContextMenuItem.MenuTag = menuTag;
		handlerContextMenuItem.Level = level;
		HandlerContextMenuItem item = handlerContextMenuItem;
		Menu.Add(item);
	}
}
