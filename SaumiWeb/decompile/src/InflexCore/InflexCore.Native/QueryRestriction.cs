using System.Collections.Generic;

namespace InflexCore.Native;

public abstract class QueryRestriction
{
	public IInflexReport Report { get; set; }

	public QueryRestriction(IInflexReport report)
	{
		Report = report;
	}

	public virtual void OnBeforeCombineSql(ref List<string> partSelect, ref List<string> partFrom, ref List<string> partWhere, ref string partOrderBy, ref string partGroupBy)
	{
	}
}
