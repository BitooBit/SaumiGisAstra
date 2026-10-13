using System.Collections.Generic;
using System.Linq;
using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Accessors;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.InflexSchema, "fm_tables")]
public class InflexType : SessionCacheObject<InflexType>, IInflexType
{
	private string _Name;

	private LazyOneLoader<InflexType, InflexTypeAccessor> _Parent = new LazyOneLoader<InflexType, InflexTypeAccessor>();

	private LazyListLoader<InflexType, InflexTypeAccessor> _Subtypes = new LazyListLoader<InflexType, InflexTypeAccessor>();

	private LazyOneLoader<InflexReport, InflexReportAccessor> _AssignedReport = new LazyOneLoader<InflexReport, InflexReportAccessor>();

	[PrimaryKey]
	[MapField("id")]
	public decimal ID { get; set; }

	[MapField("table_name")]
	public string TableName { get; set; }

	[MapIgnore]
	public string TableOwner
	{
		get
		{
			return AppManager.InflexSchema;
		}
		set
		{
		}
	}

	[MapIgnore]
	public string Name
	{
		get
		{
			if (_Name == null)
			{
				_Name = AppManager.AccessorStatic<InflexTypeAccessor>().GetCommentForTable(AppManager.InflexSchema, TableName);
			}
			return _Name;
		}
	}

	[MapIgnore]
	public IInflexType Parent
	{
		get
		{
			if (!TableR.HasValue)
			{
				return null;
			}
			return _Parent.Value(_Parent.Accessor.SelectParent, TableR.Value);
		}
	}

	[MapField("table_r")]
	public decimal? TableR { get; set; }

	[MapField("type_id")]
	public decimal? TypeID { get; set; }

	[MapField("subtype")]
	public decimal? SubtypeID { get; set; }

	[MapIgnore]
	public bool IsSubtype
	{
		get
		{
			decimal? subtypeID = SubtypeID;
			if (subtypeID.GetValueOrDefault() > 0m)
			{
				return subtypeID.HasValue;
			}
			return false;
		}
	}

	[MapIgnore]
	public IEnumerable<IInflexType> Subtypes => _Subtypes.List(_Subtypes.Accessor.SelectSubtypes, ID).Cast<IInflexType>();

	[MapIgnore]
	public bool CanHistory => DirectCanHistory == 'Y';

	[MapField("can_history")]
	public char DirectCanHistory { get; set; }

	[MapIgnore]
	public bool IsInternalType => DirectIsInternalType == 'Y';

	[MapField("internal_type")]
	public char DirectIsInternalType { get; set; }

	[MapIgnore]
	public bool UseInSearch => InSearchPosition > 0m;

	[MapIgnore]
	public decimal InSearchPosition
	{
		get
		{
			if (!DirectInSearchPosition.HasValue)
			{
				return 0m;
			}
			return DirectInSearchPosition.Value;
		}
	}

	[MapField("use_in_search")]
	public decimal? DirectInSearchPosition { get; set; }

	[MapField("creation_name")]
	public string CreationName { get; set; }

	[MapField("creation_role")]
	public string CreationRole { get; set; }

	[MapIgnore]
	public bool CanCompareAddress
	{
		get
		{
			if (DirectCanCompareAddress.HasValue)
			{
				return DirectCanCompareAddress.Value == 'Y';
			}
			return false;
		}
	}

	[MapField("can_compare_address")]
	protected char? DirectCanCompareAddress { get; set; }

	[MapIgnore]
	public IInflexReport AssignedReport
	{
		get
		{
			IInflexReport inflexReport = (TypeID.HasValue ? _AssignedReport.Value(_AssignedReport.Accessor.SelectByKey, TypeID.Value) : null);
			if (inflexReport != null && SubtypeID.HasValue)
			{
				return inflexReport.GetSubReport(SubtypeID.Value);
			}
			return inflexReport;
		}
	}

	public bool IsTypecast(IInflexType type)
	{
		if (type == null)
		{
			return false;
		}
		if (!Equals(type))
		{
			if (Parent != null)
			{
				return Parent.Equals(type);
			}
			return false;
		}
		return true;
	}

	protected override bool DirectEquals(InflexType obj)
	{
		return ID == obj.ID;
	}

	protected override int DirectGetHashCode()
	{
		return ID.GetHashCode();
	}

	public override string ToString()
	{
		return TableName;
	}
}
