using System;
using System.IO;

namespace InflexCLR.Environment;

public static class Inflex_GW
{
	private static int Init_Count;

	private static void Auto_Init()
	{
		if (Init_Count <= 0)
		{
			Inflex_Init("SAUMI30", Saumi4.UserName, "", Saumi4.Database);
		}
	}

	public static bool Inflex_Init(string ProgID, string AutoLogonName, string AutoLogonPwd, string AutoLogonDBName)
	{
		try
		{
			bool flag = _Inflex_GW.Inflex_Init(ProgID, AutoLogonName, AutoLogonPwd, AutoLogonDBName);
			if (flag)
			{
				Init_Count++;
			}
			return flag;
		}
		catch (DllNotFoundException innerException)
		{
			throw new Exception("Библиотека inflex_gw.dll не найдена в каталоге " + Directory.GetCurrentDirectory(), innerException);
		}
	}

	public static void Inflex_Done()
	{
		try
		{
			_Inflex_GW.Inflex_Done();
			Init_Count--;
		}
		catch (DllNotFoundException innerException)
		{
			throw new Exception("Библиотека inflex_gw.dll не найдена в каталоге " + Directory.GetCurrentDirectory(), innerException);
		}
	}

	public static bool Inflex_CheckInstance()
	{
		Auto_Init();
		try
		{
			return _Inflex_GW.Inflex_CheckInstance();
		}
		catch (DllNotFoundException innerException)
		{
			throw new Exception("Библиотека inflex_gw.dll не найдена в каталоге " + Directory.GetCurrentDirectory(), innerException);
		}
	}

	public static int Inflex_NewItem(byte ObjType, byte ObjSubType, int Parent_ObjID, byte Parent_ObjType)
	{
		Auto_Init();
		try
		{
			return _Inflex_GW.Inflex_NewItem(ObjType, ObjSubType, Parent_ObjID, Parent_ObjType);
		}
		catch (DllNotFoundException innerException)
		{
			throw new Exception("Библиотека inflex_gw.dll не найдена в каталоге " + Directory.GetCurrentDirectory(), innerException);
		}
	}

	public static void Inflex_ShowItem(int ObjID, int ObjType)
	{
		Auto_Init();
		try
		{
			_Inflex_GW.Inflex_ShowItem(ObjID, ObjType);
		}
		catch (DllNotFoundException innerException)
		{
			throw new Exception("Библиотека inflex_gw.dll не найдена в каталоге " + Directory.GetCurrentDirectory(), innerException);
		}
	}

	public static bool Inflex_DeleteItem(int ObjID, int ObjType)
	{
		Auto_Init();
		try
		{
			return _Inflex_GW.Inflex_DeleteItem(ObjID, ObjType);
		}
		catch (DllNotFoundException innerException)
		{
			throw new Exception("Библиотека inflex_gw.dll не найдена в каталоге " + Directory.GetCurrentDirectory(), innerException);
		}
	}

	public static void Inflex_ShowDataWindow(int QueryID, string Restriction, string Hint)
	{
		Auto_Init();
		try
		{
			_Inflex_GW.Inflex_ShowDataWindow(QueryID, Restriction, Hint);
		}
		catch (DllNotFoundException innerException)
		{
			throw new Exception("Библиотека inflex_gw.dll не найдена в каталоге " + Directory.GetCurrentDirectory(), innerException);
		}
	}
}
