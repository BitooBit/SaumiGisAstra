using System.Collections;
using System.Collections.Generic;
using InflexCore;

namespace InflexData;

public interface IInflexDataQuery : IEnumerable<InflexQueryResultRow>, IEnumerable
{
	IInflexDataQuery SubQuery { get; }

	IEnumerable<InflexDataColumn> Columns { get; }

	IInflexType InflexType { get; }

	string HumanyAdaptedRestrictionTextShort { get; }

	string HumanyAdaptedRestrictionTextFull { get; }

	IInflexDataQuery MapInflexIDs(IInflexID[] inflexIDs);
}
