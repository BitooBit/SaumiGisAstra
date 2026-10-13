using System.Collections.Generic;
using InflexCore.DataImplementation;
using InflexCore.DataImplementation.Accessors;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.Native;

public class ManualItemDataInflexType : IInflexType
{
	private readonly ItemData _ItemData;

	private LazyOneLoader<InflexReport, InflexReportAccessor> _AssignedReport = new LazyOneLoader<InflexReport, InflexReportAccessor>();

	private decimal _ID;

	private static int _NewID;

	public decimal ID
	{
		get
		{
			return _ID;
		}
		set
		{
		}
	}

	public string TableName
	{
		get
		{
			return "";
		}
		set
		{
		}
	}

	public string TableOwner
	{
		get
		{
			return "";
		}
		set
		{
		}
	}

	public string Name => _ItemData.ObjTypeTxt;

	public IInflexType Parent => null;

	public decimal? TypeID
	{
		get
		{
			return _ItemData.ObjType;
		}
		set
		{
		}
	}

	public decimal? SubtypeID
	{
		get
		{
			return _ItemData.ObjSubType;
		}
		set
		{
		}
	}

	public bool IsSubtype => false;

	public IEnumerable<IInflexType> Subtypes => new List<IInflexType>();

	public bool CanHistory => false;

	public bool IsInternalType => false;

	public bool UseInSearch => false;

	public decimal InSearchPosition => 0m;

	public string CreationName
	{
		get
		{
			return "";
		}
		set
		{
		}
	}

	public string CreationRole
	{
		get
		{
			return "";
		}
		set
		{
		}
	}

	public bool CanCompareAddress => false;

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

	public ManualItemDataInflexType(ItemData itemData)
	{
		_ItemData = itemData;
		_ID = NewID();
	}

	private static decimal NewID()
	{
		_NewID--;
		return _NewID;
	}

	public bool IsTypecast(IInflexType type)
	{
		if (type == null)
		{
			return false;
		}
		return Equals(type);
	}

	public override bool Equals(object obj)
	{
		if (obj is IInflexType inflexType && TypeID == inflexType.TypeID)
		{
			return SubtypeID == inflexType.SubtypeID;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = decimal.ToInt32(TypeID.HasValue ? TypeID.Value : 0m);
		int num2 = decimal.ToInt32(SubtypeID.HasValue ? SubtypeID.Value : 0m);
		return num ^ num2;
	}
}
