namespace InflexCore.Action;

public abstract class ShowInflexObjectAction : InflexAction
{
	public override string Caption => "Свойства";

	public bool ShowModal { get; set; }
}
