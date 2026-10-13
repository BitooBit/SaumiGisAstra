using InflexCore.Native.Action;

namespace InflexCore.Native.GW;

public interface IInflexGw
{
	IFindExGw FindEx { get; }

	IExplorerWindowGw ExplorerWindow { get; }

	bool AllowReportTreeSave { get; }

	bool AllowParseParamsSQL { get; }

	IDataHandler DataHandler { get; }

	IFastReport FastReport { get; }

	void ReportTreeSave(int reportID, string reportName, string restrictionSQL);

	bool ParseParamsSQL(string sql, out string resultSql);

	bool AllowLicense(int license);
}
