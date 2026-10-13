using InflexCore;
using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(EditAction), GroupIndex = 100, Index = 500, GroupOrder = 500, HotKey = "Ctrl+Shift+C", GroupName = "Выделение")]
[AutoInstanceAction]
[DataContextAction]
public class GridSelectHoldAction : ExplorerWindowToolAction
{
	protected override bool IsBeforeExecuteAllowed
	{
		get
		{
			IInflexID[] objectIDs = base.Context.ActionArgs.ObjectIDs;
			if (objectIDs != null && objectIDs.Length > 0)
			{
				return base.DirectContext.DataView.AllowHold(objectIDs);
			}
			return false;
		}
	}

	public override string NotExecuteMessage => "Для текущего набора данных операция не поддерживается.";

	public override string Caption => "Оставить выделенное";

	protected override bool? DirectIsAllowed()
	{
		IInflexID[] objectIDs = base.Context.ActionArgs.ObjectIDs;
		if (objectIDs != null && objectIDs.Length > 0)
		{
			return null;
		}
		return false;
	}

	protected override void ProtectedDirectExecute()
	{
		base.DirectContext.DataView?.Hold(base.Context.ActionArgs.ObjectIDs);
	}
}
