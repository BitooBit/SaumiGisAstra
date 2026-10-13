using System.Reflection;

namespace CommonUtils.EnvironmentUtils;

public static class AppPlatformBitsMatchExtension
{
	public static bool Matches(this AppPlatformBits appPlatformBits, PePlatformType pePlatformType)
	{
		return pePlatformType switch
		{
			PePlatformType.AnyCPU => true, 
			PePlatformType.x86 => appPlatformBits == AppPlatformBits.X86, 
			PePlatformType.x64 => appPlatformBits == AppPlatformBits.X64, 
			_ => false, 
		};
	}

	public static bool Matches(this AppPlatformBits appPlatformBits, ProcessorArchitecture processorArchitecture)
	{
		switch (processorArchitecture)
		{
		case ProcessorArchitecture.MSIL:
			return true;
		case ProcessorArchitecture.X86:
			return appPlatformBits == AppPlatformBits.X86;
		case ProcessorArchitecture.IA64:
		case ProcessorArchitecture.Amd64:
			return appPlatformBits == AppPlatformBits.X64;
		default:
			return false;
		}
	}
}
