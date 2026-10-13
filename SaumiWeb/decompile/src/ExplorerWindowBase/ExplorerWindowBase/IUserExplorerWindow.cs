using System.Collections.Generic;
using InflexCore.Action;

namespace ExplorerWindowBase;

public interface IUserExplorerWindow
{
	IList<InflexAction> ContextActions { get; }

	IList<InflexAction> Actions { get; }
}
