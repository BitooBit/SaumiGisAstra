using System;

namespace InflexCore;

public class SubReportNotFoundException : InvalidOperationException
{
	public SubReportNotFoundException()
		: base("Sub report not found")
	{
	}
}
