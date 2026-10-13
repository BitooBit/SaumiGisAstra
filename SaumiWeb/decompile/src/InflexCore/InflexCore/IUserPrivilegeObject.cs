namespace InflexCore;

public interface IUserPrivilegeObject
{
	string GranteeName { get; }

	string TableName { get; }

	string Privelege { get; }

	bool? GrantableByUserman { get; }
}
