using InflexCore;
using InflexCore.Native;

namespace InflexData.RestrictionQuery;

public class RestrictionDataQueryRestriction
{
	public IInflexColumn[] RestrictionColumns { get; private set; }

	public QueryRestriction Query { get; private set; }

	public RestrictionDataQueryRestriction(QueryRestriction query, params IInflexColumn[] restrictionColumns)
	{
		RestrictionColumns = restrictionColumns ?? new IInflexColumn[0];
		Query = query;
	}
}
