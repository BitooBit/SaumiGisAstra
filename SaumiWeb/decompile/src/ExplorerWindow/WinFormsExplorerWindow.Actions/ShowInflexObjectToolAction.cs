using InflexCore.Action;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(FileAction), GroupIndex = 200, Index = 100, GlyphUri = "Show", LargeGlyphUri = "ShowLarge", Position = ActionPosition.Up)]
public class ShowInflexObjectToolAction : ExplorerWindowToolAction
{
	private readonly ShowInflexObjectAction _Action;

	public override string Caption => "Свойства";

	public ShowInflexObjectToolAction(ShowInflexObjectAction action)
	{
		_Action = action;
		Initialize(action.Context);
	}

	protected override bool? DirectIsAllowed()
	{
		return _Action.IsAllowed;
	}

	protected override void ProtectedDirectExecute()
	{
		_Action.Execute();
	}
}
