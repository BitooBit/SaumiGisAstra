using System.Collections.Generic;
using System.Linq;
using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Accessors;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.InflexSchema, "reportstree")]
public class InflexReport : SessionCacheObject<InflexReport>, IInflexReport
{
	private readonly InflexReport _ParentReport;

	private readonly decimal _SubReportID;

	private string _SubReportName;

	private decimal _ID;

	private string _Name;

	private decimal? _InheritedReportID;

	private LazyOneLoader<InflexType, InflexTypeAccessor> _AssignedType = new LazyOneLoader<InflexType, InflexTypeAccessor>();

	private bool _SubReportsLoad;

	private Dictionary<decimal, string> _SubReports;

	private IInflexType _DirectAssignedType;

	[PrimaryKey]
	[MapField("id")]
	public decimal ID
	{
		get
		{
			if (_ParentReport == null)
			{
				return _ID;
			}
			return _ParentReport.ID;
		}
		set
		{
			_ID = value;
		}
	}

	[MapIgnore]
	public decimal SubReportID => _SubReportID;

	[MapIgnore]
	public string SubReportName
	{
		get
		{
			if (string.IsNullOrEmpty(_SubReportName))
			{
				_SubReportName = (from sr in SubReports
					where sr.Key == _SubReportID
					select sr.Value).FirstOrDefault();
			}
			return _SubReportName;
		}
	}

	[MapField("name")]
	public string Name
	{
		get
		{
			if (_ParentReport == null)
			{
				return _Name;
			}
			return _ParentReport._Name;
		}
		set
		{
			_Name = value;
		}
	}

	[MapField("call_proc")]
	public string PrepareReportProcedureText { get; set; }

	[MapValue(true, "Y")]
	[MapValue(false, "N")]
	[MapField("rb_retrieve")]
	public bool RollbackAfterPrepareReport { get; set; }

	[MapIgnore]
	public IInflexReport ParentReport => _ParentReport;

	[MapIgnore]
	public IInflexReport RootReport
	{
		get
		{
			if (_ParentReport != null)
			{
				if (_ParentReport.ParentReport != null)
				{
					return _ParentReport.RootReport;
				}
				return _ParentReport;
			}
			return null;
		}
	}

	[MapField("inherited")]
	public decimal? InheritedReportID
	{
		get
		{
			if (_ParentReport == null)
			{
				return _InheritedReportID;
			}
			return _ParentReport._InheritedReportID;
		}
		set
		{
			_InheritedReportID = value;
		}
	}

	[MapIgnore]
	public IInflexType AssignedType
	{
		get
		{
			if (_DirectAssignedType == null)
			{
				decimal num = (from c in GetExplorerColumns(_SubReportID, subTypeSpecific: false)
					where c.IsPrimary() && c.ReferenceID.HasValue
					select c).FirstOrDefault()?.ReferenceID.Value ?? (InheritedReportID.HasValue ? InheritedReportID.Value : ID);
				_DirectAssignedType = _AssignedType.Value(_AssignedType.Accessor.GeneralSelectByType, num, _SubReportID);
				if (_DirectAssignedType == null && RootReport != null)
				{
					_DirectAssignedType = RootReport.AssignedType;
				}
				if (_DirectAssignedType == null)
				{
					_DirectAssignedType = new ManualAssignedReportType(this, num);
				}
			}
			return _DirectAssignedType;
		}
	}

	[MapIgnore]
	public Dictionary<decimal, string> SubReports
	{
		get
		{
			if (!_SubReportsLoad)
			{
				try
				{
					InflexSubReportAccessor inflexSubReportAccessor = AppManager.AccessorStatic<InflexSubReportAccessor>();
					_SubReports = inflexSubReportAccessor.SelectSubReports(ID);
				}
				finally
				{
					_SubReportsLoad = true;
					if (_SubReports == null)
					{
						_SubReports = new Dictionary<decimal, string>();
					}
				}
			}
			return _SubReports;
		}
	}

	public InflexReport()
	{
		_SubReportID = 0m;
	}

