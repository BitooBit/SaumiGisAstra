namespace InflexData;

public interface IValueToDisplayStringConverter
{
	string ToString(IGridColumn gridColumn, object value, bool useUnits);

	string GetFormatString(IGridColumn gridColumn);
}
