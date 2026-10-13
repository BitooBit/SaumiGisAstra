namespace InflexCore.Native.Action;

public interface IFastReport
{
	void Preview(int reportID, params IInflexID[] items);

	void Print(int reportID, params IInflexID[] items);
}
