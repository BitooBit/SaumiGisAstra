using InflexCore.MetaData;

namespace InflexCore.DataImplementation.TransactionComponent.MetaData;

public class DbFieldMetaData : AbstractInflexMetaData
{
	public IInflexProperty Property { get; set; }

	public DbTableMetaData Table { get; set; }

	public bool IsPrimary { get; set; }

	public bool IsGeneralize { get; set; }

	public string DefaultValue { get; set; }

	public bool IsChildIdent { get; set; }

	public DbFieldMetaData()
	{
		IsPrimary = false;
		IsGeneralize = false;
		IsChildIdent = false;
	}

	public bool IsSimpleDbType()
	{
		IInflexProperty property = Property;
		if (!property.IsNumber && !property.IsString && !property.IsBoolean)
		{
			return property.IsTime;
		}
		return true;
	}
}
