namespace InflexCore.Native.GW;

internal class FindExGw : InflexGw.InflexGwPart, IFindExGw
{
	private delegate int NativeFindExFindAllHandler(int reportID, ref string sql, ref string xml);

	private delegate int NativeFindExEditFindAllHandler(int reportID, string oldXml, ref string sql, ref string xml);

	private NativeFindExFindAllHandler _FindEx_FindAll;

	private NativeFindExEditFindAllHandler _FindEx_EditFindAll;

	public bool AllowAsync => false;

	public FindExGw(InflexGw inflexGw)
		: base(inflexGw)
	{
	}

	protected override bool DirectInit()
	{
		_FindEx_FindAll = LoadInflex32Proc<NativeFindExFindAllHandler>("FindEx_FindAll", _FindEx_FindAll) as NativeFindExFindAllHandler;
		_FindEx_EditFindAll = LoadInflex32Proc<NativeFindExEditFindAllHandler>("FindEx_EditFindAll", _FindEx_EditFindAll) as NativeFindExEditFindAllHandler;
		return true;
	}

	public int FindAll(int queryHandle, bool async, decimal reportID, ref string sql, ref string xml)
	{
		TryInitNow();
		if (_FindEx_FindAll != null)
		{
			return _FindEx_FindAll(decimal.ToInt32(reportID), ref sql, ref xml);
		}
		throw new Inflex32ProcNotFoundException("FindEx_FindAll");
	}

	public int FindAll(int queryHandle, bool async, decimal reportID, string oldXml, ref string sql, ref string xml)
	{
		TryInitNow();
		if (_FindEx_EditFindAll != null)
		{
			return _FindEx_EditFindAll(decimal.ToInt32(reportID), oldXml, ref sql, ref xml);
		}
		throw new Inflex32ProcNotFoundException("FindEx_FindAll");
	}
}
