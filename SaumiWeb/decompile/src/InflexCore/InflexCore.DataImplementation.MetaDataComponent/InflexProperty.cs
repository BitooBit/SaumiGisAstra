using System;
using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;
using InflexCore.MetaData;

namespace InflexCore.DataImplementation.MetaDataComponent;

[InflexTableName(InflexTableOwners.Manual, "sys", "all_tab_columns")]
public class InflexProperty : IInflexProperty, IInflexMetaData
{
	private IInflexType _PropertyType;

	private bool _IsCalcPropertyType;

	[MapField("owner")]
	[PrimaryKey(1)]
	public string Owner { get; set; }

	[PrimaryKey(2)]
	[MapField("table_name")]
	public string TableName { get; set; }

	[MapField("column_name")]
	[PrimaryKey(3)]
	public string Name { get; set; }

	[MapField("data_length")]
	public decimal DataLength { get; set; }

	[MapField("data_precision")]
	public decimal? DataPrecision { get; set; }

	[MapField("data_scale")]
	public decimal? Scale { get; set; }

	[MapField("nullable")]
	public char Nullable { get; set; }

	[MapField("history")]
	public char? History { get; set; }

	[MapIgnore]
	public bool IsNullable => Nullable == 'Y';

	[MapIgnore]
	public int Length => decimal.ToInt32(DataPrecision.HasValue ? DataPrecision.Value : DataLength);

	[MapIgnore]
	public int Precision
	{
		get
		{
			if (!Scale.HasValue)
			{
				return 0;
			}
			return decimal.ToInt32(Scale.Value);
		}
	}

	public bool IsHistory
	{
		get
		{
			if (History.HasValue)
			{
				return History.Value == 'Y';
			}
			return false;
		}
	}

	[MapIgnore]
	public InflexMetaDataCollection Attributes { get; private set; }

	[MapField("data_type")]
	public string Type { get; set; }

	[MapIgnore]
	public IInflexType PropertyType
	{
		get
		{
			if (!_IsCalcPropertyType)
			{
				_IsCalcPropertyType = true;
				if (PropertyTypeID.HasValue)
				{
					if (PropertyTypeSubID.HasValue)
					{
						_PropertyType = AppManager.GetType(PropertyTypeID.Value, PropertyTypeSubID.Value);
					}
					else
					{
						_PropertyType = AppManager.GetType(PropertyTypeID.Value);
					}
				}
				else if (PropertyTypeDictID.HasValue)
				{
					_PropertyType = AppManager.GetDictType(PropertyTypeDictID.Value);
				}
				else
				{
					_PropertyType = null;
				}
			}
			return _PropertyType;
		}
	}

	[MapIgnore]
	public bool IsBoolean { get; set; }

	[MapIgnore]
	public bool IsString
	{
		get
		{
			switch (Type.ToUpper())
			{
			case "VARCHAR2":
			case "NVARCHAR2":
			case "LONG":
			case "CHAR":
				return !IsBoolean;
			default:
				return false;
			}
		}
	}

	[MapIgnore]
	public bool IsNumber
	{
		get
		{
			string text = Type.ToUpper();
			if (!(text == "NUMBER"))
			{
				return text == "FLOAT";
			}
			return true;
		}
	}

	[MapIgnore]
	public bool IsTime => Type.ToUpper() == "DATE";

	[MapIgnore]
	public bool IsID { get; set; }

	[MapIgnore]
	public IInflexType OwnerType { get; set; }

	[MapIgnore]
	public AppManager AppManager { get; set; }

	[MapIgnore]
	public decimal? PropertyTypeID { get; set; }

	[MapIgnore]
	public decimal? PropertyTypeSubID { get; set; }

	[MapIgnore]
	public decimal? PropertyTypeDictID { get; set; }

	public InflexProperty()
	{
		Attributes = new InflexMetaDataCollection();
		IsBoolean = false;
	}

	public void SetValue(IInflexObject inflexObject, object value)
	{
		if (inflexObject == null)
		{
			throw new NullReferenceException("Указанного инфлекс-объекта не существует!");
		}
		inflexObject.DirectSetPropertyValue(this, value);
	}

	public void ValidateSetValue(IInflexObject inflexObject, object value)
	{
		if (inflexObject == null)
		{
			throw new NullReferenceException("Указанного инфлекс-объекта не существует!");
		}
		inflexObject.ValidateSetPropertyValue(this, value);
	}

	public object GetValue(IInflexObject inflexObject)
	{
		if (inflexObject == null)
		{
			throw new NullReferenceException("Указанного инфлекс-объекта не существует!");
		}
		return inflexObject.DirectGetPropertyValue(this);
	}

	public IInflexObject GetInflexValue(IInflexObject inflexObject)
	{
		if (inflexObject == null)
		{
			throw new NullReferenceException("Указанного инфлекс-объекта не существует!");
		}
		return inflexObject.DirectGetInflexValue(this);
	}

	public bool? GetInflexBoolean(IInflexObject inflexObject)
	{
		if (inflexObject == null)
		{
			throw new NullReferenceException("Указанного инфлекс-объекта не существует!");
		}
		return inflexObject.DirectGetBoolean(this);
	}

	public IInflexID GetInflexID(IInflexObject inflexObject)
	{
		if (inflexObject == null)
		{
			throw new NullReferenceException("Указанного инфлекс-объекта не существует!");
		}
		return inflexObject.DirectGetInflexID(this);
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (object.ReferenceEquals(this, obj))
		{
			return true;
		}
		if ((object)obj.GetType() != typeof(InflexProperty))
		{
			return false;
		}
		return Equals((InflexProperty)obj);
	}

	public bool Equals(InflexProperty other)
	{
		if (object.ReferenceEquals(null, other))
		{
			return false;
		}
		if (object.ReferenceEquals(this, other))
		{
			return true;
		}
		if (object.Equals(other.Owner, Owner) && object.Equals(other.TableName, TableName))
		{
			return object.Equals(other.Name, Name);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = ((Owner != null) ? Owner.GetHashCode() : 0);
		num = (num * 397) ^ ((TableName != null) ? TableName.GetHashCode() : 0);
		return (num * 397) ^ ((Name != null) ? Name.GetHashCode() : 0);
	}
}
