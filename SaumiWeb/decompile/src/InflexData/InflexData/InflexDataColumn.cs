using System;
using System.Data;
using System.Globalization;

namespace InflexData;

public class InflexDataColumn : DataColumn, IGridColumn, IValueToDisplayStringConverter
{
	private bool _Hidden;

	private bool _IsPrimary;

	private bool _IsDate;

	private TotalSummModeEnum _TotalSummMode = TotalSummModeEnum.None;

	private Type _ColumnType;

	private BestColumnWidth _ColumnWidth;

	private string _Units;

	private int _Accuracy;

	private bool _IsIndicator;

	private bool _InnerTreeKey;

	private bool _OuterTreeKey;

	private bool _HierarchicalLevel;

	private static readonly Type _TypeofIComparable = typeof(IComparable);

	private static readonly Type _TypeDecimal = typeof(decimal);

	private static readonly Type _TypeDateTime = typeof(DateTime);

	private string _DecimalPattern;

	private int _SortIndex = -1;

	private bool _SortDesc;

	private bool _IsNestedColumn;

	private int _GroupIndex = -1;

	bool IGridColumn.Hidden => _Hidden;

	string IGridColumn.Title => base.Caption;

	int IGridColumn.SortIndex => _SortIndex;

	bool IGridColumn.SortDesc => _SortDesc;

	int IGridColumn.GroupIndex => _GroupIndex;

	bool IGridColumn.InnerTreeKey => _InnerTreeKey;

	bool IGridColumn.OuterTreeKey => _OuterTreeKey;

	bool IGridColumn.HierarchicalLevel => _HierarchicalLevel;

	bool IGridColumn.IsPrimary => _IsPrimary;

	bool IGridColumn.IsDate => _IsDate;

	bool IGridColumn.IsIndicator => _IsIndicator;

	public bool IsNestedColumn => _IsNestedColumn;

	TotalSummModeEnum IGridColumn.TotalSummMode => _TotalSummMode;

	Type IGridColumn.ColumnType => _ColumnType;

	BestColumnWidth IGridColumn.ColumnWidth
	{
		get
		{
			if (_ColumnWidth == null)
			{
				_ColumnWidth = new BestColumnWidth();
			}
			return _ColumnWidth;
		}
	}

	string IGridColumn.Units => _Units;

	int IGridColumn.Accuracy => _Accuracy;

	public InflexDataColumn(string columnName, Type dataType)
		: base(columnName, dataType)
	{
		_ColumnType = dataType;
	}

	public void SetHidden(bool hidden)
	{
		_Hidden = hidden;
	}

	public void SetIsPrimary(bool isPrimary)
	{
		_IsPrimary = isPrimary;
	}

	public void SetIsDate(bool isDate)
	{
		_IsDate = isDate;
	}

	public void SetIsIndicator(bool isIndicator)
	{
		_IsIndicator = isIndicator;
	}

	public void SetTotalSummMode(TotalSummModeEnum totalSummMode)
	{
		_TotalSummMode = totalSummMode;
	}

	public void SetColumnType(Type columnType)
	{
		_ColumnType = columnType;
	}

	public void SetColumnWidth(double columnWidth)
	{
		if (_ColumnWidth == null)
		{
			_ColumnWidth = new BestColumnWidth();
		}
		_ColumnWidth.BestWidth = columnWidth;
	}

	public void SetUnits(string units)
	{
		_Units = units;
	}

	public void SetAccuracy(int value)
	{
		_Accuracy = value;
	}

	public void SetInnerTreeKey(bool isInnerTreeKey)
	{
		_InnerTreeKey = isInnerTreeKey;
	}

	public void SetOuterTreeKey(bool isOuterTreeKey)
	{
		_OuterTreeKey = isOuterTreeKey;
	}

	public void SetHierarchicalLevel(bool isHierarchicalLevel)
	{
		_HierarchicalLevel = isHierarchicalLevel;
	}

	public void SetSortIndex(int sortIndex)
	{
		_SortIndex = sortIndex;
	}

	public void SetSortDesc(bool isDesc)
	{
		_SortDesc = isDesc;
	}

	public void SetIsNestedColumn(bool isNested)
	{
		_IsNestedColumn = isNested;
	}

	public void SetGroupIndex(int groupIndex)
	{
		_GroupIndex = groupIndex;
	}

	string IValueToDisplayStringConverter.ToString(IGridColumn gridColumn, object value, bool useUnits)
	{
		if (value == null || value == DBNull.Value)
		{
			return "";
		}
		if (gridColumn != this)
		{
			return value.ToString().Trim();
		}
		if (gridColumn.TotalSummMode != TotalSummModeEnum.None && (object)gridColumn.ColumnType == _TypeDecimal)
		{
			if (string.IsNullOrEmpty(_DecimalPattern))
			{
				_DecimalPattern = "#,##0";
				if (gridColumn.Accuracy > 0)
				{
					_DecimalPattern += ".";
					_DecimalPattern = _DecimalPattern.PadRight(_DecimalPattern.Length + gridColumn.Accuracy, '0');
				}
			}
			string text = Convert.ToDecimal(value).ToString(_DecimalPattern, CultureInfo.CurrentCulture);
			if (useUnits && !string.IsNullOrEmpty(gridColumn.Units))
			{
				text = text + " " + gridColumn.Units;
			}
			return text;
		}
		if (gridColumn.IsDate && (object)gridColumn.ColumnType == _TypeDateTime)
		{
			return ((DateTime)value).ToShortDateString();
		}
		return value.ToString().Trim();
	}

	string IValueToDisplayStringConverter.GetFormatString(IGridColumn gridColumn)
	{
		if (gridColumn != this)
		{
			return null;
		}
		if (gridColumn.TotalSummMode != TotalSummModeEnum.None && (object)gridColumn.ColumnType == _TypeDecimal)
		{
			if (string.IsNullOrEmpty(_DecimalPattern))
			{
				_DecimalPattern = "#,##0";
				if (gridColumn.Accuracy > 0)
				{
					_DecimalPattern += ".";
					_DecimalPattern = _DecimalPattern.PadRight(_DecimalPattern.Length + gridColumn.Accuracy, '0');
				}
			}
			return _DecimalPattern;
		}
		return null;
	}
}
