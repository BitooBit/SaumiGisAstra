using System.Collections.Generic;
using System.Linq;

namespace WinFormsExplorerWindow.InflexReportTree.ViewModel;

internal class GroupItem : BaseReportTreeItemViewModel
{
	public GroupItem(string name, IReportTreeItemViewModel parent)
	{
		base.Caption = name;
		base.CanCheck = true;
		SetCheckedDirect(false);
		base.Visible = true;
		base.Parent = parent;
	}

	public void AddItems(IEnumerable<IReportTreeItemViewModel> list)
	{
		base.ChildItems.AddRange(list);
		AfterAddItems();
	}

	public override void SetChecked(bool? value, bool updateParent, bool updateChilds, object parameter)
	{
		if (base.IsChecked == value)
		{
			return;
		}
		SetCheckedDirect(value);
		if (!updateChilds || (base.IsChecked != true && base.IsChecked != false))
		{
			return;
		}
		bool? setValue = base.IsChecked;
		base.ChildItems.ForEach(delegate(IReportTreeItemViewModel c)
		{
			if (c.ChildItems.Count == 0)
			{
				c.SetChecked(setValue, updateParent: false, updateChilds: false, false);
			}
		});
	}

	private void AfterAddItems()
	{
		base.IsChecked = SetStartCheckState();
		base.IsExpanded = base.IsChecked == true || !base.IsChecked.HasValue;
	}

	private bool? SetStartCheckState()
	{
		int num = base.ChildItems.Where(delegate(IReportTreeItemViewModel c)
		{
			bool? isChecked = c.IsChecked;
			return isChecked == true && isChecked.HasValue && c.Visible && c.ChildItems.Count == 0;
		}).Count();
		int num2 = base.ChildItems.Where((IReportTreeItemViewModel c) => c.Visible && c.ChildItems.Count == 0).Count();
		if (num == 0)
		{
			return false;
		}
		if (num == num2)
		{
			return true;
		}
		return null;
	}
}
