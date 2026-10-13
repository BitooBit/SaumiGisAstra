using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using InflexCore;
using InflexCore.DataImplementation.Accessors;
using InflexCore.log;
using InflexData.Handlers;
using InflexData.RestrictionQuery;

namespace InflexData;

public sealed class InflexDataView : IDisposable, IInflexDataView
{
	private delegate void FlushLoadDataHandler(InflexDataTable dataTable, ref IInflexID[] bufferIDs, ref decimal[] bufferIDIDs, ref object[][] bufferValues, int count, int indicatorIndex);

	private class Synchronizer
	{
		public bool Stoped { get; set; }

		public bool Stop { get; set; }
	}

	private AppManager _AppManager;

	private IQueryColumnCollectionProvider _QueryColumnsProvider;

	private readonly Synchronizer _Synchronizer = new Synchronizer();

	private VirtualDataStorage _DataStorage;

	private IInflexDataQuery _DataQuery;

	private bool _OnBeforeUpdateExecuting;

	private bool _OnBeginUpdate;

	private bool _OnEndUpdate;

	private readonly object _DisposeSync = new object();

	private bool _IsDisposed;

	private readonly object _UpdateDataQuerySync = new object();

	private BackgroundWorker _InsertBackgroundWorker;

	public IInflexDataQuery DataQuery
	{
		get
		{
			return _DataQuery;
		}
		set
		{
			_DataQuery = value;
			DataQueryUpdated?.Invoke();
		}
	}

	public bool IsBrokenQuery { get; private set; }

	public bool IsPartialQuery
	{
		get
		{
			if (_DataStorage != null && DataQuery is RestrictionDataQuery restrictionDataQuery)
			{
				return _DataStorage.Count != restrictionDataQuery.PrefetchCount;
			}
			return false;
		}
	}

	private InflexDataSet DataSet { get; set; }

	public InflexDataTable DataTable { get; private set; }

	public Exception LastException { get; private set; }

