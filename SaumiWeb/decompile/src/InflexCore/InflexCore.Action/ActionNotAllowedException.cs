using System;

namespace InflexCore.Action;

public class ActionNotAllowedException : Exception
{
	public InflexAction Action { get; set; }

	public ActionContext Context { get; set; }

	public ActionNotAllowedException(InflexAction action, ActionContext context)
	{
		Action = action;
		Context = context;
	}
}
