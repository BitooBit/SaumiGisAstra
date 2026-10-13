using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using BLToolkit.DataAccess;
using InflexCore;
using InflexCore.DataImplementation;
using InflexCore.DataImplementation.Accessors;
using InflexCore.Native;
using InflexCore.log;
using InflexUtils.ParamsResolver;

namespace InflexData.RestrictionQuery;

public class RestrictionDataQuery : AbstractReportBasedDataQuery, ISqlQuery
{
	private interface IDataQueryIteratorBuilder
	{
		IEnumerator<InflexQueryResultRow> Build();
	}

	private class EmptyDataQueryIteratorBuilder : IDataQueryIteratorBuilder
	{
		public IEnumerator<InflexQueryResultRow> Build()
		{
			return new List<InflexQueryResultRow>().GetEnumerator();
		}
	}

	private class IndicatorBuilder
	{
		private readonly int _Count;

		private readonly int _Index;

		private readonly int[] _Indicators;

		private int _IndicatorsLength;

		public IndicatorBuilder(int index, int[] indicators)
		{
			_Index = index;
			_Indicators = indicators;
			_IndicatorsLength = indicators.Length;
		}

		public object[] Build(RestrictionDataQueryIterator iterator, ref object[] values)
		{
			object[] array = new object[_Index + 1];
			int num = 0;
			int num2 = 0;
			int num3 = _Indicators[num2];
			for (int i = 0; i < _Index; i++)
			{
				array[i] = values[i];
				if (i == num3)
				{
					try
					{
						num |= decimal.ToInt32((decimal)values[i]);
					}
					catch (Exception ex)
					{
						num = 0;
						num2 = _IndicatorsLength;
						FastLog.Loger(this).Error((object)"public object[] Build(ref object[] values)", ex);
					}
					num2++;
					num3 = ((num2 >= _IndicatorsLength) ? (-1) : _Indicators[num2]);
				}
			}
			array[_Index] = num;
			return array;
		}
	}

	private class DataQueryIteratorBuilder : IDataQueryIteratorBuilder
	{
		private readonly RestrictionDataQuery _DataQuery;

		private readonly IInflexType _InflexType;

		private readonly PreQuery _PreQuery;

		private string _SQL;

		private readonly AppManager _AppManager;

		private readonly int _IDIndex;

		private readonly int _NativeTypeIdIndex;

		private bool _PrepareSQL;

		public IndicatorBuilder IndicatorBuilder { get; set; }

		public int IndicatorIndex { get; set; }

		private bool PrepareSQL()
		{
			if (!_PrepareSQL)
			{
				_PrepareSQL = _PreQuery.PrepareSql(ref _SQL);
			}
			return _PrepareSQL;
		}

		public DataQueryIteratorBuilder(RestrictionDataQuery dataQuery, IInflexType inflexType, PreQuery preQuery, string sql, int idIndex, int nativeTypeIdIndex)
		{
			_AppManager = dataQuery._AppManager;
			_DataQuery = dataQuery;
			_InflexType = inflexType;
			_PreQuery = preQuery;
			_SQL = sql;
			_PrepareSQL = false;
			_IDIndex = idIndex;
			_NativeTypeIdIndex = nativeTypeIdIndex;
		}

		public IEnumerator<InflexQueryResultRow> Build()
		{
			if (PrepareSQL())
			{
				Monitor.Enter(_LockObject);
				TempQueryAccessor tempQueryAccessor = _PreQuery.BeginTransaction();
				IInflexReport inflexReport = ((_DataQuery != null) ? _DataQuery.Report : null);
				if (inflexReport != null && !string.IsNullOrEmpty(inflexReport.PrepareReportProcedureText))
				{
					try
					{
						StoredProcedureAccessor storedProcedureAccessor = _AppManager.Accessor<StoredProcedureAccessor>();
						storedProcedureAccessor.ExecuteProcedure(inflexReport.PrepareReportProcedureText);
					}
					catch (Exception ex)
					{
						try
						{
							tempQueryAccessor.RollbackTransaction();
						}
						finally
						{
							Monitor.Exit(_LockObject);
						}
						throw new InvalidOracleProcedureException(ex.Message);
					}
				}
				TempRestrictionQueryAccessor tempRestrictionQueryAccessor = null;
				try
				{
					tempRestrictionQueryAccessor = _AppManager.Accessor<TempRestrictionQueryAccessor>();
					RestrictionDataQuery dataQuery = _DataQuery;
					if (dataQuery != null && dataQuery.Restriction is RestrictByQuery)
					{
						_PreQuery.ExecuteTempRestriction(tempRestrictionQueryAccessor, (RestrictByQuery)dataQuery.Restriction);
					}
				}
				catch (Exception)
				{
					try
					{
						tempRestrictionQueryAccessor?.RollbackTransaction();
					}
					finally
					{
						Monitor.Exit(_LockObject);
					}
					throw;
				}
				try
				{
					_PreQuery.Execute(tempQueryAccessor);
				}
				catch
				{
					try
					{
						tempQueryAccessor.RollbackTransaction();
					}
					finally
					{
						Monitor.Exit(_LockObject);
					}
					throw;
				}
				string sql = GetSql(isZeroResult: false);
				try
				{
					bool needCommitTransaction = inflexReport != null && !inflexReport.RollbackAfterPrepareReport;
					IDataReader dataReader = _AppManager.NativeSQLAccessor.ExecuteSQL(sql);
					if (IndicatorBuilder == null)
					{
						return new RestrictionDataQueryIterator(_DataQuery._Columns, _InflexType, dataReader, _IDIndex, _NativeTypeIdIndex, tempQueryAccessor, IndicatorIndex, needCommitTransaction);
					}
					return new RestrictionDataQueryIteratorCalcIndicator(_DataQuery._Columns, _InflexType, dataReader, _IDIndex, _NativeTypeIdIndex, tempQueryAccessor, IndicatorBuilder, needCommitTransaction);
				}
				catch (Exception innerException)
				{
					throw new InvalidSqlException(sql, innerException);
				}
			}
			return new RestrictionEmptyDataQueryIterator();
		}

