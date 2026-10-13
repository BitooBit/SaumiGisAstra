namespace InflexCore;

public interface IInflexID
{
	decimal ID { get; set; }

	IInflexType Type { get; set; }

	IInflexType NativeType { get; set; }
}
