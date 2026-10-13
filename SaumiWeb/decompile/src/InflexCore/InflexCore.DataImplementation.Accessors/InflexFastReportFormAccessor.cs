using System.Collections.Generic;
using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.Accessors;

public abstract class InflexFastReportFormAccessor : AppManager.InflexAccessorBase<InflexFastReportForm, InflexFastReportFormAccessor>
{
	private const string sql = "select ID, \"DESCRIPTION\", IS_DEFAULT, SHORTCUT_CODE, ITEM_HANDLING, MENU_GROUP, NAME, '2' as ORD\r\nfrom <T>\r\nwhere Report_ID=:reportID and MENU_GROUP is not null\r\nUNION\r\nselect ID, DESCRIPTION, IS_DEFAULT, SHORTCUT_CODE, ITEM_HANDLING, MENU_GROUP, NAME, '1' as ORD\r\nfrom <T>\r\nwhere Report_ID=:reportID and MENU_GROUP is null\r\nORDER BY ORD, MENU_GROUP, \"DESCRIPTION\", ITEM_HANDLING DESC";

	[InflexSqlQuery("select ID, \"DESCRIPTION\", IS_DEFAULT, SHORTCUT_CODE, ITEM_HANDLING, MENU_GROUP, NAME, '2' as ORD\r\nfrom <T>\r\nwhere Report_ID=:reportID and MENU_GROUP is not null\r\nUNION\r\nselect ID, DESCRIPTION, IS_DEFAULT, SHORTCUT_CODE, ITEM_HANDLING, MENU_GROUP, NAME, '1' as ORD\r\nfrom <T>\r\nwhere Report_ID=:reportID and MENU_GROUP is null\r\nORDER BY ORD, MENU_GROUP, \"DESCRIPTION\", ITEM_HANDLING DESC")]
	public abstract IList<InflexFastReportForm> SelectByReport(decimal reportID);
}
