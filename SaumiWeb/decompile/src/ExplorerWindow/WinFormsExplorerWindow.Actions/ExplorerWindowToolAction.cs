using System;
using InflexCore.Action;

namespace WinFormsExplorerWindow.Actions;

public abstract class ExplorerWindowToolAction : InflexAction
{
	public ExplorerWindowContext DirectContext
	{
		get
		{
			return base.Context as ExplorerWindowContext;
		}
		set
		{
			Initialize(value);
		}
	}

	protected sealed override bool DirectIsAllowedContext()
	{
		if (DirectContext != null && DirectContext.ExplorerWindow != null && !DirectContext.IsDataBlocked)
		{
			return ProtectedIsAllowedContext();
		}
		return false;
	}

	protected virtual bool ProtectedIsAllowedContext()
	{
		return true;
	}

	protected sealed override void DirectExecute()
	{
		if (DirectContext == null)
		{
			throw new NotSupportedException();
		}
		ProtectedDirectExecute();
	}

	protected abstract void ProtectedDirectExecute();
}
