using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace InflexData;

public class InflexTreeNode : INotifyPropertyChanged, IDisposable
{
	public static class DirectInflexTreeNodeAccessor
	{
		public static void AddChild(InflexTreeNode rootNode, InflexTreeNode childNode)
		{
			rootNode._Children.Add(childNode);
		}

		public static void AddChildren(InflexTreeNode rootNode, IEnumerable<InflexTreeNode> childNodes)
		{
			rootNode._Children.AddRange(childNodes);
		}

		public static void RemoveChild(InflexTreeNode rootNode, InflexTreeNode childNode)
		{
			rootNode._Children.Remove(childNode);
		}

		public static void RemoveChildren(InflexTreeNode rootNode, IEnumerable<InflexTreeNode> childNodes)
		{
			foreach (InflexTreeNode childNode in childNodes)
			{
				childNode._Parent = null;
				rootNode._Children.Remove(childNode);
			}
		}
	}

	private InflexTreeNode _Parent;

	private List<InflexTreeNode> _Children = new List<InflexTreeNode>();

	private decimal _ID;

	private object[] _Content;

	private bool _IsExpanded;

	public InflexTreeNode Parent => _Parent;

	public IList<InflexTreeNode> Children => _Children;

	public decimal ID => _ID;

	public object[] Content => _Content;

	public bool HasChildren
	{
		get
		{
			if (_Children != null)
			{
				return _Children.Count > 0;
			}
			return false;
		}
	}

	public bool IsExpanded
	{
		get
		{
			return _IsExpanded;
		}
		set
		{
			_IsExpanded = value;
			OnPropertyChanged("IsExpanded");
		}
	}

	public bool HasParent => _Parent != null;

	public bool IsRoot => !HasParent;

	public int NestedLevel
	{
		get
		{
			int num = 0;
			for (InflexTreeNode parent = _Parent; parent != null; parent = parent._Parent)
			{
				num++;
			}
			return num;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public InflexTreeNode(decimal id, object[] values, InflexTreeNode parent)
	{
		_Content = values;
		_ID = id;
		_Parent = parent;
	}

	private Dictionary<InflexDataColumn, object> ConvertDataRowToColumnsValuesDictionary(DataRow dataRow)
	{
		return dataRow.Table.Columns.Cast<InflexDataColumn>().ToDictionary((InflexDataColumn column) => column, (InflexDataColumn column) => dataRow[column]);
	}

	public void AddChild(InflexTreeNode childNode)
	{
		bool hasChildren = HasChildren;
		_Children.Add(childNode);
		if (!hasChildren)
		{
			OnPropertyChanged("HasChildren");
		}
	}

	public void AddChildren(IEnumerable<InflexTreeNode> childNodes)
	{
		bool hasChildren = HasChildren;
		_Children.AddRange(childNodes);
		if (!hasChildren)
		{
			OnPropertyChanged("HasChildren");
		}
	}

	public void RemoveChild(InflexTreeNode childNode)
	{
		bool hasChildren = HasChildren;
		_Children.Remove(childNode);
		if (hasChildren && !HasChildren)
		{
			OnPropertyChanged("HasChildren");
		}
	}

	public void RemoveChildren(IEnumerable<InflexTreeNode> childNodes)
	{
		bool hasChildren = HasChildren;
		foreach (InflexTreeNode childNode in childNodes)
		{
			childNode._Parent = null;
			_Children.Remove(childNode);
		}
		if (hasChildren && !HasChildren)
		{
			OnPropertyChanged("HasChildren");
		}
	}

	public void RemoveAllChildren()
	{
		foreach (InflexTreeNode child in _Children)
		{
			child._Parent = null;
		}
		_Children.Clear();
		OnPropertyChanged("HasChildren");
	}

	public void Dispose()
	{
		_Parent = null;
		RemoveAllChildren();
		_Children = null;
		_Content = null;
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	[DebuggerStepThrough]
	[Conditional("DEBUG")]
	public void VerifyPropertyName(string propertyName, bool throwOnInvalidName)
	{
		if (TypeDescriptor.GetProperties(this)[propertyName] == null)
		{
			string message = "Invalid property name: " + propertyName;
			if (throwOnInvalidName)
			{
				throw new Exception(message);
			}
		}
	}

	protected void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
