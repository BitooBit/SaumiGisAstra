using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.XtraGrid.Columns;
using InflexCore;
using InflexData;
using WinFormsExplorerWindow.Config;
using WinFormsExplorerWindow.PrimitiveControls;

namespace WinFormsExplorerWindow;

public sealed class QueryColumnCollectionProvider : IQueryColumnCollectionProvider, IDisposable
{
	private AppManager _AppManager;

	private ExplorerWindow _ExplorerWindow;

	private Dictionary<IInflexReport, QueryColumnCollection> _Collection = new Dictionary<IInflexReport, QueryColumnCollection>();

	private List<IInflexReport> _NeadLoad = new List<IInflexReport>();

	private bool _InflexExplorerWindowLoaded;

	private readonly object _SyncObject = new object();

	private readonly object _DisposeSync = new object();

	private bool _IsDisposed;

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

	public event Action<IInflexReport> QueryColumnsChange;

	public QueryColumnCollectionProvider(AppManager appManager, ExplorerWindow explorerWindow)
	{
		_AppManager = appManager;
		_ExplorerWindow = explorerWindow;
	}

	public QueryColumnCollection GetQueryColumns(IInflexReport report)
	{
		lock (_SyncObject)
		{
			if (!_Collection.TryGetValue(report, out var value))
			{
				value = LoadQueryColumns(report);
				if (value == null)
				{
					_NeadLoad.Add(report);
					value = GetQueryColumnCollection(report);
				}
				_Collection.Add(report, value);
				if (_NeadLoad.Contains(report))
				{
					StartQueryColumnsLoading(report);
				}
			}
			return value;
		}
	}

	public void ResetQueryColumns(IInflexReport report)
	{
		lock (_SyncObject)
		{
			_Collection.Remove(report);
		}
		OnQueryColumnsChange(report);
	}

	public void SetQueryColumns(IInflexReport report, QueryColumnCollection queryColumns)
	{
		lock (_SyncObject)
		{
			_Collection.Remove(report);
			_Collection.Add(report, queryColumns);
		}
		OnQueryColumnsChange(report);
	}

	private void OnQueryColumnsChange(IInflexReport report)
	{
		QueryColumnsChange?.Invoke(report);
	}

	private QueryColumnCollection GetQueryColumnCollection(IInflexReport report)
	{
		QueryColumnCollection queryColumnCollection = new QueryColumnCollection();
		foreach (IInflexColumn item in from c in report.GetExplorerColumns()
			where c.ShowColumn && c.CanShow() && !c.IsRestriction()
			select c)
		{
			queryColumnCollection.Add(new QueryColumn(item, visible: true));
		}
		return queryColumnCollection;
	}

	public void SaveDefaultQueryColumns(IInflexReport report)
	{
		string name = ((object)_ExplorerWindow).GetType().Name;
		string userName = _AppManager.UserName;
		_AppManager.Accessor<ConfigAccessor>().SaveDefaultColumns(report, name, userName);
	}

	public void RemoveQueryColumns(IInflexReport report)
	{
		_AppManager.Accessor<ConfigAccessor>().RemoveColumns(report, ((object)_ExplorerWindow).GetType().Name, _AppManager.UserName);
	}

	public void SaveQueryColumns(IInflexReport report, QueryColumnCollection queryColumns)
	{
		IList<GridColumn> realColumns = _ExplorerWindow.DataGrid.RealColumns;
		_AppManager.Accessor<ConfigAccessor>().SaveColumns(report, ((object)_ExplorerWindow).GetType().Name, _AppManager.UserName, queryColumns, realColumns);
	}

	private QueryColumnCollection LoadQueryColumns(IInflexReport report)
	{
		QueryColumnCollection result = _AppManager.Accessor<ConfigAccessor>().LoadColumns(report, ((object)_ExplorerWindow).GetType().Name, _AppManager.UserName, out var gridColumnsSettings);
		foreach (KeyValuePair<string, Tuple<string, string, string, string, string>> item in gridColumnsSettings)
		{
			int columnGroupIndex = (string.IsNullOrEmpty(item.Value.Item1) ? (-1) : int.Parse(item.Value.Item1));
			int columnSortIndex = (string.IsNullOrEmpty(item.Value.Item2) ? (-1) : int.Parse(item.Value.Item2));
			int isColumnSortOrderDescending = int.Parse(item.Value.Item3);
			int columnVisibleIndex = (string.IsNullOrEmpty(item.Value.Item4) ? (-1) : int.Parse(item.Value.Item4));
			int columnWidth = int.Parse(item.Value.Item5);
			_ExplorerWindow.DataGrid.SetDefaultColumnContext(item.Key, columnGroupIndex, columnSortIndex, isColumnSortOrderDescending, columnVisibleIndex, columnWidth);
		}
		return result;
	}

	private void StartQueryColumnsLoading(IInflexReport report)
	{
		lock (_SyncObject)
		{
			if (!_NeadLoad.Contains(report) || !_InflexExplorerWindowLoaded)
			{
				return;
			}
			QueryColumnCollection queryColumns = GetQueryColumns(report);
			InflexReportTreeWindow inflexReportTreeWindow = new InflexReportTreeWindow();
			try
			{
				inflexReportTreeWindow.RememberAsDefault = true;
				if (inflexReportTreeWindow.ShowWindow(report, queryColumns) == true)
				{
					SetQueryColumns(report, inflexReportTreeWindow.ResultQueryColumnCollection);
					if (inflexReportTreeWindow.RememberAsDefault)
					{
						SaveQueryColumns(report, queryColumns);
					}
				}
				_AppManager.Accessor<ConfigAccessor>().SaveDefaultColumns(report, ((object)_ExplorerWindow).GetType().Name, _AppManager.UserName);
				_NeadLoad.Remove(report);
			}
			finally
			{
				((IDisposable)inflexReportTreeWindow)?.Dispose();
			}
		}
	}

	public void OnWindowLoaded()
	{
		lock (_SyncObject)
		{
			_InflexExplorerWindowLoaded = true;
			IInflexReport[] array = _NeadLoad.ToArray();
			IInflexReport[] array2 = array;
			foreach (IInflexReport item in array2)
			{
				_NeadLoad.Remove(item);
			}
		}
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
			if (_IsDisposed)
			{
				return;
			}
			if (disposing)
			{
				Dictionary<IInflexReport, QueryColumnCollection> collection = _Collection;
				if (collection != null)
				{
					collection.Clear();
					_Collection = null;
				}
				List<IInflexReport> neadLoad = _NeadLoad;
				if (neadLoad != null)
				{
					neadLoad.Clear();
					_NeadLoad = null;
				}
				_ExplorerWindow = null;
				_AppManager = null;
			}
			_IsDisposed = true;
		}
	}
}
