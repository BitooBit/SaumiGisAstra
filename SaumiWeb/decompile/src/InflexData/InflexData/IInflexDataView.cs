using System;
using System.Collections.Generic;
using InflexCore;

namespace InflexData;

public interface IInflexDataView
{
	InflexDataTable DataTable { get; }

	IInflexDataQuery DataQuery { get; }

	bool HasHierarchicalData { get; }

	event Action PulseUpdate;

	event Action BeginUpdate;

	event Action EndUpdate;

	event Action DataSourceChanged;

	IInflexID GetRowInflexID(decimal rowPrimaryValue);

	bool AllowInsert(params IInflexID[] inflexIDs);

	void Expand(InflexTreeNode node);

	void Collapse(InflexTreeNode node);

	void Insert(params IInflexID[] inflexIDs);

	bool InExecute();

	void CollapseAll();

	void FindNodes(List<string> searchStrings);

	void ExpandAll();

	void HideRow(IInflexID inflexID);
}
