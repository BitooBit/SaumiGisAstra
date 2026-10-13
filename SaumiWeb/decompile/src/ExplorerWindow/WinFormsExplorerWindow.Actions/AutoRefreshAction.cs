using System;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 200, Index = 200)]
public class AutoRefreshAction : ExplorerWindowToolAction
{
	public override string Caption => "Автоматическое обновление";

	protected override bool? DirectIsAllowed()
	{
		return true;
	}

	protected override void ProtectedDirectExecute()
	{
		throw new NotImplementedException();
	}
}
