using System.Runtime.InteropServices;

namespace InflexCLR.Environment;

internal static class _Inflex_GW
{
	[DllImport("inflex_gw.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern bool Inflex_Init(string ProgID, string AutoLogonName, string AutoLogonPwd, string AutoLogonDBName);

	[DllImport("inflex_gw.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern void Inflex_Done();

	[DllImport("inflex_gw.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern bool Inflex_CheckInstance();

	[DllImport("inflex_gw.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern int Inflex_NewItem(byte ObjType, byte ObjSubType, int Parent_ObjID, byte Parent_ObjType);

	[DllImport("inflex_gw.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern void Inflex_ShowItem(int ObjID, int ObjType);

	[DllImport("inflex_gw.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern bool Inflex_DeleteItem(int ObjID, int ObjType);

	[DllImport("inflex_gw.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern void Inflex_ShowDataWindow(int QueryID, string Restriction, string Hint);
}
