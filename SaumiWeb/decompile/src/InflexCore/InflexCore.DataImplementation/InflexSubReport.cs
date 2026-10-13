using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.InflexSchema, "reportssubtypes")]
public class InflexSubReport
{
	[MapField("report_id")]
	public decimal ReportID { get; set; }

	[MapField("subtype_id")]
	public decimal SubTypeID { get; set; }

	[MapField("name")]
	public string Name { get; set; }
}
