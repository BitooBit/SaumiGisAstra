using System;

namespace InflexData;

public class InflexCellValue : IConvertible
{
	public object Value;

	public int Indicator;

	public InflexCellValue(object value)
	{
		Value = value;
	}

	public override string ToString()
	{
		if (Value != null)
		{
			return Value.ToString();
		}
		return string.Empty;
	}

	public override bool Equals(object obj)
	{
		if (obj is InflexCellValue inflexCellValue)
		{
			if (Value != null)
			{
				return Value.Equals(inflexCellValue.Value);
			}
			return Value == inflexCellValue.Value;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		if (Value == null)
		{
			return base.GetHashCode();
		}
		return Value.GetHashCode();
	}

	TypeCode IConvertible.GetTypeCode()
	{
		return Convert.GetTypeCode(Value);
	}

	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		return Convert.ToBoolean(Value, provider);
	}

	byte IConvertible.ToByte(IFormatProvider provider)
	{
		return Convert.ToByte(Value, provider);
	}

	char IConvertible.ToChar(IFormatProvider provider)
	{
		return Convert.ToChar(Value, provider);
	}

	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		return Convert.ToDateTime(Value, provider);
	}

	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		return Convert.ToDecimal(Value, provider);
	}

	double IConvertible.ToDouble(IFormatProvider provider)
	{
		return Convert.ToDouble(Value, provider);
	}

	short IConvertible.ToInt16(IFormatProvider provider)
	{
		return Convert.ToInt16(Value, provider);
	}

	int IConvertible.ToInt32(IFormatProvider provider)
	{
		return Convert.ToInt32(Value, provider);
	}

	long IConvertible.ToInt64(IFormatProvider provider)
	{
		return Convert.ToInt64(Value, provider);
	}

	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		return Convert.ToSByte(Value, provider);
	}

	float IConvertible.ToSingle(IFormatProvider provider)
	{
		return Convert.ToSingle(Value, provider);
	}

	string IConvertible.ToString(IFormatProvider provider)
	{
		return Convert.ToString(Value, provider);
	}

	object IConvertible.ToType(Type conversionType, IFormatProvider provider)
	{
		return Convert.ChangeType(Value, conversionType, provider);
	}

	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		return Convert.ToUInt16(Value, provider);
	}

	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		return Convert.ToUInt32(Value, provider);
	}

	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		return Convert.ToUInt64(Value, provider);
	}
}
