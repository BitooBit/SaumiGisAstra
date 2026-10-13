using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace InflexCore.DataImplementation;

public class InflexReportSqlBuilder
{
	private class ForeignColumn
	{
		public QueryColumn Column { get; set; }

		public string Name { get; set; }
	}

	private class LevelColumn : IInflexColumn
	{
		private readonly IInflexColumn _RootColumn;

		public IInflexReport Report => _RootColumn.Report;

		public decimal ID
		{
			get
			{
				return -1m;
			}
			set
			{
			}
		}

		public decimal? SeqN
		{
			get
			{
				return _RootColumn.SeqN;
			}
			set
			{
			}
		}

		public decimal DataType => -1m;

		public decimal? ObjectType
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public string ColumnName
		{
			get
			{
				return "level";
			}
			set
			{
			}
		}

		public string SrcSelect
		{
			get
			{
				return "level";
			}
			set
			{
			}
		}

		public string SrcFrom
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		public string SrcWhere
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		public bool ShowColumn => false;

		public decimal? ReferenceID
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public string ReferenceName
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		public bool Hidden => true;

		public bool SummFlag => false;

		public string Units
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		public int Accuracy => -1;

		public string GroupName => "";

		public LevelColumn(IInflexColumn rootColumn)
		{
			_RootColumn = rootColumn;
		}

		public bool CanShow()
		{
			return true;
		}

		public bool IsOuterReference()
		{
			return false;
		}

		public IInflexReport GetOuterReference()
		{
			return null;
		}

		public bool IsMain()
		{
			return false;
		}

		public bool IsOrder()
		{
			return false;
		}

		public bool IsGroupBy()
		{
			return false;
		}

		public bool IsPrimary()
		{
			return false;
		}

		public bool IsIndicator()
		{
			return false;
		}

		public bool IsRestriction()
		{
			return false;
		}

		public bool IsRootKey()
		{
			return false;
		}

		public bool IsSubtypeId()
		{
			return false;
		}

		public bool IsDate()
		{
			return false;
		}

		public bool IsDependentColumn(IInflexColumn column)
		{
			return false;
		}
	}

	public const string MAIN_TABLE_SYMBOL = "{MAIN_TABLE}";

	public const string RESTRICTION_SYMBOL = "{RESTRICTION}";

	private int _ColNo;

	private int _IndNo;

	private int _IndexT;

	private int _SelectIndex;

	public bool IncludePrimaryColumn { get; set; }

	public bool IncludeIndicatorColumn { get; set; }

	public bool IncludeSubtypeColumn { get; set; }

	public bool IncludeLastOrderByColumn { get; set; }

	public bool IncludeLastGroupByColumn { get; set; }

	public bool IncludeHierarchicalIfAvailable { get; set; }

	private string DefaultSchema { get; set; }

	public event BeforeCombineSqlHandler BeforeCombineSql;

	public InflexReportSqlBuilder(string defaultSchema)
	{
		DefaultSchema = defaultSchema;
		IncludePrimaryColumn = false;
		IncludeIndicatorColumn = false;
		IncludeSubtypeColumn = false;
		IncludeLastOrderByColumn = false;
		IncludeHierarchicalIfAvailable = false;
	}

	private string GetColumnName(IInflexColumn column)
	{
		string text = "";
		if (column.IsPrimary())
		{
			text = "PRIMARY_ID";
		}
		if (column.ReferenceName == "TYPE")
		{
			text = "TYPE_ID";
		}
		if (column.IsSubtypeId())
		{
			text = "SUBTYPE_ID";
		}
		if (column.IsIndicator())
		{
			text = "INDICATOR" + _IndNo;
			_IndNo++;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "SEL" + _ColNo;
			_ColNo++;
		}
		return text;
	}

