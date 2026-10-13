using InflexCore.MetaData;

namespace InflexCore.DataImplementation.MetaDataComponent;

public class InflexLinkProperty
{
	private readonly IInflexProperty _Property;

	public InflexLinkProperty(IInflexProperty property)
	{
		_Property = property;
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
		if ((object)obj.GetType() != typeof(InflexLinkProperty))
		{
			return false;
		}
		return Equals((InflexLinkProperty)obj);
	}

	public bool Equals(InflexLinkProperty other)
	{
		if (object.ReferenceEquals(null, other))
		{
			return false;
		}
		if (object.ReferenceEquals(this, other))
		{
			return true;
		}
		return object.Equals(other._Property, _Property);
	}

	public override int GetHashCode()
	{
		if (_Property == null)
		{
			return 0;
		}
		return _Property.GetHashCode();
	}
}
