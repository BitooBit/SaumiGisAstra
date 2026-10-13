using System;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 100, Index = 300)]
public class ToolBarVisibleAction : CheckExplorerWindowToolAction
{
	public override string Caption => "Панель инструментов";

	protected override bool? DirectIsAllowed()
	{
		return true;
	}

	protected override void ProtectedDirectExecute()
	{
		throw new NotImplementedException();
	}

	protected override void AfterContextUpdate()
	{
		base.AfterContextUpdate();
	}
}
