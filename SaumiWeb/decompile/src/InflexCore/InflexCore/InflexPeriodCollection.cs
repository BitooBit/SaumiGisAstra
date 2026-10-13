using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InflexCore;

public class InflexPeriodCollection : IEnumerable<InflexPeriod>, IEnumerable
{
	private List<InflexPeriod> _Data = new List<InflexPeriod>();

	public void Add(InflexPeriod key)
	{
		_Data.Add(key);
		_Data = new List<InflexPeriod>(_Data.OrderBy((InflexPeriod kv) => kv));
	}

	public IEnumerator<InflexPeriod> GetEnumerator()
	{
		return _Data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
