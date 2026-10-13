namespace InflexCore.Action.ActionMenu;

public interface ICheckableAction
{
	bool IsChecked { get; set; }

	void OnCheckStateChanged(bool checkState);
}
