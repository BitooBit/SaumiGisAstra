using BLToolkit.Mapping;
using BLToolkit.Reflection;

namespace InflexCore.DataImplementation.Mapping;

public class SessionCacheSupportMapDataDestinationList : IMapDataDestinationList
{
	private readonly IMapDataDestinationList _InnerSelf;

	public SessionCacheSupportMapDataDestinationList(IMapDataDestinationList innerSelf)
	{
		_InnerSelf = innerSelf;
	}

	void IMapDataDestinationList.EndMapping(InitContext initContext)
	{
		_InnerSelf.EndMapping(initContext);
		if (_InnerSelf is SessionCacheSupportObjectListMapper sessionCacheSupportObjectListMapper)
		{
			sessionCacheSupportObjectListMapper.CacheSynchronize();
		}
	}

	IMapDataDestination IMapDataDestinationList.GetDataDestination(InitContext initContext)
	{
		return _InnerSelf.GetDataDestination(initContext);
	}

	object IMapDataDestinationList.GetNextObject(InitContext initContext)
	{
		return _InnerSelf.GetNextObject(initContext);
	}

	void IMapDataDestinationList.InitMapping(InitContext initContext)
	{
		_InnerSelf.InitMapping(initContext);
	}
}
