using System;

namespace InflexCore.Components;

public interface IInflexTransaction : IDisposable
{
	IInflexTransaction NewSubTansaction();

	IInflexObject Create(IInflexType inflexType);

	IInflexObject Select(IInflexID inflexID);

	bool IsNew(IInflexObject inflexObject);

	void Delete(IInflexObject inflexObject);

	void Delete(IInflexID inflexID);

	ValidateResult Validate();

	void Commit();

	void Rollback();
}
