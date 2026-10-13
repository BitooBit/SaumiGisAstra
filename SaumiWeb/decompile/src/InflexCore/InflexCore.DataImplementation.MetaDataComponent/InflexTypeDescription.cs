using InflexCore.MetaData;

namespace InflexCore.DataImplementation.MetaDataComponent;

public class InflexTypeDescription : AbstractInflexMetaData, IInflexTypeDescription, IInflexMetaData
{
	public IInflexType InflexType { get; private set; }

	public InflexPropertyCollection Properties { get; private set; }

	public bool IsAbstract => InflexType.Subtypes.GetEnumerator().MoveNext();

	public InflexTypeDescription(IInflexType inflexType, InflexPropertyCollection properties)
	{
		InflexType = inflexType;
		Properties = properties;
	}
}
