using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

public class InflexDictionaryColumn : SessionCacheObject<InflexDictionaryColumn>, IInflexColumn
{
	private string _Comments;

	private string _ColumnName;

	private string _SrcSelect;

	[MapField("table_name")]
	public string TableName { get; set; }

	[MapField("owner")]
	public string Schema { get; set; }

	[MapIgnore]
	public IInflexReport Report { get; set; }

	[MapField("column_id")]
	public decimal ID { get; set; }

	[MapIgnore]
	public decimal? SeqN { get; set; }

	[MapIgnore]
	public decimal DataType => 0m;

	[MapIgnore]
	public decimal? ObjectType { get; set; }

	[MapField("comments")]
	public string Comments
	{
		get
		{
			return _Comments;
		}
		set
		{
			_Comments = value;
			if (string.IsNullOrEmpty(_Comments))
			{
				_ColumnName = string.Empty;
				return;
			}
			_ColumnName = _Comments.Split(new char[1] { '{' })[0];
		}
	}

	[MapIgnore]
	public string ColumnName
	{
		get
		{
			if (!string.IsNullOrEmpty(_ColumnName))
			{
				return _ColumnName;
			}
			return SrcSelect;
		}
		set
		{
		}
	}

	[MapField("full_column_name")]
	public string SrcSelect
	{
		get
		{
			if (_SrcSelect != null)
			{
				return _SrcSelect;
			}
			return DirectColumnName;
		}
		set
		{
			_SrcSelect = value;
		}
	}

	[MapField("column_name")]
	public string DirectColumnName { get; set; }

	[MapField("full_table_name")]
	public string SrcFrom { get; set; }

	[MapIgnore]
	public string SrcWhere { get; set; }

	[MapIgnore]
	public bool ShowColumn
	{
		get
		{
			string text = DirectColumnName.ToUpper();
			if (text == "NAME")
			{
				return true;
			}
			if (Report is InflexDictionaryReport inflexDictionaryReport)
			{
				return text == inflexDictionaryReport.DisplayColumn.ToUpper();
			}
			return false;
		}
	}

	[MapIgnore]
	public decimal? ReferenceID { get; set; }

	[MapIgnore]
	public string ReferenceName { get; set; }

	[MapIgnore]
	public bool Hidden => !ShowColumn;

	[MapIgnore]
	public bool SummFlag => false;

	[MapIgnore]
	public string Units { get; set; }

	[MapIgnore]
	public int Accuracy => 0;

	[MapIgnore]
	public string GroupName => string.Empty;

	protected override bool DirectEquals(InflexDictionaryColumn obj)
	{
		if (Schema == obj.Schema && TableName == obj.TableName)
		{
			return DirectColumnName == obj.DirectColumnName;
		}
		return false;
	}

	protected override int DirectGetHashCode()
	{
		return DirectColumnName.GetHashCode();
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
		if (!IsPrimary() && !ShowColumn)
		{
			return IsRootKey();
		}
		return true;
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
		string text = DirectColumnName.ToUpper();
		if (Report is InflexDictionaryReport inflexDictionaryReport && !string.IsNullOrEmpty(inflexDictionaryReport.PrimaryColumn))
		{
			return text == inflexDictionaryReport.PrimaryColumn.ToUpper();
		}
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
		string text = DirectColumnName.ToUpper();
		if (Report is InflexDictionaryReport { RootColumn: var rootColumn })
		{
			bool flag = string.IsNullOrEmpty(rootColumn);
			if (!flag || !(text == "ROOT"))
			{
				if (!flag)
				{
					return text == rootColumn.ToUpper();
				}
				return false;
			}
			return true;
		}
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
