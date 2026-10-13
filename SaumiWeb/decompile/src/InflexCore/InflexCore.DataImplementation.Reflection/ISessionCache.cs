namespace InflexCore.DataImplementation.Reflection;

public interface ISessionCache
{
	object Get(ISessionCacheObject cacheObject);

	void Set(ISessionCacheObject cacheObject);
}
