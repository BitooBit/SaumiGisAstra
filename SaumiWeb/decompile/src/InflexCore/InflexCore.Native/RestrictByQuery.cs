using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace InflexCore.Native;

public class RestrictByQuery : QueryRestriction
{
	private const int LIMIT_COUNT_IN_GROUP = 1000;

	private string _RestrictionXMLText;

	private IInflexColumn _RestrictionColumn;

	private XmlDocument _RestrictionXML;

	private List<XmlDocument> _AdditionalRestrictionXmls;

	public string ManualFrom { get; set; }

	public string ManualWhere { get; set; }

	public IEnumerable<IInflexID> RestrictionIDs { get; set; }

	public bool IsFullQuery
	{
		get
		{
			if (string.IsNullOrEmpty(RestrictionQueryText))
			{
				return string.IsNullOrEmpty(ManualWhere);
			}
			return false;
		}
	}

	public IInflexColumn RestrictionColumn
	{
		get
		{
			if (_RestrictionColumn == null)
			{
				_RestrictionColumn = base.Report.GetPrimaryColumn();
			}
			return _RestrictionColumn;
		}
	}

	public XmlDocument RestrictionXML
	{
		get
		{
			if (_RestrictionXML == null)
			{
				_RestrictionXML = new XmlDocument();
			}
			return _RestrictionXML;
		}
		private set
		{
			_RestrictionXML = value;
		}
	}

	public List<XmlDocument> AdditionalRestrictionXmls
	{
		get
		{
			if (_AdditionalRestrictionXmls == null)
			{
				_AdditionalRestrictionXmls = new List<XmlDocument>();
			}
			return _AdditionalRestrictionXmls;
		}
	}

	public string RestrictionXMLText
	{
		get
		{
			return _RestrictionXMLText;
		}
		private set
		{
			_RestrictionXMLText = value;
			RestrictionXML = new XmlDocument();
			if (!string.IsNullOrEmpty(_RestrictionXMLText))
			{
				try
				{
					RestrictionXML.LoadXml(_RestrictionXMLText);
				}
				catch
				{
					RestrictionXML = null;
				}
			}
		}
	}

	public string[] RestrictionQuery { get; private set; }

	public string RestrictionQueryText
	{
		get
		{
			if (RestrictionQuery == null)
			{
				return null;
			}
			if (RestrictionQuery == null)
			{
				RestrictionQuery = new string[0];
			}
			if (RestrictionQuery.Length <= 0)
			{
				return string.Empty;
			}
			return RestrictionQuery.Aggregate((string s1, string s2) => s1 + Environment.NewLine + s2);
		}
		private set
		{
			if (string.IsNullOrEmpty(value))
			{
				RestrictionQuery = new string[0];
				return;
			}
			RestrictionQuery = value.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		}
	}

	public static RestrictByQuery GetFullQuery(IInflexReport report)
	{
		return new RestrictByQuery(report, string.Empty);
	}

	public RestrictByQuery(IInflexReport report, string restrictionQueryText)
		: base(report)
	{
		RestrictionQueryText = restrictionQueryText;
	}

	public RestrictByQuery(IInflexReport report, string[] restrictionQuery)
		: base(report)
	{
		RestrictionQuery = restrictionQuery;
	}

	public RestrictByQuery(IInflexReport report, string[] restrictionQuery, string restrictionXml)
		: this(report, restrictionQuery)
	{
		RestrictionXMLText = restrictionXml;
	}

	public RestrictByQuery(IInflexReport report, string restrictionQuery, string restrictionXml)
		: this(report, restrictionQuery)
	{
		RestrictionXMLText = restrictionXml;
	}

	public RestrictByQuery(IInflexReport report, IInflexColumn restrictionColumn, string restrictionQuery, string restrictionXml)
		: this(report, restrictionQuery, restrictionXml)
	{
		_RestrictionColumn = restrictionColumn;
	}

	public RestrictByQuery(IInflexReport report, IInflexColumn restrictionColumn, string[] restrictionQuery, string restrictionXml)
		: this(report, restrictionQuery, restrictionXml)
	{
		_RestrictionColumn = restrictionColumn;
	}

