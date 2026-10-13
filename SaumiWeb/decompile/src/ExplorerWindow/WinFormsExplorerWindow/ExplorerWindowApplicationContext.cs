using System;
using System.Windows.Forms;
using InflexCore.Native;

namespace WinFormsExplorerWindow;

public class ExplorerWindowApplicationContext : ApplicationContext
{
	private ExplorerWindowApplicationContext()
	{
		Application.ApplicationExit += ApplicationExit;
	}

	protected override void OnMainFormClosed(object sender, EventArgs e)
	{
		Application.Exit();
	}

	private void ApplicationExit(object sender, EventArgs e)
	{
		Application.ApplicationExit -= ApplicationExit;
		ExplorerWindowProxy.Dispose();
		AppManagerProxy.Dispose();
	}

	public ExplorerWindowApplicationContext(int assignHandle, string userName, string password, string dataBase, string inflexSchema, int reportID, string restrictionSQL, string restrictionXML, int restrictionColumnID, string infoText, string manualFrom = null, string manualWhere = null)
		: this()
	{
		Form createdwindow = null;
		ExplorerWindowProxy.ShowExplorerWindow(assignHandle, userName, password, dataBase, inflexSchema, reportID, restrictionSQL, restrictionXML, restrictionColumnID, infoText, out createdwindow);
		((ApplicationContext)this).MainForm = createdwindow;
	}
}
