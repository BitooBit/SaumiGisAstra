using InflexCore.Action;
using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ActionType(GlyphUri = "OuterReference", GroupName = "Связи", GroupOrder = 600, GroupGlyphUri = "OuterReference")]
public class ShowLinksToolAction : ExplorerWindowToolAction
{
	private readonly OuterReferenceAction _Action;

	public override string Caption => _Action.Caption;

	public override string Name => $"{base.Name}_{_Action.Report.ID}_{_Action.Column.ID}";

	public ShowLinksToolAction(OuterReferenceAction action)
	{
		_Action = action;
		Initialize(action.Context);
	}

	protected override void ProtectedDirectExecute()
	{
		_Action.Execute();
	}

	protected override bool? DirectIsAllowed()
	{
		return _Action.IsAllowed;
	}
}
