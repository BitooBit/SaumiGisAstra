using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using InflexCore;

namespace InflexData;

internal class VirtualTreeDataStorage : VirtualDataStorage
{
	private IInflexType _AssignedType;

	private readonly List<InflexTreeNode> _RootNodes = new List<InflexTreeNode>();

	private int _InnerTreeKeyColumnIndex;

	private int _OuterTreeKeyColumnIndex;

	private int _HierarchicalLevelColumnIndex;

	private bool[] _ColumnsVisibilities;

	private Stopwatch _Stopwatch = new Stopwatch();

	private decimal _LastRoot;

	private decimal _LastLevel;

	private InflexTreeNode _LastNode;

	private Dictionary<decimal, InflexTreeNode> _LastLevelParents;

	public IList<InflexTreeNode> RootNodes => _RootNodes;

	public VirtualTreeDataStorage(InflexDataTable mainTable, IInflexType assignedType)
		: base(mainTable)
	{
		_AssignedType = assignedType;
	}

	private void UpdateTreeNodeConverter()
	{
		DataRowToInflexTreeNodeConverter.UpdateTreeNodes(_RootNodes);
	}

	public int Expand(InflexTreeNode parentNode, bool shouldExpandChildren = false, int? position = null)
	{
		IInflexID key = base.InflexIDs[parentNode.ID];
		int num;
		if (position.HasValue)
		{
			num = position.Value;
		}
		else
		{
			DataRow row = base.InflexIdRows[key];
			num = base.MainTable.Rows.IndexOf(row) + 1;
		}
		foreach (InflexTreeNode child in parentNode.Children)
		{
			InsertAt(child.ID, new MockInflexID(_AssignedType, child.ID), child.Content, num++);
			if (child.HasChildren && (child.IsExpanded || shouldExpandChildren))
			{
				num = Expand(child, shouldExpandChildren, num);
			}
		}
		parentNode.IsExpanded = true;
		return num;
	}

	public void CollapseOld(InflexTreeNode parentNode, bool shouldCollapseChildren = false)
	{
		foreach (InflexTreeNode child in parentNode.Children)
		{
			if (child.HasChildren && child.IsExpanded)
			{
				if (shouldCollapseChildren)
				{
					CollapseOld(child, shouldCollapseChildren: true);
				}
				else
				{
					HideNode(child);
				}
			}
			HideRow(base.InflexIDs[child.ID]);
		}
		parentNode.IsExpanded = false;
	}

	public void Collapse(InflexTreeNode parentNode, bool shouldCollapseChildren = false)
	{
		HideRows(base.InflexIDs[parentNode.Children.FirstOrDefault().ID], GetAllChildrenCount(parentNode));
		SetExpanded(parentNode, isExpanded: false, shouldCollapseChildren);
	}

	private int GetAllChildrenCount(InflexTreeNode parentNode)
	{
		int num = parentNode.Children.Count;
		foreach (InflexTreeNode child in parentNode.Children)
		{
			if (child.HasChildren && child.IsExpanded)
			{
				num += GetAllChildrenCount(child);
			}
		}
		return num;
	}

	private void HideNode(InflexTreeNode parentNode)
	{
		foreach (InflexTreeNode child in parentNode.Children)
		{
			if (child.Children.Count > 0)
			{
				HideNode(child);
			}
			HideRow(base.InflexIDs[child.ID]);
		}
	}

	private void SetExpanded(InflexTreeNode parentNode, bool isExpanded, bool shouldSetExpandedChildren = false)
	{
		if (shouldSetExpandedChildren)
		{
			foreach (InflexTreeNode child in parentNode.Children)
			{
				if (child.HasChildren)
				{
					SetExpanded(child, isExpanded, shouldSetExpandedChildren: true);
				}
				child.IsExpanded = isExpanded;
			}
		}
		parentNode.IsExpanded = isExpanded;
	}

	private void AddNode(InflexTreeNode node)
	{
		Add(node.ID, new MockInflexID(_AssignedType, node.ID), node.Content);
		foreach (InflexTreeNode child in node.Children)
		{
			AddNode(child);
			node.IsExpanded = true;
		}
	}

	private void AddNodeWithParents(InflexTreeNode node)
	{
		if (node.HasParent)
		{
			AddNodeWithParents(node.Parent);
			SetExpanded(node.Parent, isExpanded: true);
		}
		if (!base.InflexIDs.ContainsKey(node.ID))
		{
			Add(node.ID, new MockInflexID(_AssignedType, node.ID), node.Content);
		}
	}

