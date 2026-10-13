using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace InflexCore.DataImplementation.Reflection;

internal class SimpleSessionCache : ISessionCache
{
	private class CacheWeakReference : WeakReference
	{
		private int _HashCode;

		public CacheWeakReference(object target)
			: base(target)
		{
			_HashCode = target.GetHashCode();
		}

		public CacheWeakReference(object target, bool trackResurrection)
			: base(target, trackResurrection)
		{
			_HashCode = target.GetHashCode();
		}

		protected CacheWeakReference(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public override int GetHashCode()
		{
			return _HashCode;
		}

		public override bool Equals(object obj)
		{
			object target = Target;
			if (target == null)
			{
				return false;
			}
			if (!(obj is CacheWeakReference cacheWeakReference))
			{
				return base.Equals(obj);
			}
			return target.Equals(cacheWeakReference.Target);
		}
	}

	private Dictionary<CacheWeakReference, WeakReference> _Cache = new Dictionary<CacheWeakReference, WeakReference>();

	public object Get(ISessionCacheObject cacheObject)
	{
		CacheWeakReference key = new CacheWeakReference(cacheObject);
		_Cache.TryGetValue(key, out var value);
		if (value != null)
		{
			if (value.Target is ISessionCacheUpdateObject { IsUpdateSelf: not false } sessionCacheUpdateObject)
			{
				sessionCacheUpdateObject.UpdateSelf(cacheObject);
			}
			return value.Target;
		}
		return null;
	}

	public void Set(ISessionCacheObject cacheObject)
	{
		if (Get(cacheObject) == null)
		{
			ClearEmpty();
			CacheWeakReference key = new CacheWeakReference(cacheObject);
			_Cache.Add(key, new WeakReference(cacheObject));
		}
	}

	private void ClearEmpty()
	{
		if (_Cache.Count < 100)
		{
			return;
		}
		Dictionary<CacheWeakReference, WeakReference> dictionary = new Dictionary<CacheWeakReference, WeakReference>();
		bool flag = false;
		foreach (KeyValuePair<CacheWeakReference, WeakReference> item in _Cache)
		{
			if (item.Key.IsAlive)
			{
				dictionary.Add(item.Key, item.Value);
			}
			else
			{
				flag = true;
			}
		}
		if (flag)
		{
			_Cache = dictionary;
		}
	}
}
