using System.Windows.Forms;
using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(FileAction), GroupIndex = int.MaxValue, HotKey = "Ctrl+Q")]
[AutoInstanceAction]
public class CloseExplorerWindowAction : ExplorerWindowToolAction
{
	public override string Caption => "Закрыть";

	protected override bool? DirectIsAllowed()
	{
		return base.DirectContext.ExplorerWindow != null;
	}

	protected override void ProtectedDirectExecute()
	{
		((Form)base.DirectContext.ExplorerWindow).Close();
	}
}
