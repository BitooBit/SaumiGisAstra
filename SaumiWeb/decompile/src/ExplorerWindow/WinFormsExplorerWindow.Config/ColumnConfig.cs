using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace WinFormsExplorerWindow.Config;

[InflexTableName(InflexTableOwners.InflexSchema, "sm_column_settings")]
public class ColumnConfig
{
	[PrimaryKey]
	[MapField("key")]
	public string Key { get; set; }

	[MapField("value")]
	public string Value { get; set; }
}
