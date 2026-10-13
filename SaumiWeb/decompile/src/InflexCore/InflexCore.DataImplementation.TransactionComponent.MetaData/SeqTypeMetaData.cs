using System.Collections.Generic;
using InflexCore.MetaData;

namespace InflexCore.DataImplementation.TransactionComponent.MetaData;

public class SeqTypeMetaData : AbstractInflexMetaData
{
	private Dictionary<IInflexType, string> _Seqs = new Dictionary<IInflexType, string>();

	private IInflexType _MainSeqType;

	protected string SeqName { get; set; }

	public IInflexType MainSeqType
	{
		get
		{
			return _MainSeqType;
		}
		set
		{
			_MainSeqType = value;
			SeqName = _Seqs[_MainSeqType];
		}
	}

	public decimal GetNextID(INativeSQLAccessor nativeSQLAccessor)
	{
		return new InnerDbAccessor(nativeSQLAccessor).GetNextID(SeqName);
	}

	public decimal GetNextID(IInflexType inflexType, InnerDbAccessor accessor)
	{
		return accessor.GetNextID(_Seqs[inflexType]);
	}

	public void AddSeq(IInflexType inflexType, string Schema, string tableName)
	{
		_Seqs.Add(inflexType, Schema + ".seq" + tableName);
	}
}
