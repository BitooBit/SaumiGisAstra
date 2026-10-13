using System.Collections.Generic;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.Accessors;

public abstract class StyleLegendAccessor : AppManager.InflexAccessorBase<StyleLegendItem, StyleLegendAccessor>
{
	private const string SqlGetStyleLegends = "select * from <T> where ReportID in (select ID from <SCHEMA>.ReportsTree start with ID = :reportId connect by prior INHERITED=ID)";

	[InflexSqlQuery("select * from <T> where ReportID in (select ID from <SCHEMA>.ReportsTree start with ID = :reportId connect by prior INHERITED=ID)")]
	public abstract IList<StyleLegendItem> GetStyleLegendItems(decimal reportId);
}
