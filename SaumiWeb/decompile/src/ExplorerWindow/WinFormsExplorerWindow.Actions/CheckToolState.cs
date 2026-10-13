using System;

namespace WinFormsExplorerWindow.Actions;

public class CheckToolState
{
	private bool _IsChecked;

	public bool IsChecked
	{
		get
		{
			return _IsChecked;
		}
		set
		{
			if (value != _IsChecked)
			{
				_IsChecked = value;
				RaiseCheckedPropertyChanged();
			}
		}
	}

	public event EventHandler CheckedChanged;

	private void RaiseCheckedPropertyChanged()
	{
		CheckedChanged?.Invoke(this, new EventArgs());
	}
}