	private InflexReport(InflexReport baseReport, decimal subReportID)
	{
		_ParentReport = baseReport;
		_SubReportID = subReportID;
	}

	public IInflexReport GetSubReport(decimal subReportID)
	{
		if (_ParentReport != null)
		{
			return _ParentReport.GetSubReport(subReportID);
		}
		if (subReportID == 0m)
		{
			return this;
		}
		if (SubReports.ContainsKey(subReportID))
		{
			return new InflexReport(this, subReportID);
		}
		throw new SubReportNotFoundException();
	}

	public IInflexColumn GetPrimaryColumn()
	{
		IEnumerable<IInflexColumn> enumerable = from c in GetExplorerColumns()
			where c.IsPrimary()
			select c;
		IEnumerator<IInflexColumn> enumerator = enumerable.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw new PrimaryColumnNotFoundException(this);
		}
		IInflexColumn current = enumerator.Current;
		if (enumerator.MoveNext())
		{
			throw new PrimaryColumnDuplicateException(this);
		}
		return current;
	}

	public IEnumerable<IInflexColumn> GetExplorerColumns()
	{
		return GetExplorerColumns(_SubReportID, subTypeSpecific: false);
	}

	private IEnumerable<IInflexColumn> GetExplorerColumns(decimal subReportID, bool subTypeSpecific)
	{
		InflexColumnAccessor inflexColumnAccessor = AppManager.AccessorStatic<InflexColumnAccessor>();
		return inflexColumnAccessor.GetExplorerColumns(ID, subReportID, subTypeSpecific).Cast<IInflexColumn>();
	}

	public IEnumerable<OuterReference> GetOuterReferenceReports()
	{
		InflexColumnAccessor inflexColumnAccessor = AppManager.AccessorStatic<InflexColumnAccessor>();
		IList<InflexColumn> columns = inflexColumnAccessor.SelectReferenceColumns(ID);
		return (from r in AppManager.GetReportsStatic()
			from c in columns
			where c.IsOuterReference() && r.ID == c.ReportID
			select new OuterReference
			{
				Column = c,
				Report = r
			}).ToArray();
	}

	public IEnumerable<Dictionary<IInflexColumn, object>> GetValues(IInflexColumn idColumn, decimal id, params IInflexColumn[] columns)
	{
		return new InflexReportHelper(AppManager.Instance, this).GetValues(idColumn, id, columns);
	}

	public IEnumerable<Dictionary<IInflexColumn, object>> GetValues(params IInflexColumn[] columns)
	{
		return new InflexReportHelper(AppManager.Instance, this).GetValues(columns);
	}

	public Dictionary<IInflexColumn, object> GetValues(decimal id, params IInflexColumn[] columns)
	{
		return new InflexReportHelper(AppManager.Instance, this).GetValues(id, columns);
	}

	public Dictionary<IInflexColumn, object> GetValues(IInflexID id, params IInflexColumn[] columns)
	{
		return new InflexReportHelper(AppManager.Instance, this).GetValues(id.ID, columns);
	}

	public decimal GetCountValues(IInflexColumn column, decimal id)
	{
		return new InflexReportHelper(AppManager.Instance, this).GetCountValues(column, id);
	}

	protected override bool DirectEquals(InflexReport obj)
	{
		if (ID == obj.ID)
		{
			return _SubReportID == obj._SubReportID;
		}
		return false;
	}

	protected override int DirectGetHashCode()
	{
		return ID.GetHashCode();
	}

	public IEnumerable<IStyleLegendItem> GetStyleLegends()
	{
		StyleLegendAccessor styleLegendAccessor = AppManager.AccessorStatic<StyleLegendAccessor>();
		return styleLegendAccessor.GetStyleLegendItems(ID).Cast<IStyleLegendItem>();
	}

	public override string ToString()
	{
		return string.Format("Report \"{0}\" (ID = {1}) with subreport \"{2}\" (ID = {3})", new object[4] { Name, ID, SubReportName, SubReportID });
	}
}
