using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InflexCore;

namespace WinFormsExplorerWindow.InflexReportTree.ViewModel;

internal class Collection : IEnumerable<IReportTreeItemViewModel>, IEnumerable
{
	private readonly List<IReportTreeItemViewModel> _ItemsList = new List<IReportTreeItemViewModel>();

	public Collection(IInflexReport report, InflexColumnViewModel parentViewModel, QueryColumnCollection columnCollection)
	{
		if (report != null)
		{
			List<IInflexColumn> source = report.GetExplorerColumns().ToList();
			List<IInflexColumn> list = new List<IInflexColumn>();
			list.AddRange(source.Where((IInflexColumn c) => !c.IsOuterReference() && !c.Hidden));
			list.AddRange(source.Where((IInflexColumn c) => c.IsOuterReference()));
			_ItemsList = FillColumnsViewModel(list, parentViewModel, columnCollection);
		}
	}

	private List<IReportTreeItemViewModel> FillColumnsViewModel(IEnumerable<IInflexColumn> columns, InflexColumnViewModel parentViewModel, QueryColumnCollection queryColumns)
	{
		List<IReportTreeItemViewModel> list = new List<IReportTreeItemViewModel>();
		IEnumerable<IGrouping<string, IInflexColumn>> enumerable = from c in columns
			group c by c.GroupName;
		if (enumerable.Count() > GetMinGroupCount())
		{
			foreach (IGrouping<string, IInflexColumn> item2 in enumerable)
			{
				IInflexColumn inflexColumn = item2.ElementAtOrDefault(0);
				List<IReportTreeItemViewModel> list2 = new List<IReportTreeItemViewModel>();
				GroupItem groupItem = new GroupItem(inflexColumn.GroupName, parentViewModel);
				foreach (IInflexColumn item3 in item2)
				{
					list2.Add(new InflexColumnViewModel(item3, groupItem, queryColumns, this));
				}
				groupItem.AddItems(list2);
				list.Add(groupItem);
			}
		}
		else
		{
			foreach (IInflexColumn column in columns)
			{
				InflexColumnViewModel item = new InflexColumnViewModel(column, parentViewModel, queryColumns, this);
				list.Add(item);
			}
		}
		return list;
	}

	private int GetMinGroupCount()
	{
		return 1;
	}

	public IEnumerator<IReportTreeItemViewModel> GetEnumerator()
	{
		return _ItemsList.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
