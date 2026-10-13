using System.Windows.Forms;
using InflexCore.Action;
using InflexUtils.CommandInfrastructure;

namespace InflexUtils;

public abstract class InflexActionExecuter : ViewModelBase
{
	private DelegateCommand<InflexAction> _ExecuteActionCommand;

	protected DelegateCommand<InflexAction> ExecuteActionCommand
	{
		get
		{
			if (_ExecuteActionCommand == null)
			{
				_ExecuteActionCommand = new DelegateCommand<InflexAction>(OnExecuteActionCommand, ExecuteActionCommandCanExecute);
			}
			return _ExecuteActionCommand;
		}
	}

	private bool ExecuteActionCommandCanExecute(InflexAction arg)
	{
		return arg.IsAllowed != false;
	}

	private void OnExecuteActionCommand(InflexAction obj)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!obj.Execute())
		{
			string text = obj.NotExecuteMessage ?? "Выполнить действие невозможно";
			MessageBox.Show(text);
		}
	}
}
