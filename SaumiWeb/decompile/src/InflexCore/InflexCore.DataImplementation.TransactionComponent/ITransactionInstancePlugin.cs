namespace InflexCore.DataImplementation.TransactionComponent;

public interface ITransactionInstancePlugin
{
	InflexObject CreateInstance(InflexTransactionManager.AbstractInflexTransaction transaction, IInflexID inflexID, InflexObject inflexObject);

	void AfterFill(ref InflexObject inflexObject);

	void TryFillChronologyInfo(ref InflexObject inflexObject);

	void AfterFillDefaultValue(ref InflexObject inflexObject);
}
