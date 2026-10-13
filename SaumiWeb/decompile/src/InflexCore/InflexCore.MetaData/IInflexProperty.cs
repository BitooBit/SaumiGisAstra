namespace InflexCore.MetaData;

public interface IInflexProperty : IInflexMetaData
{
	string Type { get; }

	string Name { get; }

	bool IsNullable { get; }

	int Length { get; }

	int Precision { get; }

	bool IsHistory { get; }

	IInflexType OwnerType { get; }

	IInflexType PropertyType { get; }

	bool IsBoolean { get; }

	bool IsString { get; }

	bool IsNumber { get; }

	bool IsTime { get; }

	bool IsID { get; }

	void SetValue(IInflexObject inflexObject, object value);

	object GetValue(IInflexObject inflexObject);

	IInflexObject GetInflexValue(IInflexObject inflexObject);

	bool? GetInflexBoolean(IInflexObject inflexObject);

	IInflexID GetInflexID(IInflexObject inflexObject);

	void ValidateSetValue(IInflexObject inflexObject, object value);
}
