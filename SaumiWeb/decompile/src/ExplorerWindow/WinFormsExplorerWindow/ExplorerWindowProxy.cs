using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using InflexCore;
using InflexCore.Native;
using InflexCore.Native.GW;
using InflexCore.Native.Messages;
using InflexCore.log;
using InflexGisComponent;
using InflexGisComponent.Map;
using InflexGisComponent.Preview;
using WinFormsExplorerWindow.Config;

namespace WinFormsExplorerWindow;

public static class ExplorerWindowProxy
{
	public sealed class TwoWindow : IDisposable
	{
		private readonly object _DisposeSync = new object();

		private bool _IsDisposed;

		public WeakReference DotNetWindow { get; private set; }

		public IntPtr DotNetHandle { get; set; }

		public IntPtr Win32Window { get; private set; }

		protected bool IsDisposed
		{
			get
			{
				lock (_DisposeSync)
				{
					return _IsDisposed;
				}
			}
		}

		public TwoWindow(WeakReference dotNetWindow, IntPtr win32Window)
		{
			DotNetWindow = dotNetWindow;
			Win32Window = win32Window;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			lock (_DisposeSync)
			{
				if (!_IsDisposed)
				{
					DotNetWindow = null;
					DotNetHandle = IntPtr.Zero;
					Win32Window = IntPtr.Zero;
					_IsDisposed = true;
				}
			}
		}
	}

	private const int _WM_SYSCOMMAND = 274;

	private const int _SC_CLOSE = 61536;

	private static List<TwoWindow> _Windows;

	private static string _GisBinPath;

	private static HashSet<string> _NotFoundFiles;

	private static int _EWWM_SHOWED;

	private static GisPreviewProvider _GisPreviewProvider;

	private static MapProvider _MapProvider;

	private static bool _IsInitialized;

	public static Dictionary<Type, XmlSerializer> _Serializers;

	public static Form MainForm { get; private set; }

	static ExplorerWindowProxy()
	{
		_Windows = new List<TwoWindow>();
		_NotFoundFiles = new HashSet<string>();
		_EWWM_SHOWED = MessageHelper.WM_USER + 108;
		_IsInitialized = false;
		_Serializers = new Dictionary<Type, XmlSerializer>();
		_GisBinPath = ConfigLoader.GetGisBinPath();
		AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainAssemblyResolve;
	}

