using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using InflexCore;
using WinFormsExplorerWindow.InflexReportTree.ViewModel;

namespace WinFormsExplorerWindow.InflexReportTree;

public class InflexReportTree : TreeView
{
	private struct TVITEM
	{
		public int mask;

		public IntPtr hItem;

		public int state;

		public int stateMask;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpszText;

		public int cchTextMax;

		public int iImage;

		public int iSelectedImage;

		public int cChildren;

		public IntPtr lParam;
	}

	private const int TVIF_STATE = 8;

	private const int TVIS_STATEIMAGEMASK = 61440;

	private const int TV_FIRST = 4352;

	private const int TVM_SETITEM = 4415;

	private IInflexReport _Report;

	private QueryColumnCollection _ColumnCollection;

	private InflexReportTreeSource _ReportTreeSource;

	public InflexReportTreeSource ReportTreeSource
	{
		get
		{
			return _ReportTreeSource;
		}
		set
		{
			ApplySource(value);
		}
	}

	[DllImport("user32.dll")]
	private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	public InflexReportTree()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		((TreeView)this)._002Ector();
		((TreeView)this).CheckBoxes = true;
		((TreeView)this).DrawMode = (TreeViewDrawMode)2;
		((TreeView)this).DrawNode += new DrawTreeNodeEventHandler(InflexReportTreeDrawNode);
		((TreeView)this).AfterCheck += new TreeViewEventHandler(AfterCheckItem);
		((TreeView)this).BeforeExpand += new TreeViewCancelEventHandler(BeforeExpandItem);
		((TreeView)this).AfterCollapse += new TreeViewEventHandler(AfterCollapseItem);
	}

	private void AfterCollapseItem(object sender, TreeViewEventArgs e)
	{
		InflexTreeViewNode inflexTreeViewNode = (InflexTreeViewNode)(object)e.Node;
		inflexTreeViewNode.SetExpandedToItem(value: false);
	}

	private void BeforeExpandItem(object sender, TreeViewCancelEventArgs e)
	{
		InflexTreeViewNode inflexTreeViewNode = (InflexTreeViewNode)(object)e.Node;
		inflexTreeViewNode.SetExpandedToItem(value: true);
		((TreeNode)inflexTreeViewNode).Nodes.Clear();
		((TreeNode)inflexTreeViewNode).Nodes.AddRange(FillNodes(inflexTreeViewNode.ItemVM.ChildItems));
	}

	private void AfterCheckItem(object sender, TreeViewEventArgs e)
	{
		InflexTreeViewNode inflexTreeViewNode = (InflexTreeViewNode)(object)e.Node;
		inflexTreeViewNode.SetCheckedToItem(e.Node.Checked);
	}

	private void InflexReportTreeDrawNode(object sender, DrawTreeNodeEventArgs e)
	{
		InflexTreeViewNode inflexTreeViewNode = (InflexTreeViewNode)(object)e.Node;
		if (!inflexTreeViewNode.CheckBoxVisible)
		{
			HideCheckBox(e.Node);
		}
		e.DrawDefault = true;
	}

	private void HideCheckBox(TreeNode node)
	{
		TVITEM tVITEM = new TVITEM
		{
			hItem = node.Handle,
			mask = 8,
			stateMask = 61440,
			state = 0
		};
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((object)tVITEM));
		Marshal.StructureToPtr((object)tVITEM, intPtr, false);
		SendMessage(((Control)this).Handle, 4415u, IntPtr.Zero, intPtr);
	}

	public QueryColumnCollection GetQueryColumnCollection()
	{
		return _ColumnCollection;
	}

	private Collection GetViewColumns(IInflexReport report, QueryColumnCollection queryColumnCollection)
	{
		return new Collection(report, null, queryColumnCollection);
	}

	private void ApplySource(InflexReportTreeSource source)
	{
		_ReportTreeSource = source;
		if (source == null)
		{
			ResetSource();
			return;
		}
		if (source.Report == null)
		{
			ResetSource();
			return;
		}
		_Report = source.Report;
		if (source.QueryColumns == null)
		{
			_ColumnCollection = new QueryColumnCollection();
		}
		else
		{
			_ColumnCollection = source.QueryColumns;
		}
		Collection viewColumns = GetViewColumns(_Report, _ColumnCollection);
		((TreeView)this).Nodes.AddRange(FillNodes(viewColumns));
	}

	private TreeNode[] FillNodes(IEnumerable<IReportTreeItemViewModel> collection)
	{
		List<InflexTreeViewNode> list = new List<InflexTreeViewNode>();
		foreach (IReportTreeItemViewModel item in collection)
		{
			InflexTreeViewNode inflexTreeViewNode = new InflexTreeViewNode(item);
			if (item.CanCheck)
			{
				inflexTreeViewNode.CheckBoxVisible = true;
				((TreeNode)inflexTreeViewNode).Checked = item.IsChecked.HasValue && item.IsChecked.Value;
			}
			else
			{
				inflexTreeViewNode.CheckBoxVisible = false;
			}
			list.Add(inflexTreeViewNode);
			if (item.ChildItems.Count > 0)
			{
				((TreeNode)inflexTreeViewNode).Nodes.AddRange(FillNodes(item.ChildItems));
			}
		}
		return (TreeNode[])(object)list.ToArray();
	}

	private void ResetSource()
	{
		_Report = null;
		_ColumnCollection = null;
		((TreeView)this).Nodes.Clear();
	}
}
