using System;
using System.Collections.Generic;
using InflexCore.Action.Attributes;
using InflexCore.Native.Action;

namespace InflexCore.Action;

[ActionType(GroupName = "Открыть")]
public class ShowReferenceObjectAction : InflexAction
{
	private readonly IDataHandler _DataHandler;

	private readonly IInflexReport _Report;

	private readonly IInflexColumn _Column;

	private bool _IsAllowedContext;

	private ShowInflexObjectAction _InnerAction;

	private string _NotExecuteMessage;

	private IInflexType _OuterType;

	public override string Caption => _Column.ReferenceName;

	protected override bool IsBeforeExecuteAllowed => IsAllowedInnerAction;

	public bool IsAllowedInnerAction
	{
		get
		{
			if (_InnerAction == null)
			{
				UpdateInnerAction();
			}
			bool flag = _InnerAction != null && _InnerAction.IsAllowed != false;
			_NotExecuteMessage = (flag ? "" : "Нет связанных данных");
			return flag;
		}
	}

	public override string NotExecuteMessage => _NotExecuteMessage;

	public ShowReferenceObjectAction(IDataHandler dataHandler, IInflexReport report, IInflexColumn column, ActionContext context)
	{
		_DataHandler = dataHandler;
		_Report = report;
		_Column = column;
		_OuterType = _Column.GetOuterReference()?.AssignedType;
		Initialize(context);
		ResetActionState();
	}

	private void UpdateInnerAction()
	{
		ResetActionState();
		CheckIsAllowedContext();
		if (!_IsAllowedContext)
		{
			return;
		}
		ActionContext actionContext = base.Context.Clone();
		actionContext.Type = _OuterType;
		actionContext.ActionArgs.ObjectIDs = new IInflexID[0];
		IInflexID[] objectIDs = base.Context.ActionArgs.ObjectIDs;
		if (objectIDs != null && objectIDs.Length > 0)
		{
			Dictionary<IInflexColumn, object> values = _Report.GetValues(base.Context.ActionArgs.ObjectIDs[0], _Column);
			if (values != null && values.ContainsKey(_Column))
			{
				object obj = values[_Column];
				if (obj != null && decimal.TryParse(obj.ToString(), out var result))
				{
					actionContext.ActionArgs.ObjectIDs = new IInflexID[1]
					{
						new MockInflexID(actionContext.Type, result)
					};
				}
			}
		}
		_InnerAction = new ShowInflex32ObjectAction(_DataHandler, actionContext);
	}

	protected override bool? DirectIsAllowed()
	{
		IInflexID[] array = base.Context.ActionArgs.ObjectIDs ?? new IInflexID[0];
		if (array.Length != 1)
		{
			return false;
		}
		return null;
	}

	protected override bool DirectIsAllowedContext()
	{
		return CheckIsAllowedContext();
	}

	private bool CheckIsAllowedContext()
	{
		if (!_IsAllowedContext)
		{
			if (base.Context.Type == null || (!object.Equals(base.Context.Type.AssignedReport, _Report) && !object.Equals(base.Context.Type.AssignedReport.ParentReport, _Report)) || !_DataHandler.IsShowHandler(_OuterType))
			{
				return _IsAllowedContext;
			}
			_IsAllowedContext = true;
		}
		return _IsAllowedContext;
	}

	protected override void DirectExecute()
	{
		if (_InnerAction == null)
		{
			throw new NotSupportedException();
		}
		_InnerAction.Execute();
	}

	protected override void AfterContextUpdate()
	{
		ResetActionState();
	}

	private void ResetActionState()
	{
		_InnerAction = null;
		_IsAllowedContext = false;
	}
}
