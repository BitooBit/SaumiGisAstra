using System;
using System.Collections.Generic;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.Accessors;

public abstract class InflexColumnAccessor : AppManager.InflexAccessorBase<InflexColumn, InflexColumnAccessor>
{
	public IEnumerable<InflexColumn> GetExplorerColumns(decimal reportID, decimal subReportID, bool subTypeSpecific)
	{
		decimal objMainType = (subTypeSpecific ? reportID : 0m);
		return SelectExplorerColumns(reportID, subReportID, objMainType);
	}

	[InflexSqlQuery("select dc.*, pc.visible preset_columns_visible from <T> dc, <USER>.preset_columns pc\r\nwhere (dc.report_id = :reportID) and ((dc.objecttype = :subReportID) or (dc.objecttype = :objMainType))\r\n  and pc.column_id(+)=dc.id\r\norder by dc.seqn, dc.reference_id desc, dc.objecttype")]
	public abstract IList<InflexColumn> SelectExplorerColumns(decimal reportID, decimal subReportID, decimal objMainType);

	[InflexSqlQuery("select * from <T> where reference_id = :referenceID")]
	public abstract IList<InflexColumn> SelectReferenceColumns(decimal referenceID);

	public override InflexColumn SelectByKey(params object[] keys)
	{
		if (keys == null || keys.Length != 2)
		{
			throw new ArgumentException();
		}
		return DirectSelectByKey((decimal)keys[0], (decimal)keys[1]);
	}

	[InflexSqlQuery("select dc.*, pc.visible preset_columns_visible from <T> dc, <USER>.preset_columns pc\r\nwhere (dc.id = :id) and (dc.report_id = :reportID)\r\n  and pc.column_id(+)=dc.id")]
	protected abstract InflexColumn DirectSelectByKey(decimal id, decimal reportID);
}
