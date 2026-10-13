using System.Data;
using BLToolkit.Data;
using InflexCore;
using InflexCore.DataImplementation.Reflection;
using Oracle.DataAccess.Client;

namespace InflexData.RestrictionQuery;

public abstract class TempRestrictionQueryAccessor : AppManager.InflexAccessorBase<TempRestrictionQuery, TempRestrictionQueryAccessor>
{
	[InflexSqlQuery("delete from <T>")]
	public abstract void Clear();

	public static string GetTempRestrictionQueryText(AppManager appManager)
	{
		return string.Format("select {0} from {1}", "id", GetRestrictionTempQueryTableName(appManager));
	}

	public static string GetRestrictionTempQueryTableName(AppManager appManager)
	{
		string text = appManager.MainSchema + ".";
		return text + "sm_restriction_query";
	}

	public void Fill(decimal[] ids)
	{
		using DbManager dbManager = GetDbManager();
		string commandText = $"insert into {GetRestrictionTempQueryTableName(base.AppManager)}(id) values(:ids)";
		OracleParameter oracleParameter = new OracleParameter("ids", OracleDbType.Decimal, ids.Length, ParameterDirection.Input);
		oracleParameter.CollectionType = OracleCollectionType.None;
		oracleParameter.Value = ids;
		if (dbManager.InsertCommand is OracleCommand oracleCommand)
		{
			oracleCommand.Parameters.Clear();
			oracleCommand.Parameters.Add(oracleParameter);
			oracleCommand.CommandText = commandText;
			oracleCommand.ArrayBindCount = ids.Length;
			oracleCommand.ExecuteNonQuery();
		}
	}
}
