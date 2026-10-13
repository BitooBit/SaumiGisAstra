using System.Collections.Generic;
using InflexCore;

namespace WinFormsExplorerWindow.InflexReportTree.Utils;

internal class QueryColumnEqualityComparer : IEqualityComparer<QueryColumn>
{
	public bool Equals(QueryColumn x, QueryColumn y)
	{
		if (x == null && y == null)
		{
			return true;
		}
		if (x == null || y == null)
		{
			return false;
		}
		if (x.ExplorerColumn.ID == y.ExplorerColumn.ID && Equals(x.ForeignColumn, y.ForeignColumn))
		{
			return true;
		}
		return false;
	}

	public int GetHashCode(QueryColumn obj)
	{
		int num = 17;
		num = 37 * num + obj.ExplorerColumn.ID.GetHashCode();
		return 37 * num + GetHashCode(obj.ForeignColumn);
	}
}
