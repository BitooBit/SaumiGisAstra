namespace InflexCore.Native.GW;

public interface IFindExGw
{
	bool AllowAsync { get; }

	int FindAll(int queryHandle, bool async, decimal reportID, ref string sql, ref string xml);

	int FindAll(int queryHandle, bool async, decimal reportID, string oldXml, ref string sql, ref string xml);
}
