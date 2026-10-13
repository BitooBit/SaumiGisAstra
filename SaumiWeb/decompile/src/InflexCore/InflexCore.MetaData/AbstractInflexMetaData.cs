namespace InflexCore.MetaData;

public abstract class AbstractInflexMetaData : IInflexMetaData
{
	public InflexMetaDataCollection Attributes { get; private set; }

	protected AbstractInflexMetaData()
	{
		Attributes = new InflexMetaDataCollection();
	}
}
