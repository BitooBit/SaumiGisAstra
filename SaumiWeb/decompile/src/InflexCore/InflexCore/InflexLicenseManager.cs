using InflexCore.Native.GW;

namespace InflexCore;

public class InflexLicenseManager
{
	private readonly IInflexGw _InflexGw;

	public bool IsGisAllowed => _InflexGw.AllowLicense(128);

	public InflexLicenseManager(IInflexGw inflexGw)
	{
		_InflexGw = inflexGw;
	}
}
