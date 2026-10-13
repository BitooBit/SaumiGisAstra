using System;
using System.Collections.Generic;
using InflexCore.Components;
using InflexCore.DataImplementation.MetaDataComponent;
using InflexCore.MetaData;
using InflexCore.log;

namespace InflexCore.DataImplementation.TransactionComponent;

public class InflexObject : IInflexObject
{
	protected class InflexObjectTransactionState
	{
		private readonly InflexObject _Owner;

		private Dictionary<object, object> _Data = new Dictionary<object, object>();

		public InflexObjectTransactionState ParentState { get; set; }

		public object this[object key]
		{
			set
			{
				_Data[key] = value;
			}
		}

		public InflexObjectTransactionState(InflexObject owner, InflexObjectTransactionState parentState)
		{
			_Owner = owner;
			ParentState = parentState;
		}

		public bool TryGetValue(object key, out object value)
		{
			for (InflexObjectTransactionState inflexObjectTransactionState = this; inflexObjectTransactionState != null; inflexObjectTransactionState = inflexObjectTransactionState.ParentState)
			{
				if (inflexObjectTransactionState._Data.TryGetValue(key, out value))
				{
					return true;
				}
			}
			value = null;
			return false;
		}

		public void CopyToParent()
		{
			if (ParentState == null)
			{
				throw new InvalidOperationException();
			}
			foreach (KeyValuePair<object, object> datum in _Data)
			{
				ParentState._Data[datum.Key] = datum.Value;
			}
			InflexObject owner = ParentState._Owner;
			owner?._InnerTransaction.ObjectIsChange(owner);
		}

		public Dictionary<IInflexProperty, object> GetPropertyChangeValues()
		{
			Dictionary<IInflexProperty, object> dictionary = new Dictionary<IInflexProperty, object>();
			foreach (KeyValuePair<object, object> datum in _Data)
			{
				if (datum.Key is IInflexProperty key)
				{
					dictionary.Add(key, datum.Value);
				}
			}
			return dictionary;
		}

		public object DirectGet(object key, bool nullIfNotFound)
		{
			if (nullIfNotFound)
			{
				_Data.TryGetValue(key, out var value);
				return value;
			}
			return _Data[key];
		}
	}

	private InflexPeriod _CurrentLifePeriod;

	private InflexObjectTransactionState _ObjectState;

	private readonly InflexPeriod _OriginalLifePeriod;

	private InflexTransactionManager.AbstractInflexTransaction _InnerTransaction;

	public IInflexID ID { get; private set; }

	public IInflexTransaction Transaction { get; private set; }

	private Dictionary<InflexPeriod, InflexObjectTransactionState> ObjectStates { get; set; }

	protected InflexObjectTransactionState ObjectState
	{
		get
		{
			if (_ObjectState == null && !ObjectStates.TryGetValue(CurrentLifePeriod, out _ObjectState))
			{
				InflexObjectTransactionState parentState = ObjectStates[_OriginalLifePeriod];
				_ObjectState = new InflexObjectTransactionState(this, parentState);
				ObjectStates.Add(CurrentLifePeriod, _ObjectState);
			}
			return _ObjectState;
		}
	}

	private InflexObjectTransactionState OriginalObjectState => ObjectStates[_OriginalLifePeriod];

	public InflexMetaDataCollection RuntimeAttributes { get; private set; }

	public InflexPeriod CurrentLifePeriod
	{
		get
		{
			return _CurrentLifePeriod ?? _OriginalLifePeriod;
		}
		set
		{
			_CurrentLifePeriod = value;
			_ObjectState = null;
		}
	}

	public InflexPeriodCollection LifePeriods { get; private set; }

	public InflexObject(IInflexTransaction transaction, IInflexID inflexID)
	{
		if (transaction == null)
		{
			throw new NotSupportedException("Transaction can't be null.");
		}
		_InnerTransaction = transaction as InflexTransactionManager.AbstractInflexTransaction;
		if (_InnerTransaction == null)
		{
			throw new NotSupportedException(string.Concat("Transaction type (", transaction.GetType(), ") not supported."));
		}
		Transaction = transaction;
		ID = inflexID;
		LifePeriods = new InflexPeriodCollection();
		ObjectStates = new Dictionary<InflexPeriod, InflexObjectTransactionState>();
		RuntimeAttributes = new InflexMetaDataCollection();
		_OriginalLifePeriod = InflexPeriod.FullInfinity();
		CreateOriginalObjectState();
	}

	private void CreateOriginalObjectState()
	{
		ObjectStates[_OriginalLifePeriod] = new InflexObjectTransactionState(this, null);
	}

