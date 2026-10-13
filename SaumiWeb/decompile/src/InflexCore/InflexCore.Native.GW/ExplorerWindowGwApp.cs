using System;
using InflexCore.Native.Messages;
using InflexCore.log;

namespace InflexCore.Native.GW;

public class ExplorerWindowGwApp : IExplorerWindowGw
{
	public void Find(decimal reportID, string xml, bool useAnalitic)
	{
		FindExplorerWindowArgument findExplorerWindowArgument = new FindExplorerWindowArgument();
		findExplorerWindowArgument.ReportID = decimal.ToInt32(reportID);
		findExplorerWindowArgument.Xml = xml;
		findExplorerWindowArgument.UseAnalitic = useAnalitic;
		FindExplorerWindowArgument sendArgument = findExplorerWindowArgument;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, sendArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"Find(decimal reportID, string xml, bool useAnalitic)", ex);
		}
	}
}
