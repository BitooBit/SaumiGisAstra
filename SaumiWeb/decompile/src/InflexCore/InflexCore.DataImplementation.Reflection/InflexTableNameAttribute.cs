using BLToolkit.DataAccess;

namespace InflexCore.DataImplementation.Reflection;

public class InflexTableNameAttribute : TableNameAttribute
{
	public InflexTableOwners OwnerType { get; set; }

	public string OwnerName { get; set; }

	public InflexTableNameAttribute(InflexTableOwners ownerType, string tableName)
		: this(ownerType, null, tableName)
	{
	}

	public InflexTableNameAttribute(InflexTableOwners ownerType, string ownerName, string tableName)
		: base(tableName)
	{
		OwnerType = ownerType;
		OwnerName = ownerName;
	}
}
