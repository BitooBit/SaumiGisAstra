namespace InflexCore.DataImplementation.Reflection;

public abstract class SessionCacheObject<T> : ISessionCacheObject where T : class
{
	protected virtual bool DirectUseCache => true;

	protected virtual bool DirectEqualsNull => false;

	bool ISessionCacheObject.UseCache => DirectUseCache;

	protected abstract bool DirectEquals(T obj);

	protected abstract int DirectGetHashCode();

	public override bool Equals(object obj)
	{
		T val = obj as T;
		if ((obj == null && DirectEqualsNull) || val != null)
		{
			return DirectEquals(val);
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return DirectGetHashCode();
	}
}
