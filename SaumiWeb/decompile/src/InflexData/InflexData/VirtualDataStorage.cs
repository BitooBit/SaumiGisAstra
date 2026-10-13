using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InflexCore;

namespace InflexData;

internal class VirtualDataStorage
{
	private readonly InflexDataTable _MainTable;

	private readonly Dictionary<decimal, IInflexID> _InflexIDs = new Dictionary<decimal, IInflexID>();

	private readonly Dictionary<IInflexID, DataRow> _InflexIdRows = new Dictionary<IInflexID, DataRow>();

	protected InflexDataTable MainTable => _MainTable;

	protected Dictionary<decimal, IInflexID> InflexIDs => _InflexIDs;

	protected Dictionary<IInflexID, DataRow> InflexIdRows => _InflexIdRows;

	public int Count => InflexIDs.Count;

	public VirtualDataStorage(InflexDataTable mainTable)
	{
		_MainTable = mainTable;
	}

	public bool ContainsID(decimal id)
	{
		return InflexIDs.ContainsKey(id);
	}

	public IInflexID GetInflexID(decimal id)
	{
		return InflexIDs[id];
	}

	public IEnumerable<IInflexID> GetInflexIDs()
	{
		return InflexIDs.Select((KeyValuePair<decimal, IInflexID> inflexID) => inflexID.Value);
	}

	public IEnumerable<decimal> GetIDs()
	{
		return InflexIDs.Select((KeyValuePair<decimal, IInflexID> inflexID) => inflexID.Key);
	}

	public void Clear()
	{
		MainTable.BeginLoadData();
		MainTable.AppliedIndicators.Clear();
		InflexIDs.Clear();
		MainTable.Rows.Clear();
		InflexIdRows.Clear();
		MainTable.EndLoadData();
	}

	public virtual void BeginLoadData()
	{
	}

	public virtual void EndLoadData()
	{
	}

	public virtual void LoadData(decimal id, IInflexID inflexID, object[] values, int? indicatorIndex = null)
	{
		Add(id, inflexID, values, indicatorIndex);
	}

	public virtual void Add(decimal id, IInflexID inflexID, object[] values, int? indicatorIndex = null)
	{
		InflexIDs.Add(id, inflexID);
		DataRow dataRow = MainTable.Rows.Add(values);
		InflexIdRows.Add(inflexID, dataRow);
		if (indicatorIndex.HasValue)
		{
			MainTable.AppliedIndicators.Add(Convert.ToInt16(MainTable.GetRowValue(dataRow, indicatorIndex.Value)));
		}
	}

	public void InsertAt(decimal id, IInflexID inflexID, object[] values, int position, int? indicatorIndex = null)
	{
		InflexIDs.Add(id, inflexID);
		MainTable.BeginLoadData();
		DataRow dataRow = MainTable.NewRow();
		dataRow.ItemArray = values;
		MainTable.Rows.InsertAt(dataRow, position);
		MainTable.AcceptChanges();
		MainTable.EndLoadData();
		InflexIdRows.Add(inflexID, dataRow);
		if (indicatorIndex.HasValue)
		{
			MainTable.AppliedIndicators.Add(Convert.ToInt16(MainTable.GetRowValue(dataRow, indicatorIndex.Value)));
		}
	}

	public void HideRow(IInflexID inflexID)
	{
		MainTable.BeginLoadData();
		if (InflexIdRows.TryGetValue(inflexID, out var value))
		{
			MainTable.Rows.Remove(value);
			InflexIdRows.Remove(inflexID);
			InflexIDs.Remove(inflexID.ID);
		}
		MainTable.AcceptChanges();
		MainTable.EndLoadData();
	}

	public void HideRows(IInflexID inflexID, int count)
	{
		MainTable.BeginLoadData();
		if (InflexIdRows.TryGetValue(inflexID, out var value))
		{
			int num = MainTable.Rows.IndexOf(value);
			for (int i = num; i < num + count; i++)
			{
				decimal key = (decimal)MainTable.Rows[num][MainTable.PrimaryColumn];
				MainTable.Rows.RemoveAt(num);
				InflexIdRows.Remove(_InflexIDs[key]);
				InflexIDs.Remove(key);
			}
		}
		MainTable.AcceptChanges();
		MainTable.EndLoadData();
	}
}
