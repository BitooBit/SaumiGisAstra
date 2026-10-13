using BLToolkit.Mapping;

namespace InflexCore.DataImplementation.MetaDataComponent;

public class InflexPropertyLink
{
	[MapField("column_name")]
	public string ColumnName { get; set; }

	[MapField("type_id")]
	public decimal? TypeID { get; set; }

	[MapField("subtype_id")]
	public decimal? SubtypeID { get; set; }

	[MapField("dict_id")]
	public decimal? DictionaryID { get; set; }
}
