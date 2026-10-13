using InflexCore;

namespace WinFormsExplorerWindow.InflexReportTree;

public class InflexReportTreeSource
{
	private readonly IInflexReport _Report;

	private readonly QueryColumnCollection _QueryColumns;

	public IInflexReport Report => _Report;

	public QueryColumnCollection QueryColumns => _QueryColumns;

	public InflexReportTreeSource(IInflexReport report, QueryColumnCollection columnCollection)
	{
		_Report = report;
		_QueryColumns = columnCollection;
	}
}
