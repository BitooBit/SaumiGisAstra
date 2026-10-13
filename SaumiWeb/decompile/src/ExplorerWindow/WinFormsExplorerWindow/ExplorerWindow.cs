using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using System.Xml;
using System.Xml.Serialization;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using ExplorerWindowBase;
using ExplorerWindowBase.Action;
using InflexCore;
using InflexCore.Action;
using InflexCore.Action.Attributes;
using InflexCore.Native;
using InflexCore.log;
using InflexData;
using InflexData.RestrictionQuery;
using InflexDataGridComponent;
using InflexGisComponent;
using InflexUtils.ExceptionBox;
using WinFormsExplorerWindow.Actions;
using WinFormsExplorerWindow.GisComponent;
using WinFormsExplorerWindow.PrimitiveControls;
using WinFormsExplorerWindow.PrimitiveControls.RestrictionsPanelControls;
using WinFormsExplorerWindow.PrimitiveControls.StyleLegendPanelControls;
using WinFormsExplorerWindow.PrimitiveControls.WindowTitle;
using WinFormsExplorerWindow.Properties;

namespace WinFormsExplorerWindow;

public class ExplorerWindow : Form, IShortcutActionActivator, IDisposable, IMessageFilter
{
	private class ShortCutActionActivaror : IShortcutActionActivator, IDisposable
	{
		private Dictionary<Keys, InflexAction> _Index = new Dictionary<Keys, InflexAction>();

		private readonly object _DisposeSync = new object();

		public void Register(Keys keys, InflexAction action)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (!_Index.ContainsKey(keys))
			{
				_Index.Add(keys, action);
			}
			else
			{
				_Index[keys] = action;
			}
		}