	public bool HasHierarchicalData
	{
		get
		{
			if (DataQuery != null)
			{
				bool flag = false;
				bool flag2 = false;
				foreach (InflexDataColumn column in DataQuery.Columns)
				{
					if (((IGridColumn)column).InnerTreeKey)
					{
						flag = true;
					}
					if (((IGridColumn)column).OuterTreeKey)
					{
						flag2 = true;
					}
					if (flag && flag2)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

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

	public event Action DataQueryUpdated;

	public event Action BeforeUpdateDataQuery;

	public event Action DataSourceChanged;

	public event AllowingInsertEventHandler AllowingInsert;

	public event InsertingEventHandler Inserting;

	public event Action BeginUpdate;

	public event Action EndUpdate;

	public event Action PulseUpdate;

	public InflexDataView(AppManager appManager, IQueryColumnCollectionProvider queryColumnsProvider)
	{
		_AppManager = appManager;
		_QueryColumnsProvider = queryColumnsProvider;
		_OnBeginUpdate = false;
		_OnEndUpdate = true;
		lock (_Synchronizer)
		{
			_Synchronizer.Stoped = true;
			Monitor.PulseAll(_Synchronizer);
		}
	}

	private void OnBeforeUpdateDataQuery()
	{
		_OnBeforeUpdateExecuting = true;
		BeforeUpdateDataQuery?.Invoke();
	}

	public void UpdateDataQuery(bool executeQuery, bool background)
	{
		if (background)
		{
			lock (_UpdateDataQuerySync)
			{
				OnBeforeUpdateDataQuery();
				FreeDataSet();
				IInflexDataQuery dataQuery = DataQuery;
				Action action = delegate
				{
					DataSet = NewInflexDataSet(dataQuery);
					DataTable = DataSet.MainTable;
					_DataStorage = (HasHierarchicalData ? new VirtualTreeDataStorage(DataTable, ((IInflexReportBasedDataQuery)dataQuery).Report.AssignedType) : new VirtualDataStorage(DataTable));
				};
				Action action2 = delegate
				{
					UpdateGrid(executeQuery, background);
				};
				ParameterizedThreadStart start = delegate(object obj)
				{
					if (obj is object[] array && array.Length == 3)
					{
						object obj2 = array[0];
						Dispatcher val = (Dispatcher)((obj2 is Dispatcher) ? obj2 : null);
						Action action3 = array[1] as Action;
						Action action4 = array[2] as Action;
						if (val != null && action3 != null && action4 != null)
						{
							try
							{
								Dispatcher.CurrentDispatcher.Invoke((Delegate)action3, new object[0]);
							}
							finally
							{
								val.Invoke((Delegate)action4, new object[0]);
							}
						}
					}
				};
				Thread thread = new Thread(start);
				thread.IsBackground = true;
				thread.Start(new object[3]
				{
					Dispatcher.CurrentDispatcher,
					action,
					action2
				});
				return;
			}
		}
		OnBeforeUpdateDataQuery();
		FreeDataSet();
		DataSet = NewInflexDataSet(DataQuery);
		DataTable = DataSet.MainTable;
		if (HasHierarchicalData)
		{
			_DataStorage = new VirtualTreeDataStorage(DataTable, ((IInflexReportBasedDataQuery)DataQuery).Report.AssignedType);
		}
		else
		{
			_DataStorage = new VirtualDataStorage(DataTable);
		}
		UpdateGrid(executeQuery, background);
	}

	public IEnumerable<IInflexID> GetInflexIDs()
	{
		if (_DataStorage != null)
		{
			return _DataStorage.GetInflexIDs();
		}
		return new List<IInflexID>();
	}

	private InflexDataSet NewInflexDataSet(IInflexDataQuery dataQuery)
	{
		InflexDataSet inflexDataSet = new InflexDataSet();
		inflexDataSet.MainTable = AddTable(inflexDataSet, dataQuery);
		return inflexDataSet;
	}

	private InflexDataTable AddTable(InflexDataSet dataSet, IInflexDataQuery inflexDataQuery)
	{
		InflexDataTable inflexDataTable = new InflexDataTable(this);
		foreach (InflexDataColumn column in inflexDataQuery.Columns)
		{
			inflexDataTable.Columns.Add(column);
			IGridColumn gridColumn = column;
			if (gridColumn.IsIndicator)
			{
				inflexDataTable.IndicatorColumn = column;
			}
		}
		dataSet.Tables.Add(inflexDataTable);
		if (inflexDataQuery.SubQuery != null)
		{
			throw new NotSupportedException();
		}
		return inflexDataTable;
	}

	private void FireDataSourceChanged()
	{
		DataSourceChanged?.Invoke();
	}

	private void UpdateGrid(bool executeQuery, bool background)
	{
		FireDataSourceChanged();
		if (executeQuery)
		{
			RefreshQuery(background);
		}
	}

	private void ExecuteDataQuery(IInflexDataQuery dataQuery, bool background)
	{
		InflexDataTable dataTable = DataTable;
		Stop(wait: true);
		if (!IsLinked(dataTable, dataQuery))
		{
			throw new DataViewSynchronizeException();
		}
		OnBeginUpdate();
		IsBrokenQuery = false;
		_DataStorage.Clear();
		try
		{
			if (background)
			{
				Thread thread = new Thread(BackgroundLoadData);
				thread.IsBackground = true;
				thread.Name = "Background loader";
				PrepareBackgroundStart();
				thread.Start(new object[3]
				{
					Dispatcher.CurrentDispatcher,
					dataTable,
					dataQuery
				});
			}
			else
			{
				ReadDataQuery(dataQuery);
			}
		}
		catch
		{
			SetLastException(dataQuery as RestrictionDataQuery);
			dataTable.EndLoadData();
			OnEndUpdate();
			throw;
		}
	}

	private void SetLastException(RestrictionDataQuery dataQuery)
	{
		if (dataQuery != null)
		{
			LastException = dataQuery.LastPrepareException;
		}
	}

	private bool IsLinked(InflexDataTable dataTable, IInflexDataQuery inflexQuery)
	{
		if (dataTable.Columns.Count != inflexQuery.Columns.Count())
		{
			return false;
		}
		foreach (InflexDataColumn column in dataTable.Columns)
		{
			if (!inflexQuery.Columns.Contains(column))
			{
				return false;
			}
		}
		return true;
	}

	private void PrepareBackgroundStart()
	{
		lock (_Synchronizer)
		{
			_Synchronizer.Stop = false;
			_Synchronizer.Stoped = false;
			Monitor.PulseAll(_Synchronizer);
		}
	}

	private void BackgroundLoadData(object obj)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Invalid comparison between Unknown and I4
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		object[] array = (object[])obj;
		Dispatcher val = (Dispatcher)array[0];
		InflexDataTable inflexDataTable = (InflexDataTable)array[1];
		IInflexDataQuery inflexDataQuery = (IInflexDataQuery)array[2];
		List<DispatcherOperation> list = new List<DispatcherOperation>();
		bool flag = false;
		try
		{
			if (val == null)
			{
				return;
			}
			InvokeBeginLoadData(inflexDataTable, val);
			int num = ((inflexDataTable.IndicatorColumn == null) ? (-1) : inflexDataTable.Columns.IndexOf(inflexDataTable.IndicatorColumn));
			FlushLoadDataHandler flushLoadDataHandler = ((num == -1) ? new FlushLoadDataHandler(FlushLoadData) : new FlushLoadDataHandler(FlushLoadDataCalcIndicator));
			int num2 = 100;
			IInflexID[] array2 = null;
			decimal[] array3 = null;
			object[][] array4 = null;
			int num3 = 0;
			lock (_Synchronizer)
			{
				if (_Synchronizer.Stop)
				{
					return;
				}
			}
			using (IEnumerator<InflexQueryResultRow> enumerator = inflexDataQuery.GetEnumerator())
			{
				try
				{
					enumerator.Reset();
				}
				catch (NotImplementedException ex)
				{
					FastLog.Loger(this).Error((object)"IEnumerator<InflexQueryResultRow> iterator = dataQuery.GetEnumerator(); iterator.Reset();", (Exception)ex);
				}
				while (enumerator.MoveNext())
				{
					InflexQueryResultRow current = enumerator.Current;
					if (num3 == 0)
					{
						array2 = new IInflexID[num2];
						array3 = new decimal[num2];
						array4 = new object[num2][];
					}
					array2[num3] = current.ID;
					ref decimal reference = ref array3[num3];
					reference = current.ID.ID;
					array4[num3] = current.Values;
					num3++;
					if (num3 == num2)
					{
						lock (_Synchronizer)
						{
							if (_Synchronizer.Stop)
							{
								return;
							}
						}
						list.Add(val.BeginInvoke((DispatcherPriority)9, (Delegate)flushLoadDataHandler, (object)inflexDataTable, new object[5] { array2, array3, array4, num3, num }));
						num3 = 0;
					}
					lock (_Synchronizer)
					{
						if (_Synchronizer.Stop)
						{
							return;
						}
					}
				}
			}
			if (num3 > 0)
			{
				list.Add(val.BeginInvoke((DispatcherPriority)9, (Delegate)flushLoadDataHandler, (object)inflexDataTable, new object[5] { array2, array3, array4, num3, num }));
			}
			flag = true;
		}
		finally
		{
			foreach (DispatcherOperation item in list)
			{
				if ((int)item.Status == 0)
				{
					lock (_Synchronizer)
					{
						if (_Synchronizer.Stop)
						{
							item.Abort();
						}
						else
						{
							item.Wait();
						}
					}
				}
				else if ((int)item.Status == 3)
				{
					item.Wait();
				}
			}
			lock (_Synchronizer)
			{
				_Synchronizer.Stoped = true;
				Monitor.PulseAll(_Synchronizer);
			}
			if (val != null && flag)
			{
				val.Invoke((Delegate)new Action<bool>(CheckIntegrity), new object[1] { true });
			}
			InvokeEndLoadData(inflexDataTable, val, inflexDataQuery);
		}
	}

	private void CheckIntegrity(bool isBackgroundLoading)
	{
		if ((!isBackgroundLoading || (!_Synchronizer.Stop && _Synchronizer.Stoped)) && _DataStorage != null && DataQuery is RestrictionDataQuery)
		{
			RestrictionDataQuery restrictionDataQuery = (RestrictionDataQuery)DataQuery;
			int count = _DataStorage.Count;
			int prefetchCount = restrictionDataQuery.PrefetchCount;
			if (count != prefetchCount)
			{
				_AppManager.Accessor<InflexLogMessagesAccessor>()?.Log(string.Format("Количество записей ({0}), загруженных в результате выполнения запроса для отчета \"{1}\" (ID {2}) с подотчетом \"{3}\" (ID {4}), отличается от количества записей ({5}) соответствующей предварительной выборки", new object[6]
				{
					count,
					restrictionDataQuery.Report.Name,
					restrictionDataQuery.Report.ID,
					restrictionDataQuery.Report.SubReportName,
					restrictionDataQuery.Report.SubReportID,
					prefetchCount
				}), "InflexDataView.CheckIntegrity()", 1m);
			}
		}
	}

	private void InvokeBeginLoadData(InflexDataTable dataTable, Dispatcher dispatcher)
	{
		if (dispatcher != null)
		{
			dispatcher.Invoke((Delegate)new Action(OnBeginUpdate), new object[0]);
			if (dataTable != null)
			{
				dispatcher.Invoke((Delegate)new Action(dataTable.BeginLoadData), new object[0]);
			}
			VirtualDataStorage dataStorage = _DataStorage;
			if (dataStorage != null)
			{
				dispatcher.Invoke((Delegate)new Action(dataStorage.BeginLoadData), new object[0]);
			}
		}
	}

	private void InvokeEndLoadData(InflexDataTable dataTable, Dispatcher dispatcher, IInflexDataQuery query)
	{
		if (dispatcher != null)
		{
			dispatcher.Invoke((Delegate)new Action<RestrictionDataQuery>(SetLastException), new object[1] { query as RestrictionDataQuery });
			VirtualDataStorage dataStorage = _DataStorage;
			if (dataStorage != null)
			{
				dispatcher.Invoke((Delegate)new Action(dataStorage.EndLoadData), new object[0]);
			}
			if (dataTable != null)
			{
				dispatcher.Invoke((Delegate)new Action(dataTable.EndLoadData), new object[0]);
			}
			dispatcher.Invoke((Delegate)new Action(OnEndUpdate), new object[0]);
		}
	}

	private void FlushLoadData(InflexDataTable dataTable, ref IInflexID[] bufferIDs, ref decimal[] bufferIDIDs, ref object[][] bufferValues, int count, int indicatorIndex)
	{
		for (int i = 0; i < count; i++)
		{
			try
			{
				_DataStorage.LoadData(bufferIDIDs[i], bufferIDs[i], bufferValues[i]);
			}
			catch (Exception ex)
			{
				FastLog.Loger(this).Warn((object)"FlushLoadData", ex);
			}
		}
		OnPulseUpdate();
	}

	private void FlushLoadDataCalcIndicator(InflexDataTable dataTable, ref IInflexID[] bufferIDs, ref decimal[] bufferIDIDs, ref object[][] bufferValues, int count, int indicatorIndex)
	{
		for (int i = 0; i < count; i++)
		{
			try
			{
				_DataStorage.LoadData(bufferIDIDs[i], bufferIDs[i], bufferValues[i], indicatorIndex);
			}
			catch (Exception ex)
			{
				FastLog.Loger(this).Warn((object)"FlushLoadDataCalcIndicator", ex);
			}
		}
		OnPulseUpdate();
	}

	public IInflexID GetRowInflexID(decimal rowPrimaryValue)
	{
		return _DataStorage.GetInflexID(rowPrimaryValue);
	}

	public IInflexID TryGetRowInflexID(decimal rowPrimaryValue)
	{
		if (_DataStorage.ContainsID(rowPrimaryValue))
		{
			return _DataStorage.GetInflexID(rowPrimaryValue);
		}
		return null;
	}

	private void FreeDataSet()
	{
		Stop(wait: true);
		if (DataSet != null)
		{
			DataSet.Dispose();
			DataSet = null;
		}
	}

	public void FreeData()
	{
		DataQuery = null;
		DataTable = null;
		_DataStorage = null;
	}

	private void Free()
	{
		FreeDataSet();
		FreeData();
	}

	public bool AllowRemove(params IInflexID[] inflexIDs)
	{
		return AllowEdit(inflexIDs);
	}

	private bool AllowEdit(params IInflexID[] inflexIDs)
	{
		if (inflexIDs == null || inflexIDs.Length == 0)
		{
			return true;
		}
		IInflexType inflexType = DataQuery.InflexType;
		foreach (IInflexID inflexID in inflexIDs)
		{
			if (inflexID.Type.IsTypecast(inflexType))
			{
				return true;
			}
		}
		return false;
	}

	public bool AllowInsert(params IInflexID[] inflexIDs)
	{
		if (AllowEdit(inflexIDs))
		{
			return FireAllowingInsert(inflexIDs);
		}
		return false;
	}

	private bool FireAllowingInsert(IInflexID[] inflexIDs)
	{
		AllowingInsertEventHandler allowingInsert = AllowingInsert;
		if (allowingInsert != null)
		{
			AllowingInsertEventArgs args = new AllowingInsertEventArgs(inflexIDs);
			return allowingInsert(args);
		}
		return true;
	}

	private void FireInserting(IInflexID[] inflexIDs)
	{
		InsertingEventHandler inserting = Inserting;
		if (inserting != null)
		{
			InsertingEventArgs args = new InsertingEventArgs(inflexIDs);
			inserting(args);
		}
	}

	public bool InExecute()
	{
		if (_OnBeginUpdate)
		{
			return !_OnEndUpdate;
		}
		return false;
	}

	public bool QueryInPrepare()
	{
		return _OnBeforeUpdateExecuting;
	}

	public void Append(params IInflexID[] inflexIDs)
	{
		if (inflexIDs == null)
		{
			return;
		}
		IInflexID[] inflexIDs2 = inflexIDs.Where((IInflexID inflexID) => !_DataStorage.ContainsID(inflexID.ID)).ToArray();
		FireInserting(inflexIDs2);
		IInflexDataQuery inflexDataQuery = DataQuery.MapInflexIDs(inflexIDs2);
		foreach (InflexQueryResultRow item in inflexDataQuery)
		{
			_DataStorage.Add(item.ID.ID, item.ID, item.Values);
		}
	}

	public void Insert(params IInflexID[] inflexIDs)
	{
		if (inflexIDs != null)
		{
			if (_InsertBackgroundWorker == null)
			{
				_InsertBackgroundWorker = new BackgroundWorker();
				_InsertBackgroundWorker.DoWork += InsertBackgroundWorkerDoWork;
				_InsertBackgroundWorker.RunWorkerCompleted += InsertBackgroundWorkerRunWorkerCompleted;
			}
			_InsertBackgroundWorker.RunWorkerAsync(inflexIDs);
		}
	}

	private void InsertBackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (e.Result is IInflexDataQuery dataQuery)
		{
			DataQuery = dataQuery;
			UpdateDataQuery(executeQuery: true, background: true);
		}
	}

	private void InsertBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
	{
		IInflexDataQuery result = InsertDirect(e.Argument as IInflexID[]);
		e.Result = result;
	}

	private IInflexDataQuery InsertDirect(IInflexID[] insertedIDs)
	{
		HashSet<IInflexID> hashSet = new HashSet<IInflexID>(_DataStorage.GetInflexIDs());
		int count = hashSet.Count;
		foreach (IInflexID inflexID in insertedIDs)
		{
			if (!_DataStorage.ContainsID(inflexID.ID))
			{
				hashSet.Add(inflexID);
			}
		}
		if (hashSet.Count == count)
		{
			return null;
		}
		FireInserting(insertedIDs);
		return DataQuery.MapInflexIDs(hashSet.ToArray());
	}

	public void Remove(params IInflexID[] inflexIDs)
	{
		if (!(DataQuery is IInflexReportBasedDataQuery inflexReportBasedDataQuery))
		{
			return;
		}
		IInflexType currentType = inflexReportBasedDataQuery.Report.AssignedType;
		inflexIDs = inflexIDs.Where((IInflexID id) => id.Type.IsTypecast(currentType)).ToArray();
		if (inflexIDs == null || inflexIDs.Length == 0)
		{
			return;
		}
		InflexDataTable dataTable = DataTable;
		_ = DataQuery;
		int index = GetIndex(dataTable);
		if (index < 0)
		{
			return;
		}
		HashSet<decimal> hashSet = new HashSet<decimal>(inflexIDs.Select((IInflexID aa) => aa.ID));
		Stop(wait: true);
		List<int> list = new List<int>();
		List<decimal> list2 = new List<decimal>();
		List<object[]> list3 = new List<object[]>(dataTable.Rows.Count - inflexIDs.Length);
		for (int num = 0; num < dataTable.Rows.Count; num++)
		{
			object[] itemArray = dataTable.Rows[num].ItemArray;
			if (decimal.TryParse((itemArray[index] == null) ? null : itemArray[index].ToString(), out var result) && hashSet.Contains(result))
			{
				list.Add(num);
				list2.Add(result);
				hashSet.Remove(result);
			}
			else
			{
				list3.Add(itemArray);
			}
		}
		NewQuery(inflexReportBasedDataQuery, currentType, list3, list, list2);
	}

	private int GetIndex(InflexDataTable dataTable)
	{
		int result = -1;
		for (int i = 0; i < dataTable.Columns.Count; i++)
		{
			if (dataTable.Columns[i] is IGridColumn { IsPrimary: not false })
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void ReadDataQuery(IInflexDataQuery dataQuery)
	{
		OnBeginUpdate();
		InflexDataTable dataTable = DataTable;
		dataTable.BeginLoadData();
		try
		{
			using (IEnumerator<InflexQueryResultRow> enumerator = dataQuery.GetEnumerator())
			{
				enumerator.Reset();
				int num = ((dataTable.IndicatorColumn == null) ? (-1) : dataTable.Columns.IndexOf(dataTable.IndicatorColumn));
				if (num == -1)
				{
					while (enumerator.MoveNext())
					{
						InflexQueryResultRow current = enumerator.Current;
						_DataStorage.LoadData(current.ID.ID, current.ID, current.Values);
					}
				}
				else
				{
					while (enumerator.MoveNext())
					{
						InflexQueryResultRow current2 = enumerator.Current;
						_DataStorage.LoadData(current2.ID.ID, current2.ID, current2.Values, num);
					}
				}
			}
			CheckIntegrity(isBackgroundLoading: false);
		}
		finally
		{
			SetLastException(dataQuery as RestrictionDataQuery);
			dataTable.EndLoadData();
			OnEndUpdate();
		}
	}

	private void OnBeginUpdate()
	{
		if (_OnEndUpdate)
		{
			_OnBeforeUpdateExecuting = false;
			_OnEndUpdate = false;
			_OnBeginUpdate = true;
			BeginUpdate?.Invoke();
		}
	}

	private void OnEndUpdate()
	{
		if (_OnBeginUpdate)
		{
			_OnBeginUpdate = false;
			_OnEndUpdate = true;
			EndUpdate?.Invoke();
		}
	}

	private void OnPulseUpdate()
	{
		PulseUpdate?.Invoke();
	}

	public void Stop(bool wait)
	{
		lock (_Synchronizer)
		{
			if (_Synchronizer.Stoped)
			{
				return;
			}
			_Synchronizer.Stop = true;
			Monitor.PulseAll(_Synchronizer);
			IsBrokenQuery = true;
		}
		if (!wait)
		{
			return;
		}
		int num = 0;
		do
		{
			lock (_Synchronizer)
			{
				if (_Synchronizer.Stoped)
				{
					return;
				}
				Monitor.Wait(_Synchronizer, 1000);
			}
			num++;
		}
		while (num != 10);
		throw new StopBackgroundLoadDataException();
	}

	public bool AllowHold(params IInflexID[] inflexIDs)
	{
		if (AllowEdit(inflexIDs) && inflexIDs != null)
		{
			return inflexIDs.Length > 0;
		}
		return false;
	}

	public void Hold(params IInflexID[] inflexIDs)
	{
		if (!(DataQuery is IInflexReportBasedDataQuery inflexReportBasedDataQuery))
		{
			return;
		}
		IInflexType currentType = inflexReportBasedDataQuery.Report.AssignedType;
		inflexIDs = inflexIDs.Where((IInflexID id) => id.Type.IsTypecast(currentType)).ToArray();
		if (inflexIDs == null || inflexIDs.Length == 0)
		{
			return;
		}
		InflexDataTable dataTable = DataTable;
		_ = DataQuery;
		int index = GetIndex(dataTable);
		if (index < 0)
		{
			return;
		}
		HashSet<decimal> hashSet = new HashSet<decimal>(inflexIDs.Select((IInflexID aa) => aa.ID));
		Stop(wait: true);
		List<int> list = new List<int>();
		List<decimal> list2 = new List<decimal>();
		List<object[]> list3 = new List<object[]>(dataTable.Rows.Count - inflexIDs.Length);
		for (int num = 0; num < dataTable.Rows.Count; num++)
		{
			object[] itemArray = dataTable.Rows[num].ItemArray;
			if (decimal.TryParse((itemArray[index] == null) ? null : itemArray[index].ToString(), out var result) && hashSet.Contains(result))
			{
				list3.Add(itemArray);
				hashSet.Remove(result);
				continue;
			}
			list.Add(num);
			list2.Add(result);
			if (hashSet.Count == 0 && list.Count < list3.Count)
			{
				break;
			}
		}
		NewQuery(inflexReportBasedDataQuery, currentType, list3, list, list2);
	}

	private void NewQuery(IInflexReportBasedDataQuery reportBased, IInflexType currentType, List<object[]> rows, List<int> removeIndex, List<decimal> removeValue)
	{
		IInflexDataQuery dataQuery = DataQuery;
		ManualFilledDataQuery manualFilledDataQuery = new ManualFilledDataQuery(rows, dataQuery.Columns, currentType, _AppManager, _QueryColumnsProvider);
		manualFilledDataQuery.SetQueryColumns(reportBased.QueryColumns);
		manualFilledDataQuery.SetReport(reportBased.Report);
		DataQuery = manualFilledDataQuery;
		UpdateDataQuery(executeQuery: true, background: true);
	}

	public void RefreshQuery(bool background)
	{
		IInflexDataQuery dataQuery = DataQuery;
		if (DataTable != null && dataQuery != null)
		{
			ExecuteDataQuery(dataQuery, background);
		}
	}

	public void HideRow(IInflexID inflexID)
	{
		_DataStorage.HideRow(inflexID);
		if (DataQuery is ISupportHideRow supportHideRow)
		{
			supportHideRow.HideRow(inflexID);
		}
	}

	public void FindNodes(List<string> searchStrings)
	{
		if (_DataStorage is VirtualTreeDataStorage virtualTreeDataStorage)
		{
			virtualTreeDataStorage.FindNodes(searchStrings);
		}
	}

	public void Expand(InflexTreeNode node)
	{
		if (_DataStorage is VirtualTreeDataStorage virtualTreeDataStorage)
		{
			virtualTreeDataStorage.Expand(node);
		}
	}

	public void Collapse(InflexTreeNode node)
	{
		if (_DataStorage is VirtualTreeDataStorage virtualTreeDataStorage)
		{
			virtualTreeDataStorage.Collapse(node);
		}
	}

	public void ExpandAll()
	{
		if (_DataStorage is VirtualTreeDataStorage virtualTreeDataStorage)
		{
			virtualTreeDataStorage.ExpandAll();
		}
	}

	public void CollapseAll()
	{
		if (_DataStorage is VirtualTreeDataStorage virtualTreeDataStorage)
		{
			virtualTreeDataStorage.CollapseAll();
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
				BeginUpdate = null;
				EndUpdate = null;
				PulseUpdate = null;
				BackgroundWorker insertBackgroundWorker = _InsertBackgroundWorker;
				if (insertBackgroundWorker != null)
				{
					insertBackgroundWorker.DoWork -= InsertBackgroundWorkerDoWork;
					insertBackgroundWorker.RunWorkerCompleted -= InsertBackgroundWorkerRunWorkerCompleted;
					insertBackgroundWorker.Dispose();
					_InsertBackgroundWorker = null;
				}
				Free();
			}
			_QueryColumnsProvider = null;
			_AppManager = null;
			_IsDisposed = true;
		}
	}
}
