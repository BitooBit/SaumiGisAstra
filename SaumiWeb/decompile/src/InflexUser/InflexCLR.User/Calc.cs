using InflexCLR.Environment;
using InflexCLR.SPTypes;

namespace InflexCLR.User;

[SaumiExport]
public class Calc
{
	public bool CalcObligationEx(ref TMOVESET MoveSet, int CurrentPeriodIndex)
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		if (MoveSet == null)
		{
			return false;
		}
		if (MoveSet.MOVEPERIODS == null)
		{
			return false;
		}
		if (MoveSet.MOVEPERIODS[CurrentPeriodIndex] == null)
		{
			return false;
		}
		if (MoveSet.MOVEPERIODS[CurrentPeriodIndex].MOVEITEMS == null)
		{
			return false;
		}
		double num = 0.0;
		TMOVEITEM[] mOVEITEMS = MoveSet.MOVEPERIODS[CurrentPeriodIndex].MOVEITEMS;
		foreach (TMOVEITEM val in mOVEITEMS)
		{
			num += val.MOVEDSQUARE * 15.0;
		}
		MoveSet.MOVEPERIODS[CurrentPeriodIndex].OBLIGATIONS = (TOBLIGATION[])(object)new TOBLIGATION[1];
		TOBLIGATION val2 = new TOBLIGATION();
		if (val2 == null)
		{
			return false;
		}
		val2.OBLIGTYPE = "Y";
		val2.FINEDATE = 5;
		val2.FINEPERCENT = 0.7;
		val2.FINERFRPART = 0;
		if (num != 0.0)
		{
			val2.PAYSIZE = num;
		}
		else
		{
			val2.PAYSIZE = 1000.0;
		}
		val2.KIND = "B";
		val2.PURPOSE_ID = 81;
		val2.RECEPIENT_ID = 61;
		MoveSet.MOVEPERIODS[CurrentPeriodIndex].OBLIGATIONS[0] = val2;
		return true;
	}
}
