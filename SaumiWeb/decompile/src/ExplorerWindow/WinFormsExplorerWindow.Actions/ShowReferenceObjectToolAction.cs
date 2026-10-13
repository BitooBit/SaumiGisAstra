using InflexCore.Action;
using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ActionType(GroupName = "Открыть", GroupOrder = 200, GlyphUri = "Show")]
public class ShowReferenceObjectToolAction : ExplorerWindowToolAction
{
	private readonly ShowReferenceObjectAction _Action;

	private int _ID;

	private static readonly object _LookObject = new object();

	private static int _InstanceID = 0;

	public override string Caption => _Action.Caption;

	public override string Name => base.Name + _ID;

	protected override bool IsBeforeExecuteAllowed => _Action.IsAllowedInnerAction;

	public override string NotExecuteMessage => _Action.NotExecuteMessage;

	public ShowReferenceObjectToolAction(ShowReferenceObjectAction action)
	{
		_ID = NextID();
		_Action = action;
		Initialize(action.Context);
	}

	private static int NextID()
	{
		lock (_LookObject)
		{
			_InstanceID++;
			return _InstanceID;
		}
	}

	protected override bool? DirectIsAllowed()
	{
		return _Action.IsAllowed;
	}

	protected override bool ProtectedIsAllowedContext()
	{
		return _Action.IsAllowedContext;
	}

	protected override void ProtectedDirectExecute()
	{
		_Action.Execute();
	}
}
