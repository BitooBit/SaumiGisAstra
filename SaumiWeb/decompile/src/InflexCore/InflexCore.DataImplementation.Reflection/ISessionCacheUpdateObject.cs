namespace InflexCore.DataImplementation.Reflection;

public interface ISessionCacheUpdateObject
{
	bool IsUpdateSelf { get; }

	void UpdateSelf(object newObject);
}
