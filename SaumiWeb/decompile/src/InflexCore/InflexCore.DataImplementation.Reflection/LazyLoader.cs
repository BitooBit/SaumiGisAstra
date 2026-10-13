using System.Collections.Generic;
using BLToolkit.DataAccess;

namespace InflexCore.DataImplementation.Reflection;

public abstract class LazyLoader<T, TA> : AppManager.InflexAccessorBase<T, TA> where TA : DataAccessor<T>
{
	public delegate T FindOneByID(decimal id);

	public delegate T FindOneByIDs(params object[] keys);

	public delegate IList<T> FindListByID(decimal id);

	public delegate IList<T> FindList();

	private readonly object _AccessorGetterSync = new object();

	private readonly AppManager _AppManager;

	private bool _IsLoad;

	private TA _Accessor;

	protected bool IsLoad
	{
		get
		{
			return _IsLoad;
		}
		set
		{
			_IsLoad = value;
		}
	}

	public TA Accessor
	{
		get
		{
			lock (_AccessorGetterSync)
			{
				if (_Accessor == null)
				{
					_Accessor = ((_AppManager == null) ? AppManager.AccessorStatic<TA>() : _AppManager.Accessor<TA>());
				}
				return _Accessor;
			}
		}
	}

	protected LazyLoader(AppManager appManager)
	{
		_AppManager = appManager;
	}

	protected LazyLoader()
	{
	}
}