	public void ExpandAll()
	{
		Clear();
		base.MainTable.BeginLoadData();
		foreach (InflexTreeNode rootNode in _RootNodes)
		{
			AddNode(rootNode);
		}
		base.MainTable.AcceptChanges();
		base.MainTable.EndLoadData();
	}

	public void CollapseAll()
	{
		Clear();
		base.MainTable.BeginLoadData();
		foreach (InflexTreeNode rootNode in _RootNodes)
		{
			SetExpanded(rootNode, isExpanded: false, shouldSetExpandedChildren: true);
			Add(rootNode.ID, new MockInflexID(_AssignedType, rootNode.ID), rootNode.Content);
		}
		base.MainTable.AcceptChanges();
		base.MainTable.EndLoadData();
	}

	public void FindNodes(List<string> searchStrings)
	{
		Clear();
		base.MainTable.BeginLoadData();
		foreach (InflexTreeNode rootNode in _RootNodes)
		{
			if (IsNodeContainsSearchStrings(rootNode, searchStrings))
			{
				AddNodeWithParents(rootNode);
			}
			if (rootNode.HasChildren)
			{
				FindChildNodes(rootNode, searchStrings);
			}
		}
		base.MainTable.AcceptChanges();
		base.MainTable.EndLoadData();
	}

	private bool IsNodeContainsSearchStrings(InflexTreeNode node, List<string> searchStrings)
	{
		if (node.Content == null)
		{
			return false;
		}
		foreach (string searchString in searchStrings)
		{
			bool flag = false;
			for (int i = 0; i < node.Content.Length; i++)
			{
				object obj = node.Content[i];
				if (obj != null && _ColumnsVisibilities[i] && obj.ToString().ToLower().Contains(searchString))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	private void FindChildNodes(InflexTreeNode parentNode, List<string> searchStrings)
	{
		foreach (InflexTreeNode child in parentNode.Children)
		{
			if (IsNodeContainsSearchStrings(child, searchStrings))
			{
				AddNodeWithParents(child);
			}
			if (child.HasChildren)
			{
				FindChildNodes(child, searchStrings);
			}
		}
	}

	private void SetTreeKeyColumns()
	{
		if (base.MainTable == null)
		{
			return;
		}
		_ColumnsVisibilities = new bool[base.MainTable.Columns.Count];
		int num = -1;
		foreach (IGridColumn column in base.MainTable.Columns)
		{
			num++;
			_ColumnsVisibilities[num] = !column.Hidden;
			if (column.InnerTreeKey)
			{
				_InnerTreeKeyColumnIndex = num;
			}
			if (column.OuterTreeKey)
			{
				_OuterTreeKeyColumnIndex = num;
			}
			if (column.HierarchicalLevel)
			{
				_HierarchicalLevelColumnIndex = num;
			}
		}
	}

	public override void BeginLoadData()
	{
		_Stopwatch.Start();
		_LastRoot = -1m;
		_LastLevel = -1m;
		_LastNode = null;
		_LastLevelParents = new Dictionary<decimal, InflexTreeNode>();
		SetTreeKeyColumns();
	}

	public override void EndLoadData()
	{
		UpdateTreeNodeConverter();
		_Stopwatch.Stop();
	}

	public override void LoadData(decimal id, IInflexID inflexID, object[] values, int? indicatorIndex)
	{
		decimal num = -1m;
		decimal num2 = -1m;
		InflexTreeNode inflexTreeNode = ((_LastNode != null) ? _LastNode.Parent : null);
		if (values[_InnerTreeKeyColumnIndex] is DBNull)
		{
			inflexTreeNode = null;
		}
		else
		{
			num = (decimal)values[_InnerTreeKeyColumnIndex];
			num2 = (decimal)values[_HierarchicalLevelColumnIndex];
			if (num != _LastRoot && num2 > _LastLevel)
			{
				inflexTreeNode = _LastNode;
				_LastLevelParents[num2] = inflexTreeNode;
			}
			if (num != _LastRoot && num2 < _LastLevel)
			{
				inflexTreeNode = _LastLevelParents[num2];
			}
		}
		InflexTreeNode inflexTreeNode2 = new InflexTreeNode(id, values, inflexTreeNode);
		if (inflexTreeNode != null)
		{
			InflexTreeNode.DirectInflexTreeNodeAccessor.AddChild(inflexTreeNode, inflexTreeNode2);
		}
		else
		{
			_RootNodes.Add(inflexTreeNode2);
			Add(inflexTreeNode2.ID, new MockInflexID(_AssignedType, inflexTreeNode2.ID), inflexTreeNode2.Content);
		}
		_LastRoot = num;
		_LastLevel = num2;
		_LastNode = inflexTreeNode2;
	}
}
