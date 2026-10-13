using System;
using System.Runtime.Serialization;

namespace InflexCore;

[Serializable]
public class PrimaryColumnNotFoundException : Exception
{
	private const string EXCEPTION_MESSAGE_PATTERN = "{0} hasn't any primary columns";

	public PrimaryColumnNotFoundException()
	{
	}

	public PrimaryColumnNotFoundException(string message)
		: base(message)
	{
	}

	public PrimaryColumnNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected PrimaryColumnNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public PrimaryColumnNotFoundException(IInflexReport report)
		: base($"{report} hasn't any primary columns")
	{
	}

	public PrimaryColumnNotFoundException(IInflexReport report, Exception innerException)
		: base($"{report} hasn't any primary columns", innerException)
	{
	}
}
