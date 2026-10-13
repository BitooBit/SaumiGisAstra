using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace InflexCore;

public class ParamsSqlHandler
{
	public class SqlVariable
	{
		public string Name { get; set; }

		public int Index { get; set; }

		public int Length { get; set; }

		public bool IsDate { get; set; }

		public bool IsInteger { get; set; }

		public bool IsNumber { get; set; }

		public bool IsString { get; set; }

		public bool IsList { get; set; }

		public string Caption { get; set; }

		public string DefaultValue { get; set; }

		public List<ListValue> ListValues { get; private set; }

		public ParamSqlValue Value { get; set; }

		public SqlVariable()
		{
			IsDate = false;
			IsInteger = false;
			IsList = false;
			IsNumber = false;
			IsString = false;
			ListValues = new List<ListValue>();
		}

		public string ValueToString()
		{
			if (!IsNumber)
			{
				return Value.Value;
			}
			return Value.Value.Replace(',', '.');
		}
	}

	public class ListValue
	{
		public string Caption { get; set; }

		public string Value { get; set; }

		public ListValue(string caption, string value)
		{
			Caption = caption;
			Value = value;
		}

		public override string ToString()
		{
			return Caption;
		}
	}

	private readonly string _SQL;

	private int _SystemNameIndex;

	private string _SystemName = "\\{8718BD71-1D20-42B7-9E63-F474355F6DB4\\}";

	private List<SqlVariable> _Variables;

	private Regex _RxFoundParams = new Regex("[^\\\\]\\{(.*?[^\\\\])\\}");

	private Regex _RxOnlyName = new Regex("^\\w+$");

	private Regex _RxFullParams = new Regex("(\\w+:)?(DATE)?(INTEGER)?(NUMBER)?(STRING)?(LIST)?\\s*=\\s*(\\(([^\\)]*)\\))?\\s*(.+)", RegexOptions.IgnoreCase);

	private Regex _RxAllListElements = new Regex("\\((.+?)\\)");

	private Regex _RxFoundElements = new Regex("[\\|\\(]?([^\\|]+)\\|([^\\|]+)[\\|]?");

	public IEnumerable<SqlVariable> Variables => _Variables;

	public IEnumerable<ParamSqlValue> Params { get; private set; }

	public ParamsSqlHandler(string sql, params ParamSqlValue[] values)
	{
		_SQL = sql;
		FoundVariables(values ?? new ParamSqlValue[0]);
	}

	private void FoundVariables(ParamSqlValue[] sqlValues)
	{
		List<SqlVariable> list = new List<SqlVariable>();
		foreach (Match item in _RxFoundParams.Matches(_SQL))
		{
			string value = item.Groups[1].Value;
			SqlVariable sqlVariable = new SqlVariable();
			sqlVariable.Index = item.Groups[1].Index - 1;
			sqlVariable.Length = item.Groups[1].Length + 2;
			SqlVariable sqlVariable2 = sqlVariable;
			if (_RxOnlyName.IsMatch(value))
			{
				sqlVariable2.Name = value;
				list.Add(sqlVariable2);
				continue;
			}
			Match match2 = _RxFullParams.Match(value);
			if (match2.Success)
			{
				int num = 0;
				if (!string.IsNullOrEmpty(match2.Groups[2].Value))
				{
					sqlVariable2.IsDate = true;
					num++;
				}
				if (!string.IsNullOrEmpty(match2.Groups[3].Value))
				{
					sqlVariable2.IsInteger = true;
					num++;
				}
				if (!string.IsNullOrEmpty(match2.Groups[4].Value))
				{
					sqlVariable2.IsNumber = true;
					num++;
				}
				if (!string.IsNullOrEmpty(match2.Groups[5].Value))
				{
					sqlVariable2.IsString = true;
					num++;
				}
				if (!string.IsNullOrEmpty(match2.Groups[6].Value))
				{
					sqlVariable2.IsList = true;
					num++;
				}
				if (num != 1)
				{
					throw new ParamsSqlResolverException();
				}
				string value2 = match2.Groups[1].Value;
				sqlVariable2.Name = (string.IsNullOrEmpty(value2) ? NextSystemName() : value2.Substring(0, value2.Length - 1));
				sqlVariable2.DefaultValue = match2.Groups[8].Value;
				string text = match2.Groups[9].Value.Trim();
				if (sqlVariable2.IsList)
				{
					Match match3 = _RxAllListElements.Match(text);
					if (!match3.Success)
					{
						throw new ParamsSqlResolverException();
					}
					bool flag = false;
					foreach (Match item2 in _RxFoundElements.Matches(match3.Groups[1].Value))
					{
						flag = true;
						sqlVariable2.ListValues.Add(new ListValue(item2.Groups[1].Value, item2.Groups[2].Value));
					}
					if (!flag)
					{
						throw new ParamsSqlResolverException();
					}
					sqlVariable2.Caption = text.Substring(0, match3.Groups[0].Index).Trim();
				}
				else
				{
					sqlVariable2.Caption = text;
				}
				list.Add(sqlVariable2);
				continue;
			}
			throw new ParamsSqlResolverException();
		}
		Dictionary<string, ParamSqlValue> dictionary = new Dictionary<string, ParamSqlValue>();
		foreach (ParamSqlValue paramSqlValue in sqlValues)
		{
			dictionary.Add(paramSqlValue.Name, paramSqlValue);
		}
		foreach (SqlVariable item3 in list)
		{
			if (!dictionary.TryGetValue(item3.Name, out var value3))
			{
				ParamSqlValue paramSqlValue2 = new ParamSqlValue();
				paramSqlValue2.Name = item3.Name;
				value3 = paramSqlValue2;
				dictionary.Add(value3.Name, value3);
			}
			item3.Value = value3;
		}
		_Variables = list;
		Params = dictionary.Select((KeyValuePair<string, ParamSqlValue> kv) => kv.Value).ToArray();
	}

	private string NextSystemName()
	{
		_SystemNameIndex++;
		return _SystemName + _SystemNameIndex;
	}

	public bool IsResolved()
	{
		foreach (ParamSqlValue item in Params)
		{
			if (!item.IsResolved())
			{
				return false;
			}
		}
		return true;
	}

	public string GetResolvedSql()
	{
		string text = _SQL;
		for (int num = _Variables.Count - 1; num >= 0; num--)
		{
			SqlVariable sqlVariable = _Variables[num];
			text = text.Remove(sqlVariable.Index, sqlVariable.Length).Insert(sqlVariable.Index, sqlVariable.ValueToString());
		}
		return text;
	}

	public string[] GetDescriptionResolvedVariables()
	{
		List<string> list = new List<string>();
		HashSet<string> hashSet = new HashSet<string>();
		foreach (SqlVariable variable2 in _Variables)
		{
			if (hashSet.Contains(variable2.Value.Name))
			{
				continue;
			}
			SqlVariable variable = variable2;
			if (string.IsNullOrEmpty(variable2.Caption))
			{
				foreach (SqlVariable variable3 in _Variables)
				{
					if (variable3.Value == variable2.Value && !string.IsNullOrEmpty(variable3.Caption))
					{
						variable = variable3;
					}
				}
			}
			list.Add(GetDescriptionVariableValue(variable));
			hashSet.Add(variable2.Value.Name);
		}
		return list.ToArray();
	}

	private string GetDescriptionVariableValue(SqlVariable variable)
	{
		string text = (variable.Name.StartsWith(_SystemName) ? "" : (variable.Name + " "));
		text += (string.IsNullOrEmpty(variable.Caption) ? "" : ("(" + variable.Caption + ")"));
		return text + " = " + variable.ValueToString();
	}
}
