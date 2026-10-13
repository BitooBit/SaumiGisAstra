using System;
using System.Data.Common;

namespace InflexCLR.Oracle;

public class Row
{
	private DbDataRecord _record;

	public DbDataRecord Record
	{
		get
		{
			return _record;
		}
		set
		{
			_record = value;
		}
	}

	public Row(DbDataRecord Record)
	{
		if (Record == null)
		{
			throw new Exception("Не правильный тип записи");
		}
		_record = Record;
	}

	public string FieldAsString(string FieldName)
	{
		int num = NameToIndex(FieldName);
		if (num >= 0)
		{
			if (_record.IsDBNull(num))
			{
				return "";
			}
			return _record.GetValue(num).ToString();
		}
		throw new Exception("Поле не найдено");
	}

	public object FieldAsObject(string FieldName)
	{
		int num = NameToIndex(FieldName);
		if (num >= 0)
		{
			if (_record.IsDBNull(num))
			{
				return null;
			}
			return _record.GetValue(num);
		}
		throw new Exception("Поле не найдено");
	}

	public decimal FieldAsNumber(string FieldName)
	{
		int num = NameToIndex(FieldName);
		if (num >= 0)
		{
			if (_record.IsDBNull(num))
			{
				return 0m;
			}
			return _record.GetDecimal(num);
		}
		throw new Exception("Поле не найдено");
	}

	private int NameToIndex(string FieldName)
	{
		FieldName = FieldName.ToLower();
		for (int i = 0; i < _record.FieldCount; i++)
		{
			if (_record.GetName(i).ToLower() == FieldName)
			{
				return i;
			}
		}
		return -1;
	}
}
