using InflexCore.Action;

namespace InflexCore;

public interface IActionListAccessor
{
	ActionCollection GetActions(params IInflexID[] ids);
}
