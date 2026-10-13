using System;
using System.Collections.Generic;
using InflexCLR.Environment;
using InflexCLR.SPTypes;

namespace InflexUser;

[SaumiExport]
public class TestCalc
{
	private const string OT_Quarterly = "Q";

	private List<string> _writeln = new List<string>();

	private string _messageBox = string.Empty;

	private string GetDictItemName(int DictItem_ID, string Table_Name)
	{
		int sQL_ID = OracleProvider.SQL_BindOpen("select NAME from SM." + Table_Name + " where ID=:ID", DictItem_ID);
		if (!OracleProvider.SQL_EOF(sQL_ID))
		{
			return OracleProvider.SQL_FieldAsString(sQL_ID, "NAME").Trim();
		}
		OracleProvider.SQL_Close(sQL_ID);
		return string.Empty;
	}

	private string GetObjectDescription(int Obj_ID)
	{
		int sQL_ID = OracleProvider.SQL_BindOpen("select DESCRIPTION from SM.OBJECTS where ID=:ID", Obj_ID);
		if (!OracleProvider.SQL_EOF(sQL_ID))
		{
			return OracleProvider.SQL_FieldAsString(sQL_ID, "DESCRIPTION").Trim();
		}
		OracleProvider.SQL_Close(sQL_ID);
		return string.Empty;
	}

	private double GetUPKSZ(int Object_ID, DateTime DateCost)
	{
		int sQL_ID = OracleProvider.SQL_BindOpen("select sm.GetObjectCost(:OBJ_ID,to_date(:COSTDATE,'DD.MM.YYYY'),'U') UPKSZ from dual", Object_ID, DateCost.ToString("dd.MM.yyyy"));
		if (!OracleProvider.SQL_EOF(sQL_ID))
		{
			return OracleProvider.SQL_FieldAsFloat(sQL_ID, "UPKSZ");
		}
		OracleProvider.SQL_Close(sQL_ID);
		return 0.0;
	}

	public bool CalcObligationEx(ref TMOVESET MoveSet, int CurrentPeriodIndex)
	{
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Expected O, but got Unknown
		if (MoveSet.MOVETYPE != 4)
		{
			MessageBox("Расчёт невозможен!" + Environment.NewLine + "Автоматический расчёт величины обязательства описан только для операции движения Аренда!");
			return false;
		}
		double num = 0.0;
		bool flag = false;
		DateTime dateTime = DateTime.FromOADate(MoveSet.MOVEPERIODS[CurrentPeriodIndex].DATEFROM);
		double num2 = 1.0;
		string text = string.Empty;
		if (MoveSet.CLIENT.ORGFORM_ID != 0)
		{
			text = GetDictItemName(MoveSet.CLIENT.ORGFORM_ID, "orgforms");
			num2 = SaumiHelper.GetFX1("K1", MoveSet.CLIENT.ORGFORM_ID, dateTime);
			if (num2 == 0.0)
			{
				MessageBox("Не задан коэффициент для организационно-правовой формы арендатора: " + text);
				return false;
			}
		}
		writeln("Описание порядка расчёта величины арендной платы для земельного участка.");
		writeln("");
		writeln("Размер годовой арендной платы за использование земельного участка,");
		writeln("представляемого без проведения торгов, определяется по формуле");
		writeln("  A = S * КС * K * K1");
		writeln("где");
		writeln(" S  - площадь по договору арендуемого участка, кв.м.");
		writeln(" КС - удельный показатель кадастровой стоимости ЗУ, руб./кв.м");
		writeln("коэффициенты");
		writeln(" К  - назначение использования ЗУ (разрешённое использование по договору)");
		writeln(" К1 - коэффициент, определяемый организационно-правовой формой арендатора");
		writeln("");
		int num3 = 0;
		int num4 = MoveSet.MOVEPERIODS[CurrentPeriodIndex].MOVEITEMS.Length;
		for (int i = 0; i < num4; i++)
		{
			TMOVEITEM val = MoveSet.MOVEPERIODS[CurrentPeriodIndex].MOVEITEMS[i];
			if (val.MOVEDOBJECT.OBJTYPE_ID == 7)
			{
				flag = true;
				num3++;
				string objectDescription = GetObjectDescription(val.MOVEDOBJECT.ID);
				double mOVEDSQUARE = val.MOVEDSQUARE;
				writeln(objectDescription + ":");
				writeln("");
				writeln("S= " + mOVEDSQUARE + " кв.м.");
				double uPKSZ = GetUPKSZ(val.MOVEDOBJECT.ID, dateTime);
				if (uPKSZ == 0.0)
				{
					MessageBox("Для земельного участка: " + objectDescription + " не задано значение УПКСЗ!");
					return false;
				}
				writeln("КС=" + uPKSZ + " руб./кв.м.");
				if (val.LEASEPURPOSE_ID == 0)
				{
					MessageBox("Для земельного участка: " + objectDescription + " не задано целевое назначение по договору!");
					return false;
				}
				double fX = SaumiHelper.GetFX1("K", val.LEASEPURPOSE_ID, dateTime);
				string dictItemName = GetDictItemName(val.LEASEPURPOSE_ID, "LEASEPURPOSE");
				if (fX == 0.0)
				{
					MessageBox("Не задан коэффициент для целевого использования: " + dictItemName + " (участок " + objectDescription + ") ");
					return false;
				}
				writeln("К= " + fX + " (" + dictItemName + ")");
				string text2 = "К1= " + num2;
				if (MoveSet.CLIENT.ORGFORM_ID != 0)
				{
					text2 = text2 + " (" + text + ")";
				}
				writeln(text2);
				writeln("");
				double num5 = mOVEDSQUARE * uPKSZ * fX * num2;
				writeln("A= " + num5 + " руб.");
				num += num5;
			}
		}
		if (!flag)
		{
			MessageBox("В расчётном периоде не обнаружено ни одного земельного участка." + Environment.NewLine + "Расчёт величины арендной платы описан толко для земельных участков.");
			return false;
		}
		if (num3 > 1)
		{
			writeln("");
			writeln("Всего участков по договору: " + num3);
			writeln("Величина арендной платы по всем участкам: " + num + " руб.");
		}
		TOBLIGATION val2 = new TOBLIGATION();
		val2.OBLIGTYPE = "Q";
		val2.PAYSIZE = num;
		val2.ISYEARLYPAYSIZE = "Y";
		val2.KIND = "B";
		val2.FINEDATE = 16;
		val2.FINEPERCENT = 0.0;
		val2.FINERFRPART = 300;
		val2.PURPOSE_ID = 101;
		val2.RECEPIENT_ID = 61;
		val2.MAINKBK_ID = 567;
		MoveSet.MOVEPERIODS[CurrentPeriodIndex].OBLIGATIONS = (TOBLIGATION[])(object)new TOBLIGATION[1] { val2 };
		return true;
	}

	private void writeln(string mess)
	{
		_writeln.Add(mess);
	}

	public string[] Getwriteln()
	{
		return _writeln.ToArray();
	}

	private void MessageBox(string mess)
	{
		_messageBox = mess;
	}

	public string GetMessageBox()
	{
		return _messageBox;
	}
}
