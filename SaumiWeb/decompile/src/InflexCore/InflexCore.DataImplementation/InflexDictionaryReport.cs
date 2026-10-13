using System;
using System.Collections.Generic;
using System.Linq;
using InflexCore.DataImplementation.Accessors;

namespace InflexCore.DataImplementation;

public class InflexDictionaryReport : IInflexReport
{
	private readonly InflexDictionaryType _InflexDictType;

	private IEnumerable<IInflexColumn> _ExplorerColumns;

	public decimal ID { get; set; }

	public decimal SubReportID => 0m;

	public string SubReportName => "Все данные";

	public string Name { get; set; }

	public IInflexReport ParentReport => null;

	public IInflexReport RootReport => null;

	public IInflexType AssignedType => _InflexDictType;

	public string PrepareReportProcedureText => string.Empty;

	public bool RollbackAfterPrepareReport => false;

	public Dictionary<decimal, string> SubReports => new Dictionary<decimal, string>();

	public string DisplayColumn => _InflexDictType.DisplayColumn;

	public string PrimaryColumn => _InflexDictType.PrimaryColumn;

	public string RootColumn => _InflexDictType.RootColumn;

	public InflexDictionaryReport(InflexDictionaryType inflexDictType)
	{
		_InflexDictType = inflexDictType;
		ID = _InflexDictType.ID;
		Name = _InflexDictType.Name;
	}

	public IInflexReport GetSubReport(decimal subReportID)
	{
		if (subReportID == 0m)
		{
			return this;
		}
		throw new InvalidOperationException("Subreports not allowed.");
	}

	public IEnumerable<IInflexColumn> GetExplorerColumns()
	{
		if (_ExplorerColumns == null)
		{
			AppManager instance = AppManager.Instance;
			IList<InflexDictionaryColumn> list = instance.Accessor<InflexDictionaryColumnAccessor>().SelectExplorerColumns(_InflexDictType.TableOwner.ToUpper(), _InflexDictType.TableName);
			foreach (InflexDictionaryColumn item in list)
			{
				item.Report = this;
				if (item.DirectColumnName.ToUpper() == "NAME")
				{
					item.SeqN = 1m;
				}
				else if (item.DirectColumnName.ToUpper() == DisplayColumn.ToUpper())
				{
					item.SeqN = 2m;
				}
			}
			_ExplorerColumns = list.Where((InflexDictionaryColumn c) => c.ShowColumn || c.IsRootKey() || c.IsPrimary()).Select((Func<InflexDictionaryColumn, IInflexColumn>)((InflexDictionaryColumn c) => c)).ToArray();
		}
		return _ExplorerColumns;
	}

	public IInflexColumn GetPrimaryColumn()
	{
		IEnumerable<IInflexColumn> enumerable = from c in GetExplorerColumns()
			where c.IsPrimary()
			select c;
		IEnumerator<IInflexColumn> enumerator = enumerable.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw new PrimaryColumnNotFoundException(this);
		}
		IInflexColumn current = enumerator.Current;
		if (enumerator.MoveNext())
		{
			throw new PrimaryColumnDuplicateException(this);
		}
		return current;
	}

	public IEnumerable<OuterReference> GetOuterReferenceReports()
	{
		return new List<OuterReference>();
	}

	public IEnumerable<Dictionary<IInflexColumn, object>> GetValues(IInflexColumn idColumn, decimal id, params IInflexColumn[] columns)
	{
		return new InflexReportHelper(AppManager.Instance, this).GetValues(idColumn, id, columns);
	}

	public IEnumerable<Dictionary<IInflexColumn, object>> GetValues(params IInflexColumn[] columns)
	{
		return new InflexReportHelper(AppManager.Instance, this).GetValues(columns);
	}

	public Dictionary<IInflexColumn, object> GetValues(decimal id, params IInflexColumn[] columns)
	{
		return new InflexReportHelper(AppManager.Instance, this).GetValues(id, columns);
	}

	public Dictionary<IInflexColumn, object> GetValues(IInflexID id, params IInflexColumn[] columns)
	{
		return new InflexReportHelper(AppManager.Instance, this).GetValues(id.ID, columns);
	}

	public decimal GetCountValues(IInflexColumn column, decimal id)
	{
		return new InflexReportHelper(AppManager.Instance, this).GetCountValues(column, id);
	}

	public IEnumerable<IStyleLegendItem> GetStyleLegends()
	{
		return new List<IStyleLegendItem>();
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (object.ReferenceEquals(this, obj))
		{
			return true;
		}
		if ((object)obj.GetType() != typeof(InflexDictionaryReport))
		{
			return false;
		}
		return Equals((InflexDictionaryReport)obj);
	}

	public bool Equals(InflexDictionaryReport other)
	{
		if (object.ReferenceEquals(null, other))
		{
			return false;
		}
		if (object.ReferenceEquals(this, other))
		{
			return true;
		}
		return other.ID == ID;
	}

	public override int GetHashCode()
	{
		return ID.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("Dictionary report \"{0}\" (ID = {1}) with subreport \"{2}\" (ID = {3})", new object[4] { Name, ID, SubReportName, SubReportID });
	}
}
