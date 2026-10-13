using System;

namespace InflexData;

public interface IGridColumn
{
	bool Hidden { get; }

	string Title { get; }

	int SortIndex { get; }

	bool SortDesc { get; }

	int GroupIndex { get; }

	bool InnerTreeKey { get; }

	bool OuterTreeKey { get; }

	bool HierarchicalLevel { get; }

	bool IsPrimary { get; }

	bool IsDate { get; }

	TotalSummModeEnum TotalSummMode { get; }

	Type ColumnType { get; }

	string Units { get; }

	int Accuracy { get; }

	BestColumnWidth ColumnWidth { get; }

	bool IsIndicator { get; }

	bool IsNestedColumn { get; }
}
