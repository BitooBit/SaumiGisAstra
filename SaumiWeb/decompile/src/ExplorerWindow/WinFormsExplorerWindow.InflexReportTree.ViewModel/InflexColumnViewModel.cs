using System.Collections.Generic;
using System.Linq;
using InflexCore;
using WinFormsExplorerWindow.InflexReportTree.Utils;

namespace WinFormsExplorerWindow.InflexReportTree.ViewModel;

internal class InflexColumnViewModel : BaseReportTreeItemViewModel
{
	private const string FAKE_COLUMN_NAME = "fake";

	private readonly IInflexColumn _Column;

	private readonly QueryColumnCollection _QueryColumnCollection;

	private readonly QueryColumn _QueryColumn;

	private readonly Collection _ParentCollection;

	private readonly bool _HasChilds;

	private readonly InflexColumnViewModel _DirectParent;

	public IInflexColumn Column => _Column;

	private InflexColumnViewModel()
	{
		base.Caption = "fake";
	}

	private void InflexColumnVM_OnExpandedChanged(IReportTreeItemViewModel sender)
	{
		if (sender is InflexColumnViewModel inflexColumnViewModel)
		{
			inflexColumnViewModel.ChildView();
		}
	}

	private void AddDependentColumns(IEnumerable<IReportTreeItemViewModel> collection)
	{
		if (_HasChilds)
		{
			return;
		}
		foreach (IReportTreeItemViewModel item in collection)
		{
			if (item is InflexColumnViewModel)
			{
				InflexColumnViewModel inflexColumnViewModel = item as InflexColumnViewModel;
				if (inflexColumnViewModel.Visible && _Column.IsDependentColumn(inflexColumnViewModel.Column) && IsValidDependentColumn(inflexColumnViewModel.Column))
				{
					inflexColumnViewModel.SetCheckedDirect(base.IsChecked);
					if (inflexColumnViewModel._DirectParent != inflexColumnViewModel.Parent)
					{
						inflexColumnViewModel.Parent.SetCheckedDirect(GetGroupCheckedValue(inflexColumnViewModel.Parent));
					}
					QueryColumnCollectionHelper.SetCheckValue(inflexColumnViewModel._QueryColumn, _QueryColumnCollection, inflexColumnViewModel.IsChecked.Value);
				}
			}
			else
			{
				AddDependentColumns(item.ChildItems);
			}
		}
	}

	private bool IsValidDependentColumn(IInflexColumn column)
	{
		return !column.IsOuterReference();
	}

	public InflexColumnViewModel(IInflexColumn column, IReportTreeItemViewModel parentViewModel, QueryColumnCollection queryColumns, Collection parentCollection)
	{
		_Column = column;
		_DirectParent = GetDirectParent(parentViewModel);
		_HasChilds = _Column.IsOuterReference();
		_QueryColumnCollection = queryColumns;
		_ParentCollection = parentCollection;
		QueryColumn foreignColumn = ((_DirectParent == null) ? null : _DirectParent._QueryColumn);
		_QueryColumn = new QueryColumn(_Column, !_HasChilds)
		{
			ForeignColumn = foreignColumn
		};
		base.OnExpandedChanged += InflexColumnVM_OnExpandedChanged;
		base.Parent = parentViewModel;
		base.Caption = _Column.ColumnName;
		base.IsChecked = QueryColumnCollectionHelper.GetColumnCheckedState(_QueryColumn, queryColumns);
		base.Visible = _Column.CanShow();
		base.CanCheck = !_HasChilds;
		base.CheckedThreeState = false;
		if (base.IsChecked.Value && _HasChilds)
		{
			base.IsExpanded = true;
		}
		if (_HasChilds && !base.IsExpanded)
		{
			base.ChildItems.Add(new InflexColumnViewModel());
		}
	}

	public override void SetChecked(bool? value, bool updateParent, bool updateChids, object parameter)
	{
		SetCheckedDirect(value);
		AddDependentColumns(_ParentCollection);
		if (updateParent)
		{
			if (_DirectParent != null && _DirectParent.IsChecked != value && (_DirectParent.ChildItems.Count != 0 || !_DirectParent._HasChilds || _DirectParent.IsChecked != true))
			{
				int num = _DirectParent.ChildItems.Where((IReportTreeItemViewModel c) => c.IsChecked == true).Count();
				_DirectParent.SetChecked(num > 0, updateParent: true, updateChids: false, null);
			}
			if (_DirectParent != base.Parent && base.Parent != null)
			{
				base.Parent.SetCheckedDirect(GetGroupCheckedValue(base.Parent));
			}
		}
		QueryColumnCollectionHelper.SetCheckValue(_QueryColumn, _QueryColumnCollection, base.IsChecked.Value);
	}

	private bool? GetGroupCheckedValue(IReportTreeItemViewModel item)
	{
		int num = item.ChildItems.Where(delegate(IReportTreeItemViewModel c)
		{
			bool? isChecked = c.IsChecked;
			return isChecked == true && isChecked.HasValue && c.Visible && c.ChildItems.Count == 0;
		}).Count();
		int num2 = item.ChildItems.Where((IReportTreeItemViewModel c) => c.Visible && c.ChildItems.Count == 0).Count();
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

	private InflexColumnViewModel GetDirectParent(IReportTreeItemViewModel parentViewModel)
	{
		if (parentViewModel == null)
		{
			return null;
		}
		if (parentViewModel is InflexColumnViewModel)
		{
			return parentViewModel as InflexColumnViewModel;
		}
		if (parentViewModel.Parent != null)
		{
			return GetDirectParent(parentViewModel.Parent);
		}
		return null;
	}

	private void ChildView()
	{
		if (_HasChilds && base.IsExpanded)
		{
			IInflexReport outerReference = _Column.GetOuterReference();
			if (NeedLoadChilds())
			{
				base.ChildItems.Clear();
				base.ChildItems.AddRange(new Collection(outerReference, this, _QueryColumnCollection));
			}
		}
	}

	private bool NeedLoadChilds()
	{
		if (base.ChildItems.Count == 0)
		{
			return true;
		}
		IEnumerable<IReportTreeItemViewModel> source = base.ChildItems.Where((IReportTreeItemViewModel childColumn) => childColumn.Caption == "fake");
		return source.Count() == 1;
	}
}