	public RestrictByQuery AddRestrictionQuery(IInflexReport report, string queryText)
	{
		return AddRestrictionQuery(report, queryText.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries), null);
	}

	public RestrictByQuery AddRestrictionQuery(IInflexReport report, string[] query, XmlDocument restrictionXml)
	{
		IInflexReport inflexReport = null;
		if (base.Report == report || base.Report == report.RootReport)
		{
			inflexReport = base.Report;
		}
		else if (base.Report.RootReport == report)
		{
			inflexReport = report;
		}
		else if (base.Report.RootReport == report.RootReport)
		{
			inflexReport = report.RootReport;
		}
		if (inflexReport == null)
		{
			return null;
		}
		RestrictByQuery restrictByQuery = new RestrictByQuery(inflexReport, RestrictionQueryText, RestrictionXMLText);
		restrictByQuery.RestrictionIDs = RestrictionIDs;
		RestrictByQuery restrictByQuery2 = restrictByQuery;
		if (!string.IsNullOrEmpty(RestrictionXML.InnerXml))
		{
			restrictByQuery2.AdditionalRestrictionXmls.Add(RestrictionXML);
		}
		if (AdditionalRestrictionXmls.Count > 0)
		{
			restrictByQuery2.AdditionalRestrictionXmls.AddRange(AdditionalRestrictionXmls);
		}
		if (restrictionXml != null)
		{
			restrictByQuery2.AdditionalRestrictionXmls.Add(restrictionXml);
		}
		List<string> list = new List<string>();
		try
		{
			if (query == null || query.Length == 0)
			{
				return null;
			}
			if (restrictByQuery2.RestrictionQuery == null || restrictByQuery2.RestrictionQuery.Length == 0)
			{
				list.AddRange(query);
				return restrictByQuery2;
			}
			try
			{
				bool flag = restrictByQuery2.IsSelect(restrictByQuery2.RestrictionQuery);
				bool flag2 = restrictByQuery2.IsSelect(query);
				if ((flag && flag2) || (!flag && !flag2))
				{
					list.AddRange(restrictByQuery2.RestrictionQuery);
					list.Add((flag && flag2) ? "union all" : ",");
					list.AddRange(query);
					return restrictByQuery2;
				}
				if (!flag)
				{
					string item = CreateSqlQueryFromIdValues(restrictByQuery2.RestrictionQuery, restrictByQuery2.Report);
					list.Add(item);
					list.Add(" union all ");
					list.AddRange(query);
					return restrictByQuery2;
				}
				string item2 = CreateSqlQueryFromIdValues(query, restrictByQuery2.Report);
				list.AddRange(restrictByQuery2.RestrictionQuery);
				list.Add(" union all ");
				list.Add(item2);
				return restrictByQuery2;
			}
			catch (NotSupportedException)
			{
			}
		}
		finally
		{
			if (list.Count > 0)
			{
				restrictByQuery2.RestrictionQuery = list.ToArray();
				restrictByQuery2.RestrictionXMLText = null;
			}
		}
		return null;
	}

	private string CreateSqlQueryFromIdValues(string[] restrictionStrings, IInflexReport report)
	{
		IInflexReport inflexReport = report.RootReport ?? report;
		string tableName = inflexReport.AssignedType.TableName;
		string tableOwner = inflexReport.AssignedType.TableOwner;
		string text = string.Format("select {1}.Id from {0}.{1} where {1}.Id in", tableOwner, tableName);
		StringBuilder stringBuilder = new StringBuilder();
		List<string> list = SplitRestrictValuesByCount(restrictionStrings, 1000);
		for (int i = 0; i < list.Count; i++)
		{
			string text2 = list[i];
			stringBuilder.Append(text + text2);
			if (i < list.Count - 1)
			{
				stringBuilder.Append(" union all ");
			}
		}
		return stringBuilder.ToString();
	}

	private List<string> SplitRestrictValuesByCount(string[] restrinctionStrings, int count)
	{
		int num = 0;
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder("(");
		for (int i = 0; i < restrinctionStrings.Length; i++)
		{
			string value = restrinctionStrings[i];
			stringBuilder.Append(value);
			num++;
			if (num < count && restrinctionStrings.Length - 1 > i)
			{
				stringBuilder.Append(", ");
				continue;
			}
			stringBuilder.Append(")");
			list.Add(stringBuilder.ToString());
			if (restrinctionStrings.Length - 1 > i)
			{
				stringBuilder = new StringBuilder("(");
				num = 0;
			}
		}
		return list;
	}

	private bool IsSelect(string[] query)
	{
		if (query == null || query.Length == 0)
		{
			throw new NotSupportedException();
		}
		query = query.Where((string s) => !string.IsNullOrEmpty(s)).ToArray();
		if (query == null || query.Length == 0)
		{
			throw new NotSupportedException();
		}
		return query[0].ToLower().Contains("select");
	}
}
