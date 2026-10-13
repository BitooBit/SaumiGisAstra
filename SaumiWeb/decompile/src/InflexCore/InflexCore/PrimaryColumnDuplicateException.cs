using System;
using System.Runtime.Serialization;

namespace InflexCore;

[Serializable]
public class PrimaryColumnDuplicateException : Exception
{
	private const string EXCEPTION_MESSAGE_PATTERN = "{0} has duplicate primary columns";

	public PrimaryColumnDuplicateException()
	{
	}

	public PrimaryColumnDuplicateException(string message)
		: base(message)
	{
	}

	public PrimaryColumnDuplicateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected PrimaryColumnDuplicateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public PrimaryColumnDuplicateException(IInflexReport report)
		: base($"{report} has duplicate primary columns")
	{
	}

	public PrimaryColumnDuplicateException(IInflexReport report, Exception innerException)
		: base($"{report} has duplicate primary columns", innerException)
	{
	}
}
