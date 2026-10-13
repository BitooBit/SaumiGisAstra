using System.Collections.Generic;

namespace InflexCore.Native.Action;

public interface IDataHandler
{
	bool IsNewHandler(IInflexType type);

	ItemData NewItem(IInflexType type);

	bool IsShowItem(ItemData item);

	bool IsShowHandler(IInflexType type);

	void ShowItem(ItemData item);

	bool IsDeleteItem(ItemData item);

	bool IsDeleteHandler(IInflexType type);

	bool DeleteItem(ItemData item);

	void ShowOuterReference(IInflexType type, IInflexReport report, IInflexColumn column, ItemData[] items);

	IEnumerable<IDataHandlerContextMenuItem> GetContextMenu(IInflexType type);

	void ExecuteContextMenuItem(IDataHandlerContextMenuItem menuItem, ItemDataCollection collection);
}
