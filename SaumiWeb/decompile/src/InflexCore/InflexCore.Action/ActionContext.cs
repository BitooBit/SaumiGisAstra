using System;

namespace InflexCore.Action;

public class ActionContext : IDisposable
{
	public abstract class ContextHandlerProxy
	{
		private ActionContext _Context;

		protected abstract void ContextOnAfterUpdate();

		protected abstract void ContextOnBeforeDispose();

		protected ContextHandlerProxy(ActionContext context)
		{
			_Context = context;
			_Context.AfterUpdate += ContextOnAfterUpdate;
			_Context.BeforeDispose += ContextOnBeforeDisposeInternal;
		}

		private void ContextOnBeforeDisposeInternal()
		{
			_Context.AfterUpdate -= ContextOnBeforeDispose;
			_Context.BeforeDispose -= ContextOnBeforeDisposeInternal;
			ContextOnBeforeDispose();
			_Context = null;
		}
	}

	private bool _IsDataBlocked;

	private IInflexType _Type;

	public bool IsDataBlocked
	{
		get
		{
			return _IsDataBlocked;
		}
		set
		{
			_IsDataBlocked = value;
			FireDataBlockedChanged(value);
		}
	}

	public IInflexType Type
	{
		get
		{
			CheckType();
			return _Type;
		}
		set
		{
			_Type = value;
		}
	}

	public ActionArgs ActionArgs { get; set; }

	private bool IsSingleType { get; set; }

	public event Action<bool> DataBlockedChanged;

	private event System.Action AfterUpdate;

	private event System.Action BeforeDispose;

	protected ActionContext()
	{
		IsSingleType = false;
	}

	private void FireDataBlockedChanged(bool isDataBlocked)
	{
		DataBlockedChanged?.Invoke(isDataBlocked);
	}

	public static ActionContext GetEmptyContext(bool isSingleType)
	{
		ActionContext actionContext = new ActionContext();
		actionContext.ActionArgs = new ActionArgs();
		actionContext.IsSingleType = isSingleType;
		return actionContext;
	}

	public ActionContext Clone()
	{
		ActionContext fillContext = Instance();
		fillContext.IsSingleType = IsSingleType;
		fillContext.Type = Type;
		fillContext.ActionArgs = ActionArgs.Clone();
		DirectFill(ref fillContext);
		return fillContext;
	}

	protected virtual void DirectFill(ref ActionContext fillContext)
	{
	}

	protected virtual ActionContext Instance()
	{
		return (ActionContext)Activator.CreateInstance(GetType());
	}

	public void Update()
	{
		CheckType();
		System.Action afterUpdate = AfterUpdate;
		BeforeHandleUpdate();
		afterUpdate?.Invoke();
		AfterHandleUpdate();
	}

	private void CheckType()
	{
		if (IsSingleType && _Type == null)
		{
			throw new InvalidOperationException("For this 'ActionContext' property [Type] can't be null.");
		}
	}

	protected virtual void AfterHandleUpdate()
	{
	}

	protected virtual void BeforeHandleUpdate()
	{
	}

	public void Dispose()
	{
		BeforeDispose?.Invoke();
		DirectDispose();
	}

	protected virtual void DirectDispose()
	{
		_Type = null;
		if (ActionArgs != null)
		{
			ActionArgs.ObjectIDs = null;
			ActionArgs = null;
		}
	}
}
