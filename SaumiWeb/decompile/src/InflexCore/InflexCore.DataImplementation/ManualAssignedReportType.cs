using System.Collections.Generic;

namespace InflexCore.DataImplementation;

public class ManualAssignedReportType : IInflexType
{
	private readonly IInflexReport _Report;

	private static int _NewID;

	private decimal _TypeID;

	private decimal _ID;

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

	public string Name => _Report.Name;

	public IInflexType Parent => null;

	public decimal? TypeID
	{
		get
		{
			return _TypeID;
		}
		set
		{
		}
	}

	public decimal? SubtypeID
	{
		get
		{
			return _Report.SubReportID;
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

	public IInflexReport AssignedReport => _Report;

	public ManualAssignedReportType(IInflexReport report, decimal typeID)
	{
		_Report = report;
		_ID = NewID();
		_TypeID = typeID;
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
