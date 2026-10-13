using System.Data;

namespace InflexCore;

public class NativeSQLParam
{
	public string Key { get; set; }

	public DbType? Type { get; set; }

	public object Value { get; set; }

	public NativeSQLParam(string key, object value)
		: this(key, null, value)
	{
	}

	public NativeSQLParam(string key, DbType? type, object value)
	{
		Key = key;
		Type = type;
		Value = value;
	}
}
