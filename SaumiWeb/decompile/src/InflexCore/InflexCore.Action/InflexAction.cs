using System;
using InflexCore.Action.Attributes;

namespace InflexCore.Action;

public abstract class InflexAction
{
	private class ActionContextHandlerProxy : ActionContext.ContextHandlerProxy, IDisposable
	{
		private InflexAction _Action;

		public ActionContextHandlerProxy(ActionContext context, InflexAction action)
			: base(context)
		{
			_Action = action;
		}

		protected override void ContextOnBeforeDispose()
		{
			_Action.BeforeContextDispose();
			_Action.Context = null;
			_Action._InExecuteInstance = null;
			_Action = null;
		}

		protected override void ContextOnAfterUpdate()
		{
			_Action.AfterContextUpdate();
		}

		public void Dispose()
		{
			ContextOnBeforeDispose();
		}
	}

	private class InflexActionExecuteInstance
	{
		private int _Count;

		public bool IsInstance => _Count > 0;

		public void BeginExecute()
		{
			_Count++;
		}

		public void EndExecute()
		{
			_Count--;
			if (_Count < 0)
			{
				throw new InvalidOperationException("EndExecute(), this.Count<0");
			}
		}
	}

	private InflexActionExecuteInstance _InExecuteInstance = new InflexActionExecuteInstance();

	private ActionContextHandlerProxy _ContextHandler;

	private readonly bool _IsAutoInstanceAction;

	private readonly bool _IsDataContextAction;

	public bool IsAutoInstanceAction => _IsAutoInstanceAction;

	public bool IsDataContextAction => _IsDataContextAction;

	public ActionContext Context { get; private set; }

	public virtual string Name => GetType().Name;

	public abstract string Caption { get; }

	public virtual bool IsHiddenWhenDisabled => false;

	public bool? IsAllowed
	{
		get
		{
			if (!IsAllowedContext)
			{
				return false;
			}
			return DirectIsAllowed();
		}
	}

	public bool IsAllowedContext
	{
		get
		{
			if (InExecute && !AllowedMultiExecute())
			{
				return false;
			}
			if (Context == null && !AllowedNullContext())
			{
				return false;
			}
			return DirectIsAllowedContext();
		}
	}

	protected bool InExecute
	{
		get
		{
			lock (_InExecuteInstance)
			{
				return _InExecuteInstance.IsInstance;
			}
		}
	}

	protected virtual bool IsBeforeExecuteAllowed => true;

	public virtual string NotExecuteMessage => "";

	public event Action<Exception> UnhandledActionException;

	protected InflexAction()
	{
		_IsAutoInstanceAction = Attribute.IsDefined(GetType(), typeof(AutoInstanceActionAttribute));
		_IsDataContextAction = Attribute.IsDefined(GetType(), typeof(DataContextActionAttribute));
	}

	private void OnUnhandledActionException(Exception exception)
	{
		UnhandledActionException?.Invoke(exception);
	}

	public void Initialize(ActionContext context)
	{
		if (Context != null)
		{
			throw new InvalidOperationException("Can't initialize action, cause its context value has already been set.");
		}
		Context = context;
		if (_ContextHandler != null)
		{
			ActionContextHandlerProxy contextHandler = _ContextHandler;
			contextHandler.Dispose();
			_ContextHandler = null;
		}
		_ContextHandler = new ActionContextHandlerProxy(context, this);
	}

	protected abstract bool? DirectIsAllowed();

	protected virtual bool AllowedMultiExecute()
	{
		return false;
	}

	protected virtual bool AllowedNullContext()
	{
		return false;
	}

	protected abstract bool DirectIsAllowedContext();

	public bool Execute()
	{
		try
		{
			if (IsAllowed != false)
			{
				if (IsBeforeExecuteAllowed)
				{
					try
					{
						lock (_InExecuteInstance)
						{
							_InExecuteInstance.BeginExecute();
						}
						DirectExecute();
					}
					finally
					{
						lock (_InExecuteInstance)
						{
							_InExecuteInstance.EndExecute();
						}
					}
					return true;
				}
				return false;
			}
			throw new ActionNotAllowedException(this, Context);
		}
		catch (Exception exception)
		{
			OnUnhandledActionException(exception);
			return false;
		}
	}

	protected abstract void DirectExecute();

	protected void BeforeContextDispose()
	{
		OnBeforeContextDispose();
		_ContextHandler = null;
	}

	protected virtual void OnBeforeContextDispose()
	{
	}

	protected virtual void AfterContextUpdate()
	{
	}
}
