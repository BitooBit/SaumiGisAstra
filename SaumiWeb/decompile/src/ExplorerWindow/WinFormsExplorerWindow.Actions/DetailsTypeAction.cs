namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 100, Index = 200)]
public class DetailsTypeAction : ToolMultiAction
{
	public override string Caption => "Детальность информации";
}
