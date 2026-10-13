using System;

namespace InflexCore.Action.ActionMenu;

public interface ICheckableItem
{
	bool IsCheckable { get; set; }

	bool IsChecked { get; set; }

	Action<bool> CheckedChangedHandler { get; set; }
}
