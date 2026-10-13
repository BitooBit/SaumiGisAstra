using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InflexCore.log;

namespace InflexCore.DataImplementation.TransactionComponent.MetaData;

public class InnerDbAccessor : AppManager.InflexAccessorBase<object, InnerDbAccessor>
{
	private INativeSQLAccessor _SQLAccessor;

	private INativeSQLAccessor SQLAccessor
	{
		get
		{
			if (_SQLAccessor == null)
			{
				_SQLAccessor = ((base.AppManager == null) ? null : base.AppManager.NativeSQLAccessor);
			}
			return _SQLAccessor;
		}
	}

	public InnerDbAccessor()
	{
	}

	public InnerDbAccessor(INativeSQLAccessor sqlAccessor)
	{
		_SQLAccessor = sqlAccessor;
	}

	public string GetPrimaryFieldName(string owner, string tableName)
	{
		using (IDataReader dataReader = SQLAccessor.ExecuteSQL("   select cc.column_name from sys.all_constraints c, sys.all_cons_columns cc\r\n                                    where c.owner=:owner and c.table_name=:tableName \r\n                                                         and c.constraint_type='P'\r\n                                                         and cc.constraint_name=c.constraint_name", new NativeSQLParam("owner", owner.ToUpper()), new NativeSQLParam("tableName", tableName.ToUpper())))
		{
			if (dataReader.Read())
			{
				string result = dataReader.GetString(0);
				if (!dataReader.Read())
				{
					return result;
				}
			}
		}
		throw new NotSupportedException("Only one primary field supported. Table = " + tableName);
	}

	public decimal GetNextID(string seqName)
	{
		string sql = $"select {seqName}.nextval from dual";
		using IDataReader dataReader = SQLAccessor.ExecuteSQL(sql);
		dataReader.Read();
		return dataReader.GetDecimal(0);
	}

	public string GetGeneralizeFieldName(string owner, string tableName)
	{
		string sql = $"select t.supertype_column from {owner}.fm_tables t where t.table_name=:tableName";
		using (IDataReader dataReader = SQLAccessor.ExecuteSQL(sql, new NativeSQLParam("tableName", tableName)))
		{
			if (dataReader.Read())
			{
				string result = dataReader.GetString(0);
				if (dataReader.Read())
				{
					throw new NotSupportedException("Only one generalization field supported. Table = " + tableName);
				}
				return result;
			}
		}
		throw new InvalidOperationException("Unable to find generalization field. Table = " + tableName);
	}

	public object[] GetInflexObjectValues(IEnumerable<string> fields, string owner, string tableName, string restrictField, object restrictValue)
	{
		string sql = string.Format("select {0} from {1}.{2} where {3}=:value", new object[4]
		{
			fields.Aggregate((string s1, string s2) => s1 + ", " + s2),
			owner,
			tableName,
			restrictField
		});
		using (IDataReader dataReader = SQLAccessor.ExecuteSQL(sql, new NativeSQLParam("value", restrictValue)))
		{
			if (dataReader.Read())
			{
				List<object> list = new List<object>(dataReader.FieldCount);
				for (int num = 0; num < dataReader.FieldCount; num++)
				{
					list.Add(dataReader.IsDBNull(num) ? null : dataReader.GetValue(num));
				}
				if (!dataReader.Read())
				{
					return list.ToArray();
				}
			}
		}
		throw new NotSupportedException("Only one object needed. Table = " + tableName);
	}

	public object GetDefaultValue(string defaultValue)
	{
		string sql = "select " + defaultValue + " from dual";
		using (IDataReader dataReader = SQLAccessor.ExecuteSQL(sql))
		{
			if (dataReader.Read())
			{
				return dataReader.GetValue(0);
			}
		}
		throw new NotSupportedException("Only one result needed. DefaultValue = " + defaultValue);
	}

