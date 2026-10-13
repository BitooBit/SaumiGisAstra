using System;
using System.Windows.Forms;
using WinFormsExplorerWindow.InflexReportTree.ViewModel;

namespace WinFormsExplorerWindow.InflexReportTree;

public class InflexTreeViewNode : TreeNode
{
	private int _InnerChangeCount;

	public IReportTreeItemViewModel ItemVM { get; private set; }

	public bool CheckBoxVisible { get; set; }

	private bool CanHandleEvent => _InnerChangeCount == 0;

	private void BeginInnerChange()
	{
		_InnerChangeCount++;
	}

	private void EndInnerChange()
	{
		if (_InnerChangeCount == 0)
		{
			throw new InvalidOperationException();
		}
		_InnerChangeCount--;
	}

	public InflexTreeViewNode(IReportTreeItemViewModel itemVM)
		: base(itemVM.Caption)
	{
		ItemVM = itemVM;
		SyncInitState(itemVM);
		itemVM.OnCheckedChanged += ItemOnCheckedChanged;
		itemVM.OnExpandedChanged += ItemOnExpandedChanged;
	}

	private void SyncInitState(IReportTreeItemViewModel itemVM)
	{
		if (itemVM.IsExpanded && !((TreeNode)this).IsExpanded)
		{
			((TreeNode)this).Expand();
		}
		if (!itemVM.IsExpanded && ((TreeNode)this).IsExpanded)
		{
			((TreeNode)this).Collapse(true);
		}
		if (CheckBoxVisible)
		{
			bool? isChecked = itemVM.IsChecked;
			if (isChecked == true && isChecked.HasValue && !((TreeNode)this).Checked)
			{
				((TreeNode)this).Checked = true;
			}
			bool? isChecked2 = itemVM.IsChecked;
			if (isChecked2 != true && isChecked2.HasValue && ((TreeNode)this).Checked)
			{
				((TreeNode)this).Checked = false;
			}
		}
	}

	internal void SetExpandedToItem(bool value)
	{
		BeginInnerChange();
		ItemVM.IsExpanded = value;
		EndInnerChange();
	}

	internal void SetCheckedToItem(bool value)
	{
		BeginInnerChange();
		ItemVM.IsChecked = value;
		EndInnerChange();
	}

	private void ItemOnExpandedChanged(IReportTreeItemViewModel sender)
	{
		if (CanHandleEvent)
		{
			if (sender.IsExpanded)
			{
				((TreeNode)this).Expand();
			}
			else
			{
				((TreeNode)this).Collapse(true);
			}
		}
	}

	private void ItemOnCheckedChanged(IReportTreeItemViewModel sender)
	{
		if (CanHandleEvent && CheckBoxVisible)
		{
			bool flag = sender.IsChecked.HasValue && sender.IsChecked.Value;
			if (((TreeNode)this).Checked != flag)
			{
				((TreeNode)this).Checked = flag;
			}
		}
	}
}
