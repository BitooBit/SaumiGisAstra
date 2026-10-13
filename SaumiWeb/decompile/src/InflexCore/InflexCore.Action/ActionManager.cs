using System.Collections.Generic;

namespace InflexCore.Action;

public class ActionManager : IActionFactory
{
	private List<IActionFactory> _Factories = new List<IActionFactory>();

	public void AddActionFactory(IActionFactory actionFactory)
	{
		_Factories.Add(actionFactory);
	}

	public void RemoveAction(IActionFactory actionFactory)
	{
		_Factories.Remove(actionFactory);
	}

	public ActionCollection GetActions(ActionContext context)
	{
		ActionCollection actionCollection = new ActionCollection();
		foreach (IActionFactory factory in _Factories)
		{
			actionCollection.AddCollection(factory.GetActions(context));
		}
		return actionCollection;
	}
}
