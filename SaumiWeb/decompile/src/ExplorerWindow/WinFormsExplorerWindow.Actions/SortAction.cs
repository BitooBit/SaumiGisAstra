namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 100, Index = 100)]
public class SortAction : ToolMultiAction
{
	public override string Caption => "Сортировать";
}
