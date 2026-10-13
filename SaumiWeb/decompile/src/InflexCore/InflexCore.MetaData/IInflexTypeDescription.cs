namespace InflexCore.MetaData;

public interface IInflexTypeDescription : IInflexMetaData
{
	IInflexType InflexType { get; }

	InflexPropertyCollection Properties { get; }

	bool IsAbstract { get; }
}
