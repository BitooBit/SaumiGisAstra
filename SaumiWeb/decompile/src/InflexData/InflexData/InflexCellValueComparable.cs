using System;

namespace InflexData;

public class InflexCellValueComparable : InflexCellValue, IComparable
{
	public InflexCellValueComparable(object value)
		: base(value)
	{
	}

	public int CompareTo(object obj)
	{
		if (obj is InflexCellValueComparable inflexCellValueComparable)
		{
			IComparable comparable = Value as IComparable;
			IComparable comparable2 = inflexCellValueComparable.Value as IComparable;
			if (comparable != null)
			{
				if (comparable2 != null)
				{
					return comparable.CompareTo(inflexCellValueComparable.Value);
				}
				return 1;
			}
			if (comparable2 == null)
			{
				return 0;
			}
			return -1;
		}
		throw new InvalidOperationException("InflexCellValueComparable.CompareTo");
	}
}
