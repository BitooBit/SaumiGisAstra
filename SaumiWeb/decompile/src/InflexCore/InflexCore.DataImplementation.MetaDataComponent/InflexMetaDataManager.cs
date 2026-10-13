using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InflexCore.Components;
using InflexCore.MetaData;

namespace InflexCore.DataImplementation.MetaDataComponent;

[SingleComponent(typeof(IInflexMetaDataManager))]
public class InflexMetaDataManager : AppManager.AppManagerComponent, IInflexMetaDataManager
{
	private Dictionary<IInflexType, IInflexTypeDescription> _Hash = new Dictionary<IInflexType, IInflexTypeDescription>();

	public IInflexTypeDescription GetInflexTypeDescription(IInflexType inflexType)
	{
		if (!_Hash.TryGetValue(inflexType, out var value))
		{
			SimpleInflexPropertyCollection simpleInflexPropertyCollection = new SimpleInflexPropertyCollection(inflexType);
			foreach (InflexProperty property in GetProperties(inflexType))
			{
				simpleInflexPropertyCollection.Add(property);
			}
			value = new InflexTypeDescription(inflexType, simpleInflexPropertyCollection);
			_Hash.Add(inflexType, value);
		}
		int num = 0;
		List<IMetaDataManagerPlugin> list = new List<IMetaDataManagerPlugin>(base.AppManagerComponents.OfType<IMetaDataManagerPlugin>());
		while (true)
		{
			List<IMetaDataManagerPlugin> list2 = new List<IMetaDataManagerPlugin>();
			foreach (IMetaDataManagerPlugin item in list)
			{
				try
				{
					item.FillMetaDataIfNeed(num, value);
				}
				catch (FailIterationException)
				{
					list2.Add(item);
				}
			}
			if (list2.Count == 0 || list2.Count == list.Count)
			{
				break;
			}
			list = list2;
			num++;
		}
		return value;
	}

	private IEnumerable<IInflexProperty> GetProperties(IInflexType inflexType)
	{
		List<IInflexProperty> list = new List<IInflexProperty>();
		if (inflexType.Parent != null)
		{
			list.AddRange(GetInflexTypeDescription(inflexType.Parent).Properties);
		}
		InflexPropertyAccessor inflexPropertyAccessor = base.AppManager.Accessor<InflexPropertyAccessor>();
		IList<InflexProperty> list2 = inflexPropertyAccessor.SelectProperties(base.AppManager.MainSchema, inflexType.TableName);
		IList<string> tableChecks = inflexPropertyAccessor.GetTableChecks(base.AppManager.MainSchema, inflexType.TableName);
		foreach (InflexProperty item in list2)
		{
			Regex regex = new Regex(item.Name + "\\s+in\\s+\\(\\s*'Y'\\s*,\\s*'N'\\s*\\)", RegexOptions.IgnoreCase);
			Regex regex2 = new Regex(item.Name + "\\s+in\\s+\\(\\s*'N'\\s*,\\s*'Y'\\s*\\)", RegexOptions.IgnoreCase);
			foreach (string item2 in tableChecks)
			{
				if (regex.IsMatch(item2))
				{
					item.IsBoolean = true;
					break;
				}
				if (regex2.IsMatch(item2))
				{
					item.IsBoolean = true;
					break;
				}
			}
			item.AppManager = base.AppManager;
			item.OwnerType = inflexType;
			list.Add(item);
		}
		if (inflexType.Parent == null)
		{
			foreach (InflexProperty item3 in list2)
			{
				if (item3.Name.ToUpper() == "ID")
				{
					item3.IsID = true;
					break;
				}
			}
		}
		IList<InflexPropertyLink> list3 = base.AppManager.Accessor<InflexPropertyLinkAccessor>().SelectForTable(base.AppManager.MainSchema, inflexType.TableName);
		foreach (InflexProperty item4 in list2)
		{
			foreach (InflexPropertyLink item5 in list3)
			{
				if (item4.Name == item5.ColumnName)
				{
					item4.PropertyTypeID = item5.TypeID;
					item4.PropertyTypeSubID = item5.SubtypeID;
					item4.PropertyTypeDictID = item5.DictionaryID;
				}
			}
		}
		return list;
	}
}
