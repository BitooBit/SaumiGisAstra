using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using InflexCore.Action.Attributes;
using InflexData;
using InflexDataGridComponent;
using WinFormsExplorerWindow.Roles;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(SaveQueryResultAction), GlyphUri = "SaveAsXML")]
[AutoInstanceAction]
public class SaveQueryResultXMLAction : ExplorerWindowToolAction
{
	private class ExcelXmlWorkbook
	{
		private IList<ExcelXmlStyle> _Styles = new List<ExcelXmlStyle>
		{
			ExcelXmlStyle.EmptyDataStyle,
			ExcelXmlStyle.ColumnHeaderStyle
		};

		private IList<ExcelXmlWorksheet> _Worksheets = new List<ExcelXmlWorksheet>();

		public IList<ExcelXmlWorksheet> Worksheets => _Worksheets;

		public ExcelXmlStyle GetStyle(IGridColumn column)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return GetStyle(column, ExcelXmlStyle.DEFAULT_COLOR, ExcelXmlStyle.DEFAULT_FONT_STYLE);
		}

		public ExcelXmlStyle GetStyle(IGridColumn column, Color color, FontStyle fontStyle)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			IList<ExcelXmlStyle> styles = _Styles;
			string dataFormat = ExcelXmlStyle.GetDataFormat(column);
			ExcelXmlStyle excelXmlStyle = styles.FirstOrDefault(delegate(ExcelXmlStyle s)
			{
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				return s.DataFormat == dataFormat && s.Color == color && s.FontStyle == fontStyle;
			});
			if (excelXmlStyle != null)
			{
				return excelXmlStyle;
			}
			ExcelXmlStyle excelXmlStyle2 = new ExcelXmlStyle(column, color, fontStyle);
			styles.Add(excelXmlStyle2);
			return excelXmlStyle2;
		}

		public void Write(XmlTextWriter writer)
		{
			writer.WriteStartDocument();
			writer.WriteProcessingInstruction("mso-application", "progid=\"Excel.Sheet\"");
			writer.WriteStartElement("Workbook");
			writer.WriteAttributeString("xmlns", "urn:schemas-microsoft-com:office:spreadsheet");
			writer.WriteAttributeString("xmlns", "o", null, "urn:schemas-microsoft-com:office:office");
			writer.WriteAttributeString("xmlns", "x", null, "urn:schemas-microsoft-com:office:excel");
			writer.WriteAttributeString("xmlns", "ss", null, "urn:schemas-microsoft-com:office:spreadsheet");
			writer.WriteAttributeString("xmlns", "html", null, "http://www.w3.org/TR/REC-html40");
			writer.WriteStartElement("DocumentProperties");
			writer.WriteAttributeString("xmlns", "urn:schemas-microsoft-com:office:office");
			writer.WriteElementString("Author", Environment.UserName);
			writer.WriteElementString("Created", DateTime.Now.ToString("s") + "Z");
			writer.WriteElementString("LastSaved", DateTime.Now.ToString("s") + "Z");
			writer.WriteEndElement();
			writer.WriteStartElement("ss", "Styles", null);
			foreach (ExcelXmlStyle style in _Styles)
			{
				style.Write(writer);
			}
			writer.WriteEndElement();
			foreach (ExcelXmlWorksheet worksheet in Worksheets)
			{
				worksheet.Write(writer);
			}
			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Flush();
			writer.Close();
		}
	}

	private class ExcelXmlWorksheet
	{
		private IList<ExcelXmlRow> _Rows = new List<ExcelXmlRow>();

		public string WorksheetName { get; private set; }

		public int ColumnsCount { get; private set; }

		public IList<ExcelXmlRow> Rows => _Rows;

		public ExcelXmlWorksheet(string worksheetName, int columnsCount)
		{
			WorksheetName = worksheetName;
			ColumnsCount = columnsCount;
		}

		public void Write(XmlTextWriter writer)
		{
			string worksheetName = WorksheetName;
			int columnsCount = ColumnsCount;
			writer.WriteStartElement("Worksheet");
			writer.WriteAttributeString("ss", "Name", null, worksheetName);
			writer.WriteStartElement("Names");
			writer.WriteStartElement("NamedRange");
			writer.WriteAttributeString("ss", "Name", null, "Print_Titles");
			writer.WriteAttributeString("ss", "RefersTo", null, "'" + worksheetName + "'!R1");
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteStartElement("Table");
			writer.WriteAttributeString("ss", "ExpandedColumnCount", null, columnsCount.ToString());
			for (int i = 0; i < columnsCount; i++)
			{
				writer.WriteStartElement("Column");
				writer.WriteAttributeString("ss", "AutoFitWidth", null, "1");
				writer.WriteAttributeString("ss", "Width", null, "120");
				writer.WriteEndElement();
			}
			foreach (ExcelXmlRow row in Rows)
			{
				row.Write(writer);
			}
			writer.WriteEndElement();
			writer.WriteStartElement("WorksheetOptions");
			writer.WriteAttributeString("xmlns", "urn:schemas-microsoft-com:office:excel");
			writer.WriteStartElement("PageSetup");
			writer.WriteStartElement("Layout");
			writer.WriteAttributeString("x", "Orientation", null, "Landscape");
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	private class ExcelXmlRow
	{
		private IList<ExcelXmlCell> _Cells = new List<ExcelXmlCell>();

		public IList<ExcelXmlCell> Cells => _Cells;

		public void Write(XmlTextWriter writer)
		{
			IList<ExcelXmlCell> cells = Cells;
			writer.WriteStartElement("Row");
			foreach (ExcelXmlCell item in cells)
			{
				item.Write(writer);
			}
			writer.WriteEndElement();
		}
	}

	private class ExcelXmlCell
	{
		private string _DataType;

		public object Value { get; private set; }

		public Type ColumnType { get; private set; }

		public string DataType => _DataType ?? (_DataType = GetDataType());

		public ExcelXmlStyle Style { get; set; }

		public bool IsEmpty
		{
			get
			{
				if (Value != null)
				{
					return Value == DBNull.Value;
				}
				return true;
			}
		}

		public ExcelXmlCell(object value, Type columnType)
		{
			Value = value;
			ColumnType = columnType;
		}

		public void Write(XmlTextWriter writer)
		{
			string dataType = DataType;
			object value = Value;
			ExcelXmlStyle style = Style;
			writer.WriteStartElement("Cell");
			if (style != null)
			{
				writer.WriteAttributeString("ss", "StyleID", null, style.Name);
			}
			writer.WriteStartElement("Data");
			writer.WriteAttributeString("ss", "Type", null, dataType);
			if (!IsEmpty)
			{
				writer.WriteValue(value);
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		private string GetDataType()
		{
			if (!IsEmpty)
			{
				Type columnType = ColumnType;
				if ((object)columnType == typeof(DateTime))
				{
					return "DateTime";
				}
				if ((object)columnType == typeof(decimal))
				{
					return "Number";
				}
			}
			return "String";
		}
	}

	private class ExcelXmlStyle
	{
		public static readonly Color DEFAULT_COLOR;

		public static readonly FontStyle DEFAULT_FONT_STYLE;

		private static readonly ExcelXmlStyle _EmptyDataStyle;

		private static readonly ExcelXmlStyle _ColumnHeaderStyle;

		private readonly string _Name;

		private readonly IGridColumn _Column;

		private ContentAlignment? _Alignment;

		private bool _IsBordered;

		private readonly Color _Color;

		private readonly FontStyle _FontStyle;

		private string _DataFormat;

		public static ExcelXmlStyle EmptyDataStyle => _EmptyDataStyle;

		public static ExcelXmlStyle ColumnHeaderStyle => _ColumnHeaderStyle;

		public string Name => _Name;

		public IGridColumn Column => _Column;

		public ContentAlignment? Alignment
		{
			get
			{
				return _Alignment;
			}
			set
			{
				_Alignment = value;
			}
		}

		public bool IsBordered
		{
			get
			{
				return _IsBordered;
			}
			set
			{
				_IsBordered = value;
			}
		}

		public Color Color => _Color;

		public FontStyle FontStyle
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _FontStyle;
			}
		}

		public string DataFormat => _DataFormat ?? (_DataFormat = GetDataFormat(Column));

		public ExcelXmlStyle(IGridColumn column, Color color, FontStyle fontStyle)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			this._002Ector(column);
			_Color = color;
			_FontStyle = fontStyle;
		}

		public ExcelXmlStyle(IGridColumn column)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			_Alignment = null;
			_IsBordered = true;
			_Color = DEFAULT_COLOR;
			_FontStyle = DEFAULT_FONT_STYLE;
			base._002Ector();
			_Name = Guid.NewGuid().ToString();
			_Column = column;
		}

		public void Write(XmlTextWriter writer)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_029e: Invalid comparison between Unknown and I4
			//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bb: Invalid comparison between Unknown and I4
			//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d8: Invalid comparison between Unknown and I4
			//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f5: Invalid comparison between Unknown and I4
			string name = Name;
			ContentAlignment? alignment = Alignment;
			bool isBordered = IsBordered;
			Color color = Color;
			FontStyle fontStyle = FontStyle;
			string dataFormat = DataFormat;
			writer.WriteStartElement("Style");
			writer.WriteAttributeString("ss", "ID", null, name);
			if (alignment.HasValue)
			{
				string horizontalAlignmentStringValue = alignment.Value.GetHorizontalAlignmentStringValue();
				string verticalAlignmentStringValue = alignment.Value.GetVerticalAlignmentStringValue();
				bool flag = !string.IsNullOrEmpty(horizontalAlignmentStringValue);
				bool flag2 = !string.IsNullOrEmpty(verticalAlignmentStringValue);
				if (flag || flag2)
				{
					writer.WriteStartElement("Alignment");
					if (flag)
					{
						writer.WriteAttributeString("ss", "Horizontal", null, horizontalAlignmentStringValue);
					}
					if (flag2)
					{
						writer.WriteAttributeString("ss", "Vertical", null, verticalAlignmentStringValue);
					}
					writer.WriteEndElement();
				}
			}
			if (isBordered)
			{
				writer.WriteStartElement("Borders");
				writer.WriteStartElement("Border");
				writer.WriteAttributeString("ss", "Position", null, "Bottom");
				writer.WriteAttributeString("ss", "LineStyle", null, "Continuous");
				writer.WriteAttributeString("ss", "Weight", null, "1");
				writer.WriteEndElement();
				writer.WriteStartElement("Border");
				writer.WriteAttributeString("ss", "Position", null, "Left");
				writer.WriteAttributeString("ss", "LineStyle", null, "Continuous");
				writer.WriteAttributeString("ss", "Weight", null, "1");
				writer.WriteEndElement();
				writer.WriteStartElement("Border");
				writer.WriteAttributeString("ss", "Position", null, "Right");
				writer.WriteAttributeString("ss", "LineStyle", null, "Continuous");
				writer.WriteAttributeString("ss", "Weight", null, "1");
				writer.WriteEndElement();
				writer.WriteStartElement("Border");
				writer.WriteAttributeString("ss", "Position", null, "Top");
				writer.WriteAttributeString("ss", "LineStyle", null, "Continuous");
				writer.WriteAttributeString("ss", "Weight", null, "1");
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
			writer.WriteStartElement("Font");
			string value = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
			writer.WriteAttributeString("ss", "Color", null, value);
			if ((fontStyle & 1) == 1)
			{
				writer.WriteAttributeString("ss", "Bold", null, "1");
			}
			if ((fontStyle & 2) == 2)
			{
				writer.WriteAttributeString("ss", "Italic", null, "1");
			}
			if ((fontStyle & 8) == 8)
			{
				writer.WriteAttributeString("ss", "StrikeThrough", null, "1");
			}
			if ((fontStyle & 4) == 4)
			{
				writer.WriteAttributeString("ss", "Underline", null, "Single");
			}
			writer.WriteEndElement();
			if (!string.IsNullOrEmpty(dataFormat))
			{
				writer.WriteStartElement("NumberFormat");
				writer.WriteAttributeString("ss", "Format", null, dataFormat);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		public static string GetDataFormat(IGridColumn column)
		{
			if (column == null)
			{
				return null;
			}
			Type columnType = column.ColumnType;
			if ((object)columnType == typeof(decimal))
			{
				if (column.TotalSummMode != TotalSummModeEnum.None)
				{
					IValueToDisplayStringConverter valueToDisplayStringConverter = (IValueToDisplayStringConverter)column;
					return valueToDisplayStringConverter.GetFormatString(column);
				}
				return "0";
			}
			if ((object)columnType == typeof(DateTime))
			{
				if (column.IsDate)
				{
					return "Short Date";
				}
				return "dd/mm/yyyy hh:mm:ss";
			}
			return "@";
		}

		static ExcelXmlStyle()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			DEFAULT_COLOR = Color.Black;
			DEFAULT_FONT_STYLE = (FontStyle)0;
			_EmptyDataStyle = new ExcelXmlStyle(null)
			{
				_DataFormat = null
			};
			_ColumnHeaderStyle = new ExcelXmlStyle(null, DEFAULT_COLOR, (FontStyle)1)
			{
				_Alignment = (ContentAlignment)32,
				_DataFormat = null
			};
		}
	}

	private StatusPanelStopedProgressBar _CurrentProgressBar;

	public override string Caption => "Экспорт в XML-файл...";

	protected override bool? DirectIsAllowed()
	{
		InflexDataTable dataTable = base.DirectContext.DataView.DataTable;
		InflexDataGrid dataGrid = base.DirectContext.ExplorerWindow.DataGrid;
		GridView dataGridView = base.DirectContext.ExplorerWindow.DataGridView;
		return _CurrentProgressBar == null && dataTable != null && dataGrid != null && dataGridView != null && ((BaseView)dataGridView).DataRowCount > 0 && dataGrid.VisibleGridColumns.Count > 0 && ((ColumnView)dataGridView).GroupCount == 0 && base.DirectContext.AppManager.RoleEnabled<ExcelExportRole>();
	}

	protected override void ProtectedDirectExecute()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Invalid comparison between Unknown and I4
		ExplorerWindow explorerWindow = base.DirectContext.ExplorerWindow;
		InflexDataGrid dataGrid = explorerWindow.DataGrid;
		GridView dataGridView = explorerWindow.DataGridView;
		if (dataGrid == null || dataGridView == null)
		{
			return;
		}
		int dataRowCount = ((BaseView)dataGridView).DataRowCount;
		int count = dataGrid.VisibleGridColumns.Count;
		if (dataRowCount == 0 || count == 0)
		{
			return;
		}
		if (((ColumnView)dataGridView).GroupCount > 0)
		{
			throw new NotSupportedException();
		}
		SaveFileDialog val = new SaveFileDialog();
		((FileDialog)val).FileName = base.DirectContext.Type.Name;
		((FileDialog)val).Filter = "Файл XML (*.xml)|*.xml|Все файлы (*.*)|*.*";
		((FileDialog)val).Title = "Выбор файла для сохранения";
		SaveFileDialog val2 = val;
		DialogResult val3 = ((CommonDialog)val2).ShowDialog();
		string fileName = ((FileDialog)val2).FileName;
		if (string.IsNullOrEmpty(fileName) || (int)val3 != 1)
		{
			return;
		}
		base.DirectContext.IsDataBlocked = true;
		_CurrentProgressBar = explorerWindow.ProgressBarFactory.CreateStopedProgressBar("Вы уверены, что хотите остановить передачу данных в XML?");
		StatusPanelStopedProgressBar progressBar = _CurrentProgressBar;
		Action<int> action = delegate(int value)
		{
			if (progressBar != null)
			{
				progressBar.Value = value;
			}
		};
		Action action2 = delegate
		{
			if (progressBar != null)
			{
				base.DirectContext.IsDataBlocked = false;
				progressBar.Free();
				progressBar.Dispose();
				_CurrentProgressBar = null;
			}
		};
		Thread thread = new Thread(StartExportThread);
		thread.IsBackground = true;
		thread.Start(new object[7] { explorerWindow, dataGrid, dataGridView, action, progressBar, action2, fileName });
	}

	private void StartExportThread(object obj)
	{
		if (obj is object[] array && array.Length == 7)
		{
			ExplorerWindow explorerWindow = array[0] as ExplorerWindow;
			InflexDataGrid inflexDataGrid = array[1] as InflexDataGrid;
			object obj2 = array[2];
			GridView val = (GridView)((obj2 is GridView) ? obj2 : null);
			Action<int> action = array[3] as Action<int>;
			StatusPanelStopedProgressBar statusPanelStopedProgressBar = array[4] as StatusPanelStopedProgressBar;
			Action action2 = array[5] as Action;
			string text = array[6] as string;
			if (explorerWindow != null && inflexDataGrid != null && val != null && action != null && statusPanelStopedProgressBar != null && action2 != null && !string.IsNullOrEmpty(text))
			{
				SaveToXml(explorerWindow, inflexDataGrid, val, action, statusPanelStopedProgressBar, action2, text);
			}
		}
	}

	private void SaveToXml(ExplorerWindow explorerWindow, InflexDataGrid dataGrid, GridView dataGridView, Action<int> valueChanged, StatusPanelStopedProgressBar progressBar, Action endAction, string fileName)
	{
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Invalid comparison between Unknown and I4
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		IList<IGridColumn> visibleGridColumns = dataGrid.VisibleGridColumns;
		int dataRowCount = ((BaseView)dataGridView).DataRowCount;
		bool flag = ((ColumnView)dataGridView).SelectedRowsCount > 1;
		ExcelXmlWorkbook excelXmlWorkbook = new ExcelXmlWorkbook();
		string worksheetName = (string.IsNullOrEmpty(((Control)explorerWindow).Text) ? "SAUMI" : DataExportToExcelHelper.GetValidSheetName(((Control)explorerWindow).Text));
		ExcelXmlWorksheet excelXmlWorksheet = new ExcelXmlWorksheet(worksheetName, visibleGridColumns.Count);
		ExcelXmlRow excelXmlRow = new ExcelXmlRow();
		foreach (IGridColumn item in visibleGridColumns)
		{
			ExcelXmlCell excelXmlCell = new ExcelXmlCell(item.Title, typeof(string));
			excelXmlCell.Style = ExcelXmlStyle.ColumnHeaderStyle;
			excelXmlRow.Cells.Add(excelXmlCell);
		}
		excelXmlWorksheet.Rows.Add(excelXmlRow);
		for (int i = 0; i < dataRowCount; i++)
		{
			if (progressBar.ShouldStop)
			{
				break;
			}
			if (flag && !((ColumnView)dataGridView).IsRowSelected(i))
			{
				continue;
			}
			ExcelXmlRow excelXmlRow2 = new ExcelXmlRow();
			RowIndicatorStyle indicatorStyleByRowHandle = dataGrid.GetIndicatorStyleByRowHandle(i);
			foreach (IGridColumn item2 in visibleGridColumns)
			{
				object value = InflexDataTable.GetValue(dataGrid.GetGridCellValue(i, item2));
				ExcelXmlCell excelXmlCell2 = new ExcelXmlCell(value, item2.ColumnType);
				excelXmlCell2.Style = (excelXmlCell2.IsEmpty ? ExcelXmlStyle.EmptyDataStyle : ((indicatorStyleByRowHandle != null) ? excelXmlWorkbook.GetStyle(item2, indicatorStyleByRowHandle.Foreground, indicatorStyleByRowHandle.FontStyle) : excelXmlWorkbook.GetStyle(item2)));
				excelXmlRow2.Cells.Add(excelXmlCell2);
			}
			excelXmlWorksheet.Rows.Add(excelXmlRow2);
			if (i % 500 == 0)
			{
				((Control)explorerWindow).Invoke((Delegate)valueChanged, new object[1] { (i + 1) * 100 / dataRowCount });
			}
		}
		excelXmlWorkbook.Worksheets.Add(excelXmlWorksheet);
		XmlTextWriter writer;
		try
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(fileName, Encoding.Default);
			xmlTextWriter.Formatting = Formatting.Indented;
			writer = xmlTextWriter;
		}
		catch (Exception ex)
		{
			MessageBox.Show(string.Format("Невозможно выполнить сохранение.{0}{0}При попытке записи в файл \"{1}\" произошла ошибка \"{2}\".", Environment.NewLine, fileName, ex.Message), "Сохранение данных в XML-файл", (MessageBoxButtons)0, (MessageBoxIcon)16);
			((Control)explorerWindow).Invoke((Delegate)endAction);
			return;
		}
		excelXmlWorkbook.Write(writer);
		((Control)explorerWindow).Invoke((Delegate)endAction);
		if (!progressBar.ShouldStop)
		{
			DialogResult val = MessageBox.Show(string.Format("Создание файла \"{0}\" успешно завершено.{1}{1}Открыть полученный XML-файл для просмотра ассоцированным приложением?", fileName, Environment.NewLine), "Сохранение данных в XML-файл", (MessageBoxButtons)4, (MessageBoxIcon)32);
			if ((int)val == 6)
			{
				Process.Start(fileName);
			}
		}
	}
}