		private string GetSql(bool isZeroResult)
		{
			return _SQL.Replace("1=1/*2D220D51-AFD0-488f-BEF3-913F6A86D53F*/", isZeroResult ? "1=2" : "1=1");
		}

		public Type[] GetColumnTypes()
		{
			if (PrepareSQL())
			{
				string sql = GetSql(isZeroResult: true);
				try
				{
					IDataReader dataReader = _AppManager.NativeSQLAccessor.ExecuteSQL(sql);
					using (dataReader)
					{
						Type[] array = new Type[dataReader.FieldCount];
						for (int i = 0; i < dataReader.FieldCount; i++)
						{
							array[i] = dataReader.GetFieldType(i);
						}
						return array;
					}
				}
				catch (Exception innerException)
				{
					throw new InvalidSqlException(sql, innerException);
				}
			}
			return null;
		}
	}

	private class RestrictionEmptyDataQueryIterator : IEnumerator<InflexQueryResultRow>, IDisposable, IEnumerator
	{
		public InflexQueryResultRow Current
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		object IEnumerator.Current => Current;

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			return false;
		}

		public void Reset()
		{
		}
	}

	private class RestrictionDataQueryIterator : IEnumerator<InflexQueryResultRow>, IDisposable, IEnumerator
	{
		protected readonly IInflexType _InflexType;

		protected readonly IDataReader _DataReader;

		protected readonly int _IDIndex;

		protected readonly int _NativeTypeIdIndex;

		private readonly DataAccessorBase _Accessor;

		private bool _First;

		protected InflexQueryResultRow _Current;

		private int _IndicatorIndex;

		protected List<bool> _IsComparableFields = new List<bool>();

		private static readonly Type _TypeOfIComparable = typeof(IComparable);

		private static readonly Type _TypeOfDecimal = typeof(decimal);

		private bool _IsErrorSummaryField;

		private List<int> _ErrorSummaryFields = new List<int>();

		private bool _NeedCommitTransaction;

		public virtual InflexQueryResultRow Current
		{
			get
			{
				if (_Current == null)
				{
					int fieldCount = _DataReader.FieldCount;
					object[] values = new object[fieldCount];
					_DataReader.GetValues(values);
					CheckAndEditSummaryValues(ref values);
					if (_IndicatorIndex >= 0)
					{
						decimal.ToInt32((decimal)values[_IndicatorIndex]);
					}
					decimal id = (decimal)values[_IDIndex];
					IInflexType nativeType = null;
					if (_NativeTypeIdIndex > 0)
					{
						decimal nativeTypeID = (decimal)values[_NativeTypeIdIndex];
						nativeType = _InflexType.Subtypes.Where(delegate(IInflexType subtype)
						{
							decimal? subtypeID = subtype.SubtypeID;
							decimal num = nativeTypeID;
							return subtypeID.GetValueOrDefault() == num && subtypeID.HasValue;
						}).FirstOrDefault();
					}
					_Current = new InflexQueryResultRow(new MockInflexID(_InflexType, id, nativeType), values);
				}
				return _Current;
			}
		}

		object IEnumerator.Current => Current;

		public RestrictionDataQueryIterator(List<InflexDataColumn> columns, IInflexType inflexType, IDataReader dataReader, int idIndex, int nativeTypeIdIndex, DataAccessorBase accessor, int indicatorIndex, bool needCommitTransaction)
		{
			_InflexType = inflexType;
			_DataReader = dataReader;
			_IDIndex = idIndex;
			_NativeTypeIdIndex = nativeTypeIdIndex;
			_Accessor = accessor;
			_IndicatorIndex = indicatorIndex;
			_First = true;
			_NeedCommitTransaction = needCommitTransaction;
			for (int i = 0; i < dataReader.FieldCount; i++)
			{
				Type fieldType = dataReader.GetFieldType(i);
				_IsComparableFields.Add(_TypeOfIComparable.IsAssignableFrom(fieldType));
				if (((IGridColumn)columns[i]).TotalSummMode != TotalSummModeEnum.None && (object)fieldType != _TypeOfDecimal)
				{
					_ErrorSummaryFields.Add(i);
				}
			}
			_IsErrorSummaryField = _ErrorSummaryFields.Count > 0;
		}

		protected void CheckAndEditSummaryValues(ref object[] values)
		{
			if (!_IsErrorSummaryField)
			{
				return;
			}
			for (int i = 0; i < _ErrorSummaryFields.Count; i++)
			{
				object obj = values[_ErrorSummaryFields[i]];
				if (obj == DBNull.Value || obj == null)
				{
					continue;
				}
				try
				{
					values[_ErrorSummaryFields[i]] = Convert.ToDecimal(obj);
				}
				catch
				{
					string text = obj.ToString();
					string text2 = string.Empty;
					int length = text.Length;
					bool flag = CultureInfo.CurrentCulture.NumberFormat.NegativeSign.Length > 0;
					char c = '-';
					if (flag)
					{
						c = CultureInfo.CurrentCulture.NumberFormat.NegativeSign[0];
					}
					for (int j = 0; j < length; j++)
					{
						text2 = ((!char.IsDigit(text[j]) && (!flag || text[j] != c)) ? (text2 + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) : (text2 + text[j]));
					}
					try
					{
						values[_ErrorSummaryFields[i]] = Convert.ToDecimal(text2);
					}
					catch
					{
						values[_ErrorSummaryFields[i]] = DBNull.Value;
					}
				}
			}
		}

		public void Dispose()
		{
			try
			{
				_DataReader.Dispose();
				if (_NeedCommitTransaction)
				{
					_Accessor.CommitTransaction();
				}
				else
				{
					_Accessor.RollbackTransaction();
				}
			}
			finally
			{
				Monitor.Exit(_LockObject);
			}
		}

		public bool MoveNext()
		{
			if (_DataReader.Read())
			{
				_Current = null;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			if (!_First)
			{
				throw new NotSupportedException();
			}
			_First = false;
			_Current = null;
		}

		public InflexCellValue GetCellValue(object value, int i)
		{
			if (_IsComparableFields[i])
			{
				return new InflexCellValueComparable(value);
			}
			return new InflexCellValue(value);
		}
	}

	private class RestrictionDataQueryIteratorCalcIndicator : RestrictionDataQueryIterator
	{
		private readonly IndicatorBuilder _IndicatorBuilder;

		public override InflexQueryResultRow Current
		{
			get
			{
				if (_Current == null)
				{
					object[] values = new object[_DataReader.FieldCount];
					_DataReader.GetValues(values);
					CheckAndEditSummaryValues(ref values);
					decimal id = (decimal)values[_IDIndex];
					IInflexType nativeType = null;
					if (_NativeTypeIdIndex > 0)
					{
						decimal nativeTypeID = (decimal)values[_NativeTypeIdIndex];
						nativeType = _InflexType.Subtypes.Where(delegate(IInflexType subtype)
						{
							decimal? subtypeID = subtype.SubtypeID;
							decimal num = nativeTypeID;
							return subtypeID.GetValueOrDefault() == num && subtypeID.HasValue;
						}).FirstOrDefault();
					}
					object[] values2 = _IndicatorBuilder.Build(this, ref values);
					_Current = new InflexQueryResultRow(new MockInflexID(_InflexType, id, nativeType), values2);
				}
				return _Current;
			}
		}

		public RestrictionDataQueryIteratorCalcIndicator(List<InflexDataColumn> columns, IInflexType inflexType, IDataReader dataReader, int idIndex, int nativeTypeIdIndex, DataAccessorBase accessor, IndicatorBuilder indicatorBuilder, bool needCommitTransaction)
			: base(columns, inflexType, dataReader, idIndex, nativeTypeIdIndex, accessor, -1, needCommitTransaction)
		{
			_IndicatorBuilder = indicatorBuilder;
		}
	}

	private class PreQuery
	{
		private readonly AppManager _AppManager;

		private readonly RestrictionDataQueryRestriction _Restriction;

		private RestrictByQuery _Query;

		private IInflexReport _Report;

		private string _Sql;

		private List<ParamSqlValue> _ParamValues = new List<ParamSqlValue>();

		public string Sql => _Sql;

		public int PrefetchCount { get; private set; }

		public QueryRestriction Restriction
		{
			get
			{
				if (_Query == null)
				{
					_Query = new ParamsRestrictByQuery(GetReport(), TempQueryAccessor.GetSelectPrimarySql(_AppManager));
				}
				return _Query;
			}
		}

		public PreQuery(AppManager appManager, RestrictionDataQueryRestriction restriction)
		{
			_AppManager = appManager;
			_Restriction = restriction;
		}

		public IInflexReport GetReport()
		{
			if (_Report == null)
			{
				IInflexReport report = _Restriction.Query.Report;
				List<IInflexReport> list = new List<IInflexReport>();
				IInflexColumn[] restrictionColumns = _Restriction.RestrictionColumns;
				foreach (IInflexColumn inflexColumn in restrictionColumns)
				{
					IInflexReport report2 = inflexColumn.Report;
					if (report2.RootReport != null && !list.Contains(report2))
					{
						list.Add(report2);
					}
				}
				if (list.Count == 1)
				{
					_Report = list[0];
				}
				else
				{
					GetSql(out var prepareResult);
					if (prepareResult)
					{
						lock (_LockObject)
						{
							TempQueryAccessor tempQueryAccessor = BeginTransaction();
							TempRestrictionQueryAccessor tempRestrictionQueryAccessor = null;
							try
							{
								tempRestrictionQueryAccessor = _AppManager.Accessor<TempRestrictionQueryAccessor>();
								if (_Restriction.Query is RestrictByQuery query)
								{
									ExecuteTempRestriction(tempRestrictionQueryAccessor, query);
								}
							}
							catch (Exception ex)
							{
								tempRestrictionQueryAccessor?.RollbackTransaction();
								throw ex;
							}
							try
							{
								Execute(tempQueryAccessor);
								IList<decimal?> list2 = tempQueryAccessor.SelectGroupSubtypesID();
								if (list2.Count == 1 && list2[0].HasValue)
								{
									try
									{
										_Report = report.GetSubReport(list2[0].Value);
									}
									catch (SubReportNotFoundException)
									{
										_Report = null;
									}
								}
							}
							finally
							{
								tempQueryAccessor.RollbackTransaction();
							}
						}
					}
				}
				if (_Report == null)
				{
					_Report = report;
				}
			}
			return _Report;
		}

		public void ExecuteTempRestriction(TempRestrictionQueryAccessor restrictionQueryAccessor, RestrictByQuery query)
		{
			try
			{
				restrictionQueryAccessor.Clear();
				if (query != null && query.RestrictionIDs != null)
				{
					decimal[] ids = query.RestrictionIDs.Select((IInflexID id) => id.ID).ToArray();
					restrictionQueryAccessor.Fill(ids);
				}
			}
			catch (Exception innerException)
			{
				throw new DataException("В базе данных отсутствует таблица \"sm_restriction_query\".", innerException);
			}
		}

		private string GetSql(out bool prepareResult)
		{
			prepareResult = true;
			if (string.IsNullOrEmpty(_Sql))
			{
				QueryRestriction query = _Restriction.Query;
				IInflexReport report = query.Report;
				IEnumerable<IInflexColumn> explorerColumns = report.GetExplorerColumns();
				RestrictByQuery restrictByQuery = query as RestrictByQuery;
				IInflexColumn inflexColumn = explorerColumns.FirstOrDefault((IInflexColumn c) => c.IsPrimary());
				if (inflexColumn == null)
				{
					throw new PrimaryColumnNotFoundException(report);
				}
				IInflexColumn inflexColumn2 = explorerColumns.FirstOrDefault((IInflexColumn c) => c.IsSubtypeId());
				if (inflexColumn2 == null)
				{
					inflexColumn2 = new ManualInflexColumn(report, "null", null, null);
				}
				QueryColumnCollection queryColumnCollection = new QueryColumnCollection();
				queryColumnCollection.Add(new QueryColumn(inflexColumn, visible: true));
				queryColumnCollection.Add(new QueryColumn(inflexColumn2, visible: true));
				QueryColumnCollection queryColumnCollection2 = queryColumnCollection;
				IInflexColumn[] restrictionColumns = _Restriction.RestrictionColumns;
				foreach (IInflexColumn explorerColumn in restrictionColumns)
				{
					queryColumnCollection2.Add(new QueryColumn(explorerColumn, visible: false));
				}
				if (_Restriction.Query is RestrictByQuery restrictByQuery2 && (!string.IsNullOrEmpty(restrictByQuery2.ManualFrom) || !string.IsNullOrEmpty(restrictByQuery2.ManualWhere)))
				{
					queryColumnCollection2.Add(new QueryColumn(new ManualInflexColumn(report, null, restrictByQuery2.ManualFrom, restrictByQuery2.ManualWhere), visible: true));
				}
				InflexReportSqlBuilder inflexReportSqlBuilder = new InflexReportSqlBuilder(_AppManager.MainSchema);
				string sql;
				try
				{
					inflexReportSqlBuilder.BeforeCombineSql += query.OnBeforeCombineSql;
					Dictionary<QueryColumn, SqlPosition> selectColumns;
					if (restrictByQuery != null)
					{
						inflexReportSqlBuilder.BuildSql(queryColumnCollection2, explorerColumns, restrictByQuery.RestrictionColumn, restrictByQuery.RestrictionQueryText, out sql, out selectColumns);
					}
					else
					{
						inflexReportSqlBuilder.BuildSql(queryColumnCollection2, explorerColumns, out sql, out selectColumns);
					}
				}
				finally
				{
					inflexReportSqlBuilder.BeforeCombineSql -= query.OnBeforeCombineSql;
				}
				prepareResult = PrepareSql(ref sql);
				_Sql = sql;
			}
			return _Sql;
		}

		public TempQueryAccessor BeginTransaction()
		{
			TempQueryAccessor tempQueryAccessor = _AppManager.Accessor<TempQueryAccessor>();
			tempQueryAccessor.UseTransaction();
			return tempQueryAccessor;
		}

		public void Execute(TempQueryAccessor accessor)
		{
			accessor.Clear();
			string sql = GetSql(out var prepareResult);
			if (prepareResult)
			{
				accessor.Fill(sql);
				PrefetchCount = (int)accessor.Count();
			}
		}

		public bool PrepareSql(ref string sql)
		{
			ParamsSqlResolver paramsSqlResolver = new ParamsSqlResolver(sql, _ParamValues.ToArray());
			if (!paramsSqlResolver.IsResolved() && !paramsSqlResolver.ShowResolveDialog())
			{
				throw new ParamsSqlResolverException();
			}
			sql = paramsSqlResolver.GetResolvedSql();
			_ParamValues.Clear();
			_ParamValues.AddRange(paramsSqlResolver.Params);
			return true;
		}

		public string[] GetDescriptionPreparedVariables(string sql)
		{
			ParamsSqlResolver paramsSqlResolver = new ParamsSqlResolver(sql, _ParamValues.ToArray());
			return paramsSqlResolver.GetDescriptionResolvedVariables();
		}
	}

	private class ManualInflexColumn : IInflexColumn
	{
		private readonly IInflexReport _Report;

		private readonly string _Select;

		private readonly string _From;

		private readonly string _Where;

		public IInflexReport Report => _Report;

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
				return -1m;
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
				return _Select;
			}
			set
			{
			}
		}

		public string SrcSelect
		{
			get
			{
				return _Select;
			}
			set
			{
			}
		}

		public string SrcFrom
		{
			get
			{
				return _From;
			}
			set
			{
			}
		}

		public string SrcWhere
		{
			get
			{
				return _Where;
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

		public ManualInflexColumn(IInflexReport report, string select, string from, string where)
		{
			_Report = report;
			_Select = select;
			_From = from;
			_Where = where;
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

		public bool IsDate()
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

		public bool IsDependentColumn(IInflexColumn column)
		{
			return false;
		}
	}

	private class ParamsRestrictByQuery : RestrictByQuery
	{
		public ParamsRestrictByQuery(IInflexReport report, string restrictionQueryText)
			: base(report, restrictionQueryText)
		{
		}

		public override void OnBeforeCombineSql(ref List<string> partSelect, ref List<string> partFrom, ref List<string> partWhere, ref string partOrderBy, ref string partGroupBy)
		{
			base.OnBeforeCombineSql(ref partSelect, ref partFrom, ref partWhere, ref partOrderBy, ref partGroupBy);
			partWhere.Insert(0, "1=1/*2D220D51-AFD0-488f-BEF3-913F6A86D53F*/");
		}
	}

	private const string _WhereParams = "1=1/*2D220D51-AFD0-488f-BEF3-913F6A86D53F*/";

	private readonly AppManager _AppManager;

	private RestrictionDataQueryRestriction _Restriction;

	private readonly IQueryColumnCollectionProvider _QueryColumnsProvider;

	private List<InflexDataColumn> _Columns;

	private QueryColumnCollection _QueryColumns;

	private string _QuerySqlText;

	private static readonly object _LockObject = new object();

	private IDataQueryIteratorBuilder _IteratorBuilder;

	private IInflexReport _Report;

	private bool _IsNewRestriction = true;

	private PreQuery _PreQuery;

	private readonly object _PrepareSync = new object();

	private bool _IsQueryPreparingCompleted;

	public int PrefetchCount
	{
		get
		{
			if (_PreQuery != null)
			{
				return _PreQuery.PrefetchCount;
			}
			return -1;
		}
	}

	public Exception LastPrepareException { get; private set; }

	private IDataQueryIteratorBuilder IteratorBuilder
	{
		get
		{
			if (_IsNewRestriction)
			{
				Prepare();
			}
			return _IteratorBuilder;
		}
	}

	private RestrictionDataQueryRestriction DataQueryRestriction
	{
		get
		{
			return _Restriction;
		}
		set
		{
			_Restriction = value;
			_IsNewRestriction = true;
		}
	}

	public QueryRestriction Restriction
	{
		get
		{
			if (_IsNewRestriction)
			{
				Prepare();
			}
			return _Restriction.Query;
		}
	}

	public override QueryColumnCollection QueryColumns
	{
		get
		{
			if (_IsNewRestriction)
			{
				Prepare();
			}
			return _QueryColumns;
		}
	}

	public override IInflexReport Report
	{
		get
		{
			if (_IsNewRestriction)
			{
				Prepare();
			}
			return _Report;
		}
	}

	public override IInflexDataQuery SubQuery => null;

	public override IEnumerable<InflexDataColumn> Columns
	{
		get
		{
			if (_IsNewRestriction)
			{
				Prepare();
			}
			return _Columns;
		}
	}

	public override string HumanyAdaptedRestrictionTextShort
	{
		get
		{
			if (Restriction is RestrictByQuery)
			{
				RestrictByQuery restrictByQuery = (RestrictByQuery)Restriction;
				if (restrictByQuery.IsFullQuery)
				{
					return "Все данные";
				}
				if (!string.IsNullOrEmpty(restrictByQuery.RestrictionXML.InnerXml))
				{
					return GetShortRestrictionText(restrictByQuery.RestrictionXML);
				}
				if (restrictByQuery.AdditionalRestrictionXmls.Count > 0)
				{
					return "Данные получены путем добавления условий";
				}
				if (restrictByQuery.RestrictionIDs != null)
				{
					return "Данные отобраны в ручном режиме";
				}
			}
			if (Restriction is RestrictEmpty)
			{
				return "Данные не запрашиваются";
			}
			return "Нет информации об ограничении для отображаемых данных";
		}
	}

	public override string HumanyAdaptedRestrictionTextFull
	{
		get
		{
			if (Restriction is RestrictByQuery)
			{
				RestrictByQuery restrictByQuery = (RestrictByQuery)Restriction;
				if (!restrictByQuery.IsFullQuery)
				{
					if (!string.IsNullOrEmpty(restrictByQuery.RestrictionXML.InnerXml))
					{
						return GetFullRestrictionText(restrictByQuery.RestrictionXML);
					}
					if (restrictByQuery.AdditionalRestrictionXmls.Count > 0)
					{
						StringBuilder stringBuilder = new StringBuilder();
						if (restrictByQuery.RestrictionIDs != null)
						{
							stringBuilder.AppendLine("Данные отобраны в ручном режиме");
							stringBuilder.AppendLine("+");
						}
						foreach (XmlDocument additionalRestrictionXml in restrictByQuery.AdditionalRestrictionXmls)
						{
							if (!string.IsNullOrEmpty(additionalRestrictionXml.InnerXml))
							{
								string fullRestrictionText = GetFullRestrictionText(additionalRestrictionXml);
								stringBuilder.AppendLine(fullRestrictionText);
								stringBuilder.AppendLine("+");
							}
						}
						return stringBuilder.Remove(stringBuilder.Length - 3, 3).ToString();
					}
				}
			}
			return HumanyAdaptedRestrictionTextShort;
		}
	}

	string ISqlQuery.QuerySqlText
	{
		get
		{
			if (_IsNewRestriction)
			{
				Prepare();
			}
			return _QuerySqlText;
		}
	}

	public RestrictionDataQuery(AppManager appManager, RestrictionDataQueryRestriction restriction, IQueryColumnCollectionProvider queryColumnsProvider)
	{
		_AppManager = appManager;
		DataQueryRestriction = restriction;
		_QueryColumnsProvider = queryColumnsProvider;
		_QueryColumnsProvider.QueryColumnsChange += OnQueryColumnsChange;
	}

	protected override AppManager GetAppManager()
	{
		return _AppManager;
	}

	protected override IQueryColumnCollectionProvider GetQueryColumnsProvider()
	{
		return _QueryColumnsProvider;
	}

	private string GetShortRestrictionText(XmlDocument restrictionXml)
	{
		XmlNodeList elementsByTagName = restrictionXml.GetElementsByTagName("Critery");
		XmlElement documentElement = restrictionXml.DocumentElement;
		XmlNode xmlNode = ((documentElement != null && documentElement.HasChildNodes) ? documentElement.FirstChild : null);
		string text = "";
		foreach (XmlNode item in elementsByTagName)
		{
			if ((xmlNode == null || xmlNode.IsChild(item)) && !string.IsNullOrEmpty(item.Attributes["AddRestrictionText"].Value))
			{
				text += string.Format("{0} = {1}, ", item.Attributes["Name"].Value.Trim(), item.Attributes["AddRestrictionText"].Value.Trim());
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = text.TrimEnd(new char[2] { ',', ' ' });
		}
		return text;
	}

	private string GetFullRestrictionText(XmlDocument restrictionXml)
	{
		XmlElement documentElement = restrictionXml.DocumentElement;
		XmlNode xmlNode = ((documentElement != null && documentElement.HasChildNodes) ? documentElement.FirstChild : null);
		XmlNodeList elementsByTagName = restrictionXml.GetElementsByTagName("Critery");
		List<string> list = new List<string>();
		string text = null;
		foreach (XmlNode item in elementsByTagName)
		{
			if ((xmlNode != null && !xmlNode.IsChild(item)) || string.IsNullOrEmpty(item.Attributes["AddRestrictionText"].Value))
			{
				continue;
			}
			Stack<string> stack = new Stack<string>();
			XmlNode xmlNode3 = item;
			while (xmlNode3.ParentNode != null && xmlNode3.ParentNode.Name != "FindNode" && xmlNode3.ParentNode.ParentNode != null && xmlNode3.ParentNode.ParentNode.Name != "FindNode")
			{
				xmlNode3 = xmlNode3.ParentNode;
				stack.Push(xmlNode3.Attributes["Name"].Value);
			}
			string text2 = "";
			string text3 = "";
			for (int i = 0; i < stack.Count; i++)
			{
				text2 += $"\n{new string(' ', (i + 1) * 4)}{stack.ElementAt(i)}: ";
				if (!list.Contains(text2))
				{
					list.Add(text2);
					text3 += $"\n{new string(' ', (i + 1) * 4)}{stack.ElementAt(i)}: ";
				}
			}
			text = text + text3 + string.Format("{0} = {1}, ", item.Attributes["Name"].Value.Trim(), item.Attributes["AddRestrictionText"].Value.Trim());
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = text.TrimEnd(new char[2] { ',', ' ' }).TrimStart(new char[1] { '\n' });
		}
		return text;
	}

	private void Prepare()
	{
		lock (_PrepareSync)
		{
			if (!_IsNewRestriction)
			{
				return;
			}
			LastPrepareException = null;
			_QuerySqlText = string.Empty;
			_IsQueryPreparingCompleted = false;
			try
			{
				_PreQuery = new PreQuery(_AppManager, _Restriction);
				IInflexReport inflexReport = null;
				try
				{
					inflexReport = _PreQuery.GetReport();
					if (_Restriction.Query.Report != inflexReport)
					{
						_Restriction = new RestrictionDataQueryRestriction(CloneRestriction(_Restriction.Query, inflexReport), _Restriction.RestrictionColumns);
					}
				}
				catch (Exception innerException)
				{
					throw new Exception("Ошибка при формировании предварительной выборки." + Environment.NewLine + _PreQuery.Sql, innerException);
				}
				_QueryColumnsProvider.QueryColumnsChange -= OnQueryColumnsChange;
				QueryColumnCollection queryColumns = _QueryColumnsProvider.GetQueryColumns(inflexReport);
				QueryColumnCollection queryColumnCollection = CloneQueryColumns(queryColumns);
				_QueryColumnsProvider.QueryColumnsChange += OnQueryColumnsChange;
				string text = null;
				Dictionary<QueryColumn, SqlPosition> selectColumns = null;
				IInflexColumn[] restrictionColumns = _Restriction.RestrictionColumns;
				foreach (IInflexColumn explorerColumn in restrictionColumns)
				{
					queryColumnCollection.Add(new QueryColumn(explorerColumn, visible: false));
				}
				if (_Restriction.Query is RestrictByQuery restrictByQuery && (!string.IsNullOrEmpty(restrictByQuery.ManualFrom) || !string.IsNullOrEmpty(restrictByQuery.ManualWhere)))
				{
					queryColumnCollection.Add(new QueryColumn(new ManualInflexColumn(inflexReport, null, restrictByQuery.ManualFrom, restrictByQuery.ManualWhere), visible: true));
				}
				string queryFormatText;
				if (_Restriction.Query is RestrictByQuery { RestrictionIDs: not null } restrictByQuery2)
				{
					queryFormatText = BuildSQL(queryColumnCollection, inflexReport.GetExplorerColumns(), _Restriction.Query, isIncludeIndicator: true, out selectColumns, needExpandRestriction: true);
					queryFormatText = CreateShowUserQueryText(restrictByQuery2, queryFormatText);
				}
				else
				{
					queryFormatText = BuildSQL(queryColumnCollection, inflexReport.GetExplorerColumns(), _Restriction.Query, isIncludeIndicator: true, out selectColumns, needExpandRestriction: false);
				}
				_QuerySqlText = queryFormatText;
				_IsQueryPreparingCompleted = true;
				text = BuildSQL(queryColumns, inflexReport.GetExplorerColumns(), _PreQuery.Restriction, isIncludeIndicator: true, out selectColumns);
				IInflexColumn primaryColumn = inflexReport.GetPrimaryColumn();
				SqlPosition sqlPosition = (from sc in selectColumns
					where sc.Key.ForeignColumn == null && sc.Key.ExplorerColumn == primaryColumn
					select sc.Value).FirstOrDefault();
				SqlPosition sqlPosition2 = (from sc in selectColumns
					where sc.Key.ExplorerColumn.IsSubtypeId()
					select sc.Value).FirstOrDefault();
				if (sqlPosition == null)
				{
					throw new PrimaryColumnNotFoundException(inflexReport);
				}
				DataQueryIteratorBuilder iteratorBuilder = new DataQueryIteratorBuilder(this, inflexReport.AssignedType, _PreQuery, text, sqlPosition.Index, sqlPosition2?.Index ?? (-1));
				List<InflexDataColumn> columns = new List<InflexDataColumn>();
				Type[] columnTypes = iteratorBuilder.GetColumnTypes();
				string[] descriptionPreparedVariables = _PreQuery.GetDescriptionPreparedVariables(queryFormatText);
				foreach (string text2 in descriptionPreparedVariables)
				{
					_QuerySqlText = _QuerySqlText + Environment.NewLine + "-- " + text2;
				}
				foreach (KeyValuePair<QueryColumn, SqlPosition> item in selectColumns)
				{
					KeyValuePair<QueryColumn, SqlPosition> columnLocal = item;
					QueryColumn queryColumn = queryColumns.Where((QueryColumn qc) => qc.Equals(columnLocal.Key)).FirstOrDefault();
					TotalSummModeEnum totalSummModeEnum = (item.Key.ExplorerColumn.SummFlag ? TotalSummModeEnum.Sum : TotalSummModeEnum.None);
					Type type = ((columnTypes == null) ? typeof(object) : columnTypes[item.Value.Index]);
					if (totalSummModeEnum != TotalSummModeEnum.None)
					{
						type = typeof(decimal);
					}
					InflexDataColumn inflexDataColumn = new InflexDataColumn(item.Value.Name, type);
					inflexDataColumn.SetHidden(queryColumn == null);
					inflexDataColumn.SetIsPrimary(item.Key.ExplorerColumn == primaryColumn);
					inflexDataColumn.SetIsIndicator(item.Key.ForeignColumn == null && item.Key.ExplorerColumn.IsIndicator());
					inflexDataColumn.SetIsDate(item.Key.ExplorerColumn.IsDate());
					inflexDataColumn.Caption = string.Empty;
					if (item.Key.ForeignColumn != null)
					{
						inflexDataColumn.SetIsNestedColumn(isNested: true);
					}
					QueryColumn queryColumn2 = item.Key;
					while (queryColumn2.ForeignColumn != null)
					{
						inflexDataColumn.Caption = queryColumn2.ForeignColumn.ExplorerColumn.ColumnName + " - " + inflexDataColumn.Caption;
						queryColumn2 = queryColumn2.ForeignColumn;
					}
					inflexDataColumn.Caption += item.Key.ExplorerColumn.ColumnName;
					inflexDataColumn.SetTotalSummMode(totalSummModeEnum);
					inflexDataColumn.SetColumnType(type);
					inflexDataColumn.SetUnits(item.Key.ExplorerColumn.Units);
					inflexDataColumn.SetAccuracy(item.Key.ExplorerColumn.Accuracy);
					columns.Add(inflexDataColumn);
				}
				PrepareIndicatorColumn(ref iteratorBuilder, ref columns);
				_QueryColumns = queryColumns;
				_Report = inflexReport;
				_Columns = columns;
				_IteratorBuilder = iteratorBuilder;
				_IsNewRestriction = false;
			}
			catch (Exception lastPrepareException)
			{
				LastPrepareException = lastPrepareException;
				_QueryColumns = new QueryColumnCollection();
				_Report = _Restriction.Query.Report;
				_Columns = new List<InflexDataColumn>(0);
				_IsNewRestriction = false;
				_IteratorBuilder = new EmptyDataQueryIteratorBuilder();
			}
		}
	}

	private string CreateShowUserQueryText(RestrictByQuery sqlQuery, string queryFormatText)
	{
		if (sqlQuery == null)
		{
			throw new ArgumentNullException("sqlQuery");
		}
		List<IInflexID> list = new List<IInflexID>(sqlQuery.RestrictionIDs);
		List<string> list2 = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			IInflexID inflexID = list[i];
			stringBuilder.Append(inflexID.ID);
			stringBuilder.Append(", ");
			num++;
			if (num >= 1000 || (num < 1000 && i == list.Count - 1))
			{
				list2.Add(queryFormatText.Replace("{RESTRICTION}", stringBuilder.ToString().TrimEnd(new char[2] { ',', ' ' })));
				num = 0;
				stringBuilder = new StringBuilder();
			}
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (string item in list2)
		{
			stringBuilder2.Append(item);
			stringBuilder2.Append(" union ");
		}
		int length = stringBuilder2.Length;
		if (length >= 7)
		{
			stringBuilder2.Remove(length - 7, 7);
		}
		return stringBuilder2.ToString();
	}

	private void PrepareIndicatorColumn(ref DataQueryIteratorBuilder iteratorBuilder, ref List<InflexDataColumn> columns)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < columns.Count; i++)
		{
			IGridColumn gridColumn = columns[i];
			if (gridColumn.IsIndicator)
			{
				list.Add(i);
			}
		}
		iteratorBuilder.IndicatorIndex = -1;
		if (list.Count == 1)
		{
			iteratorBuilder.IndicatorIndex = list[0];
		}
		if (list.Count <= 1)
		{
			return;
		}
		foreach (int item in list)
		{
			columns[item].SetIsIndicator(isIndicator: false);
		}
		InflexDataColumn inflexDataColumn = new InflexDataColumn("ABAE8E32-C0E8-4ba8-BE50-8372FD495F81", typeof(decimal));
		inflexDataColumn.Caption = "Indicator ABAE8E32-C0E8-4ba8-BE50-8372FD495F81";
		inflexDataColumn.SetHidden(hidden: true);
		inflexDataColumn.SetIsPrimary(isPrimary: false);
		inflexDataColumn.SetIsIndicator(isIndicator: true);
		inflexDataColumn.SetTotalSummMode(TotalSummModeEnum.None);
		inflexDataColumn.SetColumnType(typeof(decimal));
		inflexDataColumn.SetUnits(string.Empty);
		inflexDataColumn.SetAccuracy(0);
		columns.Add(inflexDataColumn);
		iteratorBuilder.IndicatorBuilder = new IndicatorBuilder(columns.Count - 1, list.ToArray());
	}

	private QueryRestriction CloneRestriction(QueryRestriction restriction, IInflexReport report)
	{
		if (restriction is RestrictByQuery restrictByQuery)
		{
			RestrictByQuery restrictByQuery2 = new RestrictByQuery(report, restrictByQuery.RestrictionColumn, restrictByQuery.RestrictionQueryText, restrictByQuery.RestrictionXMLText);
			restrictByQuery2.RestrictionIDs = restrictByQuery.RestrictionIDs;
			restrictByQuery2.AdditionalRestrictionXmls.AddRange(restrictByQuery.AdditionalRestrictionXmls);
			return restrictByQuery2;
		}
		return new RestrictEmpty(report);
	}

	private QueryColumnCollection CloneQueryColumns(QueryColumnCollection columns)
	{
		QueryColumnCollection queryColumnCollection = new QueryColumnCollection();
		Dictionary<QueryColumn, QueryColumn> dictionary = new Dictionary<QueryColumn, QueryColumn>();
		foreach (QueryColumn column in columns)
		{
			QueryColumn queryColumn = new QueryColumn(column.ExplorerColumn, column.Visible);
			queryColumnCollection.Add(queryColumn);
			dictionary.Add(column, queryColumn);
		}
		foreach (QueryColumn column2 in columns)
		{
			if (column2.ForeignColumn != null)
			{
				dictionary[column2].ForeignColumn = dictionary[column2.ForeignColumn];
			}
		}
		return queryColumnCollection;
	}

	private void OnQueryColumnsChange(IInflexReport report)
	{
		if (_Report == report)
		{
			_IsNewRestriction = true;
		}
	}

	private string BuildSQL(QueryColumnCollection queryColumns, IEnumerable<IInflexColumn> explorerColumns, QueryRestriction query, bool isIncludeIndicator, out Dictionary<QueryColumn, SqlPosition> selectColumns)
	{
		return BuildSQL(queryColumns, explorerColumns, query, isIncludeIndicator, out selectColumns, needExpandRestriction: false);
	}

	private string BuildSQL(QueryColumnCollection queryColumns, IEnumerable<IInflexColumn> explorerColumns, QueryRestriction query, bool isIncludeIndicator, out Dictionary<QueryColumn, SqlPosition> selectColumns, bool needExpandRestriction)
	{
		InflexReportSqlBuilder inflexReportSqlBuilder = new InflexReportSqlBuilder(_AppManager.MainSchema);
		inflexReportSqlBuilder.IncludeIndicatorColumn = isIncludeIndicator;
		inflexReportSqlBuilder.IncludePrimaryColumn = true;
		inflexReportSqlBuilder.IncludeSubtypeColumn = true;
		inflexReportSqlBuilder.IncludeLastGroupByColumn = true;
		inflexReportSqlBuilder.IncludeLastOrderByColumn = true;
		InflexReportSqlBuilder inflexReportSqlBuilder2 = inflexReportSqlBuilder;
		string sql;
		try
		{
			RestrictByQuery restrictByQuery = query as RestrictByQuery;
			inflexReportSqlBuilder2.BeforeCombineSql += OnBeforeCombineSql;
			inflexReportSqlBuilder2.BeforeCombineSql += query.OnBeforeCombineSql;
			if (restrictByQuery != null)
			{
				string tempRestrictionQueryText = TempRestrictionQueryAccessor.GetTempRestrictionQueryText(_AppManager);
				string text = restrictByQuery.RestrictionQueryText;
				if (needExpandRestriction)
				{
					text = text.Replace(tempRestrictionQueryText, "{RESTRICTION}");
				}
				inflexReportSqlBuilder2.BuildSql(queryColumns, explorerColumns, restrictByQuery.RestrictionColumn, text, out sql, out selectColumns);
			}
			else
			{
				inflexReportSqlBuilder2.BuildSql(queryColumns, explorerColumns, out sql, out selectColumns);
			}
		}
		finally
		{
			inflexReportSqlBuilder2.BeforeCombineSql -= query.OnBeforeCombineSql;
			inflexReportSqlBuilder2.BeforeCombineSql -= OnBeforeCombineSql;
		}
		return sql;
	}

	private void OnBeforeCombineSql(ref List<string> partselect, ref List<string> partfrom, ref List<string> partwhere, ref string partorderby, ref string partgroupby)
	{
		if (_IsQueryPreparingCompleted)
		{
			return;
		}
		UserPrivilegeObjectAccessor userPrivilegeObjectAccessor = _AppManager.Accessor<UserPrivilegeObjectAccessor>();
		string pattern = "[\\w]*[\\s]*[.][\\s]*(?<tableName>[\\w]*)[\\s]+";
		Regex regex = new Regex(pattern, RegexOptions.Compiled);
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string item in partfrom)
		{
			if (regex.IsMatch(item))
			{
				string text = regex.Match(item).Result("${tableName}").Trim()
					.ToUpper();
				IEnumerable<IUserPrivilegeObject> privilegesForTable = userPrivilegeObjectAccessor.GetPrivilegesForTable(_AppManager.MainSchema, text);
				if (!privilegesForTable.Any((IUserPrivilegeObject p) => p.Privelege == "VIEW" && p.GrantableByUserman == true) && privilegesForTable.Any((IUserPrivilegeObject p) => p.Privelege != "VIEW" && p.GrantableByUserman == true) && !hashSet.Contains(text))
				{
					hashSet.Add(text);
				}
			}
		}
		if (hashSet.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder("Извините, у Вас отсутствуют права на просмотр содержимого следующих таблиц, используемых в запросе:");
		stringBuilder.AppendLine();
		foreach (string item2 in hashSet)
		{
			stringBuilder.AppendLine(item2);
		}
		stringBuilder.AppendLine("Отсутствие этих прав делает невозможным отображение результата запроса.");
		stringBuilder.AppendLine("Для снятия ограничений обратитесь к администратору.");
		throw new Exception(stringBuilder.ToString());
	}

	public override IEnumerator<InflexQueryResultRow> GetEnumerator()
	{
		try
		{
			return IteratorBuilder.Build();
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"return this.IteratorBuilder.Build()", ex);
			LastPrepareException = ex;
			return new EmptyDataQueryIteratorBuilder().Build();
		}
	}
}