	private IEnumerable<string> GetFrom(string srcFrom)
	{
		if (srcFrom == null)
		{
			srcFrom = "";
		}
		return from f in srcFrom.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries)
			select f.ToUpper();
	}

	private void ResetNumbers()
	{
		_ColNo = 0;
		_IndNo = 0;
		_IndexT = 0;
		_SelectIndex = 0;
	}

	public void BuildSql(QueryColumnCollection columns, IEnumerable<IInflexColumn> explorerColumns, IInflexColumn restrictColumn, string restrictSql, out string sql, out Dictionary<QueryColumn, SqlPosition> selectColumns)
	{
		ResetNumbers();
		selectColumns = new Dictionary<QueryColumn, SqlPosition>();
		List<string> partSelect = new List<string>();
		List<string> partFrom = new List<string>();
		List<string> partWhere = new List<string>();
		string partOrderBy = null;
		string partGroupBy = null;
		string partHierarchical = null;
		columns = CombineColumns(columns, restrictColumn);
		Dictionary<string, string> tables = CreateSQLParts(columns, explorerColumns, null, ref partSelect, ref partFrom, ref partWhere, ref partOrderBy, ref partGroupBy, ref partHierarchical, ref selectColumns);
		if (!string.IsNullOrEmpty(restrictSql) && restrictColumn != null)
		{
			if (restrictSql.Contains("{MAIN_TABLE}"))
			{
				IInflexReport inflexReport = restrictColumn.Report.RootReport ?? restrictColumn.Report;
				string tableName = inflexReport.AssignedType.TableName;
				string tableOwner = inflexReport.AssignedType.TableOwner;
				if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(tableOwner))
				{
					string text = (tableOwner + "." + tableName).ToUpper();
					if (string.IsNullOrEmpty(text))
					{
						throw new NotSupportedException();
					}
					restrictSql = restrictSql.Replace("{MAIN_TABLE}", text);
				}
			}
			foreach (IInflexColumn explorerColumn in explorerColumns)
			{
				if (explorerColumn == restrictColumn)
				{
					string text2 = ReplaceTables(explorerColumn.SrcSelect, tables);
					if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(restrictSql))
					{
						partWhere.Add(text2 + " in (" + restrictSql + ")");
					}
					break;
				}
			}
		}
		sql = CombineSQLParts(partSelect, partFrom, partWhere, partOrderBy, partGroupBy, partHierarchical);
	}

	private QueryColumnCollection CombineColumns(QueryColumnCollection columns, IInflexColumn restrictColumn)
	{
		if (restrictColumn == null)
		{
			return columns;
		}
		bool flag = false;
		QueryColumnCollection queryColumnCollection = new QueryColumnCollection();
		foreach (QueryColumn column in columns)
		{
			queryColumnCollection.Add(column);
			if (restrictColumn.Equals(column.ExplorerColumn))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			bool visible = (IncludePrimaryColumn && restrictColumn.IsPrimary()) || (IncludeIndicatorColumn && restrictColumn.IsIndicator()) || (IncludeSubtypeColumn && restrictColumn.IsSubtypeId());
			queryColumnCollection.Add(new QueryColumn(restrictColumn, visible));
		}
		return queryColumnCollection;
	}

	public void BuildSql(QueryColumnCollection columns, IEnumerable<IInflexColumn> explorerColumns, out string sql, out Dictionary<QueryColumn, SqlPosition> selectColumns)
	{
		ResetNumbers();
		selectColumns = new Dictionary<QueryColumn, SqlPosition>();
		List<string> partSelect = new List<string>();
		List<string> partFrom = new List<string>();
		List<string> partWhere = new List<string>();
		string partOrderBy = null;
		string partGroupBy = null;
		string partHierarchical = null;
		CreateSQLParts(columns, explorerColumns, null, ref partSelect, ref partFrom, ref partWhere, ref partOrderBy, ref partGroupBy, ref partHierarchical, ref selectColumns);
		sql = CombineSQLParts(partSelect, partFrom, partWhere, partOrderBy, partGroupBy, partHierarchical);
	}

	private string CombineSQLParts(List<string> partSelect, List<string> partFrom, List<string> partWhere, string partOrderBy, string partGroupBy, string partHierarchical)
	{
		OnBeforeCombineSql(ref partSelect, ref partFrom, ref partWhere, ref partOrderBy, ref partGroupBy);
		string text = "SELECT" + Environment.NewLine;
		bool flag = true;
		foreach (string item in partSelect)
		{
			if (!flag)
			{
				text += ", ";
			}
			flag = false;
			text = text + item + Environment.NewLine;
		}
		text = text + "FROM" + Environment.NewLine;
		List<string> list = partFrom.Distinct().ToList();
		flag = true;
		foreach (string item2 in list)
		{
			if (!flag)
			{
				text += ", ";
			}
			flag = false;
			text = text + item2 + Environment.NewLine;
		}
		if (partWhere.Count > 0)
		{
			text = text + "WHERE" + Environment.NewLine;
			flag = true;
			foreach (string item3 in partWhere)
			{
				if (!flag)
				{
					text += " and ";
				}
				flag = false;
				string text2 = text;
				text = text2 + "(" + item3 + ")" + Environment.NewLine;
			}
		}
		if (!string.IsNullOrEmpty(partGroupBy))
		{
			text = text + "GROUP BY " + partGroupBy + Environment.NewLine;
		}
		if (!string.IsNullOrEmpty(partHierarchical))
		{
			text = text + partHierarchical + Environment.NewLine;
		}
		if (!string.IsNullOrEmpty(partOrderBy))
		{
			text = ((!string.IsNullOrEmpty(partHierarchical) && string.IsNullOrEmpty(partGroupBy)) ? (text + "ORDER SIBLINGS BY " + partOrderBy + Environment.NewLine) : (text + "ORDER BY " + partOrderBy + Environment.NewLine));
		}
		return text;
	}

	private void OnBeforeCombineSql(ref List<string> partSelect, ref List<string> partFrom, ref List<string> partWhere, ref string partOrderBy, ref string partGroupBy)
	{
		BeforeCombineSql?.Invoke(ref partSelect, ref partFrom, ref partWhere, ref partOrderBy, ref partGroupBy);
	}

	private Dictionary<string, string> CreateSQLParts(QueryColumnCollection columns, IEnumerable<IInflexColumn> explorerColumns, ForeignColumn foreignColumn, ref List<string> partSelect, ref List<string> partFrom, ref List<string> partWhere, ref string partOrderBy, ref string partGroupBy, ref string partHierarchical, ref Dictionary<QueryColumn, SqlPosition> selectColumns)
	{
		QueryColumnCollection queryColumns = new QueryColumnCollection();
		foreach (QueryColumn column in columns)
		{
			if (foreignColumn == null)
			{
				if (column.ForeignColumn == null)
				{
					queryColumns.Add(column);
				}
			}
			else if ((column.ForeignColumn == null && foreignColumn.Column == null) || (column.ForeignColumn != null && column.ForeignColumn.Equals(foreignColumn.Column)))
			{
				queryColumns.Add(column);
			}
		}
		if (queryColumns.Count == 0)
		{
			return null;
		}
		explorerColumns = explorerColumns ?? queryColumns[0].ExplorerColumn.Report.GetExplorerColumns();
		List<IInflexColumn> neadColumns = GetNeadColumns(ref queryColumns, explorerColumns, IncludePrimaryColumn && foreignColumn == null, IncludeIndicatorColumn && foreignColumn == null, IncludeSubtypeColumn && foreignColumn == null, IncludeLastOrderByColumn && foreignColumn == null, IncludeLastGroupByColumn && foreignColumn == null, IncludeHierarchicalIfAvailable && foreignColumn == null, foreignColumn?.Column);
		Dictionary<string, string> replaceRules = new Dictionary<string, string>();
		List<string> tables = GetTables(neadColumns.Select((IInflexColumn c) => c.SrcFrom), ref replaceRules);
		partFrom.AddRange(tables);
		List<string> list = ReplaceTables(neadColumns.Select((IInflexColumn c) => c.SrcWhere), replaceRules);
		if (list.Count > 0)
		{
			partWhere.AddRange(list);
		}
		foreach (QueryColumn item in queryColumns.Where((QueryColumn c) => c.Visible && c.ExplorerColumn.CanShow()))
		{
			IInflexColumn explorerColumn = item.ExplorerColumn;
			string text = ReplaceTables(explorerColumn.SrcSelect, replaceRules);
			if (!string.IsNullOrEmpty(text))
			{
				if (explorerColumn.IsOrder())
				{
					partOrderBy = text;
					continue;
				}
				if (explorerColumn.IsGroupBy())
				{
					partGroupBy = text;
					continue;
				}
				string columnName = GetColumnName(explorerColumn);
				columnName = ((foreignColumn == null) ? columnName : (columnName + "_" + _SelectIndex));
				partSelect.Add(text + " " + columnName);
				selectColumns.Add(item, new SqlPosition(_SelectIndex, columnName, explorerColumn is LevelColumn));
				_SelectIndex++;
			}
		}
		if (IncludeHierarchicalIfAvailable && foreignColumn == null)
		{
			IInflexColumn inflexColumn = explorerColumns.Where((IInflexColumn c) => c.IsRootKey()).FirstOrDefault();
			if (inflexColumn != null)
			{
				string text2 = ReplaceTables(inflexColumn.SrcSelect, replaceRules);
				IInflexColumn inflexColumn2 = explorerColumns.Where((IInflexColumn c) => c.IsPrimary()).FirstOrDefault();
				if (inflexColumn2 != null && !string.IsNullOrEmpty(text2))
				{
					string text3 = ReplaceTables(inflexColumn2.SrcSelect, replaceRules);
					if (!string.IsNullOrEmpty(text3))
					{
						partHierarchical = string.Format("start with {0} is null connect by prior {1} = {0}", text2, text3);
					}
				}
			}
		}
		if (foreignColumn != null)
		{
			IInflexColumn inflexColumn3 = explorerColumns.Where((IInflexColumn c) => c.IsPrimary()).FirstOrDefault();
			if (inflexColumn3 != null)
			{
				string text4 = ReplaceTables(inflexColumn3.SrcSelect, replaceRules);
				if (!string.IsNullOrEmpty(text4))
				{
					Regex regex = new Regex("^[.\\w]+$");
					partWhere.Add(string.Format("{0}{1}={2}", text4, regex.IsMatch(text4) ? "(+)" : "", foreignColumn.Name));
				}
			}
		}
		foreach (QueryColumn item2 in queryColumns)
		{
			string text5 = ReplaceTables(item2.ExplorerColumn.SrcSelect, replaceRules);
			if (!string.IsNullOrEmpty(text5))
			{
				ForeignColumn foreignColumn2 = new ForeignColumn();
				foreignColumn2.Column = item2;
				foreignColumn2.Name = text5;
				ForeignColumn foreignColumn3 = foreignColumn2;
				CreateSQLParts(columns, null, foreignColumn3, ref partSelect, ref partFrom, ref partWhere, ref partOrderBy, ref partGroupBy, ref partHierarchical, ref selectColumns);
			}
		}
		return replaceRules;
	}

	private string ReplaceTables(string item, Dictionary<string, string> tables)
	{
		string text = item;
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		foreach (KeyValuePair<string, string> table in tables)
		{
			int length = table.Key.Length;
			int length2 = table.Value.Length;
			int startIndex = 0;
			while (true)
			{
				int num = text.ToUpper().IndexOf(table.Key, startIndex);
				if (num == -1)
				{
					break;
				}
				if (num > 0 && ('.' == text[num - 1] || char.IsLetterOrDigit(text[num - 1])))
				{
					startIndex = num + length;
					continue;
				}
				if (num + length == text.Length || text[num + length] != '.')
				{
					startIndex = num + length;
					continue;
				}
				text = text.Remove(num, length);
				text = text.Insert(num, table.Value);
				startIndex = num + length2;
			}
		}
		return text;
	}

	private List<string> ReplaceTables(IEnumerable<string> items, Dictionary<string, string> tables)
	{
		List<string> list = new List<string>();
		foreach (string item in items)
		{
			string text = ReplaceTables(item, tables);
			if (!string.IsNullOrEmpty(text))
			{
				list.Add(text);
			}
		}
		return list;
	}

	private List<string> GetTables(IEnumerable<string> fromParts, ref Dictionary<string, string> replaceRules)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string fromPart in fromParts)
		{
			if (string.IsNullOrEmpty(fromPart))
			{
				continue;
			}
			Regex regex = new Regex("\\s+(\\w+)\\s*$");
			Regex regex2 = new Regex("^\\s*([.\\w]+)");
			foreach (string item in GetFrom(fromPart))
			{
				string text = "";
				if (regex.IsMatch(item))
				{
					text = regex.Match(item).Groups[1].Value;
				}
				string text2 = "";
				if (regex2.IsMatch(item))
				{
					text2 = regex2.Match(item).Groups[1].Value;
				}
				if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(text2))
				{
					throw new NotSupportedException();
				}
				string text3 = NewTableName();
				if (!string.IsNullOrEmpty(text))
				{
					if (replaceRules.TryGetValue(text.ToUpper(), out var value))
					{
						text3 = value;
					}
					else
					{
						replaceRules.Add(text.ToUpper(), text3);
					}
					list.Add(regex.Replace(item, " " + text3));
				}
				if (!string.IsNullOrEmpty(text2))
				{
					text2 = StrongTable(text2, DefaultSchema);
					Dictionary<string, string> dictionary2 = (string.IsNullOrEmpty(text) ? replaceRules : dictionary);
					if (!dictionary2.ContainsKey(text2))
					{
						dictionary2.Add(text2, text3);
						dictionary2.Add(text2.Split(new char[1] { '.' })[1], text3);
					}
					if (string.IsNullOrEmpty(text) && !list2.Contains(text2))
					{
						list2.Add(text2);
						list.Add(text2 + " " + text3);
					}
				}
			}
		}
		foreach (KeyValuePair<string, string> item2 in dictionary)
		{
			if (!replaceRules.ContainsKey(item2.Key))
			{
				replaceRules.Add(item2.Key, item2.Value);
			}
		}
		return list;
	}

	private string StrongTable(string table, string defaultSchema)
	{
		table = table.ToUpper();
		if (table.Contains("."))
		{
			return table;
		}
		return defaultSchema.ToUpper() + "." + table;
	}

	private string NewTableName()
	{
		_IndexT++;
		return "t" + _IndexT;
	}

	private List<IInflexColumn> GetNeadColumns(ref QueryColumnCollection queryColumns, IEnumerable<IInflexColumn> explorerColumns, bool includePrimaryColumn, bool includeIndicatorColumn, bool includeSubtypeColumn, bool includeLastOrderColumn, bool includeLastGroupColumn, bool includeHierarchicalIfAvailable, QueryColumn foreignColumn)
	{
		IInflexColumn inflexColumn = null;
		IInflexColumn inflexColumn2 = null;
		List<IInflexColumn> list = new List<IInflexColumn>();
		foreach (QueryColumn queryColumn in queryColumns)
		{
			list.Add(queryColumn.ExplorerColumn);
		}
		foreach (IInflexColumn explorerColumn in explorerColumns)
		{
			if (queryColumns.ContainsExplorer(explorerColumn))
			{
				continue;
			}
			if ((includePrimaryColumn && explorerColumn.IsPrimary()) || (includeIndicatorColumn && explorerColumn.IsIndicator()) || (includeSubtypeColumn && explorerColumn.IsSubtypeId()))
			{
				list.Add(explorerColumn);
				queryColumns.Add(new QueryColumn(explorerColumn, visible: true)
				{
					ForeignColumn = foreignColumn
				});
				continue;
			}
			if (includeHierarchicalIfAvailable && explorerColumn.IsRootKey())
			{
				list.Add(explorerColumn);
				queryColumns.Add(new QueryColumn(explorerColumn, visible: true)
				{
					ForeignColumn = foreignColumn
				});
				LevelColumn levelColumn = new LevelColumn(explorerColumn);
				list.Add(levelColumn);
				queryColumns.Add(new QueryColumn(levelColumn, visible: true)
				{
					ForeignColumn = foreignColumn
				});
				continue;
			}
			if (includeLastOrderColumn && explorerColumn.IsOrder())
			{
				inflexColumn = explorerColumn;
			}
			if (includeLastGroupColumn && explorerColumn.IsGroupBy())
			{
				inflexColumn2 = explorerColumn;
			}
			if (explorerColumn.IsMain())
			{
				list.Add(explorerColumn);
			}
		}
		if (inflexColumn != null)
		{
			list.Add(inflexColumn);
			queryColumns.Add(new QueryColumn(inflexColumn, visible: true)
			{
				ForeignColumn = foreignColumn
			});
		}
		if (inflexColumn2 != null)
		{
			list.Add(inflexColumn2);
			queryColumns.Add(new QueryColumn(inflexColumn2, visible: true)
			{
				ForeignColumn = foreignColumn
			});
		}
		return list;
	}
}
