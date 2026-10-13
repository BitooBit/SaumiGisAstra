using System;

namespace ExplorerWindowBase;

public interface IExplorerWindowExtention : IDisposable
{
	void Initialize(int dataType, IUserExplorerWindow explorerWindow);
}
