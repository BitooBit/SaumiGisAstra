namespace InflexCore;

public class MockInflexID : IInflexID
{
	public decimal ID { get; set; }

	public IInflexType Type { get; set; }

	public IInflexType NativeType { get; set; }

	public MockInflexID(IInflexType type, decimal id)
	{
		Type = type;
		ID = id;
	}

	public MockInflexID(IInflexType type, decimal id, IInflexType nativeType)
	{
		Type = type;
		ID = id;
		NativeType = nativeType;
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
		IInflexID inflexID = obj as IInflexID;
		if (!object.ReferenceEquals(null, inflexID))
		{
			return Equals(inflexID);
		}
		return false;
	}

	public bool Equals(IInflexID other)
	{
		if (object.ReferenceEquals(null, other))
		{
			return false;
		}
		if (object.ReferenceEquals(this, other))
		{
			return true;
		}
		if (other.ID == ID)
		{
			return object.Equals(other.Type, Type);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (ID.GetHashCode() * 397) ^ ((Type != null) ? Type.GetHashCode() : 0);
	}

	public override string ToString()
	{
		return ID + " - " + Type.ToString();
	}
}
