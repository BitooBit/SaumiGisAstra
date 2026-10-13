using BLToolkit.Data;
using BLToolkit.DataAccess;

namespace InflexData.RestrictionQuery;

public class StoredProcedureAccessor : DataAccessor
{
	public void ExecuteProcedure(string procName)
	{
		if (!string.IsNullOrEmpty(procName))
		{
			using (DbManager dbManager = GetDbManager())
			{
				dbManager.SetSpCommand(procName).ExecuteNonQuery();
			}
		}
	}
}
