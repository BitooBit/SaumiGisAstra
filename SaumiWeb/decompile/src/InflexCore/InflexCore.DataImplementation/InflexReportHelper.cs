using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace InflexCore.DataImplementation;

public class InflexReportHelper
{
	private readonly AppManager _AppManager;

	private readonly IInflexReport _Report;

	public InflexReportHelper(AppManager appManager, IInflexReport report)
	{
		_AppManager = appManager;
		_Report = report;
	}

	public IEnumerable<Dictionary<IInflexColumn, object>> GetValues(IInflexColumn idColumn, decimal id, params IInflexColumn[] columns)
	{
		if (columns == null || columns.Length == 0)
		{
			columns = GetReadyExplorerColumn().ToArray();
		}
		Dictionary<QueryColumn, SqlPosition> selectColumns = null;
		string sql = null;
		QueryColumnCollection queryColumnCollection = new QueryColumnCollection();
		queryColumnCollection.AddRange(columns.Select((IInflexColumn c) => new QueryColumn(c, visible: true)));
		InflexReportSqlBuilder inflexReportSqlBuilder = new InflexReportSqlBuilder(_AppManager.MainSchema);
		inflexReportSqlBuilder.BuildSql(queryColumnCollection, _Report.GetExplorerColumns(), idColumn, id.ToString(), out sql, out selectColumns);
		return ExecuteSQL(sql, columns, selectColumns);
	}

	public IEnumerable<Dictionary<IInflexColumn, object>> GetValues(params IInflexColumn[] columns)
	{
		if (columns == null || columns.Length == 0)
		{
			columns = GetReadyExplorerColumn().ToArray();
		}
		Dictionary<QueryColumn, SqlPosition> selectColumns = null;
		string sql = null;
		QueryColumnCollection queryColumnCollection = new QueryColumnCollection();
		queryColumnCollection.AddRange(columns.Select((IInflexColumn c) => new QueryColumn(c, visible: true)));
		InflexReportSqlBuilder inflexReportSqlBuilder = new InflexReportSqlBuilder(_AppManager.MainSchema);
		inflexReportSqlBuilder.BuildSql(queryColumnCollection, _Report.GetExplorerColumns(), out sql, out selectColumns);
		return ExecuteSQL(sql, columns, selectColumns);
	}

	public Dictionary<IInflexColumn, object> GetValues(decimal id, params IInflexColumn[] columns)
	{
		IInflexColumn idColumn = _Report.GetExplorerColumns().First((IInflexColumn c) => c.IsPrimary());
		return GetValues(idColumn, id, columns).FirstOrDefault();
	}

	public Dictionary<IInflexColumn, object> GetValues(IInflexID id, params IInflexColumn[] columns)
	{
		return GetValues(id.ID, columns);
	}

	public decimal GetCountValues(IInflexColumn column, decimal id)
	{
		Dictionary<QueryColumn, SqlPosition> selectColumns = null;
		string sql = null;
		InflexReportSqlBuilder inflexReportSqlBuilder = new InflexReportSqlBuilder(_AppManager.MainSchema);
		inflexReportSqlBuilder.BuildSql(GetReadyQueryColumnCollection(), _Report.GetExplorerColumns(), column, id.ToString(), out sql, out selectColumns);
		INativeSQLAccessor nativeSQLAccessor = _AppManager.NativeSQLAccessor;
		return nativeSQLAccessor.GetCountValues(sql);
	}

	private QueryColumnCollection GetReadyQueryColumnCollection()
	{
		QueryColumnCollection queryColumnCollection = new QueryColumnCollection();
		queryColumnCollection.AddRange(from c in GetReadyExplorerColumn()
			select new QueryColumn(c, visible: true));
		return queryColumnCollection;
	}

	private IEnumerable<IInflexColumn> GetReadyExplorerColumn()
	{
		return from c in _Report.GetExplorerColumns()
			where c.CanShow() && !string.IsNullOrEmpty(c.SrcSelect) && !c.SrcSelect.Contains("{")
			select c;
	}

	private IEnumerable<Dictionary<IInflexColumn, object>> ExecuteSQL(string sql, IEnumerable<IInflexColumn> columns, Dictionary<QueryColumn, SqlPosition> selectColumns)
	{
		List<Dictionary<IInflexColumn, object>> list = new List<Dictionary<IInflexColumn, object>>();
		INativeSQLAccessor nativeSQLAccessor = _AppManager.NativeSQLAccessor;
		using IDataReader dataReader = nativeSQLAccessor.ExecuteSQL(sql);
		if (dataReader.FieldCount != selectColumns.Count())
		{
			throw new Exception("Количество запрошеных колонок не соответсвует количеству полученных");
		}
		while (dataReader.Read())
		{
			int num = 0;
			Dictionary<IInflexColumn, object> dictionary = new Dictionary<IInflexColumn, object>();
			foreach (KeyValuePair<QueryColumn, SqlPosition> selectColumn in selectColumns)
			{
				if (columns.Contains(selectColumn.Key.ExplorerColumn))
				{
					dictionary.Add(selectColumn.Key.ExplorerColumn, dataReader.GetValue(num));
					num++;
				}
			}
			list.Add(dictionary);
		}
		return list;
	}
}
