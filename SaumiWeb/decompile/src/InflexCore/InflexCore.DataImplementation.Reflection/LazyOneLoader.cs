using BLToolkit.DataAccess;

namespace InflexCore.DataImplementation.Reflection;

public class LazyOneLoader<T, TA> : LazyLoader<T, TA> where TA : DataAccessor<T>
{
	private T _Value;

	public T Value(FindOneByID findMethod, decimal id)
	{
		if (!base.IsLoad)
		{
			try
			{
				_Value = findMethod(id);
			}
			finally
			{
				base.IsLoad = true;
			}
		}
		return _Value;
	}

	public T Value(FindOneByIDs findMethod, params object[] keys)
	{
		if (!base.IsLoad)
		{
			try
			{
				_Value = findMethod(keys);
			}
			finally
			{
				base.IsLoad = true;
			}
		}
		return _Value;
	}
}
