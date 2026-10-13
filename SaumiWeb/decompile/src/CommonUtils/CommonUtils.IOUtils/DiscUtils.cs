using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CommonUtils.IOUtils;

public static class DiscUtils
{
	public const char INVALID_PATH_CHAR_REPLACER = '_';

	public const char INVALID_FILENAME_CHAR_REPLACER = '_';

	public static bool IsDirectoryNullOrEmpty(string path)
	{
		if (!Directory.Exists(path))
		{
			return true;
		}
		bool flag = Directory.GetFileSystemEntries(path).Any();
		return !flag;
	}

	public static void CopyDirectory(string sourcePath, string targetPath)
	{
		CopyDirectory(new DirectoryInfo(sourcePath), new DirectoryInfo(targetPath));
	}

	public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
	{
		if (!Directory.Exists(target.FullName))
		{
			Directory.CreateDirectory(target.FullName);
		}
		FileInfo[] files = source.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			fileInfo.CopyTo(Path.Combine(target.ToString(), fileInfo.Name), overwrite: true);
		}
		DirectoryInfo[] directories = source.GetDirectories();
		foreach (DirectoryInfo directoryInfo in directories)
		{
			DirectoryInfo target2 = target.CreateSubdirectory(directoryInfo.Name);
			CopyDirectory(directoryInfo, target2);
		}
	}

	public static string GetParentDirectory(string path)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		if (directoryInfo.Parent != null)
		{
			return directoryInfo.Parent.FullName;
		}
		return null;
	}

	public static string GetSystemDrive()
	{
		string systemDirectory = Environment.SystemDirectory;
		return systemDirectory.Substring(0, systemDirectory.IndexOf(Path.VolumeSeparatorChar) + 1);
	}

	public static bool IsPathRootEquals(params string[] paths)
	{
		string text = null;
		foreach (string path in paths)
		{
			string pathRoot = Path.GetPathRoot(path);
			if (string.IsNullOrEmpty(pathRoot))
			{
				return false;
			}
			pathRoot = pathRoot.TrimEnd(new char[2]
			{
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar
			});
			if (text != null && !string.Equals(text, pathRoot, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			text = pathRoot;
		}
		return true;
	}

	public static string GetFileVersionString(string path)
	{
		FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
		return versionInfo.FileVersion;
	}

	public static DateTime GetFileLastChangesDate(string path)
	{
		FileInfo fileInfo = new FileInfo(path);
		return fileInfo.LastWriteTime;
	}

	public static long GetFileSize(string path)
	{
		FileInfo fileInfo = new FileInfo(path);
		return fileInfo.Length;
	}

	public static long GetFolderSize(string path)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		FileInfo[] files = directoryInfo.GetFiles();
		return files.Sum((FileInfo fileInfo) => fileInfo.Length) + directories.Sum((DirectoryInfo subDirInfo) => GetFolderSize(subDirInfo.FullName));
	}

	public static byte[] GetMd5Hash(byte[] bytes)
	{
		MD5 mD = MD5.Create();
		return mD.ComputeHash(bytes);
	}

	public static byte[] GetMd5Hash(string path)
	{
		if (!File.Exists(path))
		{
			throw new ArgumentException("Не существует файла по заданному пути \"" + path + "\"!", "path");
		}
		byte[] bytes = File.ReadAllBytes(path);
		return GetMd5Hash(bytes);
	}

	public static string Md5HashToString(byte[] md5Hash, string format)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (md5Hash != null)
		{
			foreach (byte b in md5Hash)
			{
				stringBuilder.Append(b.ToString(format));
			}
		}
		else
		{
			stringBuilder.Append("NULL");
		}
		return stringBuilder.ToString();
	}

	public static string Md5HashToHexString(byte[] md5Hash)
	{
		return Md5HashToString(md5Hash, "X");
	}

	public static byte[] HexStringToMd5Hash(string md5HashHexString)
	{
		int num = md5HashHexString.Length / 2;
		byte[] array = new byte[num];
		using StringReader stringReader = new StringReader(md5HashHexString);
		for (int i = 0; i < num; i++)
		{
			char[] value = new char[2]
			{
				(char)stringReader.Read(),
				(char)stringReader.Read()
			};
			string value2 = new string(value);
			array[i] = Convert.ToByte(value2, 16);
		}
		return array;
	}

	public static bool CompareMd5Hashes(byte[] md5Hash1, byte[] md5Hash2)
	{
		return CompareByteArrays(md5Hash1, md5Hash2);
	}

	public static bool CompareByteArrays(byte[] byteArray1, byte[] byteArray2)
	{
		if (byteArray1 == null || byteArray2 == null)
		{
			return false;
		}
		if (byteArray1.Length != byteArray2.Length)
		{
			return false;
		}
		for (int i = 0; i < byteArray1.Length; i++)
		{
			if (byteArray1[i] != byteArray2[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool CompareFiles(string path1, string path2)
	{
		if (!File.Exists(path1) || !File.Exists(path2))
		{
			return false;
		}
		if (string.Compare(path1, path2, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return true;
		}
		long fileSize = GetFileSize(path1);
		long fileSize2 = GetFileSize(path2);
		if (fileSize != fileSize2)
		{
			return false;
		}
		byte[] md5Hash = GetMd5Hash(path1);
		byte[] md5Hash2 = GetMd5Hash(path2);
		return CompareMd5Hashes(md5Hash, md5Hash2);
	}

	public static bool IsDiscSpaceAvailable(string path, long needFreeSpace)
	{
		try
		{
			DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(path));
			return driveInfo.AvailableFreeSpace >= needFreeSpace;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static void RenameFile(string sourceFileFullPath, string destinationFileFullPath)
	{
		if (!File.Exists(sourceFileFullPath))
		{
			throw new ArgumentException("Specified source file " + sourceFileFullPath + " not found");
		}
		if (!File.Exists(destinationFileFullPath))
		{
			FileStream fileStream = File.Create(destinationFileFullPath);
			fileStream.Close();
		}
		File.Replace(sourceFileFullPath, destinationFileFullPath, null);
	}

	private static char[] GetInvalidPathChars()
	{
		List<char> list = new List<char>(Path.GetInvalidPathChars());
		char[] collection = new char[2] { '?', '*' };
		list.AddRange(collection);
		return list.ToArray();
	}

	public static bool IsPathValid(string path)
	{
		char[] invalidPathChars = GetInvalidPathChars();
		int num = path.IndexOfAny(new char[2]
		{
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar
		});
		int num2 = num - 1;
		for (int i = 0; i < path.Length; i++)
		{
			if (path[i] == Path.VolumeSeparatorChar)
			{
				if (i != num2)
				{
					return false;
				}
			}
			else if (Enumerable.Contains(invalidPathChars, path[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static string GetValidPath(string path)
	{
		char[] invalidPathChars = GetInvalidPathChars();
		IList<int> list = new List<int>();
		int num = path.IndexOfAny(new char[2]
		{
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar
		});
		int num2 = num - 1;
		StringBuilder stringBuilder = new StringBuilder(path);
		for (int i = 0; i < stringBuilder.Length; i++)
		{
			if (stringBuilder[i] == Path.VolumeSeparatorChar)
			{
				if (i == num2)
				{
					continue;
				}
				if (i < num2)
				{
					list.Add(i);
					continue;
				}
				if (i > num2)
				{
					stringBuilder[i] = '_';
				}
			}
			if (Enumerable.Contains(invalidPathChars, stringBuilder[i]))
			{
				stringBuilder[i] = '_';
			}
		}
		for (int num3 = list.Count - 1; num3 >= 0; num3--)
		{
			stringBuilder.Remove(list[num3], 1);
		}
		path = stringBuilder.ToString();
		return path;
	}

	public static bool IsFileNameValid(string fileName)
	{
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		char[] array = invalidFileNameChars;
		foreach (char value in array)
		{
			if (Enumerable.Contains(fileName, value))
			{
				return false;
			}
		}
		return true;
	}

	public static string GetValidFileName(string fileName)
	{
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		char[] array = invalidFileNameChars;
		foreach (char oldChar in array)
		{
			fileName = fileName.Replace(oldChar, '_');
		}
		return fileName;
	}
}
