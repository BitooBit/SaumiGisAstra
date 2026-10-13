using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using System.Reflection;
using InflexCLR.Oracle;

namespace InflexCLR.Environment;

public static class OracleProvider
{
	private static int _connectID = 0;

	private static Dictionary<int, SaumiPascalSQL> _SPConnections = new Dictionary<int, SaumiPascalSQL>();

	private static OracleConnection _connect;

	private static Dictionary<string, OracleConnection> _сonnections = new Dictionary<string, OracleConnection>();

	private static string _Password;

	public static bool ContainsConnectionByKeyName(string KeyName)
	{
		return _сonnections.ContainsKey(KeyName);
	}

	public static OracleConnection CloneConnection(string KeyName)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		if (string.IsNullOrEmpty(KeyName) || _сonnections.ContainsKey(KeyName))
		{
			throw new Exception("Соединение с таким именем уже открыто");
		}
		if (_connect == null)
		{
			throw new Exception("Основное соединение не установлено");
		}
		try
		{
			OracleConnectionStringBuilder val = new OracleConnectionStringBuilder();
			val.DataSource = Saumi4.Database;
			val.UserID = Saumi4.UserName;
			val.Password = GetPassword();
			OracleConnection val2 = new OracleConnection(((DbConnectionStringBuilder)(object)val).ConnectionString);
			((DbConnection)(object)val2).Open();
			_сonnections.Add(KeyName, val2);
			return val2;
		}
		catch (Exception innerException)
		{
			throw new Exception("Ошибка соединения", innerException);
		}
	}

	public static string StartConnection(string Username, string Database, string Password)
	{
		Saumi4.UserName = Username;
		Saumi4.Database = Database;
		string result = "";
		try
		{
			StartConnection(Password);
		}
		catch (Exception ex)
		{
			result = ex.Message;
			if (ex.InnerException != null)
			{
				result = ex.InnerException.Message;
			}
		}
		return result;
	}

	public static void SetPassword(string Password)
	{
		_Password = Password;
	}

	public static string GetPassword()
	{
		return _Password;
	}

	public static bool StartConnection(string Password)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		if (_connect != null)
		{
			if (((DbConnection)(object)_connect).State != ConnectionState.Closed)
			{
				((DbConnection)(object)_connect).Close();
			}
			_connect = null;
		}
		OracleConnectionStringBuilder val = new OracleConnectionStringBuilder();
		val.DataSource = Saumi4.Database;
		val.UserID = Saumi4.UserName;
		val.Password = Password;
		try
		{
			_connect = new OracleConnection(((DbConnectionStringBuilder)(object)val).ConnectionString);
			((DbConnection)(object)_connect).Open();
			SetPassword(Password);
			return ((DbConnection)(object)_connect).State == ConnectionState.Open;
		}
		catch (Exception innerException)
		{
			throw new Exception("Ошибка соединения", innerException);
		}
	}

	public static void SetConnection(string Password, int Handle)
	{
		IntPtr intPtr = (IntPtr)Handle;
		if (StartConnection(Password))
		{
			PropertyInfo property = ((object)_connect).GetType().GetProperty("ServiceContextHandle", BindingFlags.Instance | BindingFlags.NonPublic);
			object value = property.GetValue(_connect, null);
			FieldInfo field = value.GetType().GetField("handle", BindingFlags.Instance | BindingFlags.NonPublic);
			field.SetValue(value, intPtr);
			return;
		}
		throw new Exception("Соединение не установлено");
	}

	public static int GetConnectionHandle()
	{
		if (_connect == null || ((DbConnection)(object)_connect).State == ConnectionState.Closed)
		{
			return 0;
		}
		PropertyInfo property = ((object)_connect).GetType().GetProperty("ServiceContextHandle", BindingFlags.Instance | BindingFlags.NonPublic);
		object value = property.GetValue(_connect, null);
		FieldInfo field = value.GetType().GetField("handle", BindingFlags.Instance | BindingFlags.NonPublic);
		object value2 = field.GetValue(value);
		return ((IntPtr)value2).ToInt32();
	}

	public static void CloseConnection()
	{
		if (_connect != null && ((DbConnection)(object)_connect).State != ConnectionState.Closed)
		{
			((DbConnection)(object)_connect).Close();
		}
	}

	public static void CloseConnection(string KeyName)
	{
		if (string.IsNullOrEmpty(KeyName))
		{
			return;
		}
		OracleConnection value = null;
		if (_сonnections.TryGetValue(KeyName, out value))
		{
			if (value != null && ((DbConnection)(object)value).State != ConnectionState.Closed)
			{
				((DbConnection)(object)value).Close();
			}
			_сonnections.Remove(KeyName);
		}
	}

	public static OracleConnection GetConnection()
	{
		if (_connect == null)
		{
			throw new Exception("Соединение не установлено");
		}
		try
		{
			TryRepair(_connect);
		}
		catch (Exception innerException)
		{
			throw new Exception("Ошибка при открытии соединения", innerException);
		}
		return _connect;
	}

	public static OracleConnection GetConnection(string KeyName)
	{
		if (string.IsNullOrEmpty(KeyName) || !_сonnections.ContainsKey(KeyName))
		{
			throw new Exception("Соединение с таким именем не установлено");
		}
		OracleConnection val = _сonnections[KeyName];
		if (val == null)
		{
			throw new Exception("Соединение не установлено");
		}
		try
		{
			TryRepair(val);
			return val;
		}
		catch (Exception innerException)
		{
			throw new Exception("Ошибка при открытии соединения", innerException);
		}
	}

	private static void TryRepair(OracleConnection conn)
	{
		if (((DbConnection)(object)conn).State == ConnectionState.Broken)
		{
			((DbConnection)(object)conn).Close();
		}
		if (((DbConnection)(object)conn).State == ConnectionState.Closed || ((DbConnection)(object)conn).State == ConnectionState.Broken)
		{
			((DbConnection)(object)conn).Open();
		}
	}

	public static int SQL_Open(string QueryText)
	{
		SaumiPascalSQL saumiPascalSQL = new SaumiPascalSQL(GetConnection());
		_connectID++;
		_SPConnections.Add(_connectID, saumiPascalSQL);
		saumiPascalSQL.SQL_Open(QueryText);
		return _connectID;
	}

	public static int SQL_BindOpen(string QueryText, params object[] vars)
	{
		SaumiPascalSQL saumiPascalSQL = new SaumiPascalSQL(GetConnection());
		_connectID++;
		_SPConnections.Add(_connectID, saumiPascalSQL);
		saumiPascalSQL.SQL_BindOpen(QueryText, vars);
		return _connectID;
	}

	public static bool SQL_EOF(int SQL_ID)
	{
		SaumiPascalSQL value = null;
		if (_SPConnections.TryGetValue(SQL_ID, out value))
		{
			return value.SQL_EOF();
		}
		throw new Exception("Канал доступа к базе данных закрыт");
	}

	public static void SQL_Next(int SQL_ID)
	{
		SaumiPascalSQL value = null;
		if (_SPConnections.TryGetValue(SQL_ID, out value))
		{
			value.SQL_Next();
			return;
		}
		throw new Exception("Канал доступа к базе данных закрыт");
	}

	public static void SQL_Close(int SQL_ID)
	{
		SaumiPascalSQL value = null;
		if (_SPConnections.TryGetValue(SQL_ID, out value))
		{
			value.SQL_Close();
			_SPConnections.Remove(SQL_ID);
		}
	}

	public static string SQL_FieldAsString(int SQL_ID, string FieldName)
	{
		SaumiPascalSQL value = null;
		if (_SPConnections.TryGetValue(SQL_ID, out value))
		{
			return value.SQL_FieldAsString(FieldName);
		}
		throw new Exception("Канал доступа к базе данных закрыт");
	}

	public static int SQL_FieldAsInteger(int SQL_ID, string FieldName)
	{
		SaumiPascalSQL value = null;
		if (_SPConnections.TryGetValue(SQL_ID, out value))
		{
			return value.SQL_FieldAsInteger(FieldName);
		}
		throw new Exception("Канал доступа к базе данных закрыт");
	}

	public static float SQL_FieldAsFloat(int SQL_ID, string FieldName)
	{
		SaumiPascalSQL value = null;
		if (_SPConnections.TryGetValue(SQL_ID, out value))
		{
			return value.SQL_FieldAsFloat(FieldName);
		}
		throw new Exception("Канал доступа к базе данных закрыт");
	}

	public static object SQL_FieldAsObject(int SQL_ID, string FieldName)
	{
		SaumiPascalSQL value = null;
		if (_SPConnections.TryGetValue(SQL_ID, out value))
		{
			return value.SQL_FieldAsObject(FieldName);
		}
		throw new Exception("Канал доступа к базе данных закрыт");
	}
}
