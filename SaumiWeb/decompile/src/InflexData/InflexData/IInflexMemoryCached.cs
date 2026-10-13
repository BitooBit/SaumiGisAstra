namespace InflexData;

public interface IInflexMemoryCached
{
	void Read();

	bool CheckRelevancy();

	void Synchronize();
}
