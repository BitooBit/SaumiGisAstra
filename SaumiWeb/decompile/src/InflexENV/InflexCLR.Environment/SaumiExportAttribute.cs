using System;

namespace InflexCLR.Environment;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class SaumiExportAttribute : Attribute
{
	private int _Position = -1;

	public int Position
	{
		get
		{
			return _Position;
		}
		set
		{
			_Position = value;
		}
	}
}
