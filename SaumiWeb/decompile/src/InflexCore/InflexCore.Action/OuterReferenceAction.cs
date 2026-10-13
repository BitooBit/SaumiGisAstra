using System.Linq;
using InflexCore.Action.Attributes;
using InflexCore.Native;
using InflexCore.Native.Action;

namespace InflexCore.Action;

[ActionType(GroupName = "Связи")]
public class OuterReferenceAction : InflexAction, IDataHandlerAction
{
	private DataHandlerActionHelper _HandlerHelper;

	public override string Caption => $"{Report.Name} ({Column.ColumnName})";

	public IInflexReport Report { get; set; }

	public IInflexColumn Column { get; set; }

	public IInflexType DataType { get; set; }

	public OuterReferenceAction(IDataHandler dataHandler, ActionContext context)
	{
		_HandlerHelper = new DataHandlerActionHelper(this, dataHandler, context);
	}

	protected override bool? DirectIsAllowed()
	{
		IInflexID[] objectIDs = base.Context.ActionArgs.ObjectIDs;
		if (objectIDs != null && objectIDs.Length > 0)
		{
			return null;
		}
		return false;
	}

	protected override bool DirectIsAllowedContext()
	{
		return _HandlerHelper.DirectIsAllowedContext();
	}

	protected override void DirectExecute()
	{
		_HandlerHelper.DataHandler.ShowOuterReference(DataType, Report, Column, base.Context.ActionArgs.ObjectIDs.Select((IInflexID id) => ItemData.Parse(id)).ToArray());
	}

	public bool InnerIsAllowedContext()
	{
		return true;
	}
}
