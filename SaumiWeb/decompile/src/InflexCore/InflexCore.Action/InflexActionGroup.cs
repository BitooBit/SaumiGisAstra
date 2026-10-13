namespace InflexCore.Action;

public class InflexActionGroup
{
	public InflexActionGroup ParentGroup { get; set; }

	public string Name { get; set; }

	public int GroupOrder { get; set; }

	public string GlyphUri { get; set; }

	public override bool Equals(object obj)
	{
		if (!(obj is InflexActionGroup inflexActionGroup))
		{
			return false;
		}
		if (Name != inflexActionGroup.Name)
		{
			return false;
		}
		if (ParentGroup != null)
		{
			return ParentGroup.Equals(inflexActionGroup.ParentGroup);
		}
		return inflexActionGroup.ParentGroup == null;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}
}
