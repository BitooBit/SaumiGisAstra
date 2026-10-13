using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InflexCore.MetaData;

namespace InflexCore.DataImplementation.TransactionComponent.MetaData;

public class DbTableCollectionMetaData : AbstractInflexMetaData, ICollection<DbTableMetaData>, IEnumerable<DbTableMetaData>, IEnumerable
{
	private class SmallFillHelper
	{
		private readonly FullID _FullID;

		private readonly decimal _Value;

		private InflexObject _InflexObject;

		private readonly AppManager _AppManager;

		private readonly InnerDbAccessor _Accessor;

		private readonly IInflexTypeDescription _TypeDescription;

		private decimal _LastValue;

		private Dictionary<DbTableMetaData, FullID> _FullIDs = new Dictionary<DbTableMetaData, FullID>();

		private bool _IsBreak;

		public SmallFillHelper(ref InflexObject inflexObject, AppManager appManager, InnerDbAccessor accessor, IInflexTypeDescription typeDescription)
		{
			_InflexObject = inflexObject;
			_AppManager = appManager;
			_Accessor = accessor;
			_TypeDescription = typeDescription;
		}

		public SmallFillHelper(ref FullID fullID, decimal value, AppManager appManager, InnerDbAccessor accessor)
		{
			_FullID = fullID;
			_Value = value;
			_AppManager = appManager;
			_Accessor = accessor;
		}

		public void Fill(DbTableMetaData table)
		{
			table.Fill(ref _InflexObject, _AppManager);
		}

		public void FillParentID(DbTableMetaData table)
		{
			table.FillParentID(ref _InflexObject);
		}

		public void FillFullID(DbTableMetaData table)
		{
			if (_IsBreak)
			{
				return;
			}
			decimal restrictValue = ((_LastValue > 0m) ? _LastValue : _Value);
			FullID fullID = table.GetFullID(_AppManager, _Accessor, restrictValue);
			if (fullID == null)
			{
				_FullID.Value = 0m;
				_IsBreak = true;
				return;
			}
			_LastValue = fullID.Value;
			if (table.ParentTable == null)
			{
				_FullID.FieldName = fullID.FieldName;
				_FullID.TableName = fullID.TableName;
				_FullID.Value = fullID.Value;
				_FullIDs.Add(table, _FullID);
			}
			else
			{
				_FullIDs[table.ParentTable].References.Add(fullID);
			}
		}

		public void Save(DbTableMetaData table)
		{
			table.Save(ref _InflexObject, _AppManager, _Accessor);
		}

		public void SaveNew(DbTableMetaData table)
		{
			table.SaveNew(ref _InflexObject, _TypeDescription, _AppManager, _Accessor);
		}
	}

	private HashSet<DbTableMetaData> _Data = new HashSet<DbTableMetaData>();

	public int Count => _Data.Count;

	public bool IsReadOnly => false;

	public IEnumerator<DbTableMetaData> GetEnumerator()
	{
		return _Data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(DbTableMetaData item)
	{
		_Data.Add(item);
	}

	public void Clear()
	{
		_Data.Clear();
	}

	public bool Contains(DbTableMetaData item)
	{
		return _Data.Contains(item);
	}

	public void CopyTo(DbTableMetaData[] array, int arrayIndex)
	{
		_Data.CopyTo(array, arrayIndex);
	}

	public bool Remove(DbTableMetaData item)
	{
		return _Data.Remove(item);
	}

	public void CalcPrimaryFields(AppManager appManager)
	{
		foreach (DbTableMetaData datum in _Data)
		{
			datum.CalcPrimaryField(appManager);
		}
	}

	public void CalcGeneralizationAssociation(AppManager appManager)
	{
		foreach (DbTableMetaData datum in _Data)
		{
			datum.CalcGeneralizationAssociation(appManager);
		}
	}

	public void CalcDefaultValues(AppManager appManager)
	{
		foreach (DbTableMetaData datum in _Data)
		{
			datum.CalcDefaultValues(appManager);
		}
	}

	public void FillDefaultValue(ref InflexObject inflexObject, AppManager appManager)
	{
		foreach (DbTableMetaData datum in _Data)
		{
			datum.FillDefaultValue(ref inflexObject, appManager);
		}
	}

	public void FillChildIdentField(ref InflexObject inflexObject)
	{
		DbTableMetaData dbTableMetaData = this.Where((DbTableMetaData t) => t.IsParent).FirstOrDefault();
		if (dbTableMetaData != null)
		{
			DbFieldMetaData dbFieldMetaData = dbTableMetaData.Fields.Where((DbFieldMetaData f) => f.IsChildIdent).FirstOrDefault();
			if (dbFieldMetaData != null)
			{
				decimal? subtypeID = inflexObject.ID.Type.SubtypeID;
				dbFieldMetaData.Property.SetValue(inflexObject, subtypeID.HasValue ? subtypeID.Value : 0m);
			}
		}
	}

	public decimal GetChildIdentValue(decimal id, AppManager appManager)
	{
		DbTableMetaData dbTableMetaData = this.Where((DbTableMetaData t) => t.IsParent).FirstOrDefault();
		if (dbTableMetaData != null)
		{
			return dbTableMetaData.GetChildIdentValue(id, appManager);
		}
		throw new InvalidOperationException("Parent table not found");
	}

	public void FillParentID(ref InflexObject inflexObject)
	{
		ParentToChildEach(new SmallFillHelper(ref inflexObject, null, null, null).FillParentID);
	}

	public void Fill(ref InflexObject inflexObject, AppManager appManager)
	{
		ParentToChildEach(new SmallFillHelper(ref inflexObject, appManager, null, null).Fill);
	}

	public FullID GetFullID(IInflexID id, AppManager appManager, InnerDbAccessor accessor)
	{
		FullID fullID = new FullID();
		ParentToChildEach(new SmallFillHelper(ref fullID, id.ID, appManager, accessor).FillFullID);
		if (fullID.Value > 0m)
		{
			return fullID;
		}
		return null;
	}

	public void Save(ref InflexObject inflexObject, IInflexTypeDescription typeDescription, AppManager appManager, InnerDbAccessor accessor, bool isNew)
	{
		if (isNew)
		{
			ParentToChildEach(new SmallFillHelper(ref inflexObject, appManager, accessor, typeDescription).SaveNew);
		}
		else
		{
			ParentToChildEach(new SmallFillHelper(ref inflexObject, appManager, accessor, typeDescription).Save);
		}
	}

	private void ParentToChildEach(Action<DbTableMetaData> handler)
	{
		DbTableMetaData[] parentTables = new DbTableMetaData[1];
		ParentToChildEach(parentTables, handler);
	}

	private void ParentToChildEach(DbTableMetaData[] parentTables, Action<DbTableMetaData> handler)
	{
		List<DbTableMetaData> list = new List<DbTableMetaData>();
		using (IEnumerator<DbTableMetaData> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DbTableMetaData current = enumerator.Current;
				foreach (DbTableMetaData dbTableMetaData in parentTables)
				{
					if (current.ParentTable == dbTableMetaData)
					{
						handler(current);
						list.Add(current);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			ParentToChildEach(list.ToArray(), handler);
		}
	}
}
