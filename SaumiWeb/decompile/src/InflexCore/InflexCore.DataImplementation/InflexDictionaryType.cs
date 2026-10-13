using System.Collections.Generic;
using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Accessors;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.InflexSchema, "dictionaries")]
public class InflexDictionaryType : SessionCacheObject<InflexDictionaryType>, IInflexType
{
	private InflexDictionaryReport _AssignedReport;

	private string _Name;

	private string _PrimaryColumn;

	[PrimaryKey]
	[MapField("id")]
	public decimal ID { get; set; }

	[MapIgnore]
	public string TableName { get; set; }

	[MapField("table_name")]
	public string FullTableName
	{
		get
		{
			return TableOwner + "." + TableName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				TableOwner = "";
				TableName = "";
				return;
			}
			string[] array = value.Split(new char[1] { '.' });
			int num = 0;
			if (array.Length == 2)
			{
				TableOwner = array[0];
				num = 1;
			}
			else
			{
				TableOwner = AppManager.CurrentUser;
			}
			TableName = array[num];
		}
	}

	[MapIgnore]
	public string Name
	{
		get
		{
			if (_Name == null)
			{
				_Name = AppManager.AccessorStatic<InflexTypeAccessor>().GetCommentForTable(TableOwner, TableName);
			}
			return _Name;
		}
	}

	[MapIgnore]
	public string TableOwner { get; set; }

	[MapIgnore]
	public IInflexType Parent => null;

	[MapIgnore]
	public decimal? TypeID
	{
		get
		{
			return ID;
		}
		set
		{
		}
	}

	[MapIgnore]
	public decimal? SubtypeID
	{
		get
		{
			return 0m;
		}
		set
		{
		}
	}

	[MapIgnore]
	public bool IsSubtype => false;

	[MapIgnore]
	public IEnumerable<IInflexType> Subtypes => new List<IInflexType>();

	[MapIgnore]
	public bool CanHistory => false;

	[MapIgnore]
	public bool IsInternalType => false;

	[MapIgnore]
	public bool UseInSearch => false;

	[MapIgnore]
	public decimal InSearchPosition => 0m;

	[MapIgnore]
	public string CreationName { get; set; }

	[MapIgnore]
	public string CreationRole { get; set; }

	[MapIgnore]
	public bool CanCompareAddress => false;

	[MapIgnore]
	public IInflexReport AssignedReport
	{
		get
		{
			if (_AssignedReport == null)
			{
				_AssignedReport = new InflexDictionaryReport(this);
			}
			return _AssignedReport;
		}
	}

	[MapField("display_column")]
	public string DisplayColumn { get; set; }

	[MapIgnore]
	public string PrimaryColumn
	{
		get
		{
			if (_PrimaryColumn == null)
			{
				_PrimaryColumn = AppManager.AccessorStatic<InflexDictionaryTypeAccessor>().GetPrimaryColumnForTable(TableOwner, TableName);
			}
			return _PrimaryColumn;
		}
	}

	[MapField("root_field")]
	public string RootColumn { get; set; }

	public bool IsTypecast(IInflexType type)
	{
		if (type == null)
		{
			return false;
		}
		return Equals(type);
	}

	protected override bool DirectEquals(InflexDictionaryType obj)
	{
		return ID == obj.ID;
	}

	protected override int DirectGetHashCode()
	{
		return ID.GetHashCode();
	}

	public override string ToString()
	{
		return FullTableName;
	}
}
