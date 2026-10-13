using System.Collections;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.Mapping;

public class SessionCacheSupportObjectListMapper : ObjectListMapper
{
	private IList _Data;

	public SessionCacheSupportObjectListMapper(IList list, ObjectMapper objectMapper)
		: base(list, objectMapper)
	{
		_Data = list;
	}

	public void CacheSynchronize()
	{
		ISessionCache instance = AppManager.Instance;
		for (int i = 0; i < _Data.Count; i++)
		{
			if (_Data[i] is ISessionCacheObject { UseCache: not false } sessionCacheObject)
			{
				object obj = instance.Get(sessionCacheObject);
				if (obj != null)
				{
					_Data[i] = obj;
				}
				else
				{
					instance.Set(sessionCacheObject);
				}
			}
		}
	}
}
