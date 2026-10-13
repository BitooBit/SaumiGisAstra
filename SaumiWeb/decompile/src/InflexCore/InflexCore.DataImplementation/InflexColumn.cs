using System;
using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Accessors;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.InflexSchema, "definecolumns_complex")]
public class InflexColumn : SessionCacheObject<InflexColumn>, IInflexColumn, ISessionCacheUpdateObject
{
	private LazyOneLoader<InflexReport, InflexReportAccessor> _Report = new LazyOneLoader<InflexReport, InflexReportAccessor>();

	private LazyOneLoader<InflexReport, InflexReportAccessor> _OuterReference = new LazyOneLoader<InflexReport, InflexReportAccessor>();

	private string _GroupName = string.Empty;

	private Random random = new Random();

	public IInflexReport Report
	{
		get
		{
			InflexReport inflexReport = _Report.Value(_Report.Accessor.SelectByKey, ReportID);
			if (ObjectType.HasValue)
			{
				return inflexReport.GetSubReport(ObjectType.Value);
			}
			return inflexReport;
		}
	}

	[PrimaryKey(1)]
	[MapField("id")]
	public decimal ID { get; set; }

	[MapField("report_id")]
	[PrimaryKey(2)]
	public decimal ReportID { get; set; }

	[MapField("seqn")]
	public decimal? SeqN { get; set; }

	[MapIgnore]
	public decimal DataType
	{
		get
		{
			if (!DirectDataType.HasValue)
			{
				return 0m;
			}
			return DirectDataType.Value;
		}
	}

	[MapField("datatype")]
	public decimal? DirectDataType { get; set; }

	[MapField("objecttype")]
	public decimal? ObjectType { get; set; }

	[MapField("columnname")]
	public string ColumnName { get; set; }

	[MapField("select_src")]
	public string SrcSelect { get; set; }

	[MapField("from_src")]
	public string SrcFrom { get; set; }

	[MapField("where_src")]
	public string SrcWhere { get; set; }

	[MapIgnore]
	public bool ShowColumn
	{
		get
		{
			if (!DirectVisible.HasValue)
			{
				if (DirectShowColumn.HasValue)
				{
					return DirectShowColumn.Value == 'Y';
				}
				return false;
			}
			return DirectVisible.Value == 'Y';
		}
	}

	[MapField("showcolumn")]
	public char? DirectShowColumn { get; set; }

	[MapField("preset_columns_visible")]
	public char? DirectVisible { get; set; }

	[MapField("reference_id")]
	public decimal? ReferenceID { get; set; }

	[MapField("reference_name")]
	public string ReferenceName { get; set; }

	[MapIgnore]
	public bool Hidden
	{
		get
		{
			if (DirectHidden.HasValue)
			{
				return DirectHidden.Value == 'Y';
			}
			return false;
		}
	}

	[MapField("hidden")]
	public char? DirectHidden { get; set; }

	[MapIgnore]
	public bool SummFlag
	{
		get
		{
			if (DirectSummFlag.HasValue)
			{
				return DirectSummFlag.Value == 'Y';
			}
			return false;
		}
	}

	[MapField("summflag")]
	public char? DirectSummFlag { get; set; }

	[MapField("units")]
	public string Units { get; set; }

	[MapIgnore]
	public int Accuracy
	{
		get
		{
			if (!DirectAccuracy.HasValue)
			{
				return 0;
			}
			return DirectAccuracy.Value;
		}
	}

	[MapField("accuracy")]
	public int? DirectAccuracy { get; set; }

	[MapIgnore]
	public string GroupName => _GroupName;

	bool ISessionCacheUpdateObject.IsUpdateSelf => true;

	public bool CanShow()
	{
		return !string.IsNullOrEmpty(SrcSelect);
	}

	public bool IsOuterReference()
	{
		if (ReferenceID.HasValue && ReferenceName != "PRIMARY")
		{
			return ReferenceName != "-";
		}
		return false;
	}

	public IInflexReport GetOuterReference()
	{
		if (!IsOuterReference())
		{
			return null;
		}
		return _OuterReference.Value(_OuterReference.Accessor.SelectByKey, ReferenceID.Value);
	}

	public bool IsMain()
	{
		if (string.IsNullOrEmpty(SrcSelect))
		{
			return string.IsNullOrEmpty(ReferenceName);
		}
		return false;
	}

	public bool IsOrder()
	{
		return ReferenceName == "ORDER BY";
	}

	public bool IsGroupBy()
	{
		return ReferenceName == "GROUP BY";
	}

	public bool IsPrimary()
	{
		if (ReferenceName == "PRIMARY")
		{
			return !string.IsNullOrEmpty(SrcSelect);
		}
		return false;
	}

	public bool IsIndicator()
	{
		if (ReferenceName == "INDICATOR")
		{
			return !string.IsNullOrEmpty(SrcSelect);
		}
		return false;
	}

	public bool IsRestriction()
	{
		return ReferenceName == "RESTRICTION";
	}

	public bool IsRootKey()
	{
		return false;
	}

	public bool IsSubtypeId()
	{
		if (ReferenceName == "SUBTYPE")
		{
			return !string.IsNullOrEmpty(SrcSelect);
		}
		return false;
	}

	public bool IsDate()
	{
		if (!(ReferenceName == "SEARCH.DATE"))
		{
			return ReferenceName == "DATE";
		}
		return true;
	}

	protected override bool DirectEquals(InflexColumn obj)
	{
		if (ID == obj.ID)
		{
			return ReportID == obj.ReportID;
		}
		return false;
	}

	protected override int DirectGetHashCode()
	{
		return ID.GetHashCode();
	}

	public bool IsDependentColumn(IInflexColumn column)
	{
		return false;
	}

	void ISessionCacheUpdateObject.UpdateSelf(object newObject)
	{
		if (newObject is InflexColumn inflexColumn)
		{
			ColumnName = inflexColumn.ColumnName;
			DirectAccuracy = inflexColumn.DirectAccuracy;
			DirectDataType = inflexColumn.DirectDataType;
			DirectHidden = inflexColumn.DirectHidden;
			DirectShowColumn = inflexColumn.DirectShowColumn;
			DirectSummFlag = inflexColumn.DirectSummFlag;
			DirectVisible = inflexColumn.DirectVisible;
			ID = inflexColumn.ID;
			ObjectType = inflexColumn.ObjectType;
			ReferenceID = inflexColumn.ReferenceID;
			ReferenceName = inflexColumn.ReferenceName;
			ReportID = inflexColumn.ReportID;
			SeqN = inflexColumn.SeqN;
			SrcFrom = inflexColumn.SrcFrom;
			SrcSelect = inflexColumn.SrcSelect;
			SrcWhere = inflexColumn.SrcWhere;
			Units = inflexColumn.Units;
		}
	}
}
