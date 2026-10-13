using InflexCore;

namespace InflexData.Handlers;

public class InsertingEventArgs
{
	public IInflexID[] InflexIDs { get; private set; }

	public InsertingEventArgs(IInflexID[] inflexIDs)
	{
		InflexIDs = inflexIDs;
	}
}
