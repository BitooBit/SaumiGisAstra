using System.Collections.Generic;
using System.ComponentModel;
using InflexUtils;

namespace WinFormsExplorerWindow.InflexReportTree.ViewModel;

public abstract class BaseReportTreeItemViewModel : ViewModelBase, IReportTreeItemViewModel, INotifyPropertyChanged
{
	private bool? _IsChecked;

	private string _Caption;

	private bool _Visible;

	private bool _CheckedThreeState;

	private bool _CanCheck;

	private IReportTreeItemViewModel _Parent;

	private List<IReportTreeItemViewModel> _ChildItems;

	private bool _IsExpanded;

	public string Caption
	{
		get
		{
			return _Caption;
		}
		set
		{
			if (_Caption != value)
			{
				_Caption = value;
				OnPropertyChanged("Caption");
			}
		}
	}

	public bool Visible
	{
		get
		{
			return _Visible;
		}
		set
		{
			if (_Visible != value)
			{
				_Visible = value;
				OnPropertyChanged("Visible");
			}
		}
	}

	public bool CheckedThreeState
	{
		get
		{
			return _CheckedThreeState;
		}
		set
		{
			if (_CheckedThreeState != value)
			{
				_CheckedThreeState = value;
				OnPropertyChanged("CheckedThreeState");
			}
		}
	}

	public bool CanCheck
	{
		get
		{
			return _CanCheck;
		}
		set
		{
			if (_CanCheck != value)
			{
				_CanCheck = value;
				OnPropertyChanged("CanCheck");
			}
		}
	}

	public IReportTreeItemViewModel Parent
	{
		get
		{
			return _Parent;
		}
		set
		{
			if (_Parent != value)
			{
				_Parent = value;
				OnPropertyChanged("Parent");
			}
		}
	}

	public List<IReportTreeItemViewModel> ChildItems
	{
		get
		{
			return _ChildItems;
		}
		set
		{
			if (_ChildItems != value)
			{
				_ChildItems = value;
				OnPropertyChanged("ChildItems");
			}
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
			if (_IsExpanded != value)
			{
				_IsExpanded = value;
				OnExpandedChanged?.Invoke(this);
			}
		}
	}

	public bool? IsChecked
	{
		get
		{
			return _IsChecked;
		}
		set
		{
			SetChecked(value, updateParent: true, updateChids: true, null);
		}
	}

	public event ItemCheckedChangedEventHandler OnCheckedChanged;

	public event ItemExpandedChangedEventHandler OnExpandedChanged;

	public BaseReportTreeItemViewModel()
	{
		ChildItems = new List<IReportTreeItemViewModel>();
	}

	public virtual void SetChecked(bool? value, bool updateParent, bool updateChids, object parameter)
	{
		SetCheckedDirect(value);
	}

	public void SetCheckedDirect(bool? value)
	{
		_IsChecked = value;
		OnCheckedChanged?.Invoke(this);
	}
}
