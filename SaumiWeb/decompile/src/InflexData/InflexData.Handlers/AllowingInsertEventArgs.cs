using InflexCore;

namespace InflexData.Handlers;

public class AllowingInsertEventArgs
{
	public IInflexID[] InflexIDs { get; private set; }

	public AllowingInsertEventArgs(IInflexID[] inflexIDs)
	{
		InflexIDs = inflexIDs;
	}
}
