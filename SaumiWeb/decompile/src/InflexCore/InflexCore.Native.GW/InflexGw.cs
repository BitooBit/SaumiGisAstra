using System;
using System.IO;
using System.Runtime.InteropServices;
using InflexCore.Native.Action;
using InflexCore.log;

namespace InflexCore.Native.GW;

internal class InflexGw : IInflexGw, IDisposable
{
	private delegate int NativeInflexeInitHandler(string userName, string password, string dataBase, string inflexSchema);

	private delegate void NativeInflexeDoneHandler();

	public abstract class InflexGwPart
	{
		private bool _Initialized;

		protected InflexGw InflexGw { get; set; }

		public InflexGwPart(InflexGw inflexGw)
		{
			InflexGw = inflexGw;
		}

		protected Delegate LoadInflex32Proc<T>(string procName, Delegate proc)
		{
			return InflexGw.LoadInflex32Proc<T>(procName, proc);
		}

		protected void TryInitNow()
		{
			if (!_Initialized)
			{
				InflexGw.TryInitNow();
				_Initialized = DirectInit();
			}
		}

		protected abstract bool DirectInit();
	}

	public class Imports
	{
		[DllImport("kernel32.dll")]
		public static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FreeLibrary(IntPtr hModule);
	}

	private const string _Inflex32Dll = "inflexe_gw.dll";

	private string _UserName;

	private string _Password;

	private string _DataBase;

	private string _InflexSchema;

	private IntPtr _Inflex32Handle = IntPtr.Zero;

	private bool _Initialized;

	private NativeInflexeInitHandler _InflexE_Init;

	private NativeInflexeDoneHandler _InflexE_Done;

	public IFindExGw FindEx { get; private set; }

	public IExplorerWindowGw ExplorerWindow => null;

	public bool AllowReportTreeSave => false;

	public bool AllowParseParamsSQL => false;

	public IDataHandler DataHandler => null;

	public IFastReport FastReport => null;

	public static string Inflex32DllName
	{
		get
		{
			if (!string.IsNullOrEmpty(AppManager.InflexPathStatic))
			{
				return Path.Combine(AppManager.InflexPathStatic, "inflexe_gw.dll");
			}
			return "inflexe_gw.dll";
		}
	}

	public void ReportTreeSave(int reportID, string reportName, string restrictionSQL)
	{
		throw new NotSupportedException();
	}

	public bool ParseParamsSQL(string sql, out string resultSql)
	{
		throw new NotSupportedException();
	}

	public bool AllowLicense(int license)
	{
		return false;
	}

	public InflexGw()
	{
		FindEx = new FindExGw(this);
	}

	private void TryLoadInflex32()
	{
		_InflexE_Init = LoadInflex32Proc<NativeInflexeInitHandler>("InflexE_Init", _InflexE_Init) as NativeInflexeInitHandler;
		_InflexE_Done = LoadInflex32Proc<NativeInflexeDoneHandler>("InflexE_Done", _InflexE_Done) as NativeInflexeDoneHandler;
	}

	public void Init(string userName, string password, string dataBase, string inflexSchema)
	{
		Init(userName, password, dataBase, inflexSchema, initIfNeeded: true);
	}

	public void Init(string userName, string password, string dataBase, string inflexSchema, bool initIfNeeded)
	{
		_UserName = userName;
		_Password = password;
		_DataBase = dataBase;
		_InflexSchema = inflexSchema;
		if (!initIfNeeded)
		{
			TryInitNow();
		}
	}

	public void TryInitNow()
	{
		TryLoadInflex32();
		if (!_Initialized)
		{
			if (_InflexE_Init == null)
			{
				throw new Inflex32ProcNotFoundException("InflexE_Init");
			}
			_Initialized = _InflexE_Init(_UserName, _Password, _DataBase, _InflexSchema) == 0;
		}
	}

	private Delegate LoadInflex32Proc<T>(string procName, Delegate proc)
	{
		if (_Inflex32Handle == IntPtr.Zero)
		{
			try
			{
				_Inflex32Handle = Imports.LoadLibrary(Inflex32DllName);
				if (_Inflex32Handle == IntPtr.Zero)
				{
					throw new Inflex32NotFoundException();
				}
			}
			catch (Exception ex)
			{
				try
				{
					_Inflex32Handle = Imports.LoadLibrary("inflexe_gw.dll");
					if (_Inflex32Handle == IntPtr.Zero)
					{
						throw new Inflex32NotFoundException();
					}
				}
				catch
				{
					Exception ex2 = ex;
					if (!(ex is Inflex32NotFoundException))
					{
						ex2 = new Inflex32NotFoundException(ex);
					}
					throw ex2;
				}
			}
		}
		if (_Inflex32Handle != IntPtr.Zero && (object)proc == null)
		{
			try
			{
				IntPtr procAddress = Imports.GetProcAddress(_Inflex32Handle, procName);
				if (procAddress == IntPtr.Zero)
				{
					throw new Inflex32ProcNotFoundException(procName);
				}
				return Marshal.GetDelegateForFunctionPointer(procAddress, typeof(T));
			}
			catch (Exception ex3)
			{
				Exception ex4 = ex3;
				if (!(ex3 is Inflex32ProcNotFoundException))
				{
					ex4 = new Inflex32ProcNotFoundException(procName, ex3);
				}
				throw ex4;
			}
		}
		return proc;
	}

	private void Done()
	{
		if (_Initialized)
		{
			if (_InflexE_Done == null)
			{
				throw new Inflex32ProcNotFoundException("InflexE_Done");
			}
			_InflexE_Done();
			_Initialized = false;
		}
		if (_Inflex32Handle != IntPtr.Zero)
		{
			try
			{
				Imports.FreeLibrary(_Inflex32Handle);
			}
			catch (Exception ex)
			{
				FastLog.Loger(this).Error((object)"InflexGw.Imports.FreeLibrary(this._Inflex32Handle);", ex);
			}
			_Inflex32Handle = IntPtr.Zero;
		}
		_InflexE_Init = null;
		_InflexE_Done = null;
	}

	public void Dispose()
	{
		Done();
	}
}
