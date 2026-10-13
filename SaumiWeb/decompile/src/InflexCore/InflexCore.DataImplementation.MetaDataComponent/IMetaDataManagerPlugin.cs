using InflexCore.MetaData;

namespace InflexCore.DataImplementation.MetaDataComponent;

public interface IMetaDataManagerPlugin
{
	void FillMetaDataIfNeed(int iteration, IInflexTypeDescription typeDescription);
}
