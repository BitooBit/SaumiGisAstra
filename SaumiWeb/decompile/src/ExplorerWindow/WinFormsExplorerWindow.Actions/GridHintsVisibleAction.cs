using System;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 100, Index = 800)]
public class GridHintsVisibleAction : ExplorerWindowToolAction
{
	public override string Caption => "Всплывающие подсказки";

	protected override bool? DirectIsAllowed()
	{
		return true;
	}

	protected override void ProtectedDirectExecute()
	{
		throw new NotImplementedException();
	}
}
