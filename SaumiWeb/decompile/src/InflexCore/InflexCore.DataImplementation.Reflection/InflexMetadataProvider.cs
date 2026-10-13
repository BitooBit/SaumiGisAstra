using System;
using BLToolkit.Reflection.Extension;
using BLToolkit.Reflection.MetadataProvider;

namespace InflexCore.DataImplementation.Reflection;

public class InflexMetadataProvider : MetadataProviderBase
{
	public override string GetDatabaseName(Type type, ExtensionList extensions, out bool isSet)
	{
		object[] customAttributes = type.GetCustomAttributes(typeof(InflexTableNameAttribute), inherit: true);
		if (customAttributes.Length > 0)
		{
			string text = null;
			text = ((InflexTableNameAttribute)customAttributes[0]).OwnerType switch
			{
				InflexTableOwners.CurrentUser => AppManager.CurrentUser, 
				InflexTableOwners.InflexSchema => AppManager.InflexSchema, 
				_ => ((InflexTableNameAttribute)customAttributes[0]).OwnerName, 
			};
			isSet = !string.IsNullOrEmpty(text);
			return text;
		}
		return base.GetDatabaseName(type, extensions, out isSet);
	}
}
