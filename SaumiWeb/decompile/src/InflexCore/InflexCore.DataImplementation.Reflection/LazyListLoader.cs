using System.Collections.Generic;
using BLToolkit.DataAccess;

namespace InflexCore.DataImplementation.Reflection;

public class LazyListLoader<T, TA> : LazyLoader<T, TA> where TA : DataAccessor<T>
{
	private readonly object _ListLoadSync = new object();

	private IList<T> _List;

	public LazyListLoader()
	{
	}

	public LazyListLoader(AppManager appManager)
		: base(appManager)
	{
	}

	public IList<T> List(FindListByID findMethod, decimal id)
	{
		lock (_ListLoadSync)
		{
			if (!base.IsLoad)
			{
				try
				{
					_List = findMethod(id);
				}
				finally
				{
					base.IsLoad = true;
				}
			}
			return _List;
		}
	}

	public IList<T> List(FindList findMethod)
	{
		lock (_ListLoadSync)
		{
			if (!base.IsLoad)
			{
				try
				{
					_List = findMethod();
				}
				finally
				{
					base.IsLoad = true;
				}
			}
			return _List;
		}
	}
}
