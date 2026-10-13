namespace InflexCore;

public class QueryColumn
{
	public const string NESTED_COLUMN_DELIMETER = " - ";

	public IInflexColumn ExplorerColumn { get; set; }

	public QueryColumn ForeignColumn { get; set; }

	public bool Visible { get; set; }

	public QueryColumn(IInflexColumn explorerColumn, bool visible)
	{
		ExplorerColumn = explorerColumn;
		Visible = visible;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is QueryColumn queryColumn))
		{
			return base.Equals(obj);
		}
		if (Visible == queryColumn.Visible && ExplorerColumn == queryColumn.ExplorerColumn)
		{
			if (ForeignColumn != null || queryColumn.ForeignColumn != null)
			{
				if (ForeignColumn != null)
				{
					return ForeignColumn.Equals(queryColumn.ForeignColumn);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hashCode = ExplorerColumn.GetHashCode();
		if (ForeignColumn == null)
		{
			return hashCode;
		}
		return ForeignColumn.GetHashCode() ^ hashCode;
	}
}
