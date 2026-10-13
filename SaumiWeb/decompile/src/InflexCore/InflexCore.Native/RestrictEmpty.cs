using System.Collections.Generic;

namespace InflexCore.Native;

public class RestrictEmpty : QueryRestriction
{
	public RestrictEmpty(IInflexReport report)
		: base(report)
	{
	}

	public override void OnBeforeCombineSql(ref List<string> partSelect, ref List<string> partFrom, ref List<string> partWhere, ref string partOrderBy, ref string partGroupBy)
	{
		partWhere.Add("1=2");
	}
}
