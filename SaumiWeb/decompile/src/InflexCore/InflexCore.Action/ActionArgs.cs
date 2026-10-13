using System;
using System.Collections.Generic;

namespace InflexCore.Action;

public class ActionArgs
{
	public IInflexID[] ObjectIDs { get; set; }

	public ActionArgs Clone()
	{
		ActionArgs fillArgs = Instance();
		List<IInflexID> list = new List<IInflexID>();
		list.AddRange(ObjectIDs);
		fillArgs.ObjectIDs = list.ToArray();
		DirectFill(ref fillArgs);
		return fillArgs;
	}

	protected virtual void DirectFill(ref ActionArgs fillArgs)
	{
	}

	protected virtual ActionArgs Instance()
	{
		return (ActionArgs)Activator.CreateInstance(GetType());
	}
}