	public void DirectSetPropertyValue(IInflexProperty property, object value)
	{
		if (value is IInflexID inflexID)
		{
			DirectSetInflexID(property, inflexID);
			return;
		}
		if (value is InflexObject inflexObject)
		{
			if (inflexObject.Transaction != Transaction)
			{
				throw new InvalidOperationException("InflexObject.Transaction != this.Transaction");
			}
			DirectSetInflexID(property, inflexObject.ID);
			return;
		}
		object value2 = value;
		if (property.IsBoolean && value is bool)
		{
			value2 = (((bool)value) ? "Y" : "N");
		}
		ObjectState[property] = value2;
		_InnerTransaction.ObjectIsChange(this);
	}

	private void DirectSetInflexID(IInflexProperty property, IInflexID inflexID)
	{
		if (property.PropertyType != null)
		{
			DirectSetPropertyValue(property, inflexID.ID);
			return;
		}
		throw new NotSupportedException("Property type is not InflexType");
	}

	public void ValidateSetPropertyValue(IInflexProperty property, object value)
	{
	}

	public object DirectGetPropertyValue(IInflexProperty property)
	{
		return DirectGetPropertyValue(property, defaultNull: false);
	}

	private object DirectGetPropertyValue(IInflexProperty property, bool defaultNull)
	{
		if (ObjectState.TryGetValue(property, out var value))
		{
			return value;
		}
		if (property.IsID)
		{
			return ID.ID;
		}
		if (property.IsNullable || defaultNull)
		{
			return null;
		}
		if (property.IsNumber)
		{
			return 0m;
		}
		if (property.IsString)
		{
			return string.Empty;
		}
		if (property.IsTime)
		{
			return default(DateTime);
		}
		if (property.IsBoolean)
		{
			return 'N';
		}
		throw new NotInflexObjectPropertyException(property);
	}

	public InflexObject Branch(InflexTransactionManager.AbstractInflexTransaction transaction)
	{
		InflexObject inflexObject = this;
		InflexObjectTransactionState parentState = OriginalObjectState;
		if (!object.ReferenceEquals(Transaction, transaction))
		{
			inflexObject = CreateBranchInstance(transaction);
		}
		else
		{
			parentState = new InflexObjectTransactionState(null, null);
		}
		inflexObject.OriginalObjectState.ParentState = parentState;
		return inflexObject;
	}

	protected virtual InflexObject CreateBranchInstance(InflexTransactionManager.AbstractInflexTransaction transaction)
	{
		return new InflexObject(transaction, ID);
	}

	public void Merge()
	{
		InflexObjectTransactionState originalObjectState = OriginalObjectState;
		if (originalObjectState.ParentState == null)
		{
			throw new InvalidOperationException();
		}
		originalObjectState.CopyToParent();
	}

	public void SwitchToParent()
	{
		InflexObjectTransactionState parentState = OriginalObjectState.ParentState;
		if (parentState == null)
		{
			throw new InvalidOperationException();
		}
		InflexPeriod currentLifePeriod = CurrentLifePeriod;
		ObjectStates.Clear();
		CreateOriginalObjectState();
		OriginalObjectState.ParentState = parentState;
		CurrentLifePeriod = currentLifePeriod;
	}

	public Dictionary<IInflexProperty, object> DirectGetPropertyChangeValues()
	{
		return OriginalObjectState.GetPropertyChangeValues();
	}

	public IInflexObject DirectGetInflexValue(IInflexProperty property)
	{
		IInflexID inflexID = DirectGetInflexID(property);
		if (inflexID == null)
		{
			return null;
		}
		return Transaction.Select(inflexID);
	}

	public IInflexID DirectGetInflexID(IInflexProperty property)
	{
		if (property.PropertyType != null)
		{
			try
			{
				decimal num = (decimal)DirectGetPropertyValue(property, defaultNull: true);
				object key = new InflexLinkProperty(property);
				IInflexID inflexID = ObjectState.DirectGet(key, nullIfNotFound: true) as IInflexID;
				if (inflexID == null || inflexID.ID != num)
				{
					inflexID = _InnerTransaction.ResolveInflexID(property.PropertyType, num);
					ObjectState[key] = inflexID;
				}
				return inflexID;
			}
			catch (Exception ex)
			{
				FastLog.Loger(this).Error((object)"decimal id = (decimal)this.DirectGetPropertyValue(property)", ex);
				return null;
			}
		}
		throw new NotSupportedException("Property type is not InflexType");
	}

	public bool? DirectGetBoolean(IInflexProperty property)
	{
		if (property.IsBoolean)
		{
			try
			{
				string text = (string)DirectGetPropertyValue(property);
				return text == "Y" || text == "y";
			}
			catch (Exception ex)
			{
				FastLog.Loger(this).Error((object)"char flag = (char)this.DirectGetPropertyValue(property)", ex);
				return null;
			}
		}
		throw new NotSupportedException("Property type is not boolean");
	}

	public override string ToString()
	{
		return ID.ToString();
	}
}
