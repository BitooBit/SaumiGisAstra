using System;
using System.Text;
using BLToolkit.Data;
using BLToolkit.Data.DataProvider;
using BLToolkit.DataAccess;
using BLToolkit.Reflection.Extension;
using BLToolkit.Reflection.MetadataProvider;
using InflexCore.DataImplementation.Accessors;

namespace InflexCore.DataImplementation.Reflection;

public class InflexSqlQueryAttribute : SqlQueryAttribute
{
	private string _SqlText;

	public InflexSqlQueryAttribute(string sqlText)
		: base(sqlText)
	{
		base.IsDynamic = true;
	}

	public override string GetSqlText(DataAccessor accessor, DbManager dbManager)
	{
		if (!string.IsNullOrEmpty(_SqlText))
		{
			return _SqlText;
		}
		if (accessor is IInflexAccessor inflexAccessor && !string.IsNullOrEmpty(base.SqlText))
		{
			Type accessorType = inflexAccessor.GetAccessorType();
			if ((object)accessorType != null && IsSqlContainFormatChars(base.SqlText))
			{
				MetadataProviderBase metadataProvider = accessor.MappingSchema.MetadataProvider;
				ExtensionList extensions = accessor.MappingSchema.Extensions;
				string databaseName = metadataProvider.GetDatabaseName(accessorType, extensions, out var isSet);
				string ownerName = metadataProvider.GetOwnerName(accessorType, extensions, out isSet);
				string tableName = metadataProvider.GetTableName(accessorType, extensions, out isSet);
				tableName = dbManager.DataProvider.Convert(tableName, ConvertType.NameToQueryTable).ToString();
				StringBuilder stringBuilder = new StringBuilder();
				if (databaseName == null && ownerName == null)
				{
					stringBuilder.Append(tableName);
				}
				else
				{
					if (databaseName != null)
					{
						stringBuilder.Append(dbManager.DataProvider.Convert(databaseName, ConvertType.NameToDatabase));
					}
					if (ownerName != null)
					{
						stringBuilder.Append(dbManager.DataProvider.DatabaseOwnerDelimiter).Append(dbManager.DataProvider.Convert(ownerName, ConvertType.NameToOwner)).Append(dbManager.DataProvider.OwnerTableDelimiter);
					}
					else
					{
						stringBuilder.Append(dbManager.DataProvider.DatabaseTableDelimiter);
					}
					stringBuilder.Append(tableName);
				}
				string replaceStr = (string)dbManager.DataProvider.Convert(AppManager.InflexSchema, ConvertType.NameToDatabase);
				string replaceStr2 = (string)dbManager.DataProvider.Convert(AppManager.CurrentUser, ConvertType.NameToDatabase);
				_SqlText = base.SqlText.ReplaceFormatChar(FormatSqlChars.Table, stringBuilder.ToString()).ReplaceFormatChar(FormatSqlChars.Schema, replaceStr).ReplaceFormatChar(FormatSqlChars.User, replaceStr2);
				return _SqlText;
			}
		}
		return base.GetSqlText(accessor, dbManager);
	}

	private bool IsSqlContainFormatChars(string sql)
	{
		string text = sql.ToLower();
		if (!text.Contains(FormatSqlChars.Table.ToLower()) && !text.Contains(FormatSqlChars.Schema.ToLower()))
		{
			return text.Contains(FormatSqlChars.User.ToLower());
		}
		return true;
	}
}
