using System.Collections.Generic;

namespace InflexCore;

public interface IInflexType
{
	decimal ID { get; set; }

	string TableName { get; set; }

	string TableOwner { get; set; }

	string Name { get; }

	IInflexType Parent { get; }

	decimal? TypeID { get; set; }

	decimal? SubtypeID { get; set; }

	bool IsSubtype { get; }

	IEnumerable<IInflexType> Subtypes { get; }

	bool CanHistory { get; }

	bool IsInternalType { get; }

	bool UseInSearch { get; }

	decimal InSearchPosition { get; }

	string CreationName { get; set; }

	string CreationRole { get; set; }

	bool CanCompareAddress { get; }

	IInflexReport AssignedReport { get; }

	bool IsTypecast(IInflexType type);
}
