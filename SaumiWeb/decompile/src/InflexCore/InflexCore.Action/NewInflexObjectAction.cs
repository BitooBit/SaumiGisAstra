using InflexCore.Native;

namespace InflexCore.Action;

public abstract class NewInflexObjectAction : InflexAction
{
	public override string Caption => "Создать";

	public ItemData NewItemData { get; protected set; }
}
