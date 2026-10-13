using System.Data;
using System.Text;
using BLToolkit.Data.DataProvider;
using InflexCore.log;

namespace InflexCore.DataImplementation.DataProvider;

public class InflexOdpDataProvider : OdpDataProvider
{
	private readonly bool _ShowSQL;

	public InflexOdpDataProvider(bool showSql)
	{
		_ShowSQL = showSql;
		MappingSchema = new InflexOdpMappingSchema();
	}

	public override void PrepareCommand(ref CommandType commandType, ref string commandText, ref IDbDataParameter[] commandParameters)
	{
		base.PrepareCommand(ref commandType, ref commandText, ref commandParameters);
		if (!_ShowSQL)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(commandText);
		if (commandParameters != null)
		{
			IDbDataParameter[] array = commandParameters;
			foreach (IDbDataParameter dbDataParameter in array)
			{
				stringBuilder.AppendFormat("{0} = {1}", dbDataParameter.ParameterName, dbDataParameter.Value).AppendLine();
			}
		}
		FastLog.Loger(this).Info((object)stringBuilder.ToString());
	}
}
