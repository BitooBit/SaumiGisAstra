using System.Collections.Generic;
using System.Linq;

namespace InflexCore;

public class QueryColumnCollection : List<QueryColumn>
{
	public bool ContainsExplorer(IInflexColumn explorerColumn)
	{
		return this.Where((QueryColumn qc) => qc.ExplorerColumn == explorerColumn).FirstOrDefault() != null;
	}
}
