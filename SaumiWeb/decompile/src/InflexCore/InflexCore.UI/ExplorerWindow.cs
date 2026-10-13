using System;
using InflexCore.Native;

namespace InflexCore.UI;

public class ExplorerWindow : AppManager.NativeInflexAccessor, IDisposable
{
	protected bool UseAnalitic { get; set; }

	public ExplorerWindow(AppManager appManager)
		: base(appManager)
	{
		UseAnalitic = false;
	}

	public void Find()
	{
		Find((RestrictByQuery)null);
	}

	public void Find(IInflexReport report)
	{
		Find((report == null) ? null : RestrictByQuery.GetFullQuery(report));
	}

	public void Find(RestrictByQuery queryRestriction)
	{
		if (Inflex32.ExplorerWindow != null)
		{
			decimal reportID = (queryRestriction?.Report)?.ID ?? 0m;
			string xml = ((queryRestriction == null) ? string.Empty : queryRestriction.RestrictionXMLText);
			Inflex32.ExplorerWindow.Find(reportID, xml, UseAnalitic);
		}
	}

	public void Dispose()
	{
	}
}
