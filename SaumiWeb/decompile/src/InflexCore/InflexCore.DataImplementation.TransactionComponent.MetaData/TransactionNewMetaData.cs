using InflexCore.MetaData;

namespace InflexCore.DataImplementation.TransactionComponent.MetaData;

public class TransactionNewMetaData : AbstractInflexMetaData
{
	public bool IsNew { get; set; }

	public TransactionNewMetaData(bool isNew)
	{
		IsNew = isNew;
	}
}
