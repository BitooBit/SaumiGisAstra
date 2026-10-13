using System;

namespace Log4NetWrapper.Information.OS;

[Flags]
public enum OSSuites : ushort
{
	None = 0,
	SmallBusiness = 1,
	Enterprise = 2,
	BackOffice = 4,
	Communications = 8,
	Terminal = 0x10,
	SmallBusinessRestricted = 0x20,
	EmbeddedNT = 0x40,
	Datacenter = 0x80,
	SingleUserTS = 0x100,
	Personal = 0x200,
	Blade = 0x400,
	EmbeddedRestricted = 0x800
}
