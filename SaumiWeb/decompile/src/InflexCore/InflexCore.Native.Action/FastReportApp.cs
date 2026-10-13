using System;
using System.Linq;
using InflexCore.Native.GW;
using InflexCore.Native.Messages;
using InflexCore.log;

namespace InflexCore.Native.Action;

public class FastReportApp : IFastReport
{
	public void Preview(int formID, params IInflexID[] items)
	{
		Execute(formID, items, preview: true);
	}

	public void Print(int formID, params IInflexID[] items)
	{
		Execute(formID, items, preview: false);
	}

	private void Execute(int formID, IInflexID[] items, bool preview)
	{
		ShowFastReportFormArgument showFastReportFormArgument = new ShowFastReportFormArgument();
		showFastReportFormArgument.FormID = formID;
		showFastReportFormArgument.Items = items.Select((IInflexID id) => decimal.ToInt32(id.ID)).ToArray();
		showFastReportFormArgument.Preview = preview;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, showFastReportFormArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, fastReportFormArgument);", ex);
		}
	}
}
