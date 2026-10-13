using System;

namespace InflexCore;

public class InflexPeriod : IComparable<InflexPeriod>
{
	public InflexPeriodPrecision Precision { get; private set; }

	public DateTime? TimeBegin { get; set; }

	public DateTime? TimeEnd { get; set; }

	public bool IsBeginInfinity => !TimeBegin.HasValue;

	public bool IsBeginInfinityNegative { get; set; }

	public bool IsEndInfinity => !TimeEnd.HasValue;

	public bool IsEndInfinityPositive { get; set; }

	public DateTime? PrecisionTimeBegin => TimeBegin;

	public InflexPeriod()
		: this(InflexPeriodPrecision.Default)
	{
	}

	public InflexPeriod(InflexPeriodPrecision precision)
	{
		Precision = precision;
		IsBeginInfinityNegative = true;
		IsEndInfinityPositive = true;
	}

	public int CompareTo(InflexPeriod other)
	{
		if (IsBeginInfinity && IsBeginInfinityNegative && (!other.IsBeginInfinity || !other.IsBeginInfinityNegative))
		{
			return -1;
		}
		if (other.IsBeginInfinity && other.IsBeginInfinityNegative && (!IsBeginInfinity || !IsBeginInfinityNegative))
		{
			return 1;
		}
		if (IsBeginInfinity && !IsBeginInfinityNegative && !other.IsBeginInfinityNegative)
		{
			return 1;
		}
		if (other.IsBeginInfinity && !other.IsBeginInfinityNegative && !IsBeginInfinityNegative)
		{
			return -1;
		}
		if (!IsBeginInfinity && !other.IsBeginInfinity)
		{
			if (!(PrecisionTimeBegin < other.PrecisionTimeBegin))
			{
				if (!(PrecisionTimeBegin > other.PrecisionTimeBegin))
				{
					return 0;
				}
				return 1;
			}
			return -1;
		}
		return 0;
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
		if ((object)obj.GetType() != typeof(InflexPeriod))
		{
			return false;
		}
		return Equals((InflexPeriod)obj);
	}

	public bool Equals(InflexPeriod other)
	{
		if (object.ReferenceEquals(null, other))
		{
			return false;
		}
		if (object.ReferenceEquals(this, other))
		{
			return true;
		}
		if (object.Equals(other.Precision, Precision) && other.TimeBegin.Equals(TimeBegin) && other.TimeEnd.Equals(TimeEnd) && other.IsBeginInfinityNegative.Equals(IsBeginInfinityNegative))
		{
			return other.IsEndInfinityPositive.Equals(IsEndInfinityPositive);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hashCode = Precision.GetHashCode();
		hashCode = (hashCode * 397) ^ (TimeBegin.HasValue ? TimeBegin.Value.GetHashCode() : 0);
		hashCode = (hashCode * 397) ^ (TimeEnd.HasValue ? TimeEnd.Value.GetHashCode() : 0);
		hashCode = (hashCode * 397) ^ IsBeginInfinityNegative.GetHashCode();
		return (hashCode * 397) ^ IsEndInfinityPositive.GetHashCode();
	}

	public static InflexPeriod FullInfinity()
	{
		InflexPeriod inflexPeriod = new InflexPeriod();
		inflexPeriod.TimeBegin = null;
		inflexPeriod.TimeEnd = null;
		return inflexPeriod;
	}
}
