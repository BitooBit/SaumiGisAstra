using System;
using BLToolkit.Data;
using BLToolkit.Data.DataProvider;
using BLToolkit.Mapping;
using BLToolkit.Reflection.Extension;
using BLToolkit.Reflection.MetadataProvider;

namespace InflexCore.DataImplementation.Reflection;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SeqNameAttribute : Attribute
{
	public InflexTableOwners OwnerType { get; set; }

	public string OwnerName { get; set; }

	public string SeqName { get; set; }

	public Type TableClass { get; set; }

	public string SeqPrefix { get; set; }

	public SeqNameAttribute(Type tableClass)
		: this(tableClass, "seq")
	{
	}

	public SeqNameAttribute(Type tableClass, string seqPrefix)
	{
		TableClass = tableClass;
		SeqPrefix = seqPrefix;
	}

	public SeqNameAttribute(InflexTableOwners ownerType, string seqName)
		: this(ownerType, null, seqName)
	{
	}

	public SeqNameAttribute(InflexTableOwners ownerType, string ownerName, string seqName)
	{
		OwnerType = ownerType;
		OwnerName = ownerName;
		SeqName = seqName;
	}

	public virtual string GetSequence(MappingSchema mappingSchema, DbManager dbManager)
	{
		if (!string.IsNullOrEmpty(SeqName))
		{
			string owner = GetOwner(OwnerType, OwnerName);
			return GetSequence(dbManager, owner, SeqName);
		}
		if (!string.IsNullOrEmpty(SeqPrefix) && (object)TableClass != null)
		{
			GetOwner(mappingSchema, TableClass, out var owner2, out var tableName);
			return GetSequence(dbManager, owner2, SeqPrefix + tableName);
		}
		throw new InvalidOperationException();
	}

	private void GetOwner(MappingSchema mappingSchema, Type type, out string owner, out string tableName)
	{
		MetadataProviderBase metadataProvider = mappingSchema.MetadataProvider;
		ExtensionList extensions = mappingSchema.Extensions;
		owner = metadataProvider.GetDatabaseName(type, extensions, out var isSet);
		tableName = metadataProvider.GetTableName(type, extensions, out isSet);
	}

	private string GetOwner(InflexTableOwners ownerType, string ownerName)
	{
		return ownerType switch
		{
			InflexTableOwners.CurrentUser => AppManager.CurrentUser, 
			InflexTableOwners.InflexSchema => AppManager.InflexSchema, 
			_ => ownerName, 
		};
	}

	private string GetSequence(DbManager db, string owner, string seqName)
	{
		seqName = db.DataProvider.Convert(seqName, ConvertType.NameToQueryTable).ToString();
		if (!string.IsNullOrEmpty(owner))
		{
			owner = db.DataProvider.Convert(owner, ConvertType.NameToDatabase).ToString();
			return owner + db.DataProvider.DatabaseTableDelimiter + seqName;
		}
		return seqName;
	}
}
