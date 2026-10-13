using System;

namespace InflexCore.Native.GW;

public class Inflex32ProcNotFoundException : Inflex32NotFoundException
{
	public string ProcName { get; set; }

	public Inflex32ProcNotFoundException(string procName)
		: this(procName, null)
	{
	}

	public Inflex32ProcNotFoundException(string procName, Exception innerException)
		: base(procName + " not found in " + InflexGw.Inflex32DllName, innerException)
	{
		ProcName = procName;
	}
}
