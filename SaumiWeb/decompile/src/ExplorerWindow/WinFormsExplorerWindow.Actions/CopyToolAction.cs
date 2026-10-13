using System.Linq;
using InflexCore;
using InflexCore.Action.Attributes;
using InflexCore.Native;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(EditAction), GroupIndex = 100, Index = 100, GroupName = "%CPCR", GroupOrder = 400, GlyphUri = "Copy", LargeGlyphUri = "CopyLarge", HotKey = "Ctrl+C")]
[DataContextAction]
[AutoInstanceAction]
public class CopyToolAction : ExplorerWindowToolAction
{
	public override string Caption => "Копировать";

	protected override bool? DirectIsAllowed()
	{
		IInflexID[] objectIDs = base.Context.ActionArgs.ObjectIDs;
		return objectIDs != null && objectIDs.Length > 0;
	}

	protected override void ProtectedDirectExecute()
	{
		InflexClipboard.CopyArray(base.Context.ActionArgs.ObjectIDs.Select((IInflexID id) => ItemData.Parse(id)).ToArray());
		InflexClipboard.Synchronize();
	}
}
