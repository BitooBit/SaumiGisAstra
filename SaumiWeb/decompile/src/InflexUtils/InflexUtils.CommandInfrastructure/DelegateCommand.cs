using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace InflexUtils.CommandInfrastructure;

public class DelegateCommand<T> : ICommand, INotifyPropertyChanged
{
	private bool _IsEnabled = true;

	private readonly Action<T> executeMethod;

	private readonly Func<T, bool> canExecuteMethod;

	public bool IsEnabled
	{
		get
		{
			return _IsEnabled;
		}
		set
		{
			_IsEnabled = value;
			RaiseCanExecuteChanged();
			OnPropertyChanged("IsEnabled");
		}
	}

	protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

	public event EventHandler CanExecuteChanged;

	public event PropertyChangedEventHandler PropertyChanged;

	public DelegateCommand(Action<T> executeMethod)
		: this(executeMethod, (Func<T, bool>)null)
	{
	}

	public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
	{
		if (executeMethod == null && canExecuteMethod == null)
		{
			throw new ArgumentNullException("executeMethod", "DelegateCommandDelegatesCannotBeNull");
		}
		this.executeMethod = executeMethod;
		this.canExecuteMethod = canExecuteMethod;
	}

	public bool CanExecute(T parameter)
	{
		if (canExecuteMethod == null)
		{
			return IsEnabled;
		}
		return canExecuteMethod(parameter);
	}

	public void Execute(T parameter)
	{
		if (executeMethod != null && CanExecute(parameter))
		{
			executeMethod(parameter);
		}
	}

	bool ICommand.CanExecute(object parameter)
	{
		return CanExecute((T)parameter);
	}

	void ICommand.Execute(object parameter)
	{
		Execute((T)parameter);
	}

	public void RaiseCanExecuteChanged()
	{
		OnCanExecuteChanged();
	}

	protected virtual void OnCanExecuteChanged()
	{
		_ = Thread.CurrentThread;
		CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}

	[Conditional("DEBUG")]
	[DebuggerStepThrough]
	public void VerifyPropertyName(string propertyName)
	{
		if (TypeDescriptor.GetProperties(this)[propertyName] == null)
		{
			string message = "Invalid property name: " + propertyName;
			if (ThrowOnInvalidPropertyName)
			{
				throw new Exception(message);
			}
		}
	}

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChangedEventHandler propertyChanged = PropertyChanged;
		if (propertyChanged != null)
		{
			PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
			propertyChanged(this, e);
		}
	}
}
