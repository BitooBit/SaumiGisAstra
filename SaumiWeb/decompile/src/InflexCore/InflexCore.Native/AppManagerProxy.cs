using InflexCore.Action;

namespace InflexCore.Native;

public static class AppManagerProxy
{
	public static AppManager AppManagerInstance { get; private set; }

	public static void InitializeIfNeed(string userName, string password, string dataBase, string inflexSchema)
	{
		if (AppManagerInstance == null)
		{
			AppManagerInstance = AppManager.Init(userName, password, dataBase, inflexSchema);
		}
	}

	public static void Dispose()
	{
		if (AppManagerInstance != null)
		{
			try
			{
				AppManagerInstance.Dispose();
			}
			finally
			{
				AppManagerInstance = null;
			}
		}
	}

	public static bool NewItemAllowed(int typeID, int subTypeID)
	{
		if (AppManagerInstance != null)
		{
			IInflexType type = AppManagerInstance.GetType(typeID, subTypeID);
			if (type != null)
			{
				ActionContext emptyContext = ActionContext.GetEmptyContext(isSingleType: true);
				emptyContext.Type = type;
				NewInflexObjectAction allowedAction = GetAllowedAction<NewInflexObjectAction>(emptyContext);
				if (allowedAction != null)
				{
					return !(allowedAction is NewInflex32ObjectAction);
				}
				return false;
			}
		}
		return false;
	}

	public static ItemData NewItem(int typeID, int subTypeID)
	{
		IInflexType type = AppManagerInstance.GetType(typeID, subTypeID);
		if (type != null)
		{
			ActionContext emptyContext = ActionContext.GetEmptyContext(isSingleType: true);
			emptyContext.Type = type;
			NewInflexObjectAction allowedAction = GetAllowedAction<NewInflexObjectAction>(emptyContext);
			if (allowedAction != null)
			{
				allowedAction.Execute();
				return allowedAction.NewItemData;
			}
		}
		return null;
	}

	public static bool ShowItemAllowed(ItemData itemData)
	{
		if (AppManagerInstance != null)
		{
			ActionContext emptyContext = ActionContext.GetEmptyContext(isSingleType: true);
			IInflexID inflexID = itemData.ToInflexID(AppManagerInstance);
			emptyContext.Type = inflexID.Type;
			emptyContext.ActionArgs.ObjectIDs = new IInflexID[1] { inflexID };
			ShowInflexObjectAction allowedAction = GetAllowedAction<ShowInflexObjectAction>(emptyContext);
			if (allowedAction != null)
			{
				return !(allowedAction is ShowInflex32ObjectAction);
			}
			return false;
		}
		return false;
	}

	public static void ShowItem(ItemData itemData, bool showModal)
	{
		ActionContext emptyContext = ActionContext.GetEmptyContext(isSingleType: true);
		IInflexID inflexID = itemData.ToInflexID(AppManagerInstance);
		emptyContext.Type = inflexID.Type;
		emptyContext.ActionArgs.ObjectIDs = new IInflexID[1] { inflexID };
		ShowInflexObjectAction allowedAction = GetAllowedAction<ShowInflexObjectAction>(emptyContext);
		if (allowedAction != null)
		{
			allowedAction.ShowModal = showModal;
			allowedAction.Execute();
		}
	}

	private static T GetAllowedAction<T>(ActionContext context) where T : InflexAction
	{
		ActionCollection actions = AppManagerInstance.ActionList.GetActions(context);
		T val = actions[typeof(T)] as T;
		if (val != null)
		{
			bool? isAllowed = val.IsAllowed;
			if (isAllowed.HasValue && isAllowed.Value)
			{
				return val;
			}
		}
		return null;
	}
}