	public Dictionary<string, string> GetDefaultValueMetaData(string owner, string tableName)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		using IDataReader dataReader = SQLAccessor.ExecuteSQL("select column_name, data_default from sys.all_tab_columns where data_default is not null and owner=:owner and table_name=:tableName", new NativeSQLParam("owner", owner.ToUpper()), new NativeSQLParam("tableName", tableName.ToUpper()));
		while (dataReader.Read())
		{
			dictionary.Add(dataReader.GetString(0), dataReader.GetString(1));
		}
		return dictionary;
	}

	public Dictionary<string, string> GetChildIdentFields(string owner)
	{
		string sql = $"select upper(table_name), upper(subtype_column) from {owner}.fm_tables where subtype_column is not null";
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		using IDataReader dataReader = SQLAccessor.ExecuteSQL(sql);
		while (dataReader.Read())
		{
			dictionary.Add(dataReader.GetString(0), dataReader.GetString(1));
		}
		return dictionary;
	}

	public bool IsExists(string owner, string tableName, string fieldID, decimal id)
	{
		string text = string.Format("select {0} from {1}.{2} where {0}=:id", fieldID, owner, tableName);
		try
		{
			using IDataReader dataReader = SQLAccessor.ExecuteSQL(text, new NativeSQLParam("id", id));
			return dataReader.Read();
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)text, ex);
			return false;
		}
	}

	public void Delete(string owner, string tableName, string fieldName, decimal value)
	{
		string sql = $"delete from {owner}.{tableName} where {fieldName}=:id";
		using (SQLAccessor.ExecuteSQL(sql, new NativeSQLParam("id", value)))
		{
		}
	}

	public decimal GetID(string owner, string tableName, string columnName, string restrictColumn, decimal restrictValue)
	{
		string sql = string.Format("select {0} from {1}.{2} where {3}=:id", new object[4] { columnName, owner, tableName, restrictColumn });
		using (IDataReader dataReader = SQLAccessor.ExecuteSQL(sql, new NativeSQLParam("id", restrictValue)))
		{
			if (dataReader.Read())
			{
				decimal result = dataReader.GetDecimal(0);
				if (!dataReader.Read())
				{
					return result;
				}
			}
		}
		return 0m;
	}

	public void Save(List<string> fieldNames, List<object> fieldValues, string owner, string tableName, string restrictColumn, object restrictValue)
	{
		string text = "";
		NativeSQLParam[] array = new NativeSQLParam[fieldValues.Count + 1];
		for (int i = 0; i < fieldNames.Count; i++)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			object obj = text;
			text = string.Concat(new object[4]
			{
				obj,
				fieldNames[i],
				" = :f",
				i
			});
			array[i] = new NativeSQLParam("f" + i, fieldValues[i]);
		}
		array[^1] = new NativeSQLParam("value", restrictValue);
		string sql = string.Format("update {0}.{1} set {2} where {3}=:value", new object[4] { owner, tableName, text, restrictColumn });
		using (SQLAccessor.ExecuteSQL(sql, array))
		{
		}
	}

	public void SaveNew(List<string> fieldNames, List<object> fieldValues, string owner, string tableName)
	{
		string text = "";
		string text2 = "";
		NativeSQLParam[] array = new NativeSQLParam[fieldValues.Count];
		for (int i = 0; i < fieldNames.Count; i++)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
				text2 += ", ";
			}
			text += fieldNames[i];
			text2 = text2 + ":f" + i;
			array[i] = new NativeSQLParam("f" + i, fieldValues[i]);
		}
		string sql = string.Format("insert into {0}.{1} ({2}) values({3})", new object[4] { owner, tableName, text, text2 });
		using (SQLAccessor.ExecuteSQL(sql, array))
		{
		}
	}

	public decimal GetChildIdentValue(string owner, string tableName, string fieldName, string primName, decimal id)
	{
		string sql = string.Format("select {0} from {1}.{2} where {3}=:id", new object[4] { fieldName, owner, tableName, primName });
		using (IDataReader dataReader = SQLAccessor.ExecuteSQL(sql, new NativeSQLParam("id", id)))
		{
			if (dataReader.Read())
			{
				decimal result = dataReader.GetDecimal(0);
				if (!dataReader.Read())
				{
					return result;
				}
			}
		}
		throw new NotSupportedException("Only one record result.");
	}
}
