using System.Data;

namespace InflexData;

public class InflexDataTable : DataTable
{
	public delegate void BeginLoadDataHandler();

	public delegate void EndLoadDataHandler();

	private InflexDataColumn _PrimaryColumn;

	private readonly AppliedIndicatorsCollection _AppliedIndicators = new AppliedIndicatorsCollection();

	public InflexDataColumn IndicatorColumn { get; set; }

	public InflexDataColumn PrimaryColumn
	{
		get
		{
			if (_PrimaryColumn == null)
			{
				foreach (IGridColumn column in base.Columns)
				{
					if (column.IsPrimary)
					{
						_PrimaryColumn = (InflexDataColumn)column;
					}
				}
			}
			return _PrimaryColumn;
		}
	}

	public AppliedIndicatorsCollection AppliedIndicators => _AppliedIndicators;

	private InflexDataView DataView { get; set; }

	public event BeginLoadDataHandler BeginLoadDataEvent;

	public event EndLoadDataHandler EndLoadDataEvent;

	public InflexDataTable(InflexDataView dataView)
	{
		DataView = dataView;
	}

	protected override void Dispose(bool disposing)
	{
		base.Columns.Clear();
		DataView = null;
		base.Dispose(disposing);
	}

	public object GetRowValue(DataRow row, int index)
	{
		return row[index];
	}

	public object GetRowValue(DataRow row, string columnName)
	{
		return row[columnName];
	}

	public object GetRowValue(DataRow row, InflexDataColumn column)
	{
		return row[column];
	}

	public object GetRowValue(int rowIndex, int columnIndex)
	{
		return base.Rows[rowIndex][columnIndex];
	}

	public static object GetValue(object cellValue)
	{
		return cellValue;
	}

	public new void BeginLoadData()
	{
		base.BeginLoadData();
		BeginLoadDataEvent?.Invoke();
	}

	public new void EndLoadData()
	{
		base.EndLoadData();
		EndLoadDataEvent?.Invoke();
	}
}
