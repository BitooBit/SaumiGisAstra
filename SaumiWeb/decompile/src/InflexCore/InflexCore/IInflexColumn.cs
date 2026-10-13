namespace InflexCore;

public interface IInflexColumn
{
	IInflexReport Report { get; }

	decimal ID { get; set; }

	decimal? SeqN { get; set; }

	decimal DataType { get; }

	decimal? ObjectType { get; set; }

	string ColumnName { get; set; }

	string SrcSelect { get; set; }

	string SrcFrom { get; set; }

	string SrcWhere { get; set; }

	bool ShowColumn { get; }

	decimal? ReferenceID { get; set; }

	string ReferenceName { get; set; }

	bool Hidden { get; }

	bool SummFlag { get; }

	string Units { get; set; }

	int Accuracy { get; }

	string GroupName { get; }

	bool CanShow();

	bool IsOuterReference();

	IInflexReport GetOuterReference();

	bool IsMain();

	bool IsOrder();

	bool IsGroupBy();

	bool IsPrimary();

	bool IsIndicator();

	bool IsRestriction();

	bool IsRootKey();

	bool IsSubtypeId();

	bool IsDate();

	bool IsDependentColumn(IInflexColumn column);
}