		public void UnRegister(Keys keys)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (_Index.ContainsKey(keys))
			{
				_Index.Remove(keys);
			}
		}

		public bool ActivateAction(Keys keys)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (_Index.ContainsKey(keys))
			{
				InflexAction inflexAction = _Index[keys];
				if (inflexAction != null && inflexAction.IsAllowed == true)
				{
					inflexAction.Execute();
					return true;
				}
			}
			return false;
		}

		public void Dispose()
		{
			lock (_DisposeSync)
			{
				if (_Index != null)
				{
					_Index.Clear();
					_Index = null;
				}
			}
		}
	}

	private class CombineStyleLegendItem : IStyleLegendItem
	{
		private int _Value;

		private string _Info;

		public decimal Value => _Value;

		public string Info => _Info;

		public CombineStyleLegendItem(IEnumerable<IStyleLegendItem> indicators, int indicatorValue)
		{
			_Value = indicatorValue;
			_Info = "";
			bool flag = true;
			foreach (IStyleLegendItem indicator in indicators)
			{
				if (!flag)
				{
					_Info += " + ";
				}
				flag = false;
				_Info += indicator.Info;
			}
		}
	}

	private const int WM_NCLBUTTONDOWN = 161;

	private const int HT_CAPTION = 2;

	private const int WM_KEYDOWN = 256;

	private const string EXTERNAL_ACTIONS_DEFINITION_FILE_NAME = "ExternalActions.xml";

	private IInflexType _ContentType;

	private QueryRestriction _InitializeRestriction;

	private AppManager _AppManager;

	private int _Win32Handle;

	private Stopwatch _WatchTimer;

	private Exception _LastDataViewException;

	private IInflexColumn[] _ManualRestrictionColumns;

	private ActionFilteredListAccesor _GridActionsAccessor;

	private ActionFilteredListAccesor _DefaultActionsAccessor;

	private readonly object _DisposedSync;

	private ActionsPanel _ActionsPanel;

	private PictureBox _IconPictureBox;

	private LoadInfoSplashPanel _SplashPanel;

	private Color _ErrorStatusTextColor;

	private readonly object _UpdateSync;

	private Thread _InitDataQueryThread;

	private IInflexID[] _LastContextActionArgsObjectIDs;

	private Dictionary<IInflexReport, IInflexColumn[]> _DefaultRestrictionColumns;

	private bool _IsDisposed;

	private static Assembly _StaticUserAssembly;

	private Assembly _UserAssembly;

	private UserExplorerWindow _UserExplorerWindow;

	private IExplorerWindowExtention _UserExtention;

	private ShowMapHelper _ShowMapHelper;

	private IInflexDataQuery _DefaultDataQuery;

	private IInflexDataQuery _OriginalDataQuery;

	private ExplorerWindowToolsBuilder _ToolsBuilder;

	private ActionCollection _ToolActions;

	private ActionCollection _CoreActions;

	private WindowStyle _ActiveWindowStyle;

	private WindowStyle _InactiveWindowStyle;

	private bool _IsActive;

	private FormWindowState _LastWindowState;

	private bool _IsDataChangeMessageInProcess;

	private object _DataChangedProcessSync;

	private TotalSumsPanel _TotalSumsPanel;

	private RestrictionsPanel _RestrictionsPanel;

	private FilterPanel _FilterPanel;

	private string _StatusTextPrefixString;

	private bool _IsStatusTextErrorState;

	private IGisComponentWindow _GisWindow;

	private bool _IsGisWindowAlone;

	private IShortcutActionActivator _ShortcutActionActivator;

	private IContainer components;

	internal StatusStrip _StatusBar;

	private ToolStripStatusLabel _StatusLabel;

	private MenuStrip _MainMenu;

	private Panel _GeneralLayoutPanel;

	private TableLayoutPanel _MainPanel;

	private WindowHeaderControl _HeaderPanel;

	private Panel _ActionsControlPanel;

	private Panel _GridPanel;

	private Panel _BottomPanel;

	private LinePanel _BottomLinePanel;

	private TableLayoutPanel _ActionTableLayoutPanel;

	private ToolTip _ToolTip;

	internal StopedProgressBarFactory ProgressBarFactory { get; private set; }

	public QueryColumnCollectionProvider QueryColumnProvider { get; set; }

	public InflexDataView DataView { get; private set; }

	public MenuStrip MainMenu => _MainMenu;

	public ActionsPanel ActionsPanel => _ActionsPanel;

	public string DescriptionText { get; set; }

	public bool TotalSummaryPanelVisibility
	{
		get
		{
			return ((Control)_TotalSumsPanel).Visible;
		}
		set
		{
			((Control)_TotalSumsPanel).Visible = value;
		}
	}

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = ((Form)this).CreateParams;
			createParams.Style &= -12582913;
			return createParams;
		}
	}

	public ExplorerWindowOptions Options { get; private set; }

	private WindowStyle CurrentWindowStyle
	{
		get
		{
			if (!IsActive)
			{
				return _InactiveWindowStyle;
			}
			return _ActiveWindowStyle;
		}
	}

	public bool IsActive
	{
		get
		{
			return _IsActive;
		}
		set
		{
			_IsActive = value;
			OnWindowActivityChanged(value);
		}
	}

	public InflexDataGrid DataGrid { get; private set; }

	internal GridView DataGridView
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			if (DataGrid == null)
			{
				return null;
			}
			return (GridView)((GridControl)DataGrid).MainView;
		}
	}

	public bool StatusBarVisible
	{
		get
		{
			return ((Control)_StatusBar).Visible;
		}
		set
		{
			((Control)_StatusBar).Visible = value;
		}
	}

	internal ExplorerWindowContext Context { get; private set; }

	public bool IsDataQueryChanged => _OriginalDataQuery != null && DataView.DataQuery != _OriginalDataQuery;

	private StyleLegendPanel StyleLegendPanel { get; set; }

	public IGisComponentWindow GisWindow
	{
		get
		{
			if (_GisWindow == null)
			{
				_GisWindow = GisLoader.GetNewGisWindow();
				_GisWindow.Closed += GisWindowOnClosed;
			}
			return _GisWindow;
		}
	}

	public ActionFilteredListAccesor DefaultActionsAccessor => _DefaultActionsAccessor;

	public ActionFilteredListAccesor GridActionsAccessor => _GridActionsAccessor;

	public event Action<FormWindowState> WindowStateChanged;

	public event Action<ExplorerWindow> BeforeLoaded;

	public ExplorerWindow(AppManager appManager, int win32Handle, QueryRestriction restriction)
		: this(appManager, win32Handle)
	{
		_ContentType = restriction.Report.AssignedType;
		_InitializeRestriction = restriction;
		Application.AddMessageFilter((IMessageFilter)(object)this);
	}

	public ExplorerWindow(AppManager appManager, int win32Handle)
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		_DisposedSync = new object();
		_ErrorStatusTextColor = Color.FromArgb(255, 255, 0);
		_UpdateSync = new object();
		_DefaultRestrictionColumns = new Dictionary<IInflexReport, IInflexColumn[]>();
		_ActiveWindowStyle = new WindowStyle
		{
			Background = ViewResources.ActiveBackgroundColor,
			Foreground = ViewResources.ActiveForegroundColor
		};
		_InactiveWindowStyle = new WindowStyle
		{
			Background = ViewResources.InactiveBackgroundColor,
			Foreground = ViewResources.InactiveForegroundColor
		};
		_DataChangedProcessSync = new object();
		_ShortcutActionActivator = new ShortCutActionActivaror();
		((Form)this)._002Ector();
		InitializeComponent();
		ExplorerWindowOptions explorerWindowOptions = (Options = new ExplorerWindowOptions());
		((Form)this).MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
		((ToolStrip)_StatusBar).Renderer = (ToolStripRenderer)(object)new StatusBarRenderer();
		ProgressBarFactory = new StopedProgressBarFactory(_StatusBar);
		_HeaderPanel.ControlBox.AttachedForm = this;
		((Control)_HeaderPanel).MouseDown += new MouseEventHandler(HeaderPanelOnMouseDown);
		InflexDataGrid inflexDataGrid = new InflexDataGrid();
		((Control)inflexDataGrid).Anchor = (AnchorStyles)5;
		((Control)inflexDataGrid).BackColor = Color.White;
		((Control)inflexDataGrid).Dock = (DockStyle)5;
		inflexDataGrid.UseCellValueWordWrap = explorerWindowOptions.CellValueWordWrap;
		inflexDataGrid.UseColumnHeaderWordWrap = explorerWindowOptions.ColumnHeaderWordWrap;
		DataGrid = inflexDataGrid;
		DataGrid.SetResourceManager(Resources.ResourceManager);
		DataGrid.FilterChanged += DataGridOnFilterChanged;
		((Control)_GridPanel).Controls.Add((Control)(object)DataGrid);
		LoadInfoSplashPanel loadInfoSplashPanel = new LoadInfoSplashPanel();
		((Control)loadInfoSplashPanel).Dock = (DockStyle)5;
		((Control)loadInfoSplashPanel).Visible = false;
		_SplashPanel = loadInfoSplashPanel;
		((Control)_GridPanel).Controls.Add((Control)(object)_SplashPanel);
		ShowSplashPanel(splashPanelVisibility: false);
		_ActionsPanel = new ActionsPanel();
		((Control)_ActionsControlPanel).Controls.Add((Control)(object)_ActionsPanel);
		InitializeStyleLegendPanel();
		_AppManager = appManager;
		_Win32Handle = win32Handle;
		UpdateStatusText("Инициализация ...");
		InitGridActions();
		Func<IInflexID[], ActionCollection> actionProvider = (IInflexID[] ids) => GetActions(null, null, Context.ActionArgs.ObjectIDs);
		_DefaultActionsAccessor = new ActionFilteredListAccesor(actionProvider);
		_SplashPanel.ActionsProvider = DefaultActionsAccessor;
		QueryColumnProvider = new QueryColumnCollectionProvider(_AppManager, this);
		DataView = new InflexDataView(_AppManager, QueryColumnProvider);
		DataView.BeginUpdate += OnDataViewContextUpdated;
		DataView.EndUpdate += OnDataViewContextUpdated;
		DataView.DataQueryUpdated += OnDataViewContextUpdated;
		DataView.BeginUpdate += OnDataViewStartUpdate;
		DataView.PulseUpdate += OnDataViewPulseUpdate;
		DataView.EndUpdate += OnDataViewStopUpdate;
		DataView.BeforeUpdateDataQuery += OnDataViewBeforeUpdateDataQuery;
		DataGrid.InflexDataView = DataView;
		DataGrid.ActionListAccessor = _GridActionsAccessor;
		DataGrid.AllowInflexDrop = true;
		DataGrid.AllowInflexDrag = true;
		DataGrid.SelectedRowsChanged += OnGridSelectedRowsChanged;
		((ColumnView)DataGridView).ColumnPositionChanged += DataGridViewOnColumnPositionChanged;
		((ColumnView)DataGridView).ColumnFilterChanged += DataGridViewOnColumnFilterChanged;
		InflexClipboard.Updated += InflexClipboardUpdated;
		((Form)this).Load += InflexExplorerWindowLoaded;
		((Form)this).Activated += OnActivated;
		((Form)this).Deactivate += OnDeactivate;
		InitializeIconView();
		InitializeRestrictionsPanel();
		InitializeFilterPanel();
		InitializeTotalSumsPanel();
		SetAllToolTips();
		_ShowMapHelper = new ShowMapHelper(this, _DefaultActionsAccessor);
		OnWindowStateChanged(((Form)this).WindowState);
	}

	private void OnWindowActivityChanged(bool isActive)
	{
		if (CurrentWindowStyle != null)
		{
			if (_HeaderPanel != null)
			{
				_HeaderPanel.ApplyStyle(CurrentWindowStyle);
				_HeaderPanel.SetControlBoxStyle(isActive);
			}
			if (_ActionsPanel != null)
			{
				((Control)_ActionsPanel).BackColor = CurrentWindowStyle.Background;
			}
			if (_BottomLinePanel != null)
			{
				_BottomLinePanel.LineColor = CurrentWindowStyle.Foreground;
			}
			((Control)this).Refresh();
			if (_IconPictureBox != null)
			{
				_IconPictureBox.Image = CurrentWindowStyle.IconImage;
				((Control)_IconPictureBox).Refresh();
			}
		}
	}

	private void OnActivated(object sender, EventArgs e)
	{
		IsActive = true;
	}

	private void OnDeactivate(object sender, EventArgs e)
	{
		IsActive = false;
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		StopedProgressBarFactory progressBarFactory = ProgressBarFactory;
		if (progressBarFactory != null)
		{
			bool cancel = !progressBarFactory.TryStopAllProgress();
			while (!progressBarFactory.IsAllProgressesFinished)
			{
				Application.DoEvents();
			}
			e.Cancel = cancel;
		}
		InflexDataView dataView = DataView;
		if (dataView != null && dataView.QueryInPrepare())
		{
			e.Cancel = true;
		}
		((Form)this).OnClosing(e);
	}

	protected override void OnClosed(EventArgs e)
	{
		Thread initDataQueryThread = _InitDataQueryThread;
		if (initDataQueryThread != null)
		{
			if (initDataQueryThread.IsAlive)
			{
				TimeSpan timeout = new TimeSpan(0, 0, 1);
				initDataQueryThread.Join(timeout);
			}
			_InitDataQueryThread = null;
		}
		StopedProgressBarFactory progressBarFactory = ProgressBarFactory;
		if (progressBarFactory != null)
		{
			progressBarFactory.Dispose();
			ProgressBarFactory = null;
		}
		((Form)this).OnClosed(e);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		char keyChar = e.KeyChar;
		if (char.IsLetterOrDigit(keyChar) || char.IsPunctuation(keyChar))
		{
			e.Handled = true;
			DataGrid.SearchManager.FilterText += keyChar;
			((Control)this).OnKeyPress(e);
		}
	}

	protected override void OnMove(EventArgs e)
	{
		if (((Control)_HeaderPanel).Visible)
		{
			((Control)_HeaderPanel).Invalidate();
		}
		if (((Control)_BottomPanel).Visible)
		{
			((Control)_BottomPanel).Invalidate();
		}
		((Control)this).OnMove(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (_TotalSumsPanel != null && ((Control)_TotalSumsPanel).Visible)
		{
			((Control)_TotalSumsPanel).Invalidate();
		}
		((Form)this).OnPaint(e);
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(CurrentWindowStyle.Background);
		try
		{
			e.Graphics.InterpolationMode = (InterpolationMode)2;
			e.Graphics.CompositingQuality = (CompositingQuality)2;
			e.Graphics.FillRectangle((Brush)(object)val, e.ClipRectangle);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected override void OnClientSizeChanged(EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		FormWindowState windowState = ((Form)this).WindowState;
		if (windowState != _LastWindowState)
		{
			OnWindowStateChanged(windowState);
		}
		((Control)this).OnClientSizeChanged(e);
	}

	protected void OnWindowStateChanged(FormWindowState windowState)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		WindowStateChanged?.Invoke(windowState);
		_LastWindowState = windowState;
	}

	private void InitGridActions()
	{
		_GridActionsAccessor = new ActionFilteredListAccesor((IInflexID[] ids) => GetActions((InflexAction a) => a.IsDataContextAction, IsFastReportNeedDisplayInGrid, ids));
	}

	private bool IsFastReportNeedDisplayInGrid(InflexAction action)
	{
		if (action is ShowFastReportFormToolAction showFastReportFormToolAction)
		{
			return showFastReportFormToolAction.IsOneItemForm;
		}
		return true;
	}

	[DllImport("user32.dll")]
	private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

	[DllImport("user32.dll")]
	private static extern bool ReleaseCapture();

	private void HeaderPanelOnMouseDown(object sender, MouseEventArgs e)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (e.Clicks == 2)
		{
			((Form)this).WindowState = (FormWindowState)(((int)((Form)this).WindowState == 0) ? 2 : 0);
			return;
		}
		ReleaseCapture();
		SendMessage(((Control)this).Handle, 161, 2, 0);
	}

	private void IconPictureBoxOnMouseDown(object sender, MouseEventArgs e)
	{
		ReleaseCapture();
		SendMessage(((Control)this).Handle, 161, 2, 0);
	}

	private void InitializeIconView()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		WindowStyle activeWindowStyle = _ActiveWindowStyle;
		object? obj = Resources.ResourceManager.GetObject("_1", Resources.Culture);
		activeWindowStyle.IconImage = (Image)((obj is Bitmap) ? obj : null);
		WindowStyle inactiveWindowStyle = _InactiveWindowStyle;
		object? obj2 = Resources.ResourceManager.GetObject("_1_Inactive", Resources.Culture);
		inactiveWindowStyle.IconImage = (Image)((obj2 is Bitmap) ? obj2 : null);
		if (_IconPictureBox == null)
		{
			PictureBox val = new PictureBox();
			((Control)val).Width = 70;
			((Control)val).Height = 70;
			((Control)val).Margin = new Padding(3, 3, 0, 0);
			val.Image = CurrentWindowStyle.IconImage;
			_IconPictureBox = val;
			((Control)_IconPictureBox).MouseDown += new MouseEventHandler(IconPictureBoxOnMouseDown);
		}
		_MainPanel.SetColumn((Control)(object)_IconPictureBox, 0);
		((ControlCollection)_MainPanel.Controls).Add((Control)(object)_IconPictureBox);
		_MainPanel.SetRow((Control)(object)_IconPictureBox, 0);
		_MainPanel.SetRowSpan((Control)(object)_IconPictureBox, 3);
	}

	private void OnGridSelectedRowsChanged(IInflexID[] ids, DataRowView[] rows)
	{
		if (_LastContextActionArgsObjectIDs == null)
		{
			Context.ActionArgs.ObjectIDs = ids;
			Context.Update();
		}
		else
		{
			_LastContextActionArgsObjectIDs = ids;
		}
		if (rows == null || rows.Length == 0)
		{
			return;
		}
		if (rows.Length > 1)
		{
			if (((Control)_TotalSumsPanel).Visible)
			{
				DataGrid.UpdateSelectedSummaries();
				if (((Control)_TotalSumsPanel).InvokeRequired)
				{
					((Control)_TotalSumsPanel).Invoke((Delegate)new Action<IDictionary<IGridColumn, decimal>, bool, TotalSumsKinds>(_TotalSumsPanel.UpdateTotalSumPanel), new object[3]
					{
						DataGrid.SelectedSummaries,
						false,
						TotalSumsKinds.Selected
					});
				}
				else
				{
					_TotalSumsPanel.UpdateTotalSumPanel(DataGrid.SelectedSummaries, rebuildPanel: false, TotalSumsKinds.Selected);
				}
			}
		}
		else if (((Control)_TotalSumsPanel).Visible)
		{
			if (((Control)_TotalSumsPanel).InvokeRequired)
			{
				((Control)_TotalSumsPanel).Invoke((Delegate)new Action<IDictionary<IGridColumn, decimal>, bool, TotalSumsKinds>(_TotalSumsPanel.UpdateTotalSumPanel), new object[3]
				{
					DataGrid.TotalSummaries,
					false,
					(!string.IsNullOrEmpty(((ColumnView)DataGridView).FindFilterText)) ? TotalSumsKinds.Filtered : TotalSumsKinds.All
				});
			}
			else
			{
				_TotalSumsPanel.UpdateTotalSumPanel(DataGrid.TotalSummaries, rebuildPanel: false, (!string.IsNullOrEmpty(((ColumnView)DataGridView).FindFilterText)) ? TotalSumsKinds.Filtered : TotalSumsKinds.All);
			}
		}
		UpdateStatusText();
	}

	public void InflexClipboardUpdated()
	{
		UpdateAllowedStateTools();
	}

	private bool IsWindowAlreadyDisposed()
	{
		Application.DoEvents();
		if (!((Control)this).IsDisposed)
		{
			return _IsDisposed;
		}
		return true;
	}

	private void InflexExplorerWindowLoaded(object sender, EventArgs eventArgs)
	{
		RaiseBeforeLoaded();
		InitializeDataQuery(_InitializeRestriction, background: true, delegate
		{
			if (!IsWindowAlreadyDisposed())
			{
				_OriginalDataQuery = DataView.DataQuery;
				if (!IsWindowAlreadyDisposed())
				{
					((Control)_IconPictureBox).Invalidate();
					((Control)_HeaderPanel).Invalidate();
					if (!IsWindowAlreadyDisposed())
					{
						InitializeContext();
						if (!IsWindowAlreadyDisposed())
						{
							InitializeTools();
							if (!IsWindowAlreadyDisposed())
							{
								InitializeUserTools();
								if (!IsWindowAlreadyDisposed())
								{
									((Control)DataGrid).Focus();
									if (!IsWindowAlreadyDisposed())
									{
										QueryColumnProvider.OnWindowLoaded();
										if (!IsWindowAlreadyDisposed())
										{
											if (DataView.InExecute())
											{
												DataView.Stop(wait: true);
											}
											if (!IsWindowAlreadyDisposed())
											{
												_ShowMapHelper.PreviewShowMapHandler();
												if (!IsWindowAlreadyDisposed())
												{
													DataView.UpdateDataQuery(executeQuery: true, background: true);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		});
	}

	private void FreeContext()
	{
		if (Context != null)
		{
			Context.Dispose();
			Context = null;
		}
	}

	private void InitializeContext()
	{
		if (Context == null)
		{
			Context = new ExplorerWindowContext(this, _AppManager)
			{
				Type = _ContentType
			};
			Context.DataBlockedChanged += ContextOnDataBlockedChanged;
		}
		if (Context.Type != _ContentType)
		{
			throw new NotSupportedException();
		}
		Context.ActionArgs = new ActionArgs
		{
			ObjectIDs = new IInflexID[0]
		};
	}

	private void ContextOnDataBlockedChanged(bool isDataBlocked)
	{
		DataGrid.AllowInflexDrag = !isDataBlocked;
		DataGrid.AllowInflexDrop = !isDataBlocked;
		DataGridView.OptionsCustomization.AllowSort = !isDataBlocked;
		DataGridView.OptionsCustomization.AllowGroup = !isDataBlocked;
		DataGridView.OptionsCustomization.AllowColumnMoving = !isDataBlocked;
		((Control)_FilterPanel).Enabled = !isDataBlocked;
		_RestrictionsPanel.SetEnabled(!isDataBlocked);
		if (!isDataBlocked)
		{
			_RestrictionsPanel.UpdateActionButtonsAvailability();
		}
	}

	private void OnDataViewContextUpdated()
	{
		InflexDataView dataView = DataView;
		if (dataView != null)
		{
			SaveDataQuery();
			UpdateAllowedStateTools();
			if (dataView.DataQuery is IInflexReportBasedDataQuery inflexReportBasedDataQuery)
			{
				_ContentType = inflexReportBasedDataQuery.Report.AssignedType;
			}
			if (Context != null)
			{
				Context.Type = _ContentType;
				Context.Update();
			}
		}
	}

	private void OnDataViewPulseUpdate()
	{
		RestrictionDataQuery restrictionDataQuery = DataView.DataQuery as RestrictionDataQuery;
		int count = DataView.DataTable.Rows.Count;
		if (restrictionDataQuery != null)
		{
			int prefetchCount = restrictionDataQuery.PrefetchCount;
			if (prefetchCount != 0)
			{
				int percent = count * 100 / prefetchCount;
				_SplashPanel.UpdateProgressValue(percent);
			}
			UpdateStatusText("Загружено строк: " + count + " из " + prefetchCount);
		}
		else
		{
			UpdateStatusText("Загружено строк: " + count);
		}
	}

	private void OnDataViewBeforeUpdateDataQuery()
	{
		_WatchTimer = Stopwatch.StartNew();
		_HeaderPanel.ControlBox.CanClose = false;
		Action action = delegate
		{
			((Control)_TotalSumsPanel).Visible = false;
		};
		if (((Control)_TotalSumsPanel).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)action);
		}
		else
		{
			action();
		}
		_RestrictionsPanel.SetEnabled(isEnabled: false);
		FreeStatusTextDefaultState();
		UpdateStatusText("Выполняется запрос данных...");
		ShowSplashPanel(splashPanelVisibility: true, "Сервер обрабатывает запрос...", canStop: false);
	}

	private void OnDataViewStartUpdate()
	{
		if (_WatchTimer != null && !_WatchTimer.IsRunning)
		{
			_WatchTimer = Stopwatch.StartNew();
		}
		_HeaderPanel.ControlBox.CanClose = true;
		((Control)_TotalSumsPanel).Visible = false;
		_RestrictionsPanel.SetEnabled(isEnabled: false);
		FreeStatusTextDefaultState();
		ShowSplashPanel(splashPanelVisibility: true, "Выполняется загрузка данных...", canStop: true);
		UpdateTitle();
		UpdateIcon();
	}

	internal void ShowSplashPanel(bool splashPanelVisibility, string message, bool canStop)
	{
		Action action = delegate
		{
			if (_SplashPanel != null)
			{
				((Control)DataGrid).Visible = !splashPanelVisibility;
				if (((Control)DataGrid).Visible)
				{
					((Control)DataGrid).Focus();
				}
				if (splashPanelVisibility)
				{
					_SplashPanel.SetSplashMessage(message, canStop);
					((Control)_SplashPanel).Show();
					((Control)_SplashPanel).Refresh();
				}
				else
				{
					((Control)_SplashPanel).Hide();
				}
			}
		};
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)action);
		}
		else
		{
			action();
		}
	}

	internal void ShowSplashPanel(bool splashPanelVisibility)
	{
		ShowSplashPanel(splashPanelVisibility, string.Empty, canStop: false);
	}

	private void UpdateIcon()
	{
		decimal num = 1m;
		if (DataView != null && DataView.DataQuery != null && DataView.DataQuery is IInflexReportBasedDataQuery)
		{
			IInflexReport report = ((IInflexReportBasedDataQuery)DataView.DataQuery).Report;
			if (report != null && report.AssignedType != null)
			{
				num = report.AssignedType.ID;
			}
		}
		object obj = Resources.ResourceManager.GetObject("_" + num, Resources.Culture) ?? Resources.ResourceManager.GetObject("_1", Resources.Culture);
		object obj2 = Resources.ResourceManager.GetObject("_" + num + "_Inactive", Resources.Culture) ?? Resources.ResourceManager.GetObject("_1_Inactive", Resources.Culture);
		_ActiveWindowStyle.IconImage = (Image)((obj is Bitmap) ? obj : null);
		_InactiveWindowStyle.IconImage = (Image)((obj2 is Bitmap) ? obj2 : null);
		_IconPictureBox.Image = CurrentWindowStyle.IconImage;
	}

	private void OnDataViewStopUpdate()
	{
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		string text = null;
		if (_WatchTimer != null)
		{
			_WatchTimer.Stop();
			text = _WatchTimer.Elapsed.TotalSeconds.ToString("N01");
		}
		UpdateStatusTextDefaultState();
		UpdateStatusText("Выполнено за " + text + " сек.");
		FindIndicators();
		ShowSplashPanel(splashPanelVisibility: false);
		RebuildTotalSumsPanel();
		UpdateRestrictionText();
		UpdateRestrictionColumns();
		_RestrictionsPanel.SetEnabled(isEnabled: true);
		_RestrictionsPanel.UpdateActionButtonsAvailability();
		if (DataView.LastException == _LastDataViewException)
		{
			return;
		}
		_LastDataViewException = DataView.LastException;
		if (_LastDataViewException == null)
		{
			return;
		}
		string text2 = _LastDataViewException.Message;
		for (Exception innerException = _LastDataViewException.InnerException; innerException != null; innerException = innerException.InnerException)
		{
			text2 = text2 + "\n" + innerException.Message;
		}
		if (DataView.DataQuery is ISqlQuery sqlQuery && !string.IsNullOrEmpty(sqlQuery.QuerySqlText))
		{
			string text3 = "";
			string[] array = text2.Replace("\r", "").Split(new char[1] { '\n' });
			foreach (string text4 in array)
			{
				text3 = text3 + "-- " + text4 + "\n";
			}
			text2 = text3 + sqlQuery.QuerySqlText;
		}
		UpdateStatusText("Ошибка при выполнении сформированного запроса", isError: true);
		ShowQueryTextForm showQueryTextForm = new ShowQueryTextForm(text2);
		((Form)showQueryTextForm).ShowDialog();
	}

	private void InitializeDataQuery(QueryRestriction restriction)
	{
		InitializeDataQuery(restriction, background: true, null);
	}

	private void InitializeDataQuery(QueryRestriction restriction, bool background, Action afterHandler)
	{
		ShowSplashPanel(splashPanelVisibility: true, "Сервер обрабатывает запрос...", canStop: false);
		if (background)
		{
			Thread thread = new Thread(ThreadInitializeDataQuery);
			thread.IsBackground = true;
			thread.Name = "InitializeDataQuery";
			Thread thread2 = (_InitDataQueryThread = thread);
			thread2.SetApartmentState(ApartmentState.STA);
			thread2.Start(new object[2] { restriction, afterHandler });
		}
		else
		{
			ThreadInitializeDataQuery(new object[2] { restriction, afterHandler });
		}
	}

	private void ThreadInitializeDataQuery(object obj)
	{
		object[] array = (object[])obj;
		QueryRestriction queryRestriction = (QueryRestriction)array[0];
		Action action = array[1] as Action;
		try
		{
			if (queryRestriction == null)
			{
				DataView.FreeData();
			}
			else
			{
				DataView.DataQuery = new RestrictionDataQuery(_AppManager, new RestrictionDataQueryRestriction(queryRestriction, GetRestrictionColumns(queryRestriction.Report)), QueryColumnProvider);
			}
		}
		finally
		{
			if (action != null && !((Control)this).IsDisposed && !_IsDisposed)
			{
				((Control)this).BeginInvoke((Delegate)action);
			}
		}
	}

	public void UpdateDataQuery(QueryRestriction restrict)
	{
		UpdateDataQueryInternal(restrict, background: true);
	}

	private void UpdateDataQueryInternal(QueryRestriction restrict, bool background)
	{
		DataView.DataQuery = new RestrictionDataQuery(_AppManager, new RestrictionDataQueryRestriction(restrict, GetRestrictionColumns(restrict.Report)), QueryColumnProvider);
		DataView.UpdateDataQuery(executeQuery: true, background);
	}

	private void RaiseBeforeLoaded()
	{
		BeforeLoaded?.Invoke(this);
	}

	private void UpdateTitle()
	{
		string text = null;
		if (DataView != null && DataView.DataQuery is IInflexReportBasedDataQuery)
		{
			IInflexReport report = ((IInflexReportBasedDataQuery)DataView.DataQuery).Report;
			IInflexReport inflexReport = report.RootReport ?? report;
			text = inflexReport.Name;
			if (report.RootReport != null)
			{
				text = text + "\\" + report.SubReportName;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "Данные";
		}
		if (!string.IsNullOrEmpty(DescriptionText))
		{
			text = ((!DescriptionText.Contains(text)) ? (text + " - " + DescriptionText) : DescriptionText);
		}
		string text2 = (((Control)_HeaderPanel).Text = text);
		((Control)this).Text = text2;
	}

	private IInflexColumn[] GetRestrictionColumns(IInflexReport report)
	{
		if (_ManualRestrictionColumns == null)
		{
			if (!_DefaultRestrictionColumns.TryGetValue(report, out var value))
			{
				value = (from c in report.GetExplorerColumns()
					where c.ShowColumn && c.IsRestriction()
					select c).ToArray();
				_DefaultRestrictionColumns.Add(report, value ?? new IInflexColumn[0]);
			}
			return value;
		}
		return _ManualRestrictionColumns;
	}

	private void SaveDataQuery()
	{
		if (_ManualRestrictionColumns == null)
		{
			_DefaultDataQuery = DataView.DataQuery;
		}
	}

	private void RestoreDataQuery()
	{
		InflexDataView dataView = DataView;
		if (dataView != null && _DefaultDataQuery != dataView.DataQuery)
		{
			dataView.DataQuery = _DefaultDataQuery;
		}
	}

	public void RestoreOriginalDataQuery()
	{
		if (_OriginalDataQuery != null && IsDataQueryChanged)
		{
			SetRestrictionColumns(null);
			DataView.DataQuery = _OriginalDataQuery;
			DataView.UpdateDataQuery(executeQuery: true, background: true);
		}
	}

	private void SetRestrictionColumns(IInflexColumn[] columns)
	{
		_ManualRestrictionColumns = columns;
	}

	private void FindIndicators()
	{
		if (!(DataView.DataQuery is IInflexReportBasedDataQuery inflexReportBasedDataQuery))
		{
			return;
		}
		List<IStyleLegendItem> list = new List<IStyleLegendItem>();
		IStyleLegendItem[] array = inflexReportBasedDataQuery.Report.GetStyleLegends().ToArray();
		AppliedIndicatorsCollection appliedIndicators = DataView.DataTable.AppliedIndicators;
		foreach (int item in appliedIndicators)
		{
			List<IStyleLegendItem> list2 = new List<IStyleLegendItem>();
			int localIndicator = item;
			IStyleLegendItem styleLegendItem = array.FirstOrDefault((IStyleLegendItem li) => li.Value == (decimal)localIndicator);
			if (styleLegendItem == null)
			{
				for (int num = 0; num < array.Length; num++)
				{
					int num2 = decimal.ToInt32(array[num].Value);
					if ((item & num2) == num2)
					{
						list2.Add(array[num]);
					}
				}
			}
			else
			{
				list2.Add(styleLegendItem);
			}
			if (list2.Count == 1)
			{
				list.Add(list2[0]);
			}
			else if (list2.Count > 1)
			{
				list.Add(new CombineStyleLegendItem(list2, item));
			}
		}
		StyleLegendPanel.ItemsSource = list;
	}

	private void UpdateRestrictionText()
	{
		InflexDataView dataView = DataView;
		if (dataView == null)
		{
			return;
		}
		IInflexDataQuery query = dataView.DataQuery;
		if (query == null)
		{
			return;
		}
		if (((Control)_RestrictionsPanel.RestrictionTextShort).InvokeRequired || ((Control)_RestrictionsPanel.RestrictionTextFull).InvokeRequired)
		{
			Action action = delegate
			{
				((Control)_RestrictionsPanel.RestrictionTextShort).Text = query.HumanyAdaptedRestrictionTextShort;
				((Control)_RestrictionsPanel.RestrictionTextFull).Text = query.HumanyAdaptedRestrictionTextFull;
			};
			((Control)this).Invoke((Delegate)action);
		}
		else
		{
			((Control)_RestrictionsPanel.RestrictionTextShort).Text = query.HumanyAdaptedRestrictionTextShort;
			((Control)_RestrictionsPanel.RestrictionTextFull).Text = query.HumanyAdaptedRestrictionTextFull;
		}
	}

	private void InitializeTools()
	{
		if (_ToolsBuilder == null)
		{
			_ToolsBuilder = new ExplorerWindowToolsBuilder();
			IEnumerable<InflexAction> toolActions = GetToolActions();
			_ToolsBuilder.Update(this, toolActions);
		}
	}

	private ActionCollection GetActions(Func<InflexAction, bool> toolActionsCondition, Func<InflexAction, bool> coreActionsCondition, params IInflexID[] ids)
	{
		ActionCollection actionCollection = new ActionCollection();
		ActionCollection coreActions = GetCoreActions(ids);
		if (coreActions != null)
		{
			if (coreActionsCondition != null)
			{
				for (int i = 0; i < coreActions.Count; i++)
				{
					InflexAction arg = coreActions[i];
					if (!coreActionsCondition(arg))
					{
						coreActions.RemoveAt(i);
						i--;
					}
				}
			}
			if (coreActions.Count > 0)
			{
				actionCollection.AddRange(coreActions.Where((InflexAction action) => !(action is NewInflexObjectAction)));
				actionCollection.DefaultAction = coreActions.DefaultAction;
			}
		}
		IEnumerable<InflexAction> toolActions = GetToolActions();
		if (toolActions != null)
		{
			actionCollection.AddRange((toolActionsCondition != null) ? toolActions.Where(toolActionsCondition) : toolActions);
		}
		if (_UserExplorerWindow != null)
		{
			foreach (InflexAction contextAction in _UserExplorerWindow.ContextActions)
			{
				contextAction.UnhandledActionException += OnUnhandledActionException;
				actionCollection.TryAdd(contextAction);
			}
		}
		return actionCollection;
	}

	private IEnumerable<InflexAction> GetToolActions()
	{
		if (Context == null)
		{
			_ToolActions = null;
			return new List<InflexAction>();
		}
		if (_ToolActions == null)
		{
			_ToolActions = new ActionCollection();
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (Type type in types)
			{
				if (!type.IsAbstract && type.IsSubclassOf(typeof(ExplorerWindowToolAction)) && Attribute.IsDefined(type, typeof(AutoInstanceActionAttribute)) && Activator.CreateInstance(type) is ExplorerWindowToolAction explorerWindowToolAction)
				{
					explorerWindowToolAction.UnhandledActionException += OnUnhandledActionException;
					explorerWindowToolAction.DirectContext = Context;
					_ToolActions.TryAdd(explorerWindowToolAction);
				}
			}
			try
			{
				string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExternalActions.xml");
				if (File.Exists(path))
				{
					using FileStream stream = File.OpenRead(path);
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExternalActionsDescription));
					if (xmlSerializer.Deserialize(stream) is ExternalActionsDescription { ActionAssemblies: var actionAssemblies })
					{
						foreach (string path2 in actionAssemblies)
						{
							string path3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path2);
							if (!File.Exists(path3))
							{
								continue;
							}
							Assembly assembly = Assembly.LoadFile(path3);
							if ((object)assembly == null)
							{
								continue;
							}
							Type[] types2 = assembly.GetTypes();
							Type[] array = types2;
							foreach (Type type2 in array)
							{
								if (!type2.IsAbstract && type2.IsSubclassOf(typeof(InflexAction)) && Attribute.IsDefined(type2, typeof(AutoInstanceActionAttribute)) && Activator.CreateInstance(type2) is InflexAction inflexAction)
								{
									inflexAction.UnhandledActionException += OnUnhandledActionException;
									inflexAction.Initialize(Context);
									_ToolActions.TryAdd(inflexAction);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				FastLog.Loger(this).Error((object)("Can't initialize defined external actions. Exception occurred: " + ex));
			}
			ActionCollection coreActions = GetCoreActions();
			foreach (InflexAction item in coreActions)
			{
				if (item is NewInflex32ObjectAction)
				{
					NewInflexObjectToolAction newInflexObjectToolAction = new NewInflexObjectToolAction();
					newInflexObjectToolAction.DirectContext = Context;
					newInflexObjectToolAction.SetInnerAction(item as NewInflexObjectAction);
					_ToolActions.TryAdd(newInflexObjectToolAction);
				}
				else if (item is ExplorerWindowToolAction)
				{
					_ToolActions.TryAdd(item);
				}
			}
		}
		return _ToolActions;
	}

	private ActionCollection GetCoreActions(params IInflexID[] ids)
	{
		if (Context == null)
		{
			_CoreActions = null;
			return null;
		}
		Context.ActionArgs.ObjectIDs = ids ?? new IInflexID[0];
		Context.Type = ((Context.ActionArgs.ObjectIDs.Length > 0) ? Context.ActionArgs.ObjectIDs[0].Type : _ContentType);
		Context.Update();
		if (_CoreActions == null)
		{
			_CoreActions = new ActionCollection();
			foreach (InflexAction action in _AppManager.ActionList.GetActions(Context))
			{
				action.UnhandledActionException += OnUnhandledActionException;
				if (action is ShowInflexObjectAction)
				{
					ShowInflexObjectToolAction showInflexObjectToolAction = new ShowInflexObjectToolAction((ShowInflexObjectAction)action);
					_CoreActions.Add(showInflexObjectToolAction);
					_CoreActions.DefaultAction = showInflexObjectToolAction;
				}
				else if (action is DeleteInflexObjectAction)
				{
					_CoreActions.Add(new DeleteToolAction((DeleteInflexObjectAction)action));
				}
				else if (action is ShowFastReportFormAction)
				{
					_CoreActions.Add(new ShowFastReportFormToolAction(action as ShowFastReportFormAction));
				}
				else if (action is ShowReferenceObjectAction)
				{
					_CoreActions.Add(new ShowReferenceObjectToolAction(action as ShowReferenceObjectAction));
				}
				else if (action is OuterReferenceAction)
				{
					_CoreActions.Add(new ShowLinksToolAction(action as OuterReferenceAction));
				}
				else
				{
					_CoreActions.Add(action);
				}
			}
		}
		return _CoreActions;
	}

	private void OnUnhandledActionException(Exception exception)
	{
		ExceptionBox.Show(exception);
	}

	private void InitializeUserTools()
	{
		_UserExplorerWindow = new UserExplorerWindow(this, Context, _Win32Handle);
		Assembly userExtentionAssembly = GetUserExtentionAssembly(this);
		if ((object)userExtentionAssembly == null)
		{
			((ToolStripItem)_ActionsPanel.UserControl).Enabled = false;
			return;
		}
		((ToolStripItem)_ActionsPanel.UserControl).Enabled = true;
		try
		{
			_UserExtention = (IExplorerWindowExtention)Activator.CreateInstance(userExtentionAssembly.GetType("ExplorerWindowExtention"));
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Warn((object)"CreateInstanceFromAndUnwrap", ex);
			return;
		}
		decimal? typeID = Context.Type.TypeID;
		_UserExtention.Initialize(typeID.HasValue ? decimal.ToInt32(typeID.Value) : 0, _UserExplorerWindow);
		ActionCollection actionCollection = new ActionCollection();
		ActionCollection actionCollection2 = new ActionCollection();
		foreach (InflexAction action in _UserExplorerWindow.Actions)
		{
			action.UnhandledActionException += OnUnhandledActionException;
			if (action is IAbstractSaveAction)
			{
				actionCollection.TryAdd(action);
			}
			else
			{
				actionCollection2.TryAdd(action);
			}
		}
		if (actionCollection.Count > 0)
		{
			ActionsPanel.SaveUserActions = actionCollection;
		}
		if (actionCollection2.Count > 0)
		{
			ActionsPanel.UserActions = actionCollection2;
		}
		else
		{
			((ToolStripItem)ActionsPanel.UserControl).Enabled = false;
		}
	}

	private static Assembly GetUserExtentionAssembly(ExplorerWindow currentWindow)
	{
		if ((object)_StaticUserAssembly != null)
		{
			return _StaticUserAssembly;
		}
		if ((object)currentWindow._UserAssembly == null)
		{
			try
			{
				using FileStream fileStream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExplorerWindow.UserExtention.dll"), FileMode.Open, FileAccess.Read);
				using MemoryStream memoryStream = new MemoryStream();
				int num = 8192;
				byte[] buffer = new byte[num];
				for (int num2 = fileStream.Read(buffer, 0, num); num2 != 0; num2 = fileStream.Read(buffer, 0, num))
				{
					memoryStream.Write(buffer, 0, num2);
				}
				memoryStream.Position = 0L;
				currentWindow._UserAssembly = Assembly.Load(memoryStream.ToArray());
				if ((object)currentWindow._UserAssembly != null)
				{
					DebuggableAttribute debuggableAttribute = (DebuggableAttribute)Attribute.GetCustomAttribute(currentWindow._UserAssembly, typeof(DebuggableAttribute));
					if (debuggableAttribute == null || (!debuggableAttribute.IsJITOptimizerDisabled && !debuggableAttribute.IsJITTrackingEnabled))
					{
						_StaticUserAssembly = currentWindow._UserAssembly;
					}
				}
			}
			catch (Exception ex)
			{
				FastLog.Loger(currentWindow).Warn((object)"GetUserExtentionAssembly", ex);
			}
		}
		return currentWindow._UserAssembly;
	}

	public void OnExternalDataChangeInflexHandler(int inflexTypeID, int itemID, ExternalDataChangeAction action)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!BeginProcessDataChanged())
		{
			return;
		}
		if (DataView == null)
		{
			FastLog.Loger(this).Error((object)"this.DataView == null!!!!!");
			MessageBox.Show("Error № 473.0", "Обратитесь в техподдержку! Сообщите номер ошибки!", (MessageBoxButtons)0, (MessageBoxIcon)16);
		}
		IInflexReportBasedDataQuery inflexReportBasedDataQuery = ((DataView != null) ? (DataView.DataQuery as IInflexReportBasedDataQuery) : null);
		if (inflexReportBasedDataQuery != null && inflexReportBasedDataQuery.Report == null)
		{
			FastLog.Loger(this).Error((object)"reportBased.Report == null!!!!!");
			MessageBox.Show("Error № 473.0", "Обратитесь в техподдержку! Сообщите номер ошибки!", (MessageBoxButtons)0, (MessageBoxIcon)16);
		}
		try
		{
			if (inflexReportBasedDataQuery == null || !inflexReportBasedDataQuery.Report.AssignedType.TypeID.HasValue || !(inflexReportBasedDataQuery.Report.AssignedType.TypeID.Value == (decimal)inflexTypeID))
			{
				return;
			}
			IInflexID inflexID = DataView.TryGetRowInflexID(itemID);
			if (inflexID != null)
			{
				if (action == ExternalDataChangeAction.DELETE)
				{
					DataView.HideRow(inflexID);
				}
				EndProcessDataChanged();
			}
			else
			{
				if (action != ExternalDataChangeAction.UPDATE && action != ExternalDataChangeAction.INSERT)
				{
					return;
				}
				DataView.EndUpdate += OnEndProcessDataChanged;
				RestrictionDataQuery restrictionDataQuery = (RestrictionDataQuery)DataView.DataQuery;
				RestrictByQuery restrictByQuery = (RestrictByQuery)restrictionDataQuery.Restriction;
				if (restrictByQuery.IsFullQuery)
				{
					DataView.UpdateDataQuery(executeQuery: true, background: true);
					return;
				}
				IInflexType type = _AppManager.GetType(inflexTypeID);
				string[] query = new string[1] { BuildNewItemSelectQuery(type.AssignedReport, itemID) };
				XmlDocument restrictionXml = CreateObjectsRestrictionXmlDoc(itemID);
				RestrictByQuery restrictByQuery2 = restrictByQuery.AddRestrictionQuery(type.AssignedReport, query, restrictionXml);
				if (restrictByQuery2 != null)
				{
					UpdateDataQuery(restrictByQuery2);
				}
			}
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)ex);
		}
	}

	private XmlDocument CreateObjectsRestrictionXmlDoc(int itemID)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<?xml version='1.0' encoding='windows-1251'?><FindNode><Critery Name='Объекты' Report_ID='1' SubType_ID='0' FieldType='0' AddRestrictionText='' R_Owner='' R_TableName='' R_ColumnName='' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' Spatial_PrimaryKey='' Expanded='Y' TopVisibleNode='Y' EnchancedSearch='N'><TemplateData ReferenceID='0' ReferenceType='' DisplayName='' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><cF>sm.Objects</cF><cS>Objects.ID</cS><TemplateData ReferenceID='0' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' />");
		stringBuilder.AppendFormat("<Critery Name='Идентификационный номер' Report_ID='-1' SubType_ID='0' FieldType='1' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText='{0}' FocusedNode='Y'><TemplateData ReferenceID='1' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.ID</S><RestrCond cond1='=' value1='{0}' value2='' cond2='=' /></Critery>", itemID);
		stringBuilder.Append("<Critery Name='Вид памятника' Report_ID='-1' SubType_ID='0' FieldType='9' R_Owner='SM' R_TableName='MONUMENTTYPES' R_ColumnName='ID' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='12' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.MNTYPE_ID</S></Critery><Critery Name='Дата начала существования ' Report_ID='-1' SubType_ID='0' FieldType='4' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='20' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.EXISTS_SINCE</S></Critery><Critery Name='Дата окончания существования ' Report_ID='-1' SubType_ID='0' FieldType='4' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='21' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.EXISTS_END</S></Critery><Critery Name='Заметки по памятнику' Report_ID='-1' SubType_ID='0' FieldType='3' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='13' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.MN_MEMO</S></Critery><Critery Name='Инвентарный номер' Report_ID='-1' SubType_ID='0' FieldType='3' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='4' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.INVNO</S></Critery><Critery Name='Код ОКОФ' Report_ID='-1' SubType_ID='0' FieldType='9' R_Owner='SM' R_TableName='CLS_OKOF' R_ColumnName='CODE' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='9' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.OKOF</S></Critery><Critery Name='Наименование памятника' Report_ID='-1' SubType_ID='0' FieldType='3' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='14' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.MN_NAME</S></Critery><Critery Name='Номер папки ' Report_ID='-1' SubType_ID='0' FieldType='8' R_Owner='SM.' R_TableName='OBJECTS' R_ColumnName='FOLDER' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='6' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.FOLDER</S></Critery><Critery Name='Норма амортизации' Report_ID='-1' SubType_ID='0' FieldType='2' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='8' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.WEARNORM</S></Critery><Critery Name='Пакет документов объекта' Report_ID='-1' SubType_ID='0' FieldType='9' R_Owner='SM' R_TableName='DOCSETS' R_ColumnName='ID' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='15' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.DOCSET_ID</S></Critery><Critery Name='Памятник' Report_ID='-1' SubType_ID='0' FieldType='6' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='11' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.IS_MONUMENT</S></Critery><Critery Name='Реестровый номер' Report_ID='-1' SubType_ID='0' FieldType='3' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='10' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.REGNO</S></Critery><Critery Name='Типы объектов' Report_ID='-1' SubType_ID='0' FieldType='9' R_Owner='SM' R_TableName='OBJTYPES' R_ColumnName='ID' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='2' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.OBJTYPES_ID</S></Critery><Critery Name='Шифр амортизации по классификатору' Report_ID='-1' SubType_ID='0' FieldType='3' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='5' ReferenceType='FM_V_TAB_COLUMNS' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>OBJECTS.WEARCODE</S></Critery><Critery Name='Остаточная стоимость' Report_ID='-1' SubType_ID='0' FieldType='2' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='7466' ReferenceType='SUMFIELD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>sm.GetObjectCost(objects.id,sysdate,'R')</S></Critery><Critery Name='Первонач.(восст.) стоимость' Report_ID='-1' SubType_ID='0' FieldType='2' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='7468' ReferenceType='SUMFIELD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>sm.GetObjectCost(objects.id,sysdate,'B')</S></Critery><Critery Name='Остаточная стоим. по дате' Report_ID='-1' SubType_ID='0' FieldType='2' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='7470' ReferenceType='SUMFIELD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>sm.GetObjectCost(objects.id,'{AD:DATE = Дата расчета ост. стоим.}','R')</S></Critery><Critery Name='Первонач.(восст.) стоим. по дате' Report_ID='-1' SubType_ID='0' FieldType='2' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='7471' ReferenceType='SUMFIELD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>sm.GetObjectCost(objects.id,'{BD:DATE = Дата расчета бал.стоим.}','B')</S></Critery><Critery Name='Оценочная стоимость' Report_ID='-1' SubType_ID='0' FieldType='2' UseForFullSearch='Y' UseForPartialSearch='N' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='7473' ReferenceType='SUMFIELD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>sm.GetObjectCost(objects.id,sysdate,'T')</S></Critery><Critery Name='Объекты-фантомы' Report_ID='-1' SubType_ID='0' FieldType='7' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='7472' ReferenceType='RESTRICTION' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>fake</S><W>0 = (\r\nselect\r\n count (*)\r\nfrom\r\n sm.movements\r\nwhere\r\n objects_id=objects.id and nvl(EndDate,sysdate+1)&gt;SYSDATE-60\r\n)</W></Critery><Critery Name='Запросить дату актуальности' Report_ID='-1' SubType_ID='0' FieldType='7' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' AddRestrictionText=''><TemplateData ReferenceID='8910' ReferenceType='RESTRICTION' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>fake</S><W>(Objects.EXISTS_SINCE is null or Objects.EXISTS_SINCE &lt;= '{TODDATE:DATE = Дата актуальности}') and (Objects.EXISTS_END is null or Objects.EXISTS_END &gt;= '{TODDATE}')</W></Critery><Critery Name='Прочее движимое имущество' Report_ID='1' SubType_ID='1' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='1' ReferenceType='SUBTYPE' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><cF>sm.Property</cF><cW>Property.Objects_ID=Objects.ID</cW></Critery><Critery Name='Транспортные средства' Report_ID='1' SubType_ID='2' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='2' ReferenceType='SUBTYPE' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><cF>sm.Transport</cF><cW>Transport.Objects_ID=Objects.ID</cW></Critery><Critery Name='Здания и помещения' Report_ID='1' SubType_ID='3' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='3' ReferenceType='SUBTYPE' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><cF>sm.Buildings</cF><cW>Buildings.Objects_ID=Objects.ID</cW></Critery><Critery Name='Незавершенное строительство' Report_ID='1' SubType_ID='4' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='4' ReferenceType='SUBTYPE' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><cF>sm.UnConstr</cF><cW>UnConstr.Objects_ID=Objects.ID</cW></Critery><Critery Name='Инженерная инфраструктура' Report_ID='1' SubType_ID='5' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='5' ReferenceType='SUBTYPE' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><cF>sm.EngInf</cF><cW>EngInf.Objects_ID=Objects.ID</cW></Critery><Critery Name='Юридические лица' Report_ID='1' SubType_ID='6' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='6' ReferenceType='SUBTYPE' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><cF>sm.Organisations|sm.Clients</cF><cW>Organisations.Objects_ID=Objects.ID and\r\nClients.ID=Organisations.Clients_ID</cW></Critery><Critery Name='Земельные участки' Report_ID='1' SubType_ID='7' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='0' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='7' ReferenceType='SUBTYPE' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><cF>sm.Grounds</cF><cW>Objects.ID=Grounds.Objects_ID</cW></Critery><Critery Name='Адрес' Report_ID='10' SubType_ID='0' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='8388608' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='7606' ReferenceType='REFERENCE_FWD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>objects.ADDRESS_ID</S><cF>sm.Address</cF><cS>nvl(Address.main_id,Address.ID)</cS></Critery><Critery Name='Собственник (текущий)' Report_ID='3' SubType_ID='0' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='8388608' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='7449' ReferenceType='REFERENCE_FWD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>op1.client_id</S><F>sm.object_position op1</F><W>op1.object_id(+)=Objects.ID and op1.movetype_id(+)=1</W><cF>sm.Clients</cF><cS>Clients.ID</cS></Critery><Critery Name='Арендатор (текущий)' Report_ID='3' SubType_ID='0' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='8388608' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='7453' ReferenceType='REFERENCE_FWD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>op4.client_id</S><F>sm.object_position op4</F><W>op4.object_id(+)=Objects.ID and op4.movetype_id(+)=4</W><cF>sm.Clients</cF><cS>Clients.ID</cS></Critery><Critery Name='Обслуживающая организация' Report_ID='3' SubType_ID='0' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='8388608' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='7462' ReferenceType='REFERENCE_FWD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>op8.client_id</S><F>sm.object_position op8</F><W>op8.object_id(+)=Objects.ID and op8.movetype_id(+)=8</W><cF>sm.Clients</cF><cS>Clients.ID</cS></Critery><Critery Name='Балансодержатель (текущий)' Report_ID='3' SubType_ID='0' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='8388608' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='7474' ReferenceType='REFERENCE_FWD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>op2.client_id</S><F>sm.object_position op2</F><W>op2.object_id(+)=Objects.ID and op2.movetype_id(+)=2</W><cF>sm.Clients</cF><cS>Clients.ID</cS></Critery><Critery Name='Безвозмездное пользование' Report_ID='3' SubType_ID='0' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='8388608' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='7824' ReferenceType='REFERENCE_FWD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>SC.ID</S><F>sm.Clients SC|sm.Movements SX</F><W>SX.MoveType_ID=21 and SX.Objects_ID=Objects.ID and\r\nSX.PostingDate&lt;=SYSDATE and (SX.EndDate IS NULL or SX.EndDate&gt;=SYSDATE) and\r\nSC.ID=SX.ClientsTo_ID</W><cF>sm.Clients</cF><cS>Clients.ID</cS></Critery><Critery Name='Пакет документов' Report_ID='18' SubType_ID='0' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='8388608' RelationType='1' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='7733' ReferenceType='REFERENCE_FWD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><S>sm.Objects.DocSet_id</S><cS>sm.DocSets.ID</cS><cF>sm.DocSets</cF></Critery><Critery Name='Движение объектов (Объект операции)' Report_ID='25' SubType_ID='0' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='32768' RelationType='2' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='7358' ReferenceType='REFERENCE_BWD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><cF>sm.MoveItems|sm.MovePeriods|sm.MoveSets MIMS</cF><cW>MoveItems.MovePeriod_ID=MovePeriods.ID and\r\nMovePeriods.MoveSet_ID=MIMS.ID</cW><cS>MoveItems.ID</cS><cW>MoveItems.Object_id=Objects.ID</cW></Critery><Critery Name='Состояние объектов (Объект)' Report_ID='6' SubType_ID='0' FieldType='0' UseForFullSearch='Y' UseForPartialSearch='Y' Color='32768' RelationType='2' Spatial_RestrictedSubType='0' HasChilds='Y' AddRestrictionText=''><TemplateData ReferenceID='7540' ReferenceType='REFERENCE_BWD' Ord='0' LevelSequenceID='0' HasChecked='N' FindVSTNode='0' /><cF>sm.States</cF><cS>States.ID</cS><cW>States.Objects_ID=Objects.ID</cW></Critery></Critery></FindNode>");
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(stringBuilder.ToString());
		return xmlDocument;
	}

	private void OnEndProcessDataChanged()
	{
		EndProcessDataChanged();
		DataView.EndUpdate -= OnEndProcessDataChanged;
	}

	private void EndProcessDataChanged()
	{
		lock (_DataChangedProcessSync)
		{
			_IsDataChangeMessageInProcess = false;
		}
	}

	private bool BeginProcessDataChanged()
	{
		lock (_DataChangedProcessSync)
		{
			if (!_IsDataChangeMessageInProcess)
			{
				_IsDataChangeMessageInProcess = true;
				return true;
			}
			return false;
		}
	}

	private string BuildNewItemSelectQuery(IInflexReport report, int id)
	{
		IInflexReport inflexReport = report.RootReport ?? report;
		string tableName = inflexReport.AssignedType.TableName;
		string tableOwner = inflexReport.AssignedType.TableOwner;
		return string.Format("select {1}.Id from {0}.{1} where {1}.Id={2}", tableOwner, tableName, id);
	}

	public bool OnExternalBarCodeInflexHandler(int inflexTypeID, int itemID, int actionID, string parameters)
	{
		return false;
	}

	public void UpdateAllowedStateTools()
	{
		if (_ToolsBuilder != null)
		{
			try
			{
				_ToolsBuilder.UpdateAllowedStateTool();
			}
			catch (Exception ex)
			{
				FastLog.Loger(this).Error((object)"_ToolsBuilder.UpdateAllowedStateTool();", ex);
			}
		}
	}

	private void DataGridViewOnColumnPositionChanged(object sender, EventArgs e)
	{
		if (!DataGrid.IsColumnsPopulated || DataGridView == null || !((Control)_TotalSumsPanel).Visible)
		{
			return;
		}
		GridColumn val = (GridColumn)((sender is GridColumn) ? sender : null);
		if (val == null)
		{
			return;
		}
		if (val.Visible)
		{
			if (((ColumnView)DataGridView).SelectedRowsCount > 1)
			{
				DataGrid.UpdateSelectedSummaries();
				_TotalSumsPanel.UpdateTotalSumPanel(DataGrid.SelectedSummaries, rebuildPanel: false, TotalSumsKinds.Selected);
			}
			else
			{
				DataGrid.UpdateTotalSummaries();
				_TotalSumsPanel.UpdateTotalSumPanel(DataGrid.TotalSummaries, rebuildPanel: false);
			}
		}
		else
		{
			RebuildTotalSumsPanel();
		}
	}

	private void DataGridViewOnColumnFilterChanged(object sender, EventArgs e)
	{
		UpdateStatusText();
		if (((Control)_TotalSumsPanel).Visible && DataGridView != null)
		{
			if (((ColumnView)DataGridView).SelectedRowsCount > 1)
			{
				DataGrid.UpdateSelectedSummaries();
				_TotalSumsPanel.UpdateTotalSumPanel(DataGrid.SelectedSummaries, rebuildPanel: false, TotalSumsKinds.Selected);
			}
			else
			{
				DataGrid.UpdateTotalSummaries();
				_TotalSumsPanel.UpdateTotalSumPanel(DataGrid.TotalSummaries, rebuildPanel: false, (!string.IsNullOrEmpty(((ColumnView)DataGridView).FindFilterText)) ? TotalSumsKinds.Filtered : TotalSumsKinds.All);
			}
		}
		UpdateAllowedStateTools();
	}

	private void InitializeStyleLegendPanel()
	{
		StyleLegendPanel styleLegendPanel = new StyleLegendPanel();
		((Control)styleLegendPanel).Dock = (DockStyle)5;
		StyleLegendPanel = styleLegendPanel;
		_ActionTableLayoutPanel.SetColumn((Control)(object)StyleLegendPanel, 1);
		_ActionTableLayoutPanel.SetRow((Control)(object)StyleLegendPanel, 0);
		((ControlCollection)_ActionTableLayoutPanel.Controls).Add((Control)(object)StyleLegendPanel);
	}

	private void InitializeTotalSumsPanel()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		TotalSumsPanel totalSumsPanel = new TotalSumsPanel();
		((Control)totalSumsPanel).AutoSize = true;
		((Control)totalSumsPanel).Anchor = (AnchorStyles)10;
		((Panel)totalSumsPanel).AutoSizeMode = (AutoSizeMode)0;
		((Control)totalSumsPanel).BackColor = Color.FromArgb(193, 228, 241);
		((Control)totalSumsPanel).Padding = new Padding(2);
		((Control)totalSumsPanel).Visible = false;
		_TotalSumsPanel = totalSumsPanel;
		((Control)this).Controls.Add((Control)(object)_TotalSumsPanel);
		SetDefaultTotalSumsPanelPosition();
		((Control)_TotalSumsPanel).BringToFront();
	}

	private void SetDefaultTotalSumsPanelPosition()
	{
		((Control)_TotalSumsPanel).Left = ((Form)this).ClientSize.Width - ((Control)_TotalSumsPanel).Width - 15;
		((Control)_TotalSumsPanel).Top = ((Form)this).ClientSize.Height - ((Control)_TotalSumsPanel).Height - 15;
	}

	private void RebuildTotalSumsPanel()
	{
		GridView dataGridView = DataGridView;
		if (dataGridView == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < ((ReadOnlyCollectionBase)(object)((ColumnView)dataGridView).VisibleColumns).Count; i++)
		{
			if (((ColumnView)dataGridView).VisibleColumns[i] is InflexGridColumn { DataColumn: { TotalSummMode: TotalSummModeEnum.Sum } })
			{
				flag = true;
			}
		}
		if (!flag)
		{
			((Control)_TotalSumsPanel).Visible = false;
			return;
		}
		DataGrid.UpdateTotalSummaries();
		_TotalSumsPanel.UpdateTotalSumPanel(DataGrid.TotalSummaries, rebuildPanel: true, (DataGridView != null && !string.IsNullOrEmpty(((ColumnView)DataGridView).FindFilterText)) ? TotalSumsKinds.Filtered : TotalSumsKinds.All);
		if (((Control)_TotalSumsPanel).Left + ((Control)_TotalSumsPanel).Width < ((Control)_TotalSumsPanel).Width / 2 || ((Control)_TotalSumsPanel).Top + ((Control)_TotalSumsPanel).Height < ((Control)_TotalSumsPanel).Height / 2 || ((Control)_TotalSumsPanel).Left > ((Form)this).ClientSize.Width - ((Control)_TotalSumsPanel).Width / 2 || ((Control)_TotalSumsPanel).Top > ((Form)this).ClientSize.Height - ((Control)_TotalSumsPanel).Height / 2)
		{
			SetDefaultTotalSumsPanelPosition();
		}
		((Control)_TotalSumsPanel).Visible = true;
		((Control)_TotalSumsPanel).Refresh();
	}

	private void InitializeRestrictionsPanel()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		RestrictionsPanel restrictionsPanel = new RestrictionsPanel(_DefaultActionsAccessor);
		((Control)restrictionsPanel).AutoSize = true;
		((UserControl)restrictionsPanel).AutoSizeMode = (AutoSizeMode)0;
		((Control)restrictionsPanel).Dock = (DockStyle)5;
		_RestrictionsPanel = restrictionsPanel;
		_RestrictionsPanel.SetRestrictionsPanel.RestrictionColumnsSelectionChanged += RestrictionsPanelOnSelectionChanged;
		((Control)_RestrictionsPanel).Margin = new Padding(5, 0, 0, 0);
		_MainPanel.SetColumn((Control)(object)_RestrictionsPanel, 1);
		_MainPanel.SetRow((Control)(object)_RestrictionsPanel, 1);
		((ControlCollection)_MainPanel.Controls).Add((Control)(object)_RestrictionsPanel);
		_RestrictionsPanel.SetEnabled(isEnabled: false);
	}

	private void RestrictionsPanelOnSelectionChanged(IInflexColumn[] restrictionColumns)
	{
		if (!(DataView.DataQuery is RestrictionDataQuery))
		{
			return;
		}
		SetRestrictionColumns(null);
		RestoreDataQuery();
		if (DataView.DataQuery is RestrictionDataQuery restrictionDataQuery2)
		{
			if (restrictionColumns != null)
			{
				SetRestrictionColumns(restrictionColumns);
			}
			UpdateDataQueryInternal(restrictionDataQuery2.Restriction, background: true);
		}
		else
		{
			DataView.UpdateDataQuery(executeQuery: true, background: true);
		}
	}

	private List<IInflexColumn> GetReportExplorerRestrictionColumns(IInflexReport report)
	{
		List<IInflexColumn> result = new List<IInflexColumn>();
		if (report != null)
		{
			try
			{
				result = new List<IInflexColumn>(from c in report.GetExplorerColumns()
					where c.IsRestriction()
					select c);
			}
			catch (Exception ex)
			{
				FastLog.Loger(this).Error((object)"IInflexReport.GetExplorerColumns().Where(c => c.IsRestriction())", ex);
			}
		}
		return result;
	}

	private void UpdateRestrictionColumns()
	{
		_RestrictionsPanel.SetRestrictionsPanel.ClearRestrictionColumns();
		if (!(DataView.DataQuery is RestrictionDataQuery restrictionDataQuery))
		{
			return;
		}
		IInflexColumn[] restrictionColumns = GetRestrictionColumns(restrictionDataQuery.Report);
		List<IInflexColumn> reportExplorerRestrictionColumns = GetReportExplorerRestrictionColumns(restrictionDataQuery.Report);
		reportExplorerRestrictionColumns.Sort(((Func<IInflexColumn, IInflexColumn, int>)delegate(IInflexColumn c1, IInflexColumn c2)
		{
			if (c1.SeqN.HasValue && c2.SeqN.HasValue)
			{
				int num = decimal.Compare(c1.SeqN.Value, c2.SeqN.Value);
				if (num != 0)
				{
					return num;
				}
			}
			return string.Compare(c1.ColumnName, c2.ColumnName);
		}).Invoke);
		foreach (IInflexColumn item in reportExplorerRestrictionColumns)
		{
			_RestrictionsPanel.SetRestrictionsPanel.AddRestrictionColumn(item, restrictionColumns?.Contains(item) ?? false);
		}
	}

	private void InitializeFilterPanel()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		FilterPanel filterPanel = new FilterPanel();
		((Control)filterPanel).Dock = (DockStyle)5;
		_FilterPanel = filterPanel;
		_FilterPanel.FilterTextChanged += FilterPanelOnFilterTextChanged;
		_FilterPanel.FilterEnabledChanged += FilterPanelOnFilterEnabledChanged;
		_FilterPanel.FocusChanged += FilterPanelOnFocusChanged;
		((Control)_FilterPanel).Margin = new Padding(2, 0, 3, 0);
		_MainPanel.SetColumn((Control)(object)_FilterPanel, 3);
		_MainPanel.SetRow((Control)(object)_FilterPanel, 1);
		((ControlCollection)_MainPanel.Controls).Add((Control)(object)_FilterPanel);
	}

	private void FilterPanelOnFocusChanged(bool isFocused)
	{
		if (Context != null)
		{
			if (isFocused)
			{
				_LastContextActionArgsObjectIDs = Context.ActionArgs.ObjectIDs;
				Context.ActionArgs.ObjectIDs = new IInflexID[0];
			}
			else
			{
				Context.ActionArgs.ObjectIDs = _LastContextActionArgsObjectIDs;
				_LastContextActionArgsObjectIDs = null;
			}
		}
	}

	private void FilterPanelOnFilterTextChanged(string filterText)
	{
		DataGrid.SearchManager.FilterText = filterText;
	}

	private void FilterPanelOnFilterEnabledChanged(bool isFilterEnabled)
	{
		DataGrid.SearchManager.IsFilterEnabled = isFilterEnabled;
	}

	private void DataGridOnFilterChanged(string filterText, bool isFilterEnabled)
	{
		_FilterPanel.FilterText = filterText;
	}

	private void FreeStatusTextDefaultState()
	{
		Action action = delegate
		{
			_StatusTextPrefixString = null;
			_IsStatusTextErrorState = false;
		};
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)action);
		}
		else
		{
			action();
		}
	}

	private void UpdateStatusTextDefaultState()
	{
		string text = string.Empty;
		if (DataView != null)
		{
			if (DataView.IsPartialQuery)
			{
				text = (DataView.IsBrokenQuery ? "Прерванная выборка. " : "Неполная выборка. ");
				if (DataView.DataQuery is RestrictionDataQuery restrictionDataQuery)
				{
					object obj = text;
					text = string.Concat(new object[4] { obj, "Всего: ", restrictionDataQuery.PrefetchCount, ". " });
				}
				object obj2 = text;
				text = string.Concat(new object[4]
				{
					obj2,
					"Загружено: ",
					DataView.DataTable.Rows.Count,
					". "
				});
				_IsStatusTextErrorState = true;
			}
			else
			{
				object obj3 = text;
				text = string.Concat(new object[4]
				{
					obj3,
					"Всего: ",
					DataView.DataTable.Rows.Count,
					". "
				});
			}
		}
		_StatusTextPrefixString = text;
	}

	public void UpdateStatusText()
	{
		UpdateStatusText(null, isError: false);
	}

	public void UpdateStatusText(string statusText)
	{
		UpdateStatusText(statusText, isError: false);
	}

	public void UpdateStatusText(string statusText, bool isError)
	{
		Action action = delegate
		{
			isError = isError || _IsStatusTextErrorState;
			string text = ((!string.IsNullOrEmpty(_StatusTextPrefixString) && !string.IsNullOrEmpty(((ColumnView)DataGridView).FindFilterText)) ? ("Отображено: " + ((BaseView)DataGridView).RowCount + ". ") : string.Empty);
			string text2 = ((((ColumnView)DataGridView).SelectedRowsCount > 1) ? ("Выделено: " + ((ColumnView)DataGridView).SelectedRowsCount + ". ") : string.Empty);
			if (isError)
			{
				((ToolStripItem)_StatusLabel).DisplayStyle = (ToolStripItemDisplayStyle)3;
				((ToolStripItem)_StatusLabel).ForeColor = _ErrorStatusTextColor;
			}
			else
			{
				((ToolStripItem)_StatusLabel).DisplayStyle = (ToolStripItemDisplayStyle)1;
				((ToolStripItem)_StatusLabel).ForeColor = Color.Snow;
			}
			string text3 = _StatusTextPrefixString + text + text2 + statusText;
			((ToolStripItem)_StatusLabel).Text = text3.Trim();
		};
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)action);
		}
		else
		{
			action();
		}
	}

	private void SetAllToolTips()
	{
		_RestrictionsPanel.SetToolTips(_ToolTip);
		_FilterPanel.SetToolTips(_ToolTip);
	}

	private void ClearAllToolTips()
	{
		_ToolTip.RemoveAll();
	}

	private void GisWindowOnClosed(object sender, EventArgs e)
	{
		IGisComponentWindow gisWindow = _GisWindow;
		if (gisWindow != null)
		{
			gisWindow.Closed -= GisWindowOnClosed;
			if (gisWindow is IDisposable disposable)
			{
				disposable.Dispose();
			}
			_GisWindow = null;
		}
		if (_IsGisWindowAlone)
		{
			((Form)this).Close();
		}
	}

	public void PreviewShowMap()
	{
		PreviewShowMap(-1);
	}

	public void PreviewShowMap(int workspaceID)
	{
		((Control)this).SetBoundsCore(-((Control)this).Width - 10, -((Control)this).Height - 10, ((Control)this).Width, ((Control)this).Height, (BoundsSpecified)3);
		_IsGisWindowAlone = true;
		_ShowMapHelper.InitializePreview(workspaceID);
	}

	private void InnerDispose(bool disposing)
	{
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Expected O, but got Unknown
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_050b: Expected O, but got Unknown
		lock (_DisposedSync)
		{
			if (!disposing)
			{
				return;
			}
			Application.RemoveMessageFilter((IMessageFilter)(object)this);
			Resources.ResourceManager.ReleaseAllResources();
			ClearAllToolTips();
			InflexClipboard.Updated -= InflexClipboardUpdated;
			((Form)this).Load -= InflexExplorerWindowLoaded;
			((Form)this).Activated -= OnActivated;
			((Form)this).Deactivate -= OnDeactivate;
			InflexDataGrid dataGrid = DataGrid;
			if (dataGrid != null)
			{
				GridView dataGridView = DataGridView;
				if (dataGridView != null)
				{
					((ColumnView)dataGridView).ColumnPositionChanged -= DataGridViewOnColumnPositionChanged;
					((ColumnView)dataGridView).ColumnFilterChanged -= DataGridViewOnColumnFilterChanged;
				}
				dataGrid.FilterChanged -= DataGridOnFilterChanged;
				dataGrid.SelectedRowsChanged -= OnGridSelectedRowsChanged;
				((Component)(object)dataGrid).Dispose();
			}
			DataGrid = null;
			InflexDataView dataView = DataView;
			if (dataView != null)
			{
				DataView.BeginUpdate -= OnDataViewContextUpdated;
				DataView.EndUpdate -= OnDataViewContextUpdated;
				DataView.DataQueryUpdated -= OnDataViewContextUpdated;
				DataView.BeginUpdate -= OnDataViewStartUpdate;
				DataView.PulseUpdate -= OnDataViewPulseUpdate;
				DataView.EndUpdate -= OnDataViewStopUpdate;
				DataView.BeforeUpdateDataQuery -= OnDataViewBeforeUpdateDataQuery;
				dataView.Dispose();
			}
			DataView = null;
			_DefaultActionsAccessor?.Dispose();
			_DefaultActionsAccessor = null;
			_GridActionsAccessor?.Dispose();
			_GridActionsAccessor = null;
			ActionCollection toolActions = _ToolActions;
			if (toolActions != null)
			{
				foreach (InflexAction item in toolActions)
				{
					item.UnhandledActionException -= OnUnhandledActionException;
				}
				toolActions.Clear();
			}
			_ToolActions = null;
			ActionCollection coreActions = _CoreActions;
			if (coreActions != null)
			{
				foreach (InflexAction item2 in coreActions)
				{
					item2.UnhandledActionException -= OnUnhandledActionException;
				}
				coreActions.Clear();
			}
			_CoreActions = null;
			_ToolsBuilder?.Dispose();
			_ToolsBuilder = null;
			_ShortcutActionActivator?.Dispose();
			_ShortcutActionActivator = null;
			TotalSumsPanel totalSumsPanel = _TotalSumsPanel;
			if (totalSumsPanel != null && !((Control)totalSumsPanel).IsDisposed)
			{
				((Component)(object)totalSumsPanel).Dispose();
			}
			_TotalSumsPanel = null;
			RestrictionsPanel restrictionsPanel = _RestrictionsPanel;
			if (restrictionsPanel != null)
			{
				restrictionsPanel.SetRestrictionsPanel.RestrictionColumnsSelectionChanged -= RestrictionsPanelOnSelectionChanged;
				((Component)(object)restrictionsPanel).Dispose();
			}
			_RestrictionsPanel = null;
			WindowHeaderControl headerPanel = _HeaderPanel;
			if (headerPanel != null)
			{
				((Control)headerPanel).MouseDown -= new MouseEventHandler(HeaderPanelOnMouseDown);
				((Component)(object)headerPanel).Dispose();
			}
			_HeaderPanel = null;
			FilterPanel filterPanel = _FilterPanel;
			if (filterPanel != null)
			{
				filterPanel.FilterEnabledChanged -= FilterPanelOnFilterEnabledChanged;
				filterPanel.FilterTextChanged -= FilterPanelOnFilterTextChanged;
				filterPanel.FocusChanged -= FilterPanelOnFocusChanged;
				((Component)(object)filterPanel).Dispose();
			}
			_FilterPanel = null;
			((Component)(object)_ActionsPanel)?.Dispose();
			_ActionsPanel = null;
			QueryColumnProvider?.Dispose();
			QueryColumnProvider = null;
			StyleLegendPanel styleLegendPanel = StyleLegendPanel;
			if (styleLegendPanel != null && !((Control)styleLegendPanel).IsDisposed)
			{
				((Component)(object)styleLegendPanel).Dispose();
			}
			StyleLegendPanel = null;
			IGisComponentWindow gisWindow = _GisWindow;
			if (gisWindow != null)
			{
				gisWindow.Close();
			}
			_GisWindow = null;
			LoadInfoSplashPanel splashPanel = _SplashPanel;
			if (splashPanel != null && !((Control)splashPanel).IsDisposed)
			{
				((Component)(object)splashPanel).Dispose();
			}
			_SplashPanel = null;
			_ShowMapHelper?.Dispose();
			_ShowMapHelper = null;
			FreeContext();
			_OriginalDataQuery = null;
			_InitializeRestriction = null;
			_LastDataViewException = null;
			_ManualRestrictionColumns = null;
			_DefaultDataQuery = null;
			_UserExtention = null;
			_UserAssembly = null;
			_ContentType = null;
			_AppManager = null;
			_WatchTimer = null;
			_LastContextActionArgsObjectIDs = null;
			_ActiveWindowStyle?.Dispose();
			_ActiveWindowStyle = null;
			_InactiveWindowStyle?.Dispose();
			_InactiveWindowStyle = null;
			((Component)(object)_ToolTip)?.Dispose();
			_ToolTip = null;
			if (_DefaultRestrictionColumns != null)
			{
				_DefaultRestrictionColumns.Clear();
				_DefaultRestrictionColumns = null;
			}
			PictureBox iconPictureBox = _IconPictureBox;
			if (iconPictureBox != null)
			{
				((Control)iconPictureBox).MouseDown -= new MouseEventHandler(IconPictureBoxOnMouseDown);
				iconPictureBox.Image = null;
			}
			_IconPictureBox = null;
			if (_StatusBar != null)
			{
				((ToolStrip)_StatusBar).Renderer = null;
			}
			ExplorerWindowContext context = Context;
			if (context != null)
			{
				context.DataBlockedChanged -= ContextOnDataBlockedChanged;
			}
			Context = null;
			UserExplorerWindow userExplorerWindow = _UserExplorerWindow;
			if (userExplorerWindow != null)
			{
				foreach (InflexAction action in userExplorerWindow.Actions)
				{
					action.UnhandledActionException -= OnUnhandledActionException;
				}
				foreach (InflexAction contextAction in userExplorerWindow.ContextActions)
				{
					contextAction.UnhandledActionException -= OnUnhandledActionException;
				}
				userExplorerWindow.Dispose();
			}
			_UserExplorerWindow = null;
			MenuStrip mainMenu = _MainMenu;
			if (mainMenu == null)
			{
				return;
			}
			for (int i = 0; i < ((ArrangedElementCollection)((ToolStrip)mainMenu).Items).Count; i++)
			{
				ToolStripItem val = ((ToolStrip)mainMenu).Items[i];
				if (!val.IsDisposed)
				{
					((Component)(object)val).Dispose();
				}
			}
			((ToolStrip)mainMenu).Items.Clear();
		}
	}

	public bool PreFilterMessage(ref Message m)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (((Message)(ref m)).Msg == 256)
		{
			Keys keys = (Keys)(int)((Message)(ref m)).WParam;
			return _ShortcutActionActivator.ActivateAction(keys);
		}
		return false;
	}

	public void Register(Keys keys, InflexAction action)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_ShortcutActionActivator.Register(keys, action);
	}

	public void UnRegister(Keys keys)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_ShortcutActionActivator.UnRegister(keys);
	}

	public bool ActivateAction(Keys keys)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _ShortcutActionActivator.ActivateAction(keys);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !((Control)this).IsDisposed)
		{
			if (components != null)
			{
				components.Dispose();
			}
			InnerDispose(disposing);
		}
		((Form)this).Dispose(disposing);
		GC.SuppressFinalize(this);
	}

	private void InitializeComponent()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Expected O, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Expected O, but got Unknown
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Expected O, but got Unknown
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Expected O, but got Unknown
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Expected O, but got Unknown
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Expected O, but got Unknown
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Expected O, but got Unknown
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Expected O, but got Unknown
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Expected O, but got Unknown
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Expected O, but got Unknown
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Expected O, but got Unknown
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_0580: Expected O, but got Unknown
		//IL_05d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0653: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_077c: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0898: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a2: Expected O, but got Unknown
		components = new Container();
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ExplorerWindow));
		_StatusBar = new StatusStrip();
		_StatusLabel = new ToolStripStatusLabel();
		_MainMenu = new MenuStrip();
		_GeneralLayoutPanel = new Panel();
		_MainPanel = new TableLayoutPanel();
		_ActionTableLayoutPanel = new TableLayoutPanel();
		_ActionsControlPanel = new Panel();
		_HeaderPanel = new WindowHeaderControl();
		_GridPanel = new Panel();
		_BottomPanel = new Panel();
		_BottomLinePanel = new LinePanel();
		_ToolTip = new ToolTip(components);
		((Control)_StatusBar).SuspendLayout();
		((Control)_GeneralLayoutPanel).SuspendLayout();
		((Control)_MainPanel).SuspendLayout();
		((Control)_ActionTableLayoutPanel).SuspendLayout();
		((Control)_BottomPanel).SuspendLayout();
		((Control)this).SuspendLayout();
		((ToolStrip)_StatusBar).BackColor = Color.Transparent;
		((ToolStrip)_StatusBar).Items.AddRange((ToolStripItem[])(object)new ToolStripItem[1] { (ToolStripItem)_StatusLabel });
		((Control)_StatusBar).Location = new Point(0, 578);
		((Control)_StatusBar).Name = "_StatusBar";
		((Control)_StatusBar).Size = new Size(970, 22);
		((Control)_StatusBar).TabIndex = 0;
		((Control)_StatusBar).Text = "statusStrip1";
		((ToolStripItem)_StatusLabel).BackColor = Color.Transparent;
		((ToolStripItem)_StatusLabel).DisplayStyle = (ToolStripItemDisplayStyle)1;
		((ToolStripItem)_StatusLabel).Font = new Font("Verdana", 8f, (FontStyle)0, (GraphicsUnit)3, (byte)204);
		((ToolStripItem)_StatusLabel).ForeColor = Color.Snow;
		((ToolStripItem)_StatusLabel).Image = (Image)(object)Resources.StatusError;
		((ToolStripItem)_StatusLabel).ImageScaling = (ToolStripItemImageScaling)0;
		((ToolStripItem)_StatusLabel).Margin = new Padding(3, 3, 0, 2);
		((ToolStripItem)_StatusLabel).Name = "_StatusLabel";
		((ToolStripItem)_StatusLabel).Size = new Size(0, 17);
		((Control)_MainMenu).Location = new Point(0, 0);
		((Control)_MainMenu).Name = "_MainMenu";
		((Control)_MainMenu).Size = new Size(970, 24);
		((Control)_MainMenu).TabIndex = 1;
		((Control)_MainMenu).Text = "menuStrip1";
		((Control)_MainMenu).Visible = false;
		((Control)_GeneralLayoutPanel).BackColor = Color.Transparent;
		((Control)_GeneralLayoutPanel).Controls.Add((Control)(object)_MainPanel);
		((Control)_GeneralLayoutPanel).Dock = (DockStyle)5;
		((Control)_GeneralLayoutPanel).Location = new Point(0, 0);
		((Control)_GeneralLayoutPanel).Margin = new Padding(1);
		((Control)_GeneralLayoutPanel).Name = "_GeneralLayoutPanel";
		((Control)_GeneralLayoutPanel).Size = new Size(970, 578);
		((Control)_GeneralLayoutPanel).TabIndex = 2;
		((Control)_MainPanel).BackColor = Color.Transparent;
		_MainPanel.ColumnCount = 3;
		_MainPanel.ColumnStyles.Add(new ColumnStyle());
		_MainPanel.ColumnStyles.Add(new ColumnStyle((SizeType)2, 100f));
		_MainPanel.ColumnStyles.Add(new ColumnStyle());
		_MainPanel.Controls.Add((Control)(object)_ActionTableLayoutPanel, 1, 2);
		_MainPanel.Controls.Add((Control)(object)_HeaderPanel, 1, 0);
		_MainPanel.Controls.Add((Control)(object)_GridPanel, 0, 3);
		_MainPanel.Controls.Add((Control)(object)_BottomPanel, 0, 4);
		((Control)_MainPanel).Dock = (DockStyle)5;
		((Control)_MainPanel).Location = new Point(0, 0);
		((Control)_MainPanel).Name = "_MainPanel";
		_MainPanel.RowCount = 5;
		_MainPanel.RowStyles.Add(new RowStyle((SizeType)1, 27f));
		_MainPanel.RowStyles.Add(new RowStyle());
		_MainPanel.RowStyles.Add(new RowStyle());
		_MainPanel.RowStyles.Add(new RowStyle((SizeType)2, 100f));
		_MainPanel.RowStyles.Add(new RowStyle());
		((Control)_MainPanel).Size = new Size(970, 578);
		((Control)_MainPanel).TabIndex = 3;
		((Control)_ActionTableLayoutPanel).AutoSize = true;
		((Panel)_ActionTableLayoutPanel).AutoSizeMode = (AutoSizeMode)0;
		_ActionTableLayoutPanel.ColumnCount = 2;
		_MainPanel.SetColumnSpan((Control)(object)_ActionTableLayoutPanel, 2);
		_ActionTableLayoutPanel.ColumnStyles.Add(new ColumnStyle((SizeType)2, 100f));
		_ActionTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
		_ActionTableLayoutPanel.Controls.Add((Control)(object)_ActionsControlPanel, 0, 0);
		((Control)_ActionTableLayoutPanel).Dock = (DockStyle)5;
		((Control)_ActionTableLayoutPanel).Location = new Point(5, 27);
		((Control)_ActionTableLayoutPanel).Margin = new Padding(3, 0, 0, 0);
		((Control)_ActionTableLayoutPanel).Name = "_ActionTableLayoutPanel";
		_ActionTableLayoutPanel.RowCount = 1;
		_ActionTableLayoutPanel.RowStyles.Add(new RowStyle((SizeType)2, 100f));
		((Control)_ActionTableLayoutPanel).Size = new Size(965, 1);
		((Control)_ActionTableLayoutPanel).TabIndex = 8;
		((Control)_ActionsControlPanel).AutoSize = true;
		_ActionsControlPanel.AutoSizeMode = (AutoSizeMode)0;
		((Control)_ActionsControlPanel).Location = new Point(0, 0);
		((Control)_ActionsControlPanel).Margin = new Padding(0);
		((Control)_ActionsControlPanel).Name = "_ActionsControlPanel";
		((Control)_ActionsControlPanel).Size = new Size(0, 0);
		((Control)_ActionsControlPanel).TabIndex = 5;
		((Control)_HeaderPanel).BackColor = Color.Transparent;
		_MainPanel.SetColumnSpan((Control)(object)_HeaderPanel, 2);
		((Control)_HeaderPanel).Dock = (DockStyle)5;
		((Control)_HeaderPanel).Location = new Point(2, 0);
		((Control)_HeaderPanel).Margin = new Padding(0);
		((Control)_HeaderPanel).Name = "_HeaderPanel";
		((Control)_HeaderPanel).Size = new Size(968, 27);
		((Control)_HeaderPanel).TabIndex = 6;
		((Control)_HeaderPanel).TabStop = false;
		_MainPanel.SetColumnSpan((Control)(object)_GridPanel, 3);
		((Control)_GridPanel).Dock = (DockStyle)5;
		((Control)_GridPanel).Location = new Point(3, 27);
		((Control)_GridPanel).Margin = new Padding(3, 0, 3, 0);
		((Control)_GridPanel).Name = "_GridPanel";
		((Control)_GridPanel).Size = new Size(964, 547);
		((Control)_GridPanel).TabIndex = 9;
		((Control)_BottomPanel).BackColor = Color.Transparent;
		_MainPanel.SetColumnSpan((Control)(object)_BottomPanel, 3);
		((Control)_BottomPanel).Controls.Add((Control)(object)_BottomLinePanel);
		((Control)_BottomPanel).Dock = (DockStyle)5;
		((Control)_BottomPanel).Location = new Point(0, 577);
		((Control)_BottomPanel).Margin = new Padding(0, 3, 0, 0);
		((Control)_BottomPanel).Name = "_BottomPanel";
		((Control)_BottomPanel).Size = new Size(970, 1);
		((Control)_BottomPanel).TabIndex = 10;
		((Control)_BottomLinePanel).Anchor = (AnchorStyles)13;
		_BottomLinePanel.LineColor = Color.White;
		((Control)_BottomLinePanel).Location = new Point(0, 0);
		((Control)_BottomLinePanel).Margin = new Padding(0);
		((Control)_BottomLinePanel).Name = "_BottomLinePanel";
		((Control)_BottomLinePanel).Size = new Size(925, 1);
		((Control)_BottomLinePanel).TabIndex = 0;
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)0;
		((Control)this).BackColor = SystemColors.InactiveCaptionText;
		((Form)this).ClientSize = new Size(970, 600);
		((Form)this).ControlBox = false;
		((Control)this).Controls.Add((Control)(object)_GeneralLayoutPanel);
		((Control)this).Controls.Add((Control)(object)_StatusBar);
		((Control)this).Controls.Add((Control)(object)_MainMenu);
		((Form)this).Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
		((Control)this).MinimumSize = new Size(400, 200);
		((Control)this).Name = "ExplorerWindow";
		((Control)_StatusBar).ResumeLayout(false);
		((Control)_StatusBar).PerformLayout();
		((Control)_GeneralLayoutPanel).ResumeLayout(false);
		((Control)_MainPanel).ResumeLayout(false);
		((Control)_MainPanel).PerformLayout();
		((Control)_ActionTableLayoutPanel).ResumeLayout(false);
		((Control)_ActionTableLayoutPanel).PerformLayout();
		((Control)_BottomPanel).ResumeLayout(false);
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
