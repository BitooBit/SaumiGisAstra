using System.Linq;
using InflexCore.Native.GW;

namespace InflexCore.Action;

public class NsActionManager : AppManager.AppManagerComponent, IActionFactory
{
	private class NativeHelper : AppManager.NativeInflexAccessor
	{
		public InflexGwManager InflexGwManager => base.Inflex32Manager;

		public NativeHelper(AppManager appManager)
			: base(appManager)
		{
		}
	}

	private Inflex32DataHandlerActionFactory _Inflex32DataHandlerActions;

	public override void Initialize()
	{
		_Inflex32DataHandlerActions = new Inflex32DataHandlerActionFactory(new NativeHelper(base.AppManager).InflexGwManager);
	}

	public ActionCollection GetActions(ActionContext context)
	{
		INsActionFactory nsActionFactory = base.AppManagerComponents.OfType<INsActionFactory>().FirstOrDefault();
		bool flag = nsActionFactory != null;
		ActionCollection actions = _Inflex32DataHandlerActions.GetActions(context);
		if (flag)
		{
			ActionCollection actions2 = nsActionFactory.GetActions(context);
			foreach (InflexAction item in actions2)
			{
				if (item.IsAllowed == false)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				InflexAction inflexAction = actions.Where((InflexAction a) => (object)a.GetType() == typeof(NewInflex32ObjectAction)).FirstOrDefault();
				if (inflexAction != null)
				{
					actions.Remove(inflexAction);
				}
				inflexAction = actions.Where((InflexAction a) => (object)a.GetType() == typeof(ShowInflex32ObjectAction)).FirstOrDefault();
				if (inflexAction != null)
				{
					actions.Remove(inflexAction);
				}
				actions.AddCollection(actions2);
			}
		}
		return actions;
	}
}
