using System.Collections;
using System.Collections.Generic;
using InflexCore;

namespace InflexData;

public interface IInflexReportBasedDataQuery : IInflexDataQuery, IEnumerable<InflexQueryResultRow>, IEnumerable
{
	IInflexReport Report { get; }

	QueryColumnCollection QueryColumns { get; }
}
