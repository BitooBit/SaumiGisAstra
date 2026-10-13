using System.Collections.Generic;
using System.Linq;
using InflexCore;

namespace WinFormsExplorerWindow.InflexReportTree.Utils;

internal class QueryColumnCollectionHelper
{
	private static readonly IEqualityComparer<QueryColumn> _Comparer = new QueryColumnEqualityComparer();

	public static bool GetColumnCheckedState(QueryColumn queryColumn, QueryColumnCollection queryColumnCollection)
	{
		return queryColumnCollection?.Contains(queryColumn, _Comparer) ?? false;
	}

	public static void SetCheckValue(QueryColumn queryColumn, QueryColumnCollection queryColumnCollection, bool checkedValue)
	{
		if (checkedValue)
		{
			if (!GetColumnCheckedState(queryColumn, queryColumnCollection))
			{
				queryColumnCollection.Add(queryColumn);
			}
		}
		else if (GetColumnCheckedState(queryColumn, queryColumnCollection))
		{
			QueryColumn item = queryColumnCollection.Where((QueryColumn c) => _Comparer.Equals(c, queryColumn)).FirstOrDefault();
			if (queryColumn != null)
			{
				queryColumnCollection.Remove(item);
			}
		}
	}
}
