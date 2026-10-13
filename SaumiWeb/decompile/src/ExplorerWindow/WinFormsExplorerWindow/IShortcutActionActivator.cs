using System;
using System.Windows.Forms;
using InflexCore.Action;

namespace WinFormsExplorerWindow;

public interface IShortcutActionActivator : IDisposable
{
	void Register(Keys keys, InflexAction action);

	void UnRegister(Keys keys);

	bool ActivateAction(Keys keys);
}
