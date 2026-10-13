using System;
using InflexCore.Native.Messages;

namespace ExplorerWindowBase.Messages;

public class BeginGetPreviewMessageArgument : MessageArgument
{
	public int HandlerID { get; set; }

	public int TaskUID { get; set; }

	public int ObjID { get; set; }

	public double Scale { get; set; }

	public int ImageWidth { get; set; }

	public int ImageHeight { get; set; }

	public int Workspace { get; set; }

	public int Srid { get; set; }

	public DateTime Date { get; set; }

	static BeginGetPreviewMessageArgument()
	{
		MessageHelper.RegisterMessageArgumentType("577A0E3E-8FB6-409D-8A07-7A1B971EB5C6", typeof(BeginGetPreviewMessageArgument));
	}
}
