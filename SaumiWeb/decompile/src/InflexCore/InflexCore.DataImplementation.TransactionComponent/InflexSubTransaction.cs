using System.Linq;
using InflexCore.Components;
using InflexCore.DataImplementation.TransactionComponent.MetaData;

namespace InflexCore.DataImplementation.TransactionComponent;

public class InflexSubTransaction : InflexTransactionManager.AbstractInflexTransaction
{
	private InflexTransactionManager.AbstractInflexTransaction _ParentTransaction;

	public InflexSubTransaction(InflexTransactionManager transactionManager, InflexTransactionManager.AbstractInflexTransaction parentTransaction)
		: base(transactionManager)
	{
		_ParentTransaction = parentTransaction;
	}

	protected override void DirectDispose()
	{
		_ParentTransaction.FreeSubTransaction();
		_ParentTransaction = null;
	}

	protected override InflexObject DirectCreate(IInflexType inflexType)
	{
		InflexObject inflexObject = (InflexObject)_ParentTransaction.Create(inflexType);
		return inflexObject.Branch(this);
	}

	protected override InflexObject DirectSelect(IInflexID inflexID)
	{
		if (!(_ParentTransaction.Select(inflexID) is InflexObject inflexObject))
		{
			return null;
		}
		return inflexObject.Branch(this);
	}

	public override ValidateResult Validate()
	{
		return new ValidateResult(isSuccess: true);
	}

	public override void Commit()
	{
		InflexObject[] array = base.Objects.Where((InflexObject o) => o.RuntimeAttributes.Get<TransactionChangeMetaData>().IsChanged).ToArray();
		InflexObject[] array2 = array;
		foreach (InflexObject inflexObject in array2)
		{
			inflexObject.Merge();
			inflexObject.SwitchToParent();
			ResetRuntimeAttributes(inflexObject);
		}
		foreach (IInflexID deleteObject in base.DeleteObjects)
		{
			_ParentTransaction.Delete(deleteObject);
		}
		base.DeleteObjects.Clear();
	}

	protected override void OnRollbackNew(InflexObject inflexObject)
	{
		_ParentTransaction.Delete(inflexObject);
	}
}
