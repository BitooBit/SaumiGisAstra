using System;
using System.Collections.Generic;
using System.Linq;
using InflexCore.MetaData;

namespace InflexCore.DataImplementation.TransactionComponent.MetaData;

public class DbTableMetaData : AbstractInflexMetaData
{
	private IInflexType InflexType { get; set; }

	public DbTableMetaData ParentTable { get; set; }

	public HashSet<DbFieldMetaData> Fields { get; private set; }

	private DbFieldMetaData PrimaryKey { get; set; }

	private DbFieldMetaData GeneralizeKey { get; set; }

	public bool IsParent => InflexType.Parent == null;

	public DbTableMetaData(IInflexType inflexType)
	{
		InflexType = inflexType;
		Fields = new HashSet<DbFieldMetaData>();
	}

	public void Fill(ref InflexObject inflexObject, AppManager appManager)
	{
		List<string> list = new List<string>();
		List<DbFieldMetaData> list2 = new List<DbFieldMetaData>();
		foreach (DbFieldMetaData field in Fields)
		{
			if (field.IsSimpleDbType())
			{
				list.Add(field.Property.Name);
				list2.Add(field);
			}
		}
		GetRestrictColumnAndValue(inflexObject, out var restrictColumn, out var restrictValue);
		object[] inflexObjectValues = new InnerDbAccessor(appManager.NativeSQLAccessor).GetInflexObjectValues(list, appManager.MainSchema, InflexType.TableName, restrictColumn, restrictValue);
		for (int i = 0; i < list2.Count; i++)
		{
			DbFieldMetaData dbFieldMetaData = list2[i];
			dbFieldMetaData.Property.SetValue(inflexObject, inflexObjectValues[i]);
		}
	}

	public void Save(ref InflexObject inflexObject, AppManager appManager, InnerDbAccessor accessor)
	{
		List<string> list = new List<string>();
		List<object> list2 = new List<object>();
		Dictionary<IInflexProperty, object> dictionary = inflexObject.DirectGetPropertyChangeValues();
		foreach (DbFieldMetaData field in Fields)
		{
			if (field.IsSimpleDbType() && dictionary.ContainsKey(field.Property))
			{
				list.Add(field.Property.Name);
				list2.Add(dictionary[field.Property]);
			}
		}
		if (list.Count != 0)
		{
			GetRestrictColumnAndValue(inflexObject, out var restrictColumn, out var restrictValue);
			accessor.Save(list, list2, appManager.MainSchema, InflexType.TableName, restrictColumn, restrictValue);
		}
	}

	public void SaveNew(ref InflexObject inflexObject, IInflexTypeDescription typeDescription, AppManager appManager, InnerDbAccessor accessor)
	{
		List<string> list = new List<string>();
		List<object> list2 = new List<object>();
		foreach (DbFieldMetaData field in Fields)
		{
			if (field.IsPrimary)
			{
				decimal num = (decimal)field.Property.GetValue(inflexObject);
				if (num == 0m)
				{
					num = typeDescription.Attributes.Get<SeqTypeMetaData>().GetNextID(InflexType, accessor);
					field.Property.SetValue(inflexObject, num);
				}
				break;
			}
		}
		Dictionary<IInflexProperty, object> dictionary = inflexObject.DirectGetPropertyChangeValues();
		foreach (DbFieldMetaData field2 in Fields)
		{
			if (field2.IsSimpleDbType() && (!field2.Property.IsNullable || dictionary.ContainsKey(field2.Property)))
			{
				list.Add(field2.Property.Name);
				list2.Add(inflexObject.DirectGetPropertyValue(field2.Property));
			}
		}
		if (list.Count != 0)
		{
			accessor.SaveNew(list, list2, appManager.MainSchema, InflexType.TableName);
		}
	}

