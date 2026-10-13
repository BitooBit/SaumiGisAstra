using InflexCore.Native.Messages;

namespace ExplorerWindowBase.Messages;

public class GisCreateMetafileMessageArgument : MessageArgument
{
	public int Method { get; set; }

	public int Workspace { get; set; }

	public string LambdaSql { get; set; }

	public double MinX { get; set; }

	public double MinY { get; set; }

	public double MaxX { get; set; }

	public double MaxY { get; set; }

	public double Scale { get; set; }

	public string FileName { get; set; }

	public int Dpi { get; set; }

	public int ImageWidth { get; set; }

	public int ImageHeight { get; set; }

	public double MarginFactor { get; set; }

	public int LayerIdForFind { get; set; }

	static GisCreateMetafileMessageArgument()
	{
		MessageHelper.RegisterMessageArgumentType("03C87675-3944-4D2B-A0F6-9709A6F11102", typeof(GisCreateMetafileMessageArgument));
	}
}
