using InflexCore.MetaData;

namespace InflexCore;

public interface IInflexObject
{
	IInflexID ID { get; }

	InflexMetaDataCollection RuntimeAttributes { get; }

	InflexPeriod CurrentLifePeriod { get; set; }

	InflexPeriodCollection LifePeriods { get; }

	void ValidateSetPropertyValue(IInflexProperty property, object value);

	void DirectSetPropertyValue(IInflexProperty property, object value);

	object DirectGetPropertyValue(IInflexProperty property);

	IInflexObject DirectGetInflexValue(IInflexProperty property);

	IInflexID DirectGetInflexID(IInflexProperty property);

	bool? DirectGetBoolean(IInflexProperty property);
}
