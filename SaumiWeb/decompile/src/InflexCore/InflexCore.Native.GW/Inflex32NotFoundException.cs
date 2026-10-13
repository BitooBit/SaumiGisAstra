using System;

namespace InflexCore.Native.GW;

public class Inflex32NotFoundException : Exception
{
	public Inflex32NotFoundException()
	{
	}

	public Inflex32NotFoundException(Exception innerException)
		: this("", innerException)
	{
	}

	protected Inflex32NotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
