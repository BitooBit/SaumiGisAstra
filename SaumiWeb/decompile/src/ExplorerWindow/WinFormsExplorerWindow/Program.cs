using System;
using System.Windows.Forms;
using InflexCore.Native.GW;
using InflexUtils.ExceptionBox;

namespace WinFormsExplorerWindow;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Invalid comparison between Unknown and I4
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		int inflexAppHandle = 0;
		InflexGwManager.InflexAppHandle = inflexAppHandle;
		string userName = "SM";
		string password = "smpwd";
		string dataBase = "mmpdemo";
		string inflexSchema = "sm";
		int reportId = 1;
		if (false)
		{
			AuthForm authForm = new AuthForm();
			try
			{
				((Form)authForm).StartPosition = (FormStartPosition)1;
				DialogResult val = ((Form)authForm).ShowDialog();
				if ((int)val == 1)
				{
					userName = authForm.UserName;
					password = authForm.Password;
					dataBase = authForm.Database;
					reportId = authForm.ReportId;
					inflexSchema = "sm";
					try
					{
						RunExplorerWindow(inflexSchema, reportId, password, userName, dataBase);
					}
					catch (Exception exception)
					{
						ExceptionBox.Show(exception);
					}
				}
			}
			finally
			{
				((IDisposable)authForm)?.Dispose();
			}
		}
		else
		{
			RunExplorerWindow(inflexSchema, reportId, password, userName, dataBase);
		}
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	private static void RunExplorerWindow(string inflexSchema, int reportId, string password, string userName, string dataBase)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		MessageBox.Show(string.Format("User: {0}@{1}\nPassword: {2}\nSchema: {3}", new object[4] { userName, dataBase, password, inflexSchema }));
		ExplorerWindowApplicationContext explorerWindowApplicationContext = new ExplorerWindowApplicationContext(0, userName, password, dataBase, inflexSchema, reportId, "", "", 0, "");
		Application.Run((ApplicationContext)(object)explorerWindowApplicationContext);
	}
}
