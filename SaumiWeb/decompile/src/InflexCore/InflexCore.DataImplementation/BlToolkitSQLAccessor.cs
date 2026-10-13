using System.Collections.Generic;
using System.Data;
using BLToolkit.Data;
using BLToolkit.DataAccess;
using Oracle.DataAccess.Client;

namespace InflexCore.DataImplementation;

public class BlToolkitSQLAccessor : DataAccessor, INativeSQLAccessor
{
	public int InitialLongFeatchSize { get; set; }

	public BlToolkitSQLAccessor()
	{
		InitialLongFeatchSize = -1;
	}

	public IDataReader ExecuteSQL(string sql, params NativeSQLParam[] parameters)
	{
		using DbManager dbManager = GetDbManager();
		List<IDbDataParameter> list = new List<IDbDataParameter>();
		if (parameters != null)
		{
			foreach (NativeSQLParam nativeSQLParam in parameters)
			{
				if (nativeSQLParam.Type.HasValue)
				{
					list.Add(dbManager.Parameter(nativeSQLParam.Key, nativeSQLParam.Value, nativeSQLParam.Type.Value));
				}
				else
				{
					list.Add(dbManager.Parameter(nativeSQLParam.Key, nativeSQLParam.Value));
				}
			}
		}
		dbManager.InitCommand += OnManagerInitCommand;
		try
		{
			return dbManager.SetCommand(sql, list.ToArray()).ExecuteReader();
		}
		finally
		{
			dbManager.InitCommand -= OnManagerInitCommand;
		}
	}

	private void OnManagerInitCommand(object sender, InitCommandEventArgs ea)
	{
		if (ea.Command is OracleCommand oracleCommand && oracleCommand.InitialLONGFetchSize != InitialLongFeatchSize)
		{
			oracleCommand.InitialLONGFetchSize = InitialLongFeatchSize;
		}
	}

	public decimal GetCountValues(string sql)
	{
		using IDataReader dataReader = ExecuteSQL("select count(*) from (" + sql + ")");
		dataReader.Read();
		return dataReader.GetDecimal(0);
	}
}
