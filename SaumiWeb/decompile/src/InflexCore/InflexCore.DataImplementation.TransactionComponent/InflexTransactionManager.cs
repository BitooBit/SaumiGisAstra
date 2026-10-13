using System;
using System.Collections.Generic;
using System.Linq;
using InflexCore.Components;
using InflexCore.DataImplementation.TransactionComponent.MetaData;
using InflexCore.MetaData;

namespace InflexCore.DataImplementation.TransactionComponent;

[ReferenceComponent(typeof(TransactionMetaDataManagerPlugin))]
[SingleComponent(typeof(IInflexTransactionManager))]
[ReferenceComponent(typeof(IInflexMetaDataManager))]
public class InflexTransactionManager : AppManager.AppManagerComponent, IInflexTransactionManager
{
	public abstract class AbstractInflexTransaction : IInflexTransaction, IDisposable
	{
		private InflexSubTransaction _SubTransaction;

		private bool _Disposed;

		private readonly object _IdObjectsLock = new object();

		private readonly object _DeleteObjectsLock = new object();

		protected HashSet<IInflexID> DeleteObjects { get; private set; }

		protected HashSet<InflexObject> Objects { get; private set; }

		protected Dictionary<IInflexID, InflexObject> IdObjects { get; private set; }

		protected InflexTransactionManager TransactionManager { get; set; }

		protected AbstractInflexTransaction(InflexTransactionManager transactionManager)
		{
			TransactionManager = transactionManager;
			DeleteObjects = new HashSet<IInflexID>();
			Objects = new HashSet<InflexObject>();
			IdObjects = new Dictionary<IInflexID, InflexObject>();
		}

		public IInflexTransaction NewSubTansaction()
		{
			if (_SubTransaction != null)
			{
				throw new NotSupportedException("Only one sub transaction supported");
			}
			_SubTransaction = new InflexSubTransaction(TransactionManager, this);
			return _SubTransaction;
		}

		public void FreeSubTransaction()
		{
			_SubTransaction = null;
		}

		public void Dispose()
		{
			if (!_Disposed)
			{
				_Disposed = true;
				if (_SubTransaction != null)
				{
					_SubTransaction.Dispose();
				}
				Rollback();
				DirectDispose();
			}
		}

		protected void RemoveTransaction()
		{
			TransactionManager.RemoveTransaction(this);
		}

		protected InflexObject CreateInstance(IInflexType inflexType)
		{
			return TransactionManager.CreateInstance(this, inflexType);
		}

		protected InflexObject CreateInstance(IInflexID inflexID)
		{
			return TransactionManager.CreateInstance(this, inflexID);
		}

		protected abstract void DirectDispose();

		public IInflexObject Create(IInflexType inflexType)
		{
			InflexObject inflexObject = DirectCreate(inflexType);
			RegisterObject(inflexObject, isNew: true);
			return inflexObject;
		}

		protected abstract InflexObject DirectCreate(IInflexType inflexType);

		public IInflexObject Select(IInflexID inflexID)
		{
			lock (_DeleteObjectsLock)
			{
				if (DeleteObjects.Contains(inflexID))
				{
					return null;
				}
			}
			InflexObject value;
			lock (_IdObjectsLock)
			{
				if (!IdObjects.TryGetValue(inflexID, out value))
				{
					value = DirectSelect(inflexID);
					if (value == null)
					{
						return null;
					}
					RegisterObject(value, isNew: false);
				}
			}
			return value;
		}

		public bool IsNew(IInflexObject inflexObject)
		{
			return inflexObject.RuntimeAttributes.Get<TransactionNewMetaData>().IsNew;
		}

		private void RegisterObject(InflexObject inflexObject, bool isNew)
		{
			inflexObject.RuntimeAttributes.Set(new TransactionNewMetaData(isNew));
			inflexObject.RuntimeAttributes.Set(new TransactionChangeMetaData(isNew));
			Objects.Add(inflexObject);
			IdObjects.Add(inflexObject.ID, inflexObject);
		}

		protected void ResetRuntimeAttributes(InflexObject inflexObject)
		{
			inflexObject.RuntimeAttributes.Get<TransactionNewMetaData>().IsNew = false;
			inflexObject.RuntimeAttributes.Get<TransactionChangeMetaData>().IsChanged = false;
		}

		protected abstract InflexObject DirectSelect(IInflexID inflexID);

		public void Delete(IInflexObject inflexObject)
		{
			Delete(inflexObject.ID);
		}

		public void Delete(IInflexID inflexID)
		{
			DeleteObjects.Remove(inflexID);
			DeleteObjects.Add(inflexID);
			InflexObject inflexObject = IdObjects[inflexID];
			if (inflexObject != null)
			{
				Objects.Remove(inflexObject);
			}
		}

