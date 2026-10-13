using System;

namespace CommonUtils.EnvironmentUtils.OsEnums;

[Flags]
public enum SuiteEdition : ushort
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
	EmbeddedRestricted = 0x800,
	SecurityAppliance = 0x1000,
	StorageServer = 0x2000,
	ComputeServer = 0x4000,
	HomeServer = 0x8000
}
