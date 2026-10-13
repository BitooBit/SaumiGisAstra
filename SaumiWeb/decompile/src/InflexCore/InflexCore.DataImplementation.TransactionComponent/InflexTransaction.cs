using System;
using System.Collections.Generic;
using System.Linq;
using InflexCore.Components;
using InflexCore.DataImplementation.TransactionComponent.MetaData;
using InflexCore.MetaData;

namespace InflexCore.DataImplementation.TransactionComponent;

public class InflexTransaction : InflexTransactionManager.AbstractInflexTransaction
{
	public InflexTransaction(InflexTransactionManager transactionManager)
		: base(transactionManager)
	{
	}

	protected override void DirectDispose()
	{
		RemoveTransaction();
		base.TransactionManager = null;
	}

	protected override InflexObject DirectCreate(IInflexType inflexType)
	{
		InflexObject inflexObject = CreateInstance(inflexType);
		return inflexObject.Branch(this);
	}

	protected override InflexObject DirectSelect(IInflexID inflexID)
	{
		InflexObject inflexObject = CreateInstance(inflexID);
		if (inflexObject != null)
		{
			return CreateRollbackCopy(inflexObject);
		}
		return null;
	}

	private InflexObject CreateRollbackCopy(InflexObject result)
	{
		result = result.Branch(this);
		result.Merge();
		result.SwitchToParent();
		return result;
	}

	public override ValidateResult Validate()
	{
		return new ValidateResult(isSuccess: true);
	}

	public override void Commit()
	{
		InnerDbAccessor innerDbAccessor = BeginDbTransaction();
		try
		{
			foreach (IInflexID deleteObject in base.DeleteObjects)
			{
				FullID fullID = GetFullID(innerDbAccessor, deleteObject);
				if (fullID != null)
				{
					Delete(innerDbAccessor, fullID);
				}
			}
			InflexObject[] array = base.Objects.Where((InflexObject o) => o.RuntimeAttributes.Get<TransactionNewMetaData>().IsNew).ToArray();
			InflexObject[] array2 = array;
			foreach (InflexObject inflexObject in array2)
			{
				if (inflexObject.RuntimeAttributes.Get<TransactionNewMetaData>().IsNew)
				{
					SaveNew(innerDbAccessor, inflexObject);
				}
			}
			InflexObject[] array3 = base.Objects.Where((InflexObject o) => o.RuntimeAttributes.Get<TransactionChangeMetaData>().IsChanged).ToArray();
			for (int num2 = 0; num2 < array3.Length; num2++)
			{
				InflexObject inflexObject2 = array3[num2];
				Save(innerDbAccessor, ref inflexObject2, isNew: false);
				inflexObject2.Merge();
				inflexObject2.SwitchToParent();
				ResetRuntimeAttributes(inflexObject2);
			}
			base.DeleteObjects.Clear();
			innerDbAccessor.CommitTransaction();
		}
		catch (Exception)
		{
			innerDbAccessor.RollbackTransaction();
			throw;
		}
	}

	private void SaveNew(InnerDbAccessor accessor, InflexObject inflexObject)
	{
		SaveNew(accessor, inflexObject, new HashSet<InflexObject>());
	}

	private void SaveNew(InnerDbAccessor accessor, InflexObject inflexObject, HashSet<InflexObject> stack)
	{
		if (stack.Contains(inflexObject))
		{
			throw new InvalidOperationException("Recursive save new");
		}
		stack.Add(inflexObject);
		foreach (KeyValuePair<IInflexProperty, object> item in inflexObject.DirectGetPropertyChangeValues())
		{
			if (item.Key.PropertyType != null)
			{
				IInflexID inflexID = inflexObject.DirectGetInflexID(item.Key);
				if (inflexID != null && base.IdObjects.TryGetValue(inflexID, out var value) && value != null && value.RuntimeAttributes.Get<TransactionNewMetaData>().IsNew)
				{
					SaveNew(accessor, value, stack);
				}
			}
		}
		stack.Remove(inflexObject);
		Save(accessor, ref inflexObject, isNew: true);
		inflexObject.Merge();
		inflexObject.SwitchToParent();
		ResetRuntimeAttributes(inflexObject);
	}
}
