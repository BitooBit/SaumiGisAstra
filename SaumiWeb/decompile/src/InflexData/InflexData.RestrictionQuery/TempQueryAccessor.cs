using System.Collections.Generic;
using System.Data;
using BLToolkit.Data;
using InflexCore;
using InflexCore.DataImplementation.Reflection;

namespace InflexData.RestrictionQuery;

public abstract class TempQueryAccessor : AppManager.InflexAccessorBase<TempQuery, TempQueryAccessor>
{
	public static string GetSelectPrimarySql(AppManager appManager)
	{
		string tempQueryTableName = GetTempQueryTableName(appManager);
		return string.Format("select {0} from {1}", "id", tempQueryTableName);
	}

	public static string GetTempQueryTableName(AppManager appManager)
	{
		string text = appManager.MainSchema + ".";
		return text + "sm_query";
	}

	[InflexSqlQuery("delete from <T>")]
	public abstract void Clear();

	public void Fill(string insertSelect)
	{
		using DbManager dbManager = GetDbManager();
		dbManager.SetCommand($"insert into {GetTempQueryTableName(base.AppManager)} {insertSelect}").ExecuteNonQuery();
	}

	[InflexSqlQuery("select count(*) from <T>")]
	public abstract decimal Count();

	public IList<decimal?> SelectGroupSubtypesID()
	{
		List<decimal?> list = new List<decimal?>();
		using DbManager dbManager = GetDbManager();
		using IDataReader dataReader = dbManager.SetCommand($"select subtype_id from {GetTempQueryTableName(base.AppManager)} group by subtype_id").ExecuteReader();
		while (dataReader.Read())
		{
			list.Add(dataReader.IsDBNull(0) ? ((decimal?)null) : new decimal?(dataReader.GetDecimal(0)));
		}
		return list;
	}
}
