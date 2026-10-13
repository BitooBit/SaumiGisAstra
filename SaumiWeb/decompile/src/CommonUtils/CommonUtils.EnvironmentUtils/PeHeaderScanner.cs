using System;
using System.IO;
using CommonUtils.ParseUtils;

namespace CommonUtils.EnvironmentUtils;

public static class PeHeaderScanner
{
	private const ushort IMAGE_FILE_MACHINE_AMD64 = 34404;

	private const ushort IMAGE_FILE_MACHINE_IA64 = 512;

	private const ushort IMAGE_FILE_MACHINE_I386 = 332;

	private const int PE32_ARCHITECTURE = 267;

	private const int PE32_PLUS_ARCHITECTURE = 523;

	private const int IMAGE_FILE_32BIT_MACHINE = 256;

	private const int CLI_FLAGS_32BIT_REQUIRED = 2;

	private const int PE_HEADER_POINTER_VALUE_OFFSET = 60;

	private const int CLR_IMAGE_DATA_DIRECTORIES_COUNT = 16;

	private const int M_CHAR_CODE = 77;

	private const int Z_CHAR_CODE = 90;

	private const int P_CHAR_CODE = 80;

	private const int E_CHAR_CODE = 69;

	public static PePlatformType GetPePlatformType(string filePath)
	{
		bool isManaged;
		Version clrVersion;
		return GetPePlatformType(filePath, out isManaged, out clrVersion);
	}

	public static PePlatformType GetPePlatformType(string filePath, out bool isManaged, out Version clrVersion)
	{
		isManaged = false;
		clrVersion = null;
		using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			using BinaryReader binaryReader = new BinaryReader(fileStream);
			byte[] array = binaryReader.ReadBytes(2);
			if (array[0] == 77 && array[1] == 90)
			{
				fileStream.Seek(60L, SeekOrigin.Begin);
				uint num = binaryReader.ReadUInt32();
				fileStream.Seek(num, SeekOrigin.Begin);
				byte[] array2 = binaryReader.ReadBytes(4);
				if (array2[0] == 80 && array2[1] == 69 && array2[2] == 0 && array2[3] == 0)
				{
					ushort num2 = (ushort)binaryReader.ReadInt16();
					ushort numberOfSections = binaryReader.ReadUInt16();
					ushort num3 = num2;
					if (num3 == 332 || num3 == 512 || num3 == 34404)
					{
						fileStream.Seek(12L, SeekOrigin.Current);
						ushort peOptionalHeaderSize = binaryReader.ReadUInt16();
						int num4 = binaryReader.ReadInt16();
						bool flag = (num4 & 0x100) == 256;
						ushort num5 = binaryReader.ReadUInt16();
						int num6 = ((num5 == 267) ? 90 : 106);
						fileStream.Seek(num6, SeekOrigin.Current);
						uint num7 = binaryReader.ReadUInt32();
						if (num7 == 16)
						{
							fileStream.Seek(112L, SeekOrigin.Current);
							uint num8 = binaryReader.ReadUInt32();
							if (num8 != 0)
							{
								isManaged = true;
								uint? num9 = ConvertRvaToAbsoluteOffset(fileStream, binaryReader, num, peOptionalHeaderSize, numberOfSections, num8);
								if (num9.HasValue)
								{
									fileStream.Seek(num9.Value, SeekOrigin.Begin);
									binaryReader.ReadUInt32();
									binaryReader.ReadUInt16();
									binaryReader.ReadUInt16();
									uint rva = binaryReader.ReadUInt32();
									binaryReader.ReadUInt32();
									uint num10 = binaryReader.ReadUInt32();
									uint? num11 = ConvertRvaToAbsoluteOffset(fileStream, binaryReader, num, peOptionalHeaderSize, numberOfSections, rva);
									if (num11.HasValue)
									{
										fileStream.Seek(num11.Value + 12, SeekOrigin.Begin);
										int value = binaryReader.ReadInt32();
										string text = binaryReader.ReadWholeStringValue(value);
										string versionString = text.TrimStart(new char[1] { 'v' });
										if (!versionString.TryParseToVersion(out clrVersion))
										{
											clrVersion = null;
										}
										bool flag2 = (num10 & 2) == 2;
										switch (num2)
										{
										case 512:
										case 34404:
											if (num5 == 523 && !flag && !flag2)
											{
												return PePlatformType.x64;
											}
											break;
										case 332:
											if (num5 == 267 && flag)
											{
												return (!flag2) ? PePlatformType.AnyCPU : PePlatformType.x86;
											}
											break;
										}
									}
								}
							}
							else
							{
								isManaged = false;
								clrVersion = null;
								switch (num2)
								{
								case 512:
								case 34404:
									if (num5 == 523 && !flag)
									{
										return PePlatformType.x64;
									}
									break;
								case 332:
									if (num5 == 267 && flag)
									{
										return PePlatformType.x86;
									}
									break;
								}
							}
						}
					}
				}
			}
		}
		return PePlatformType.Unknown;
	}

	private static uint? ConvertRvaToAbsoluteOffset(FileStream fileStream, BinaryReader reader, uint peHeaderAbsoluteOffset, ushort peOptionalHeaderSize, ushort numberOfSections, uint rva)
	{
		uint? result = null;
		long position = fileStream.Position;
		try
		{
			uint num = peHeaderAbsoluteOffset + 24 + peOptionalHeaderSize;
			for (int i = 0; i < numberOfSections; i++)
			{
				fileStream.Seek(num + i * 40 + 8, SeekOrigin.Begin);
				uint num2 = reader.ReadUInt32();
				uint num3 = reader.ReadUInt32();
				reader.ReadUInt32();
				uint num4 = reader.ReadUInt32();
				if (rva >= num3 && rva < num3 + num2)
				{
					result = rva - num3 + num4;
				}
			}
			return result;
		}
		finally
		{
			fileStream.Seek(position, SeekOrigin.Begin);
		}
	}
}
