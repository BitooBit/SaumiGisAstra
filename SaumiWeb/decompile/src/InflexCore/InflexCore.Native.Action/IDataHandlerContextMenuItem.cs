using InflexCore.Action;

namespace InflexCore.Native.Action;

public interface IDataHandlerContextMenuItem
{
	InflexAction CreateAction(ActionContext context);
}
