using System.Collections;
using System.Data.Common;
using System.Data.OracleClient;

namespace InflexCLR.Oracle;

public class RowEnumerator : IEnumerator
{
	private IEnumerator _Enumerator;

	public object Current
	{
		get
		{
			DbDataRecord record = _Enumerator.Current as DbDataRecord;
			return new Row(record);
		}
	}

	public RowEnumerator(OracleDataReader reader)
	{
		_Enumerator = ((DbDataReader)(object)reader).GetEnumerator();
	}

	public bool MoveNext()
	{
		return _Enumerator.MoveNext();
	}

	public void Reset()
	{
		_Enumerator.Reset();
	}
}
