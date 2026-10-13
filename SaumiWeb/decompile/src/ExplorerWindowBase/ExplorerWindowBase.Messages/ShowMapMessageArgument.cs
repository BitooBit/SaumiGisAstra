using System;
using InflexCore.Native.Messages;

namespace ExplorerWindowBase.Messages;

public class ShowMapMessageArgument : MessageArgument
{
	public int ObjID { get; set; }

	public double Scale { get; set; }

	public int WorkspaceID { get; set; }

	public int WorkspaceSrid { get; set; }

	public DateTime PreviewDate { get; set; }

	public bool IsPersonal { get; set; }

	public string Caption { get; set; }

	static ShowMapMessageArgument()
	{
		MessageHelper.RegisterMessageArgumentType("D2367410-D7F4-4343-9DB3-00B9BC3A2939", typeof(ShowMapMessageArgument));
	}
}
