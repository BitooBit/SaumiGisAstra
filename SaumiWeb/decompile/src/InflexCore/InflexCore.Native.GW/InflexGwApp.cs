using System;
using InflexCore.Native.Action;
using InflexCore.Native.Messages;
using InflexCore.log;

namespace InflexCore.Native.GW;

internal class InflexGwApp : IInflexGw
{
	private static InflexPing _InflexPing;

	public IFindExGw FindEx { get; private set; }

	public IExplorerWindowGw ExplorerWindow { get; private set; }

	public bool AllowReportTreeSave => AllowApp;

	public bool AllowParseParamsSQL => AllowApp;

	public IDataHandler DataHandler { get; private set; }

	public IFastReport FastReport { get; private set; }

	private bool AllowApp
	{
		get
		{
			if (InflexGwManager.InflexAppHandle != 0)
			{
				return Ping(InflexGwManager.InflexAppHandle);
			}
			return false;
		}
	}

	public InflexGwApp()
	{
		FindEx = new FindExGwApp();
		ExplorerWindow = new ExplorerWindowGwApp();
		FastReport = new FastReportApp();
		DataHandler = new DataHandlerApp();
	}

	public void ReportTreeSave(int reportID, string reportName, string restrictionSQL)
	{
		if (!AllowApp)
		{
			return;
		}
		ReportTreeSaveArgument reportTreeSaveArgument = new ReportTreeSaveArgument();
		reportTreeSaveArgument.ReportID = reportID;
		reportTreeSaveArgument.ReportName = reportName;
		reportTreeSaveArgument.RestrictionSQL = restrictionSQL;
		ReportTreeSaveArgument sendArgument = reportTreeSaveArgument;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, sendArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, reportTreeSaveArgument);", ex);
		}
	}

	public bool ParseParamsSQL(string sql, out string resultSql)
	{
		resultSql = sql;
		if (!AllowApp)
		{
			return true;
		}
		ParseParamsSQLArgument parseParamsSQLArgument = new ParseParamsSQLArgument();
		parseParamsSQLArgument.SQL = sql;
		ParseParamsSQLArgument sendArgument = parseParamsSQLArgument;
		MessageArgument resultArgument = null;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, sendArgument, out resultArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, paramsSQLArgument, out result);", ex);
		}
		if (resultArgument is ParseParamsSQLResultArgument parseParamsSQLResultArgument)
		{
			resultSql = parseParamsSQLResultArgument.SQL;
			return true;
		}
		return false;
	}

	public bool AllowLicense(int license)
	{
		if (!AllowApp)
		{
			return false;
		}
		CheckLicenseArgument checkLicenseArgument = new CheckLicenseArgument();
		checkLicenseArgument.License = license;
		CheckLicenseArgument sendArgument = checkLicenseArgument;
		MessageArgument resultArgument = null;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, sendArgument, out resultArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, checkLicenseArgument, out result);", ex);
		}
		if (resultArgument is CheckLicenseResultArgument checkLicenseResultArgument)
		{
			return checkLicenseResultArgument.Result;
		}
		return false;
	}

	static InflexGwApp()
	{
		_InflexPing = new InflexPing();
	}

	public static bool Ping(int handle)
	{
		return _InflexPing.Ping(handle);
	}
}