		public abstract ValidateResult Validate();

		public abstract void Commit();

		public void Rollback()
		{
			InflexObject[] array = Objects.Where((InflexObject o) => o.RuntimeAttributes.Get<TransactionNewMetaData>().IsNew).ToArray();
			InflexObject[] array2 = array;
			foreach (InflexObject inflexObject in array2)
			{
				Objects.Remove(inflexObject);
				OnRollbackNew(inflexObject);
			}
			foreach (InflexObject @object in Objects)
			{
				@object.SwitchToParent();
				ResetRuntimeAttributes(@object);
			}
			DeleteObjects.Clear();
		}

		protected virtual void OnRollbackNew(InflexObject inflexObject)
		{
		}

		public void ObjectIsChange(InflexObject inflexObject)
		{
			TransactionChangeMetaData transactionChangeMetaData = inflexObject.RuntimeAttributes.Get<TransactionChangeMetaData>(defaultAvailable: true);
			if (transactionChangeMetaData != null)
			{
				transactionChangeMetaData.IsChanged = true;
			}
		}

		protected void Delete(InnerDbAccessor accessor, FullID id)
		{
			TransactionManager.Delete(accessor, id);
		}

		protected FullID GetFullID(InnerDbAccessor accessor, IInflexID id)
		{
			return TransactionManager.GetFullID(accessor, id);
		}

		protected InnerDbAccessor BeginDbTransaction()
		{
			InnerDbAccessor innerDbAccessor = TransactionManager.AppManager.Accessor<InnerDbAccessor>();
			innerDbAccessor.UseTransaction();
			return innerDbAccessor;
		}

		protected void Save(InnerDbAccessor accessor, ref InflexObject inflexObject, bool isNew)
		{
			TransactionManager.Save(accessor, ref inflexObject, isNew);
		}

		public IInflexID ResolveInflexID(IInflexType type, decimal id)
		{
			return TransactionManager.ResolveInflexID(type, id);
		}
	}

	private HashSet<IInflexTransaction> _Transactions = new HashSet<IInflexTransaction>();

	private IInflexMetaDataManager MetaDataManager => base.AppManagerComponents.OfType<IInflexMetaDataManager>().First();

	public IInflexTransaction CreateTransaction()
	{
		IInflexTransaction inflexTransaction = new InflexTransaction(this);
		_Transactions.Add(inflexTransaction);
		return inflexTransaction;
	}

	private InflexObject CreateInstance(AbstractInflexTransaction transaction, IInflexID inflexID)
	{
		if (!IsExists(inflexID))
		{
			return null;
		}
		InflexObject inflexObject = GetInflexObject(transaction, inflexID);
		FillInflexObject(ref inflexObject);
		FillChronologyInfo(ref inflexObject);
		return inflexObject;
	}

	private bool IsExists(IInflexID inflexID)
	{
		IInflexTypeDescription inflexTypeDescription = MetaDataManager.GetInflexTypeDescription(inflexID.Type);
		IInflexProperty inflexProperty = inflexTypeDescription.Properties.Where((IInflexProperty p) => p.IsID).FirstOrDefault();
		if (inflexProperty == null)
		{
			return false;
		}
		return new InnerDbAccessor(base.AppManager.NativeSQLAccessor).IsExists(base.AppManager.MainSchema, inflexProperty.OwnerType.TableName, inflexProperty.Name, inflexID.ID);
	}

	private void FillChronologyInfo(ref InflexObject inflexObject)
	{
		foreach (ITransactionInstancePlugin item in base.AppManagerComponents.OfType<ITransactionInstancePlugin>())
		{
			item.TryFillChronologyInfo(ref inflexObject);
		}
		if (inflexObject.RuntimeAttributes.Get<ChronologyFillMetaData>(defaultAvailable: true) == null)
		{
			InnerFillChronologyInfo(ref inflexObject);
		}
	}

	private void InnerFillChronologyInfo(ref InflexObject inflexObject)
	{
		inflexObject.RuntimeAttributes.Set(new ChronologyFillMetaData());
	}

	private InflexObject CreateInstance(AbstractInflexTransaction transaction, IInflexType inflexType)
	{
		IInflexTypeDescription inflexTypeDescription = MetaDataManager.GetInflexTypeDescription(inflexType);
		if (inflexTypeDescription.IsAbstract)
		{
			throw new AbstractTypeCreateException();
		}
		IInflexID inflexID = GetInflexID(inflexTypeDescription);
		InflexObject inflexObject = GetInflexObject(transaction, inflexID);
		FillDefaultValue(ref inflexObject);
		return inflexObject;
	}

