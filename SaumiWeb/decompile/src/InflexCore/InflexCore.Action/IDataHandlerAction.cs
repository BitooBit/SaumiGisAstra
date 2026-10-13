namespace InflexCore.Action;

public interface IDataHandlerAction
{
	ActionContext Context { get; }

	void Initialize(ActionContext context);

	bool InnerIsAllowedContext();
}
