using System;
using System.Collections;
using System.Data.Common;
using System.Data.OracleClient;

namespace InflexCLR.Oracle;

public class TableRows : IEnumerable, IDisposable
{
	private OracleDataReader _Reader;

	public TableRows(OracleDataReader reader)
	{
		_Reader = reader;
	}

	public IEnumerator GetEnumerator()
	{
		return new RowEnumerator(_Reader);
	}

	public void Dispose()
	{
		OracleDataReader reader = _Reader;
		if (reader != null)
		{
			((DbDataReader)(object)reader).Close();
			((DbDataReader)(object)reader).Dispose();
		}
		_Reader = null;
	}
}
