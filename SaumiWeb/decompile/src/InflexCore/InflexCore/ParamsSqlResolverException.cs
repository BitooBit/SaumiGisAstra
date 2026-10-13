using System;

namespace InflexCore;

public class ParamsSqlResolverException : Exception
{
	public ParamsSqlResolverException()
		: base("Ошибка  обработки параметрического запроса")
	{
	}
}
