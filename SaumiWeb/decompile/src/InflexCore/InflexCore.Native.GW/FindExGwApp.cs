using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using InflexCore.Native.Messages;
using InflexCore.log;

namespace InflexCore.Native.GW;

internal class FindExGwApp : IFindExGw
{
	private class AsyncWindow : Form
	{
		private readonly FindNewArgument _FindArgument;

		private bool _IsCanClose;

		private bool _DialogResult;

		private int _FindID;

		public string Sql { get; private set; }

		public string Xml { get; private set; }

		public AsyncWindow(FindNewArgument findArgument)
		{
			_FindArgument = findArgument;
			((Form)this).StartPosition = (FormStartPosition)0;
			((Control)this).Left = -100;
			((Control)this).Top = -100;
			((Control)this).Width = 50;
			((Control)this).Height = 50;
			((Form)this).ShowInTaskbar = false;
			((Form)this).Shown += OnAsyncWindowShown;
			((Form)this).Closing += OnAsyncWindowClosing;
		}

		private void OnAsyncWindowClosing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
			if (CanClose())
			{
				e.Cancel = false;
				RemoveAsyncWindow(_FindID);
			}
		}

		private void OnAsyncWindowShown(object sender, EventArgs e)
		{
			Thread thread = new Thread(FindWindowShow);
			thread.Start();
		}

		private void FindWindowShow()
		{
			_FindID = _FindArgument.FindID;
			AddFindWindow(_FindID, this);
			MessageArgument resultArgument = null;
			try
			{
				MessageHelper.Send(InflexGwManager.InflexAppHandle, _FindArgument, out resultArgument);
			}
			catch (Exception ex)
			{
				FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, findArgument, out result);", ex);
			}
			bool flag = false;
			FindResultArgument findResultArgument = resultArgument as FindResultArgument;
			string sql = "";
			string xml = "";
			if (findResultArgument != null)
			{
				sql = findResultArgument.Sql;
				xml = findResultArgument.Xml;
				flag = true;
			}
			if (flag)
			{
				CanClose(isCanClose: true);
				((Control)this).Invoke((Delegate)(System.Action)delegate
				{
					Sql = sql;
					Xml = xml;
				});
				((Control)this).Invoke((Delegate)(System.Action)delegate
				{
					((Form)this).DialogResult = (DialogResult)1;
				});
			}
			while (!CanClose())
			{
				Thread.Sleep(100);
			}
			((Control)this).Invoke((Delegate)(System.Action)delegate
			{
				((Form)this).DialogResult = (DialogResult)(_DialogResult ? 1 : 2);
			});
			((Form)this).Close();
		}

		public void CanClose(bool isCanClose)
		{
			((Control)this).Invoke((Delegate)(System.Action)delegate
			{
				_IsCanClose = isCanClose;
			});
		}

		private bool CanClose()
		{
			return (bool)((Control)this).Invoke((Delegate)(Func<bool>)(() => _IsCanClose));
		}

		public void SetSqlAndXml(string sql, string xml)
		{
			((Control)this).Invoke((Delegate)(System.Action)delegate
			{
				Sql = sql;
				Xml = xml;
			});
		}

		public void SetDialogResult(bool res)
		{
			((Control)this).Invoke((Delegate)(System.Action)delegate
			{
				_DialogResult = res;
			});
		}
	}

	private static Dictionary<int, AsyncWindow> _AsyncWindows = new Dictionary<int, AsyncWindow>();

	public bool AllowAsync => true;

	public int FindAll(int queryHandle, bool async, decimal reportID, ref string sql, ref string xml)
	{
		FindNewArgument findNewArgument = new FindNewArgument();
		findNewArgument.FindID = queryHandle;
		findNewArgument.ReportID = decimal.ToInt32(reportID);
		findNewArgument.IsAsync = async;
		FindNewArgument findArgument = findNewArgument;
		return FindAll(findArgument, ref sql, ref xml);
	}

	public int FindAll(int queryHandle, bool async, decimal reportID, string oldXml, ref string sql, ref string xml)
	{
		FindEditArgument findEditArgument = new FindEditArgument();
		findEditArgument.FindID = queryHandle;
		findEditArgument.ReportID = decimal.ToInt32(reportID);
		findEditArgument.Xml = oldXml;
		findEditArgument.IsAsync = async;
		FindEditArgument findArgument = findEditArgument;
		return FindAll(findArgument, ref sql, ref xml);
	}

	private int FindAll(FindNewArgument findArgument, ref string sql, ref string xml)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if (findArgument.IsAsync)
		{
			AsyncWindow asyncWindow = new AsyncWindow(findArgument);
			try
			{
				if ((int)((Form)asyncWindow).ShowDialog() == 1)
				{
					sql = asyncWindow.Sql;
					xml = asyncWindow.Xml;
					return 1;
				}
			}
			finally
			{
				((IDisposable)asyncWindow)?.Dispose();
			}
			return 0;
		}
		MessageArgument resultArgument = null;
		try
		{
			MessageHelper.Send(InflexGwManager.InflexAppHandle, findArgument, out resultArgument);
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"MessageHelper.Send(InflexGwManager.InflexAppHandle, findArgument, out result);", ex);
		}
		if (resultArgument is FindResultArgument findResultArgument)
		{
			sql = findResultArgument.Sql;
			xml = findResultArgument.Xml;
			return 1;
		}
		return 0;
	}

	private static void RemoveAsyncWindow(int findID)
	{
		lock (_AsyncWindows)
		{
			_AsyncWindows.Remove(findID);
			if (_AsyncWindows.Count == 0)
			{
				MessageHelper.UserInflexMessage -= OnMessageHelperUserInflexMessage;
			}
		}
	}

	private static void AddFindWindow(int findID, AsyncWindow asyncWindow)
	{
		lock (_AsyncWindows)
		{
			_AsyncWindows[findID] = asyncWindow;
			if (_AsyncWindows.Count == 1)
			{
				MessageHelper.UserInflexMessage += OnMessageHelperUserInflexMessage;
			}
		}
	}

	private static void OnMessageHelperUserInflexMessage(ref Message m, MessageArgument argument)
	{
		if (!(argument is FindAsyncResultArgument findAsyncResultArgument))
		{
			return;
		}
		lock (_AsyncWindows)
		{
			if (_AsyncWindows.TryGetValue(findAsyncResultArgument.FindID, out var value))
			{
				value.SetDialogResult(findAsyncResultArgument.FindRes);
				if (findAsyncResultArgument.FindRes)
				{
					value.SetSqlAndXml(findAsyncResultArgument.Sql, findAsyncResultArgument.Xml);
				}
				value.CanClose(isCanClose: true);
			}
		}
	}
}
