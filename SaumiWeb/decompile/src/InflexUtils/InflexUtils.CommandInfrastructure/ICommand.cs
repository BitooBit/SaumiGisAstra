using System;

namespace InflexUtils.CommandInfrastructure;

public interface ICommand
{
	event EventHandler CanExecuteChanged;

	void Execute(object parameter);

	bool CanExecute(object parameter);
}
