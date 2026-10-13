using InflexCore.MetaData;

namespace InflexCore.DataImplementation.TransactionComponent.MetaData;

public class TransactionChangeMetaData : AbstractInflexMetaData
{
	public bool IsChanged { get; set; }

	public TransactionChangeMetaData(bool isChanged)
	{
		IsChanged = isChanged;
	}
}
