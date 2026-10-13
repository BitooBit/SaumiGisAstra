using OracleUtils.Connection;

namespace InflexCore;

public class InflexConnection
{
	public string UserName { get; set; }

	public string Password { get; set; }

	public string DataBase { get; set; }

	public string MainSchema { get; set; }

	public ConnectionPrivileges DbaPrivilege { get; set; }
}
