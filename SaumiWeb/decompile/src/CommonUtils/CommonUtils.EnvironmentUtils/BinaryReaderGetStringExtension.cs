using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommonUtils.EnvironmentUtils;

public static class BinaryReaderGetStringExtension
{
	public static string ReadWholeStringValue(this BinaryReader reader, int? length = null, bool trimLineEndChars = true)
	{
		List<char> list = new List<char>();
		while (!length.HasValue || list.Count < length.Value)
		{
			char c = reader.ReadChar();
			list.Add(c);
			if (c == '\0' && reader.BaseStream.Position % 4 == 0)
			{
				break;
			}
		}
		return trimLineEndChars ? new string(list.TakeWhile((char c2) => c2 != '\0').ToArray()) : new string(list.ToArray());
	}
}
