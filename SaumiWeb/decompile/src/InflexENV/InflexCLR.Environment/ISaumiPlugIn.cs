namespace InflexCLR.Environment;

public interface ISaumiPlugIn
{
	string PlugInName { get; }

	void Execute();
}
