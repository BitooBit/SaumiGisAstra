using System.Data;

namespace InflexData;

public class InflexDataSet : DataSet
{
	public InflexDataTable MainTable { get; set; }

	protected override void Dispose(bool disposing)
	{
		foreach (object table in base.Tables)
		{
			if (table is InflexDataTable inflexDataTable)
			{
				inflexDataTable.Dispose();
			}
		}
		base.Tables.Clear();
		base.Dispose(disposing);
	}
}
