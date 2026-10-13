using System;
using System.Collections.Generic;
using System.Windows.Forms;
using InflexCore.Native;

namespace InflexCore.UI;

public class FindWindow : AppManager.NativeInflexAccessor, IDisposable
{
	private static readonly object _LockObj = new object();

	private static int _QueryHandle = 0;

	public QueryRestriction QueryRestriction { get; set; }

	public int QueryHandle { get; set; }

	private static int NextQueryHandle()
	{
		lock (_LockObj)
		{
			_QueryHandle++;
			return _QueryHandle;
		}
	}

	public FindWindow(AppManager appManager)
		: base(appManager)
	{
		QueryHandle = NextQueryHandle();
	}

	public DialogResult FindAll(bool async = true)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return FindAll(new RestrictByQuery(null, ""), async);
	}

	public DialogResult FindAll(IInflexReport report, bool async = true)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return FindAll(new RestrictByQuery(report, ""), async);
	}

	public DialogResult FindAll(RestrictByQuery queryRestriction, bool async = true)
	{
		string sql = "";
		string xml = "";
		bool flag = false;
		IEnumerable<IInflexID> restrictionIDs = null;
		IInflexReport inflexReport = ((queryRestriction == null) ? null : (queryRestriction.Report.RootReport ?? queryRestriction.Report));
		bool async2 = async && Inflex32.FindEx.AllowAsync;
		if (queryRestriction == null || string.IsNullOrEmpty(queryRestriction.RestrictionXMLText))
		{
			flag = Inflex32.FindEx.FindAll(QueryHandle, async2, inflexReport?.ID ?? 0m, ref sql, ref xml) == 1;
		}
		else
		{
			flag = Inflex32.FindEx.FindAll(QueryHandle, async2, inflexReport?.ID ?? 0m, queryRestriction.RestrictionXMLText, ref sql, ref xml) == 1;
			restrictionIDs = queryRestriction.RestrictionIDs;
		}
		if (flag)
		{
			QueryRestriction = new RestrictByQuery(inflexReport, sql, xml)
			{
				RestrictionIDs = restrictionIDs
			};
			return (DialogResult)1;
		}
		return (DialogResult)2;
	}

	public void Dispose()
	{
	}
}
