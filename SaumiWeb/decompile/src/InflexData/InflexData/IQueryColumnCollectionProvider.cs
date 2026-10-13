using System;
using InflexCore;

namespace InflexData;

public interface IQueryColumnCollectionProvider
{
	event Action<IInflexReport> QueryColumnsChange;

	QueryColumnCollection GetQueryColumns(IInflexReport report);
}
