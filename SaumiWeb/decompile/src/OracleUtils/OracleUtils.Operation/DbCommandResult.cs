using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OracleUtils.Operation;

public class DbCommandResult
{
	private readonly CommandType _CommandType;

	private readonly string _CommandText;

	public CommandType CommandType => _CommandType;

	public string CommandText => _CommandText;

	public DateTime? ExecuteTime { get; private set; }

	public int? ExecuteResult { get; private set; }

	public Exception ExecuteException { get; private set; }

	public DbCommandResult(CommandType commandType, string commandText)
	{
		_CommandType = commandType;
		_CommandText = commandText;
	}

	public void SetSuccessResult(int executeResult)
	{
		SetSuccessResult(executeResult, DateTime.Now);
	}

	public void SetSuccessResult(int executeResult, DateTime executeTime)
	{
		ExecuteResult = executeResult;
		ExecuteTime = executeTime;
	}

	public void SetFailureResult(Exception executeException)
	{
		SetFailureResult(executeException, DateTime.Now);
	}

	public void SetFailureResult(Exception executeException, DateTime executeTime)
	{
		ExecuteException = executeException;
		ExecuteTime = executeTime;
	}

	public override string ToString()
	{
		return Regex.Replace(CommandText, "  ", " ") + " | " + (ExecuteResult.HasValue ? ExecuteResult.Value.ToString(CultureInfo.InvariantCulture) : ((ExecuteException != null) ? ExecuteException.Message : "UNDEFINED")) + " | " + (ExecuteTime.HasValue ? ExecuteTime.Value.ToString(CultureInfo.InvariantCulture) : null);
	}
}
