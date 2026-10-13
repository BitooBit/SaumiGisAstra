using System;
using InflexCore.Native.Action;
using InflexCore.Native.Messages;

namespace InflexCore.Native.GW;

public class InflexGwManager : IDisposable
{
	private readonly string _UserName;

	private readonly string _Password;

	private readonly string _DataSource;

	private readonly string _MainSchema;

	private InflexGw _InflexGw;

	private InflexGwApp _InflexGwApp;

	private IDataHandler _PropDataHandlerApp;

	private IFastReport _PropFastReportApp;

	private static int InflexAppHandle1;

	private IInflexGw PropInflexGw
	{
		get
		{
			if (_InflexGw == null)
			{
				_InflexGw = new InflexGw();
				_InflexGw.Init(_UserName, _Password, _DataSource, _MainSchema);
			}
			return _InflexGw;
		}
	}

	private IInflexGw PropInflexGwApp
	{
		get
		{
			if (_InflexGwApp == null)
			{
				_InflexGwApp = new InflexGwApp();
			}
			return _InflexGwApp;
		}
	}

	public IInflexGw Inflex32
	{
		get
		{
			if (!CheckApp())
			{
				return PropInflexGw;
			}
			return PropInflexGwApp;
		}
	}

	public static int InflexAppHandle
	{
		get
		{
			return InflexAppHandle1;
		}
		set
		{
			InflexAppHandle1 = value;
		}
	}

	public InflexGwManager(string userName, string password, string dataSource, string mainSchema)
	{
		_UserName = userName;
		_Password = password;
		_DataSource = dataSource;
		_MainSchema = mainSchema;
	}

	private static bool CheckApp()
	{
		if (InflexAppHandle != 0)
		{
			return InflexGwApp.Ping(InflexAppHandle);
		}
		return false;
	}

	public void Dispose()
	{
		if (_InflexGw != null)
		{
			InflexGw inflexGw = _InflexGw;
			_InflexGw = null;
			inflexGw.Dispose();
		}
	}

	public static bool TrySendMessage(MessageArgument argument)
	{
		int result = 0;
		return TrySendMessage(argument, out result);
	}

	public static bool TrySendMessage(MessageArgument argument, out int result)
	{
		MessageArgument resultArgument;
		return TrySendMessage(argument, out result, out resultArgument);
	}

	public static bool TrySendMessage(MessageArgument argument, out int result, out MessageArgument resultArgument)
	{
		result = 0;
		resultArgument = null;
		if (CheckApp())
		{
			result = MessageHelper.Send(InflexAppHandle, argument, out resultArgument);
			return true;
		}
		return false;
	}
}