	private void FillDefaultValue(ref InflexObject inflexObject)
	{
		IInflexTypeDescription inflexTypeDescription = MetaDataManager.GetInflexTypeDescription(inflexObject.ID.Type);
		if (inflexTypeDescription.IsAbstract)
		{
			throw new AbstractTypeCreateException();
		}
		DbTableCollectionMetaData dbTableCollectionMetaData = inflexTypeDescription.Attributes.Get<DbTableCollectionMetaData>();
		dbTableCollectionMetaData.FillDefaultValue(ref inflexObject, base.AppManager);
		dbTableCollectionMetaData.FillChildIdentField(ref inflexObject);
		dbTableCollectionMetaData.FillParentID(ref inflexObject);
		foreach (ITransactionInstancePlugin item in base.AppManagerComponents.OfType<ITransactionInstancePlugin>())
		{
			item.AfterFillDefaultValue(ref inflexObject);
		}
	}

	private IInflexID GetInflexID(IInflexTypeDescription typeDescription)
	{
		SeqTypeMetaData seqTypeMetaData = typeDescription.Attributes.Get<SeqTypeMetaData>();
		decimal nextID = seqTypeMetaData.GetNextID(base.AppManager.NativeSQLAccessor);
		return new MockInflexID(typeDescription.InflexType, nextID);
	}

	private void FillInflexObject(ref InflexObject inflexObject)
	{
		IInflexTypeDescription inflexTypeDescription = MetaDataManager.GetInflexTypeDescription(inflexObject.ID.Type);
		if (inflexTypeDescription.IsAbstract)
		{
			throw new AbstractTypeCreateException();
		}
		DbTableCollectionMetaData dbTableCollectionMetaData = inflexTypeDescription.Attributes.Get<DbTableCollectionMetaData>();
		dbTableCollectionMetaData.Fill(ref inflexObject, base.AppManager);
		foreach (ITransactionInstancePlugin item in base.AppManagerComponents.OfType<ITransactionInstancePlugin>())
		{
			item.AfterFill(ref inflexObject);
		}
	}

	private InflexObject GetInflexObject(AbstractInflexTransaction transaction, IInflexID inflexID)
	{
		InflexObject inflexObject = null;
		foreach (ITransactionInstancePlugin item in base.AppManagerComponents.OfType<ITransactionInstancePlugin>())
		{
			inflexObject = item.CreateInstance(transaction, inflexID, inflexObject);
		}
		if (inflexObject == null)
		{
			inflexObject = new InflexObject(transaction, inflexID);
		}
		return inflexObject;
	}

	private void RemoveTransaction(IInflexTransaction transaction)
	{
		_Transactions.Remove(transaction);
	}

	private void Delete(InnerDbAccessor accessor, FullID id)
	{
		foreach (FullID reference in id.References)
		{
			Delete(accessor, reference);
		}
		accessor.Delete(base.AppManager.MainSchema, id.TableName, id.FieldName, id.Value);
	}

	private FullID GetFullID(InnerDbAccessor accessor, IInflexID id)
	{
		IInflexTypeDescription inflexTypeDescription = MetaDataManager.GetInflexTypeDescription(id.Type);
		DbTableCollectionMetaData dbTableCollectionMetaData = inflexTypeDescription.Attributes.Get<DbTableCollectionMetaData>();
		return dbTableCollectionMetaData.GetFullID(id, base.AppManager, accessor);
	}

	private void Save(InnerDbAccessor accessor, ref InflexObject inflexObject, bool isNew)
	{
		IInflexTypeDescription inflexTypeDescription = MetaDataManager.GetInflexTypeDescription(inflexObject.ID.Type);
		DbTableCollectionMetaData dbTableCollectionMetaData = inflexTypeDescription.Attributes.Get<DbTableCollectionMetaData>();
		dbTableCollectionMetaData.Save(ref inflexObject, inflexTypeDescription, base.AppManager, accessor, isNew);
	}

	public IInflexID ResolveInflexID(IInflexType type, decimal id)
	{
		if (!type.IsSubtype)
		{
			using IEnumerator<IInflexType> enumerator = type.Subtypes.GetEnumerator();
			if (enumerator.MoveNext())
			{
				IInflexType current = enumerator.Current;
				IInflexTypeDescription inflexTypeDescription = MetaDataManager.GetInflexTypeDescription(current);
				decimal childIdentValue = inflexTypeDescription.Attributes.Get<DbTableCollectionMetaData>().GetChildIdentValue(id, base.AppManager);
				foreach (IInflexType subtype in type.Subtypes)
				{
					decimal? subtypeID = subtype.SubtypeID;
					decimal num = childIdentValue;
					if (subtypeID.GetValueOrDefault() == num && subtypeID.HasValue)
					{
						return new MockInflexID(subtype, id);
					}
				}
				throw new InvalidOperationException("Subtype for id not resolved. " + type.Name + ", " + id);
			}
		}
		return new MockInflexID(type, id);
	}
}
