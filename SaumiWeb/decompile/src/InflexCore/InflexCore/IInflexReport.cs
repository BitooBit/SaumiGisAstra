using System.Collections.Generic;

namespace InflexCore;

public interface IInflexReport
{
	decimal ID { get; set; }

	decimal SubReportID { get; }

	string SubReportName { get; }

	string Name { get; set; }

	IInflexReport ParentReport { get; }

	IInflexReport RootReport { get; }

	IInflexType AssignedType { get; }

	string PrepareReportProcedureText { get; }

	bool RollbackAfterPrepareReport { get; }

	Dictionary<decimal, string> SubReports { get; }

	IInflexReport GetSubReport(decimal subReportID);

	IEnumerable<IInflexColumn> GetExplorerColumns();

	IInflexColumn GetPrimaryColumn();

	IEnumerable<OuterReference> GetOuterReferenceReports();

	IEnumerable<Dictionary<IInflexColumn, object>> GetValues(IInflexColumn idColumn, decimal id, params IInflexColumn[] columns);

	IEnumerable<Dictionary<IInflexColumn, object>> GetValues(params IInflexColumn[] columns);

	Dictionary<IInflexColumn, object> GetValues(decimal id, params IInflexColumn[] columns);

	Dictionary<IInflexColumn, object> GetValues(IInflexID id, params IInflexColumn[] columns);

	decimal GetCountValues(IInflexColumn column, decimal id);

	IEnumerable<IStyleLegendItem> GetStyleLegends();
}
