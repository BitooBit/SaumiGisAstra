using System.Collections.Generic;
using BLToolkit.Data;
using BLToolkit.DataAccess;
using BLToolkit.Mapping;

namespace InflexCore.DataImplementation.Reflection;

public class InflexSqlQuery<T> : SqlQuery<T>
{
	public InflexSqlQuery(DbManager dbManager, bool dispose)
		: base(dbManager, dispose)
	{
	}

	private void InsertKeys(DbManager db, IEnumerable<T> list)
	{
		GetKeyMetaData(db, out var mms, out var sqls);
		foreach (T item in list)
		{
			InsertKeys(db, item, item as ISelfKeyGenerator, mms, sqls);
		}
	}

	private void InsertKeys(DbManager db, T obj)
	{
		ISelfKeyGenerator selfKeyGenerator = obj as ISelfKeyGenerator;
		if (selfKeyGenerator != null)
		{
			selfKeyGenerator.NewKey();
			return;
		}
		GetKeyMetaData(db, out var mms, out var sqls);
		InsertKeys(db, obj, selfKeyGenerator, mms, sqls);
	}

	private void InsertKeys(DbManager db, T obj, ISelfKeyGenerator keyGenerator, MemberMapper[] mms, string[] sqls)
	{
		if (keyGenerator != null)
		{
			keyGenerator.NewKey();
			return;
		}
		for (int i = 0; i < mms.Length; i++)
		{
			if (string.IsNullOrEmpty(sqls[i]) && mms[i].GetType().Default() == mms[i].GetValue(obj))
			{
				throw new DataAccessException("Sequence for key field not found.");
			}
			object value = db.SetCommand(sqls[i]).ExecuteScalar();
			mms[i].SetValue(obj, value);
		}
	}

	private void GetKeyMetaData(DbManager db, out MemberMapper[] mms, out string[] sqls)
	{
		mms = GetKeyFieldList(db, typeof(T));
		sqls = new string[mms.Length];
		for (int i = 0; i < mms.Length; i++)
		{
			MemberMapper memberMapper = mms[i];
			SeqNameAttribute attribute = memberMapper.MemberAccessor.GetAttribute<SeqNameAttribute>();
			sqls[i] = ((attribute == null) ? null : $"select {attribute.GetSequence(base.MappingSchema, db)}.nextval from dual");
		}
	}

	private void CacheObject(T obj)
	{
		ISessionCache instance = AppManager.Instance;
		if (obj is ISessionCacheObject { UseCache: not false } sessionCacheObject)
		{
			instance.Set(sessionCacheObject);
		}
	}

	private void CacheObject(IEnumerable<T> list)
	{
		ISessionCache instance = AppManager.Instance;
		foreach (T item in list)
		{
			if (item is ISessionCacheObject { UseCache: not false } sessionCacheObject)
			{
				instance.Set(sessionCacheObject);
			}
		}
	}

	public override int Insert(DbManager db, int maxBatchSize, IEnumerable<T> list)
	{
		InsertKeys(db, list);
		int result = base.Insert(db, maxBatchSize, list);
		CacheObject(list);
		return result;
	}

	public override int Insert(DbManager db, T obj)
	{
		InsertKeys(db, obj);
		int result = base.Insert(db, obj);
		CacheObject(obj);
		return result;
	}

	public override int Insert(IEnumerable<T> list)
	{
		using DbManager db = GetDbManager();
		InsertKeys(db, list);
		int result = base.Insert(list);
		CacheObject(list);
		return result;
	}

	public override int Insert(T obj)
	{
		using DbManager db = GetDbManager();
		InsertKeys(db, obj);
		int result = base.Insert(obj);
		CacheObject(obj);
		return result;
	}
}
