using InflexCore;
using InflexCore.Role;

namespace WinFormsExplorerWindow.Roles;

public class ExcelExportRole : NameRole
{
	public override string RoleName
	{
		get
		{
			string text = base.AppManager.AppSettings["EXCEL_EXPORT_ROLE"];
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return new AllUsersRole(base.AppManager).RoleName;
		}
	}

	public ExcelExportRole(AppManager appManager)
		: base(appManager)
	{
	}
}
