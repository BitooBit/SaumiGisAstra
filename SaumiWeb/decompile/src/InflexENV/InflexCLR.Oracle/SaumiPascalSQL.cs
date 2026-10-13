using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using System.Text.RegularExpressions;

namespace InflexCLR.Oracle;

public class SaumiPascalSQL
{
	private OracleConnection _conn;

	private OracleCommand _command;

	private OracleDataReader _reader;

	private bool _EOF = true;

	private void CheckConnection()
	{
		if (_conn != null && ((DbConnection)(object)_conn).State != ConnectionState.Closed)
		{
			return;
		}
		throw new Exception("Соединение с базой данных не установлено");
	}

	private void CheckCommand()
	{
		if (_command == null || _reader == null)
		{
			throw new Exception("Запрос к базе данных не выполнен");
		}
	}

	private void ClosePrevCommand()
	{
		if (_reader != null)
		{
			((DbDataReader)(object)_reader).Close();
			_reader = null;
		}
		if (_command != null)
		{
			((Component)(object)_command).Dispose();
			_command = null;
		}
	}

	private int GetIndex(string FieldName)
	{
		CheckConnection();
		CheckCommand();
		for (int i = 0; i < ((DbDataReader)(object)_reader).FieldCount; i++)
		{
			if (FieldName.ToLower() == ((DbDataReader)(object)_reader).GetName(i).ToLower())
			{
				return i;
			}
		}
		return -1;
	}

	public SaumiPascalSQL(OracleConnection conn)
	{
		_conn = conn;
	}

	public void SQL_Open(string QueryText)
	{
		CheckConnection();
		ClosePrevCommand();
		try
		{
			_command = _conn.CreateCommand();
			((DbCommand)(object)_command).CommandText = QueryText;
			_reader = _command.ExecuteReader();
			_EOF = !((DbDataReader)(object)_reader).Read();
		}
		catch (Exception innerException)
		{
			throw new Exception("Ошибка выполнения запроса", innerException);
		}
	}

	public void SQL_BindOpen(string QueryText, params object[] vars)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		CheckConnection();
		ClosePrevCommand();
		try
		{
			List<string> list = new List<string>();
			Regex regex = new Regex(":\\w+");
			foreach (Match item in regex.Matches(QueryText))
			{
				list.Add(item.Value);
			}
			if (list.Count != vars.Length)
			{
				throw new Exception("Количество переменных не совпадает с количеством параметров.");
			}
			_command = _conn.CreateCommand();
			((DbCommand)(object)_command).CommandText = QueryText;
			if (vars != null)
			{
				int num = 0;
				foreach (object obj in vars)
				{
					OracleParameter val = new OracleParameter(list[num], obj);
					_command.Parameters.Add(val);
					num++;
				}
			}
			_reader = _command.ExecuteReader();
			_EOF = !((DbDataReader)(object)_reader).Read();
		}
		catch (Exception ex)
		{
			throw new Exception("Ошибка выполнения запроса" + System.Environment.NewLine + ex.Message, ex);
		}
	}

	public bool SQL_EOF()
	{
		CheckConnection();
		CheckCommand();
		return _EOF;
	}

	public void SQL_Next()
	{
		CheckConnection();
		CheckCommand();
		_EOF = !((DbDataReader)(object)_reader).Read();
	}

	public void SQL_Close()
	{
		CheckConnection();
		CheckCommand();
		((DbDataReader)(object)_reader).Close();
		_reader = null;
		((DbCommand)(object)_command).Cancel();
		_command = null;
		_EOF = true;
	}

	public string SQL_FieldAsString(string FieldName)
	{
		CheckConnection();
		CheckCommand();
		int index = GetIndex(FieldName);
		if (index >= 0)
		{
			if (((DbDataReader)(object)_reader).IsDBNull(index))
			{
				return "";
			}
			return ((DbDataReader)(object)_reader).GetValue(index).ToString();
		}
		throw new Exception("Поле не найдено");
	}

	public int SQL_FieldAsInteger(string FieldName)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		CheckConnection();
		CheckCommand();
		int index = GetIndex(FieldName);
		if (index >= 0)
		{
			if (((DbDataReader)(object)_reader).IsDBNull(index))
			{
				return 0;
			}
			object value = ((DbDataReader)(object)_reader).GetValue(index);
			if (value is OracleNumber val)
			{
				return decimal.ToInt32(((OracleNumber)(ref val)).Value);
			}
			if (value is decimal)
			{
				return decimal.ToInt32((decimal)value);
			}
			return (int)value;
		}
		throw new Exception("Поле не найдено");
	}

	public float SQL_FieldAsFloat(string FieldName)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		CheckConnection();
		CheckCommand();
		int index = GetIndex(FieldName);
		if (index >= 0)
		{
			if (((DbDataReader)(object)_reader).IsDBNull(index))
			{
				return 0f;
			}
			object value = ((DbDataReader)(object)_reader).GetValue(index);
			if (value is OracleNumber val)
			{
				return decimal.ToSingle(((OracleNumber)(ref val)).Value);
			}
			if (value is decimal)
			{
				return decimal.ToSingle((decimal)value);
			}
			return (float)value;
		}
		throw new Exception("Поле не найдено");
	}

	public object SQL_FieldAsObject(string FieldName)
	{
		CheckConnection();
		CheckCommand();
		int index = GetIndex(FieldName);
		if (index >= 0)
		{
			if (((DbDataReader)(object)_reader).IsDBNull(index))
			{
				return null;
			}
			return ((DbDataReader)(object)_reader).GetValue(index);
		}
		throw new Exception("Поле не найдено");
	}
}
