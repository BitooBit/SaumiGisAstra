namespace InflexCore.Action;

public interface IActionFactory
{
	ActionCollection GetActions(ActionContext context);
}
