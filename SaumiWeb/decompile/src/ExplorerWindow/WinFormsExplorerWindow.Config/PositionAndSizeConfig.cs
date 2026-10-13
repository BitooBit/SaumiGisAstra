using System;
using System.Drawing;
using System.Windows.Forms;
using InflexCore;
using InflexCore.log;
using InflexData;

namespace WinFormsExplorerWindow.Config;

public class PositionAndSizeConfig
{
	private const string _DATA_WINDOW_LEFT = "DATA_WINDOW_LEFT";

	private const string _DATA_WINDOW_TOP = "DATA_WINDOW_TOP";

	private const string _DATA_WINDOW_WIDTH = "DATA_WINDOW_WIDTH";

	private const string _DATA_WINDOW_HEIGHT = "DATA_WINDOW_HEIGHT";

	private const int X_OFFSET_FOR_NEXT_WINDOW = 30;

	private const int Y_OFFSET_FOR_NEXT_WINDOW = 30;

	private readonly AppManager _AppManager;

	private static int LastTop;

	private static int LastLeft;

	private static void GetInitialPos(out int left, out int top)
	{
		left = 100;
		top = 100;
	}

	public PositionAndSizeConfig(AppManager appManager)
	{
		_AppManager = appManager;
	}

	public void Load(ref ExplorerWindow window, int reportID)
	{
		string text = _AppManager.UserSettings["DATA_WINDOW_WIDTH_" + reportID];
		string text2 = _AppManager.UserSettings["DATA_WINDOW_HEIGHT_" + reportID];
		if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2) && int.TryParse(text, out var result) && int.TryParse(text2, out var result2))
		{
			if (result2 > 100)
			{
				((Control)window).Height = result2;
			}
			if (result > 100)
			{
				((Control)window).Width = result;
			}
		}
		else
		{
			try
			{
				Screen primaryScreen = Screen.PrimaryScreen;
				if (primaryScreen != null)
				{
					int width = primaryScreen.WorkingArea.Width;
					if (width > 0 && width <= 1024)
					{
						((Control)window).Width = width / 4 * 3;
					}
					else if (width > 0 && width <= 1280)
					{
						((Control)window).Width = width / 3 * 2;
					}
					else
					{
						((Control)window).Width = 900;
					}
					int height = primaryScreen.WorkingArea.Height;
					if (height > 0 && height <= 768)
					{
						((Control)window).Height = height / 4 * 3;
					}
					else if (height > 0 && height <= 1024)
					{
						((Control)window).Height = height / 3 * 2;
					}
					else
					{
						((Control)window).Height = 700;
					}
				}
			}
			catch (Exception ex)
			{
				FastLog.Loger(this).Error((object)"Screen size calculate", ex);
			}
		}
		int left;
		int top;
		if (double.IsNaN(LastTop))
		{
			GetInitialPos(out left, out top);
		}
		else
		{
			left = LastLeft + 30;
			top = LastTop + 30;
		}
		((Control)window).Left = left;
		((Control)window).Top = top;
		Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
		((Control)window).Left = ((((Control)window).Left + ((Control)window).Width > workingArea.Right) ? workingArea.Left : left);
		((Control)window).Top = ((((Control)window).Top + ((Control)window).Height > workingArea.Bottom) ? workingArea.Top : top);
		LastLeft = ((Control)window).Left;
		LastTop = ((Control)window).Top;
		if (((Control)window).Left + ((Control)window).Width > workingArea.Right)
		{
			((Control)window).Width = workingArea.Right - ((Control)window).Left;
		}
		if (((Control)window).Top + ((Control)window).Height > workingArea.Bottom)
		{
			((Control)window).Height = workingArea.Bottom - ((Control)window).Top;
		}
	}

	public void Save(ref ExplorerWindow window)
	{
		try
		{
			if (window.DataView.DataQuery is IInflexReportBasedDataQuery { Report: var report })
			{
				if (report != null)
				{
					int num = (int)report.ID;
					_AppManager.UserSettings["DATA_WINDOW_LEFT_" + num] = ((Control)window).Left.ToString();
					_AppManager.UserSettings["DATA_WINDOW_TOP_" + num] = ((Control)window).Top.ToString();
					_AppManager.UserSettings["DATA_WINDOW_WIDTH_" + num] = ((Control)window).Width.ToString();
					_AppManager.UserSettings["DATA_WINDOW_HEIGHT_" + num] = ((Control)window).Height.ToString();
				}
				else
				{
					FastLog.Loger(this).Warn((object)"Save size window. Report not defined");
				}
			}
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Warn((object)"Save size window", ex);
		}
	}
}
