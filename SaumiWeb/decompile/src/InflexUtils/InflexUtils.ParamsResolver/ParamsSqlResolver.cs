using System.Collections.Generic;
using System.Windows.Forms;
using InflexCore;

namespace InflexUtils.ParamsResolver;

public class ParamsSqlResolver
{
	private ParamsSqlHandler _ParamsSqlHandler;

	public IEnumerable<ParamSqlValue> Params => _ParamsSqlHandler.Params;

	public ParamsSqlResolver(string sql, params ParamSqlValue[] values)
	{
		_ParamsSqlHandler = new ParamsSqlHandler(sql, values);
	}

	public bool IsResolved()
	{
		return _ParamsSqlHandler.IsResolved();
	}

	public bool ShowResolveDialog()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		ParamsSqlResolverForm paramsSqlResolverForm = new ParamsSqlResolverForm(_ParamsSqlHandler.Variables);
		return (int)((Form)paramsSqlResolverForm).ShowDialog() == 1;
	}

	public string GetResolvedSql()
	{
		return _ParamsSqlHandler.GetResolvedSql();
	}

	public string[] GetDescriptionResolvedVariables()
	{
		return _ParamsSqlHandler.GetDescriptionResolvedVariables();
	}
}
