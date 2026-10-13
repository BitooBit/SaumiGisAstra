using System.Collections.Generic;

namespace InflexCore.DataImplementation.TransactionComponent;

public class FullID
{
	public string TableName { get; set; }

	public string FieldName { get; set; }

	public decimal Value { get; set; }

	public List<FullID> References { get; set; }

	public FullID()
	{
		References = new List<FullID>();
	}
}
