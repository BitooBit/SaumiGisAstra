namespace InflexCLR.Gis;

public class Gis_Extent
{
	private double _XMin;

	private double _YMin;

	private double _XMax;

	private double _YMax;

	public double XMin
	{
		get
		{
			return _XMin;
		}
		set
		{
			_XMin = value;
		}
	}

	public double YMin
	{
		get
		{
			return _YMin;
		}
		set
		{
			_YMin = value;
		}
	}

	public double XMax
	{
		get
		{
			return _XMax;
		}
		set
		{
			_XMax = value;
		}
	}

	public double YMax
	{
		get
		{
			return _YMax;
		}
		set
		{
			_YMax = value;
		}
	}

	public Gis_Extent()
	{
	}

	public Gis_Extent(double xmin, double ymin, double xmax, double ymax)
	{
		XMin = xmin;
		YMin = ymin;
		XMax = xmax;
		YMax = ymax;
	}
}
