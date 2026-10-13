using System.Collections.Generic;
using System.ComponentModel;

namespace WinFormsExplorerWindow.InflexReportTree.ViewModel;

public interface IReportTreeItemViewModel : INotifyPropertyChanged
{
	string Caption { get; set; }

	bool? IsChecked { get; set; }

	bool Visible { get; set; }

	bool CanCheck { get; set; }

	bool CheckedThreeState { get; set; }

	IReportTreeItemViewModel Parent { get; set; }

	bool IsExpanded { get; set; }

	List<IReportTreeItemViewModel> ChildItems { get; set; }

	event ItemCheckedChangedEventHandler OnCheckedChanged;

	event ItemExpandedChangedEventHandler OnExpandedChanged;

	void SetCheckedDirect(bool? value);

	void SetChecked(bool? value, bool updateParent, bool updateChilds, object parameter);
}
