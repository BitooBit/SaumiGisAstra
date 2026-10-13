using BLToolkit.Mapping;
using InflexCore.DataImplementation.Accessors;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation;

[InflexTableName(InflexTableOwners.InflexSchema, "stylelegend")]
public class StyleLegendItem : IStyleLegendItem
{
	private LazyOneLoader<InflexReport, InflexReportAccessor> _ReportLoader = new LazyOneLoader<InflexReport, InflexReportAccessor>();

	[MapField("reportid")]
	public decimal ReportId { get; set; }

	[MapField("attrib")]
	public decimal Value { get; set; }

	[MapField("info")]
	public string Info { get; set; }
}
