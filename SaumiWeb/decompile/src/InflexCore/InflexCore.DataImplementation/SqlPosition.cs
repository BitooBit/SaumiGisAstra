namespace InflexCore.DataImplementation;

public class SqlPosition
{
	public int Index { get; set; }

	public string Name { get; set; }

	public bool IsHierarchicalLevel { get; set; }

	public SqlPosition(int index, string name, bool isHierarchicalLevel)
	{
		Index = index;
		Name = name;
		IsHierarchicalLevel = isHierarchicalLevel;
	}
}