	private static Assembly OnCurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			if (assembly.FullName == args.Name)
			{
				return assembly;
			}
		}
		if (_NotFoundFiles.Contains(args.Name))
		{
			return null;
		}
		AssemblyName assemblyName = new AssemblyName(args.Name);
		string text = Path.Combine(_GisBinPath, assemblyName.Name);
		try
		{
			string text2 = text + ".dll";
			if (!File.Exists(text2))
			{
				text2 = text + ".exe";
			}
			if (!File.Exists(text2))
			{
				_NotFoundFiles.Add(args.Name);
				return null;
			}
			return Assembly.LoadFrom(text2);
		}
		catch (Exception)
		{
			_NotFoundFiles.Add(args.Name);
			return null;
		}
	}

	public static int ShowExplorerWindow(int assignHandle, string userName, string password, string dataBase, string inflexSchema, int reportID, string restrictionSQL, string restrictionXML, int restrictionColumnID, string infoText, out Form createdwindow, string manualFrom = null, string manualWhere = null)
	{
		Initialize(userName, password, dataBase, inflexSchema);
		return ShowExplorerWindow(assignHandle, reportID, restrictionSQL, restrictionXML, restrictionColumnID, infoText, out createdwindow, manualFrom, manualWhere);
	}

	public static void Initialize(string userName, string password, string dataBase, string inflexSchema)
	{
		if (!_IsInitialized)
		{
			_IsInitialized = true;
			AppManagerProxy.InitializeIfNeed(userName, password, dataBase, inflexSchema);
			GisLoader.Init(AppManagerProxy.AppManagerInstance);
		}
	}

	public static int ShowExplorerWindow(int assignHandle, int reportID, string restrictionSQL, string restrictionXML, int restrictionColumnID, string infoText, out Form createdWindow, string manualFrom = null, string manualWhere = null)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (AppManagerProxy.AppManagerInstance != null)
		{
			if (InflexGwManager.InflexAppHandle > 0)
			{
				MessageHelper.CurrentApplicationBringToFront((IntPtr)InflexGwManager.InflexAppHandle);
			}
			try
			{
				IInflexReport report = AppManagerProxy.AppManagerInstance.GetReport(reportID);
				if (report == null)
				{
					MessageBox.Show($"Отчет с идентификатором {reportID} не найден.");
					WindowClose((IntPtr)assignHandle);
					createdWindow = null;
					return 0;
				}
				QueryRestriction restriction = new RestrictEmpty(report);
				if (restrictionColumnID >= 0)
				{
					IInflexColumn restrictionColumn = (from c in report.GetExplorerColumns()
						where c.ID == (decimal)restrictionColumnID
						select c).FirstOrDefault();
					RestrictByQuery restrictByQuery = new RestrictByQuery(report, restrictionColumn, restrictionSQL ?? string.Empty, restrictionXML);
					restrictByQuery.ManualFrom = manualFrom;
					restrictByQuery.ManualWhere = manualWhere;
					restriction = restrictByQuery;
				}
				ExplorerWindow window = new ExplorerWindow(AppManagerProxy.AppManagerInstance, assignHandle, restriction);
				TwoWindow twoWindow = new TwoWindow(new WeakReference(window), (IntPtr)assignHandle);
				_Windows.Add(twoWindow);
				window.BeforeLoaded += WindowBeforeLoaded;
				try
				{
					((Form)window).Closed += WindowClosed;
					window.DescriptionText = infoText;
					new PositionAndSizeConfig(AppManagerProxy.AppManagerInstance).Load(ref window, reportID);
					createdWindow = (Form)(object)window;
					((Form)window).StartPosition = (FormStartPosition)0;
					((Control)window).Show();
				}
				finally
				{
					window.BeforeLoaded -= WindowBeforeLoaded;
				}
				twoWindow.DotNetHandle = ((Control)window).Handle;
				return twoWindow.DotNetHandle.ToInt32();
			}
			catch
			{
				throw;
			}
		}
		throw new NotSupportedException("AppManager not initialized.");
	}

	private static void SendHideSplashScrinMessage(IntPtr win32Handle)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Message val = default(Message);
		((Message)(ref val)).HWnd = win32Handle;
		((Message)(ref val)).Msg = _EWWM_SHOWED;
		Message m = val;
		try
		{
			MessageHelper.PostWindowMessage(ref m);
		}
		catch
		{
			FastLog.Loger<ExplorerWindow>().Error((object)"MessageHelper.PostWindowMessage(ref m);");
		}
	}

	private static void WindowBeforeLoaded(ExplorerWindow window)
	{
		TwoWindow twoWindow = TryGetTwoWindow(window);
		if (twoWindow != null)
		{
			SendHideSplashScrinMessage(twoWindow.Win32Window);
		}
	}

	private static TwoWindow TryGetTwoWindow(ExplorerWindow window)
	{
		for (int num = _Windows.Count - 1; num >= 0; num--)
		{
			if (_Windows[num].DotNetWindow.Target == null)
			{
				_Windows.RemoveAt(num);
			}
			if (_Windows[num].DotNetWindow.Target == window)
			{
				return _Windows[num];
			}
		}
		return null;
	}

	private static void WindowClosed(object sender, EventArgs e)
	{
		ExplorerWindow window = sender as ExplorerWindow;
		if (window == null)
		{
			return;
		}
		new PositionAndSizeConfig(AppManagerProxy.AppManagerInstance).Save(ref window);
		((Form)window).Closed -= WindowClosed;
		((Component)(object)window).Dispose();
		for (int num = _Windows.Count - 1; num >= 0; num--)
		{
			TwoWindow twoWindow = _Windows[num];
			if (twoWindow.DotNetWindow.Target == null)
			{
				twoWindow.Dispose();
				_Windows.RemoveAt(num);
			}
			else if (twoWindow.DotNetWindow.Target == window)
			{
				try
				{
					WindowClose(twoWindow.Win32Window);
				}
				catch
				{
					FastLog.Loger<ExplorerWindow>().Error((object)"WindowClose(_Windows[i].Win32Window)");
				}
				twoWindow.Dispose();
				_Windows.RemoveAt(num);
				break;
			}
		}
		GC.Collect();
	}

	private static void WindowClose(IntPtr win32Window)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Message m = default(Message);
		((Message)(ref m)).HWnd = win32Window;
		((Message)(ref m)).Msg = 274;
		((Message)(ref m)).WParam = (IntPtr)61536;
		((Message)(ref m)).LParam = IntPtr.Zero;
		MessageHelper.SendWindowMessage(ref m);
	}

	public static void DataChangeInflexEvent(int windowHandle, int inflexTypeID, int itemID, int actionID)
	{
		ExplorerWindow window = GetWindow(windowHandle);
		if (window != null && !((Control)window).IsDisposed)
		{
			window.OnExternalDataChangeInflexHandler(inflexTypeID, itemID, (ExternalDataChangeAction)actionID);
		}
	}

	public static bool BarCodeInflexEvent(int windowHandle, int inflexTypeID, int itemID, int actionID, string parameters)
	{
		ExplorerWindow window = GetWindow(windowHandle);
		if (window != null && !((Control)window).IsDisposed)
		{
			return window.OnExternalBarCodeInflexHandler(inflexTypeID, itemID, actionID, parameters);
		}
		return false;
	}

	public static void SwitchToMap(int windowHandle)
	{
		SwitchToMap(windowHandle, -1, isWorkspaceID: false);
	}

	public static void SwitchToMap(int windowHandle, int workspaceID)
	{
		SwitchToMap(windowHandle, workspaceID, isWorkspaceID: true);
	}

	private static void SwitchToMap(int windowHandle, int workspaceID, bool isWorkspaceID)
	{
		ExplorerWindow window = GetWindow(windowHandle);
		if (window != null)
		{
			if (isWorkspaceID)
			{
				window.PreviewShowMap(workspaceID);
			}
			else
			{
				window.PreviewShowMap();
			}
		}
	}

	private static ExplorerWindow GetWindow(int windowHandle)
	{
		foreach (TwoWindow window in _Windows)
		{
			if (window.DotNetHandle.ToInt32() == windowHandle && window.DotNetWindow.IsAlive)
			{
				return (ExplorerWindow)window.DotNetWindow.Target;
			}
		}
		return null;
	}

	public static void Dispose()
	{
		if (_GisPreviewProvider != null)
		{
			_GisPreviewProvider.Dispose();
		}
		GisLoader.ShutDownGis();
		List<TwoWindow> windows = _Windows;
		if (windows != null)
		{
			windows.Clear();
			_Windows = null;
		}
	}

	public static void BeginGetPreview(int handlerID, int taskUnID, int objID, double scale, int imageWidth, int imageHeight, int workspaceID, int workspaceSrid, DateTime date)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (_GisPreviewProvider == null)
		{
			_GisPreviewProvider = new GisPreviewProvider(AppManagerProxy.AppManagerInstance);
		}
		_GisPreviewProvider.BeginGetPreview(handlerID, taskUnID, objID, scale, imageWidth, imageHeight, workspaceID, workspaceSrid, date);
	}

	public static void CancelAllPreview(int handlerID)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (_GisPreviewProvider == null)
		{
			_GisPreviewProvider = new GisPreviewProvider(AppManagerProxy.AppManagerInstance);
		}
		_GisPreviewProvider.CancelAllPreview(handlerID);
	}

	public static void RemovePreview(int handlerID, int taskUnID)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (_GisPreviewProvider == null)
		{
			_GisPreviewProvider = new GisPreviewProvider(AppManagerProxy.AppManagerInstance);
		}
		_GisPreviewProvider.RemovePreview(handlerID, taskUnID);
	}

	public static void ShowMap(int objID, double scale, int workspaceID, int workspaceSrid, DateTime date, bool isPersonal, string caption)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (_MapProvider == null)
		{
			_MapProvider = new MapProvider(AppManagerProxy.AppManagerInstance);
		}
		_MapProvider.ShowMap(objID, scale, workspaceID, workspaceSrid, date, isPersonal, caption);
	}

	public static void GisCreateMetafile(int methodID, int workspaceID, string lambdaSql, double minX, double minY, double maxX, double maxY, double scale, string fileName, int dpi, int imageWidth, int imageHeight, double marginFactor, int layerIdForFind)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (_GisPreviewProvider == null)
		{
			_GisPreviewProvider = new GisPreviewProvider(AppManagerProxy.AppManagerInstance);
		}
		_GisPreviewProvider.CreateMetafile(methodID, workspaceID, lambdaSql, minX, minY, maxX, maxY, scale, fileName, dpi, imageWidth, imageHeight, marginFactor, layerIdForFind);
	}
}
