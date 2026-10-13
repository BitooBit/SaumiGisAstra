using System.Windows.Forms;
using InflexCore;

namespace WinFormsExplorerWindow.PrimitiveControls.RestrictionsPanelControls;

public class RestrictionColumnCheckBox : CheckBox
{
	private readonly object _DisposeSync = new object();

	private IInflexColumn _Column;

	public IInflexColumn Column
	{
		get
		{
			return _Column;
		}
		private set
		{
			IInflexColumn inflexColumn = (_Column = value);
			if (inflexColumn != null)
			{
				((Control)this).Text = inflexColumn.ColumnName;
			}
		}
	}

	public string Caption
	{
		get
		{
			IInflexColumn column = Column;
			if (column != null)
			{
				return column.ColumnName;
			}
			return string.Empty;
		}
	}

	public bool IsCheckedChanged => ((CheckBox)this).Checked != IsCheckedKept;

	private bool IsCheckedKept { get; set; }

	public void KeepIsChecked()
	{
		IsCheckedKept = ((CheckBox)this).Checked;
	}

	public void RestoreIsChecked()
	{
		((CheckBox)this).Checked = IsCheckedKept;
	}

	public RestrictionColumnCheckBox(IInflexColumn column, bool isChecked)
	{
		Column = column;
		((CheckBox)this).Checked = isChecked;
		KeepIsChecked();
	}

	protected override void Dispose(bool disposing)
	{
		lock (_DisposeSync)
		{
			if (disposing)
			{
				Column = null;
			}
		}
		((ButtonBase)this).Dispose(disposing);
	}
}
