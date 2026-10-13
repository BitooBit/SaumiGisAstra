using InflexCore;
using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(EditAction), GroupName = "%CPCR", GroupIndex = 100, Index = 400, GroupOrder = 400, GlyphUri = "RemoveFromList", HotKey = "Del")]
[AutoInstanceAction]
[DataContextAction]
public class DeleteFromListToolAction : ExplorerWindowToolAction
{
	protected override bool IsBeforeExecuteAllowed
	{
		get
		{
			IInflexID[] objectIDs = base.Context.ActionArgs.ObjectIDs;
			if (objectIDs != null && objectIDs.Length > 0)
			{
				return base.DirectContext.DataView.AllowRemove(objectIDs);
			}
			return false;
		}
	}

	public override string Caption => "Удалить из списка";

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
		base.DirectContext.DataView?.Remove(base.Context.ActionArgs.ObjectIDs);
		base.DirectContext.ExplorerWindow?.DataGrid?.UpdateSelection();
	}
}