	private void GetRestrictColumnAndValue(InflexObject inflexObject, out string restrictColumn, out object restrictValue)
	{
		restrictColumn = ((ParentTable == null) ? PrimaryKey.Property.Name : GeneralizeKey.Property.Name);
		restrictValue = ((ParentTable == null) ? ((object)inflexObject.ID.ID) : ParentTable.PrimaryKey.Property.GetValue(inflexObject));
	}

	public void CalcPrimaryField(AppManager appManager)
	{
		string text = new InnerDbAccessor(appManager.NativeSQLAccessor).GetPrimaryFieldName(appManager.MainSchema, InflexType.TableName).ToUpper();
		foreach (DbFieldMetaData field in Fields)
		{
			if (field.Property.Name.ToUpper() == text)
			{
				field.IsPrimary = true;
				PrimaryKey = field;
				break;
			}
		}
	}

	public void CalcGeneralizationAssociation(AppManager appManager)
	{
		if (ParentTable == null)
		{
			return;
		}
		string text = new InnerDbAccessor(appManager.NativeSQLAccessor).GetGeneralizeFieldName(appManager.MainSchema, InflexType.TableName).ToUpper();
		foreach (DbFieldMetaData field in Fields)
		{
			if (field.Property.Name.ToUpper() == text)
			{
				field.IsGeneralize = true;
				GeneralizeKey = field;
				break;
			}
		}
	}

	public void FillDefaultValue(ref InflexObject inflexObject, AppManager appManager)
	{
		foreach (DbFieldMetaData field in Fields)
		{
			if (!string.IsNullOrEmpty(field.DefaultValue))
			{
				object defaultValue = new InnerDbAccessor(appManager.NativeSQLAccessor).GetDefaultValue(field.DefaultValue);
				field.Property.SetValue(inflexObject, defaultValue);
			}
		}
	}

	public void CalcDefaultValues(AppManager appManager)
	{
		Dictionary<string, string> defaultValueMetaData = new InnerDbAccessor(appManager.NativeSQLAccessor).GetDefaultValueMetaData(appManager.MainSchema, InflexType.TableName);
		foreach (DbFieldMetaData field in Fields)
		{
			if (defaultValueMetaData.TryGetValue(field.Property.Name, out var value))
			{
				field.DefaultValue = value;
			}
		}
	}

	public void FillParentID(ref InflexObject inflexObject)
	{
		GetRestrictColumnAndValue(inflexObject, out var column, out var restrictValue);
		column = column.ToUpper();
		Fields.Where((DbFieldMetaData f) => f.Property.Name.ToUpper() == column && !f.IsPrimary).FirstOrDefault()?.Property.SetValue(inflexObject, restrictValue);
	}

	public FullID GetFullID(AppManager appManager, InnerDbAccessor accessor, decimal restrictValue)
	{
		string tableName = InflexType.TableName;
		string name = PrimaryKey.Property.Name;
		string restrictColumn = ((ParentTable == null) ? name : GeneralizeKey.Property.Name);
		decimal iD = accessor.GetID(appManager.MainSchema, tableName, name, restrictColumn, restrictValue);
		if (iD > 0m)
		{
			FullID fullID = new FullID();
			fullID.FieldName = name;
			fullID.TableName = tableName;
			fullID.Value = iD;
			return fullID;
		}
		return null;
	}

	public decimal GetChildIdentValue(decimal id, AppManager appManager)
	{
		DbFieldMetaData dbFieldMetaData = Fields.Where((DbFieldMetaData f) => f.IsChildIdent).FirstOrDefault();
		string primName = (from f in Fields
			where f.IsPrimary
			select f.Property.Name).FirstOrDefault();
		if (dbFieldMetaData != null)
		{
			return new InnerDbAccessor(appManager.NativeSQLAccessor).GetChildIdentValue(appManager.MainSchema, InflexType.TableName, dbFieldMetaData.Property.Name, primName, id);
		}
		throw new InvalidOperationException("Child ident field not found.");
	}
}
