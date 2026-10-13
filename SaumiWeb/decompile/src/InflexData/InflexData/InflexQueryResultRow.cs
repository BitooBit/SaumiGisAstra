using InflexCore;

namespace InflexData;

public class InflexQueryResultRow
{
	public IInflexID ID { get; set; }

	public object[] Values { get; set; }

	public InflexQueryResultRow(IInflexID id, object[] values)
	{
		ID = id;
		Values = values;
	}
}
