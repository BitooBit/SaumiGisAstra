using System;

namespace InflexData;

public class InvalidOracleProcedureException : Exception
{
	public InvalidOracleProcedureException(string text)
		: base(text)
	{
	}
}
