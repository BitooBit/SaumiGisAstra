using InflexCore;
using InflexCore.Action.Attributes;
using InflexCore.Native;
using InflexData;

namespace WinFormsExplorerWindow.Actions;

[DataContextAction]
[ToolActionType(MultiActionType = typeof(EditAction), GroupIndex = 100, Index = 200, GroupName = "%CPCR", GroupOrder = 400, GlyphUri = "Paste", LargeGlyphUri = "PasteLarge", HotKey = "Ctrl+V")]
[AutoInstanceAction]
public class PasteToolAction : ExplorerWindowToolAction
{
	protected override bool IsBeforeExecuteAllowed
	{
		get
		{
			IInflexID[] array = new IInflexID[InflexClipboard.ItemsCount];
			for (int i = 0; i < InflexClipboard.ItemsCount; i++)
			{
				array[i] = InflexClipboard.GetItem(i).ToInflexID();
			}
			return base.DirectContext.DataView.AllowInsert(array);
		}
	}

	public override string NotExecuteMessage => "Для текущего набора данных операция не поддерживается.";

	public override string Caption => "Вставить";

	protected override bool? DirectIsAllowed()
	{
		if (base.DirectContext.DataView == null || InflexClipboard.ItemsCount == 0)
		{
			return false;
		}
		return null;
	}

	protected override void ProtectedDirectExecute()
	{
		InflexDataView dataView = base.DirectContext.DataView;
		IInflexID[] array = new IInflexID[InflexClipboard.ItemsCount];
		for (int i = 0; i < InflexClipboard.ItemsCount; i++)
		{
			array[i] = InflexClipboard.GetItem(i).ToInflexID();
		}
		dataView.Insert(array);
	}
}
