using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexData.RestrictionQuery;

[InflexTableName(InflexTableOwners.InflexSchema, "sm_query")]
public class TempQuery
{
	public const string TableName = "sm_query";

	public const InflexTableOwners TableOwner = InflexTableOwners.InflexSchema;

	public const string PrimaryField = "id";

	[PrimaryKey]
	[MapField("id")]
	public decimal ID { get; set; }

	[MapField("subtype_id")]
	public decimal? SubtypeID { get; set; }
}
