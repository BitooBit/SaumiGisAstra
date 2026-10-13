using System.Collections.Generic;
using InflexCore.Native.Messages;

namespace ExplorerWindowBase.Messages;

public class InitDataWindowResultArgument : MessageAnswerArgument
{
	public class MenuItem
	{
		public int MenuType { get; set; }

		public int ID { get; set; }

		public int ParentID { get; set; }

		public string Caption { get; set; }

		public int EnableFlag { get; set; }

		public MenuItem()
		{
		}

		public MenuItem(int menuType, int id, int parentID, string caption, int enableFlag)
		{
			MenuType = menuType;
			ID = id;
			ParentID = parentID;
			Caption = caption;
			EnableFlag = enableFlag;
		}
	}

	private List<MenuItem> _Menu = new List<MenuItem>();

	public List<MenuItem> Menu => _Menu;

	static InitDataWindowResultArgument()
	{
		MessageHelper.RegisterMessageArgumentType("B0AA8FF7-FC5A-4C72-8EE2-8591F08732FE", typeof(InitDataWindowResultArgument));
	}

	public void Add(int menuType, int id, int parentID, string caption, int enableFlag)
	{
		_Menu.Add(new MenuItem(menuType, id, parentID, caption, enableFlag));
	}
}
