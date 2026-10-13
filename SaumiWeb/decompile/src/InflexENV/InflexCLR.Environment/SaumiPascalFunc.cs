using System;
using InflexCLR.Oracle;

namespace InflexCLR.Environment;

[Obsolete("Need use class SaumiHelper. Класс устарел, вместо него нужно использовать класс SaumiHelper")]
public static class SaumiPascalFunc
{
	private static decimal GetFX_ID(string KFX_ShortName)
	{
		decimal result = 0m;
		int num = 0;
		foreach (Row item in QuerySQL.Query("select id from sm.kfxnames where shortname='" + KFX_ShortName + "'"))
		{
			num++;
			result = item.FieldAsNumber("ID");
		}
		if (num > 1)
		{
			throw new Exception("Для короткого имени '" + KFX_ShortName + "' найдено несколько видов коэффициентов.");
		}
		if (num < 1)
		{
			throw new Exception("Коэффициент с коротким именем '" + KFX_ShortName + "' не определен.");
		}
		return result;
	}

	private static double GetFX_Value(string KFX_ShortName, string sql)
	{
		int num = 0;
		decimal num2 = 0m;
		foreach (Row item in QuerySQL.Query(sql))
		{
			num++;
			num2 = item.FieldAsNumber("KFX_VALUE");
		}
		if (num > 1)
		{
			throw new Exception("Найдено несколько возможных значений.");
		}
		return (double)num2;
	}

	public static double GetFX1(string KFX_ShortName, int Dict1_ID, DateTime ForDate)
	{
		decimal fX_ID = GetFX_ID(KFX_ShortName);
		string sql = "select kfx_value from sm.kfxvalues where kfx_id=" + fX_ID + " and dict1_id=" + Dict1_ID + " and since_date<=to_date('" + ForDate.ToShortDateString() + "','DD.MM.YYYY') and end_date>=to_date('" + ForDate.ToShortDateString() + "','DD.MM.YYYY')";
		return GetFX_Value(KFX_ShortName, sql);
	}

	public static double GetFX(string KFX_ShortName, int Dict1_ID, DateTime ForDate)
	{
		return GetFX1(KFX_ShortName, Dict1_ID, ForDate);
	}

	public static double GetFX2(string KFX_ShortName, int Dict1_ID, int Dict2_ID, DateTime ForDate)
	{
		decimal fX_ID = GetFX_ID(KFX_ShortName);
		string sql = "select kfx_value from sm.kfxvalues where kfx_id=" + fX_ID + " and dict1_id=" + Dict1_ID + " and dict2_id=" + Dict2_ID + " and since_date<=to_date('" + ForDate.ToShortDateString() + "','DD.MM.YYYY') and end_date>=to_date('" + ForDate.ToShortDateString() + "','DD.MM.YYYY')";
		return GetFX_Value(KFX_ShortName, sql);
	}

	public static double GetFX(string KFX_ShortName, int Dict1_ID, int Dict2_ID, DateTime ForDate)
	{
		return GetFX2(KFX_ShortName, Dict1_ID, Dict2_ID, ForDate);
	}

	public static double GetFX3(string KFX_ShortName, int Dict1_ID, int Dict2_ID, int Dict3_ID, DateTime ForDate)
	{
		decimal fX_ID = GetFX_ID(KFX_ShortName);
		string sql = "select kfx_value from sm.kfxvalues where kfx_id=" + fX_ID + " and dict1_id=" + Dict1_ID + " and dict2_id=" + Dict2_ID + " and dict3_id=" + Dict3_ID + " and since_date<=to_date('" + ForDate.ToShortDateString() + "','DD.MM.YYYY') and end_date>=to_date('" + ForDate.ToShortDateString() + "','DD.MM.YYYY')";
		return GetFX_Value(KFX_ShortName, sql);
	}

	public static double GetFX(string KFX_ShortName, int Dict1_ID, int Dict2_ID, int Dict3_ID, DateTime ForDate)
	{
		return GetFX3(KFX_ShortName, Dict1_ID, Dict2_ID, Dict3_ID, ForDate);
	}

	public static string DateToStr(double Date)
	{
		return DateToDate(Date).ToShortDateString();
	}

	public static DateTime DateToDate(double Date)
	{
		return DateTime.FromOADate(Date);
	}
}
