namespace InflexCore;

public class ParamSqlValue
{
	private string _Value = string.Empty;

	private bool _IsSetValue;

	public string Name { get; set; }

	public string Value
	{
		get
		{
			return _Value;
		}
		set
		{
			_Value = value;
			_IsSetValue = true;
		}
	}

	public bool IsResolved()
	{
		return _IsSetValue;
	}
}
