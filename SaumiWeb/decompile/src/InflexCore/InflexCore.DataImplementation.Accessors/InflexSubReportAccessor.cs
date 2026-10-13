using System.Collections.Generic;
using BLToolkit.DataAccess;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.Accessors;

public abstract class InflexSubReportAccessor : AppManager.InflexAccessorBase<InflexSubReport, InflexSubReportAccessor>
{
	[InflexSqlQuery("select subtype_id, name from <T> where report_id = :id")]
	[Index(new string[] { "subtype_id" })]
	[ScalarFieldName("name")]
	public abstract Dictionary<decimal, string> SelectSubReports(decimal id);
}
