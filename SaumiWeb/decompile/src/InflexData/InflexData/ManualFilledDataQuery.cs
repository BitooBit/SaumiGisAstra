using System;
using System.Collections.Generic;
using System.Linq;
using InflexCore;

namespace InflexData;

public class ManualFilledDataQuery : AbstractReportBasedDataQuery, ISupportHideRow
{
	private readonly IEnumerable<InflexDataColumn> _Columns;

	private readonly IEnumerable<object[]> _Rows;

	private IEnumerable<InflexQueryResultRow> _ResultRows;

	private readonly IInflexType _Type;

	private readonly HashSet<decimal> _HideIDs = new HashSet<decimal>();

	private readonly AppManager _AppManager;

	private readonly IQueryColumnCollectionProvider _QueryColumnsProvider;

	private IInflexReport _Report;

	private QueryColumnCollection _QueryColumns;

	public override IInflexDataQuery SubQuery => null;

	public override IEnumerable<InflexDataColumn> Columns => _Columns;

	public override string HumanyAdaptedRestrictionTextShort => "Данные отобраны в ручном режиме";

	public override string HumanyAdaptedRestrictionTextFull => HumanyAdaptedRestrictionTextShort;

	public override IInflexReport Report => _Report;

	public override QueryColumnCollection QueryColumns => _QueryColumns;

	public ManualFilledDataQuery(IEnumerable<object[]> rows, IEnumerable<InflexDataColumn> columns, IInflexType inflexType, AppManager appManager, IQueryColumnCollectionProvider queryColumnsProvider)
	{
		_Columns = columns;
		_Rows = rows;
		_Type = inflexType;
		_AppManager = appManager;
		_QueryColumnsProvider = queryColumnsProvider;
		CreateResultRowsIterator();
	}

	private void CreateResultRowsIterator()
	{
		int primaryColumnPosition = GetPrimaryColumnPosition(_Columns);
		List<InflexQueryResultRow> list = new List<InflexQueryResultRow>(_Rows.Count());
		foreach (object[] row in _Rows)
		{
			object obj = row[primaryColumnPosition];
			decimal result = 0m;
			if (obj == null || !decimal.TryParse(obj.ToString(), out result))
			{
				throw new NotSupportedException();
			}
			MockInflexID id = new MockInflexID(_Type, result);
			InflexQueryResultRow item = new InflexQueryResultRow(id, row);
			list.Add(item);
		}
		_ResultRows = list;
	}

	private int GetPrimaryColumnPosition(IEnumerable<InflexDataColumn> columns)
	{
		int num = 0;
		foreach (InflexDataColumn column in columns)
		{
			if (!((IGridColumn)column).IsPrimary)
			{
				num++;
				continue;
			}
			break;
		}
		return num;
	}

	protected override AppManager GetAppManager()
	{
		return _AppManager;
	}

	protected override IQueryColumnCollectionProvider GetQueryColumnsProvider()
	{
		return _QueryColumnsProvider;
	}

	public override IEnumerator<InflexQueryResultRow> GetEnumerator()
	{
		if (_HideIDs.Count > 0)
		{
			return _ResultRows.Where((InflexQueryResultRow row) => !_HideIDs.Contains(row.ID.ID)).GetEnumerator();
		}
		return _ResultRows.GetEnumerator();
	}

	public void SetReport(IInflexReport report)
	{
		_Report = report;
	}

	public void SetQueryColumns(QueryColumnCollection queryColumns)
	{
		_QueryColumns = queryColumns;
	}

	void ISupportHideRow.HideRow(IInflexID inflexID)
	{
		if (inflexID != null && _Type.Equals(inflexID.Type))
		{
			_HideIDs.Remove(inflexID.ID);
			_HideIDs.Add(inflexID.ID);
		}
	}
}
