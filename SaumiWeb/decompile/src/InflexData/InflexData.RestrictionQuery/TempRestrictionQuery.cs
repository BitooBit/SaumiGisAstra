using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexData.RestrictionQuery;

[InflexTableName(InflexTableOwners.InflexSchema, "sm_restriction_query")]
public class TempRestrictionQuery
{
	internal const string TABLE_NAME = "sm_restriction_query";

	internal const InflexTableOwners INFLEX_TABLE_OWNER = InflexTableOwners.InflexSchema;

	internal const string PRIMARY_COLUMN_NAME = "id";

	[MapField("id")]
	[PrimaryKey]
	public decimal Id { get; set; }
}
