using System.Collections;
using BLToolkit.Data.DataProvider;
using BLToolkit.Mapping;
using BLToolkit.Reflection;
using InflexCore.DataImplementation.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.DataProvider;

public class InflexOdpMappingSchema : OdpDataProvider.OdpMappingSchema
{
	private ISessionCache _Cache;

	private ISessionCache Cache
	{
		get
		{
			if (_Cache == null)
			{
				_Cache = AppManager.Instance;
				if (_Cache == null)
				{
					_Cache = new SimpleSessionCache();
				}
			}
			return _Cache;
		}
	}

	protected override object MapInternal(InitContext initContext)
	{
		object obj = base.MapInternal(initContext);
		if (obj is ISessionCacheObject { UseCache: not false } sessionCacheObject)
		{
			object obj2 = Cache.Get(sessionCacheObject);
			if (obj2 != null)
			{
				return obj2;
			}
			Cache.Set(sessionCacheObject);
		}
		return obj;
	}

	public override ObjectListMapper CreateObjectListMapper(IList list, ObjectMapper objectMapper)
	{
		return new SessionCacheSupportObjectListMapper(list, objectMapper);
	}

	public override void MapSourceListToDestinationList(IMapDataSourceList dataSourceList, IMapDataDestinationList dataDestinationList, params object[] parameters)
	{
		base.MapSourceListToDestinationList(dataSourceList, new SessionCacheSupportMapDataDestinationList(dataDestinationList), parameters);
	}
}
