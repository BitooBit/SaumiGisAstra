using System;
using BLToolkit.Mapping;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.TransactionComponent;

[InflexTableName(InflexTableOwners.InflexSchema, "sm_v_changes")]
public class PropertyChangeItem
{
	[MapField("old_value_str")]
	public string StringValue { get; set; }

	[MapField("old_value_num")]
	public decimal? NumberValue { get; set; }

	[MapField("old_value_date")]
	public DateTime? DateTimeValue { get; set; }

	[MapField("change_date")]
	public DateTime? ChangeDate { get; set; }

	[MapField("column_name")]
	public string PropertyName { get; set; }

	[MapIgnore]
	public object OldValue
	{
		get
		{
			if (NumberValue.HasValue)
			{
				return NumberValue.Value;
			}
			if (DateTimeValue.HasValue)
			{
				return DateTimeValue.Value;
			}
			return StringValue;
		}
	}
}
