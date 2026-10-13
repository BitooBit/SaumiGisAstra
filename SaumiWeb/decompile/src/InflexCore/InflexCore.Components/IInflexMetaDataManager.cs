using InflexCore.MetaData;

namespace InflexCore.Components;

public interface IInflexMetaDataManager
{
	IInflexTypeDescription GetInflexTypeDescription(IInflexType inflexType);
}
