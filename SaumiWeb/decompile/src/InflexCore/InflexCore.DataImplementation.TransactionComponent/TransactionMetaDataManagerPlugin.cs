using System;
using System.Collections.Generic;
using System.Linq;
using InflexCore.Components;
using InflexCore.DataImplementation.MetaDataComponent;
using InflexCore.DataImplementation.TransactionComponent.MetaData;
using InflexCore.MetaData;

namespace InflexCore.DataImplementation.TransactionComponent;

[ReferenceComponent(typeof(InflexMetaDataManager))]
public class TransactionMetaDataManagerPlugin : AppManager.AppManagerComponent, IMetaDataManagerPlugin
{
	private Dictionary<string, string> _ChildIdentFields;

	public void FillMetaDataIfNeed(int iteration, IInflexTypeDescription typeDescription)
	{
		if (typeDescription.IsAbstract || typeDescription.Attributes.Get<FillTypeMetaData>(defaultAvailable: true) != null)
		{
			return;
		}
		Dictionary<IInflexType, DbTableMetaData> dictionary = new Dictionary<IInflexType, DbTableMetaData>();
		foreach (IInflexProperty property in typeDescription.Properties)
		{
			if (!dictionary.TryGetValue(property.OwnerType, out var value))
			{
				value = new DbTableMetaData(property.OwnerType);
				dictionary.Add(property.OwnerType, value);
			}
			value.Fields.Add(new DbFieldMetaData
			{
				Property = property,
				Table = value
			});
		}
		SeqTypeMetaData seqTypeMetaData = new SeqTypeMetaData();
		foreach (KeyValuePair<IInflexType, DbTableMetaData> item in dictionary)
		{
			seqTypeMetaData.AddSeq(item.Key, base.AppManager.MainSchema, item.Key.TableName);
		}
		DbTableCollectionMetaData dbTableCollectionMetaData = new DbTableCollectionMetaData();
		foreach (KeyValuePair<IInflexType, DbTableMetaData> item2 in dictionary)
		{
			dbTableCollectionMetaData.Add(item2.Value);
			if (item2.Value.IsParent)
			{
				seqTypeMetaData.MainSeqType = item2.Key;
				Dictionary<string, string> childIdentFields = GetChildIdentFields();
				if (childIdentFields.TryGetValue(item2.Key.TableName.ToUpper(), out var identField))
				{
					DbFieldMetaData dbFieldMetaData = item2.Value.Fields.Where((DbFieldMetaData f) => f.Property.Name.ToUpper() == identField).FirstOrDefault();
					if (dbFieldMetaData != null)
					{
						dbFieldMetaData.IsChildIdent = true;
					}
				}
			}
			else
			{
				IInflexType parent = item2.Key.Parent;
				if (parent == null || !dictionary.TryGetValue(parent, out var value2))
				{
					throw new Exception("Parent type not found");
				}
				item2.Value.ParentTable = value2;
			}
		}
		typeDescription.Attributes.Set(seqTypeMetaData);
		dbTableCollectionMetaData.CalcDefaultValues(base.AppManager);
		dbTableCollectionMetaData.CalcPrimaryFields(base.AppManager);
		dbTableCollectionMetaData.CalcGeneralizationAssociation(base.AppManager);
		typeDescription.Attributes.Set(dbTableCollectionMetaData);
		typeDescription.Attributes.Set(new FillTypeMetaData());
	}

	private Dictionary<string, string> GetChildIdentFields()
	{
		if (_ChildIdentFields == null)
		{
			_ChildIdentFields = new InnerDbAccessor(base.AppManager.NativeSQLAccessor).GetChildIdentFields(base.AppManager.MainSchema);
		}
		return _ChildIdentFields;
	}
}
