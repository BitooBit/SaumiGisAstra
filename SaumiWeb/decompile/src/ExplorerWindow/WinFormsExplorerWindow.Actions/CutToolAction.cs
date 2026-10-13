using System.Linq;
using InflexCore;
using InflexCore.Action.Attributes;
using InflexCore.Native;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(EditAction), GroupIndex = 100, Index = 300, GroupName = "%CPCR", GroupOrder = 400, GlyphUri = "Cut", LargeGlyphUri = "CutLarge", HotKey = "Ctrl+X")]
[AutoInstanceAction]
[DataContextAction]
public class CutToolAction : ExplorerWindowToolAction
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

	public override string NotExecuteMessage => "Для текущего набора данных операция не поддерживается.";

	public override string Caption => "Вырезать";

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
		InflexClipboard.CopyArray(base.Context.ActionArgs.ObjectIDs.Select((IInflexID id) => ItemData.Parse(id)).ToArray());
		InflexClipboard.Synchronize();
		base.DirectContext.DataView?.Remove(base.Context.ActionArgs.ObjectIDs);
		base.DirectContext.ExplorerWindow?.DataGrid?.UpdateSelection();
	}
}
