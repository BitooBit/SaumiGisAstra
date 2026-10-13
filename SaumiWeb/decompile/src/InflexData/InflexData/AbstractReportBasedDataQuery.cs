using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InflexCore;
using InflexCore.Native;
using InflexData.RestrictionQuery;

namespace InflexData;

public abstract class AbstractReportBasedDataQuery : IInflexReportBasedDataQuery, IInflexDataQuery, IEnumerable<InflexQueryResultRow>, IEnumerable
{
	private const string RESTRICTION_QUERY_TEMPLATE = "select id from {MAIN_TABLE} where {MAIN_TABLE}.id in ({RESTRICTION})";

	public abstract IInflexDataQuery SubQuery { get; }

	public abstract IEnumerable<InflexDataColumn> Columns { get; }

	public IInflexType InflexType => Report.AssignedType;

	public abstract string HumanyAdaptedRestrictionTextShort { get; }

	public abstract string HumanyAdaptedRestrictionTextFull { get; }

	public abstract IInflexReport Report { get; }

	public abstract QueryColumnCollection QueryColumns { get; }

	protected abstract AppManager GetAppManager();

	protected abstract IQueryColumnCollectionProvider GetQueryColumnsProvider();

	public IInflexDataQuery MapInflexIDs(IInflexID[] inflexIDs)
	{
		if (inflexIDs == null)
		{
			return this;
		}
		IInflexType currentType = InflexType;
		IEnumerable<IInflexID> restrictionIDs = inflexIDs.Where((IInflexID inflexID) => inflexID.Type.IsTypecast(currentType));
		AppManager appManager = GetAppManager();
		string text = "select id from {MAIN_TABLE} where {MAIN_TABLE}.id in ({RESTRICTION})".Replace("{RESTRICTION}", TempRestrictionQueryAccessor.GetTempRestrictionQueryText(appManager));
		if (string.IsNullOrEmpty(text))
		{
			return this;
		}
		IQueryColumnCollectionProvider queryColumnsProvider = GetQueryColumnsProvider();
		RestrictByQuery restrictByQuery = new RestrictByQuery(Report, text);
		restrictByQuery.RestrictionIDs = restrictionIDs;
		return new RestrictionDataQuery(appManager, new RestrictionDataQueryRestriction(restrictByQuery), queryColumnsProvider);
	}

	public abstract IEnumerator<InflexQueryResultRow> GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
