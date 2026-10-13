using System;
using System.ComponentModel;
using System.Diagnostics;

namespace InflexUtils;

public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
{
	private readonly object _DisposeSync = new object();

	public virtual string DisplayName { get; protected set; }

	protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

	public event PropertyChangedEventHandler PropertyChanged;

	[Conditional("DEBUG")]
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

	public void Dispose()
	{
		lock (_DisposeSync)
		{
			OnDispose();
		}
	}

	protected virtual void OnDispose()
	{
	}
}
