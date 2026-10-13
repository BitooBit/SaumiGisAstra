using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using InflexCore.Action.Attributes;
using InflexData;
using InflexDataGridComponent;
using InflexUtils.ExceptionBox;
using WinFormsExplorerWindow.Roles;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(SaveQueryResultAction), GlyphUri = "SaveToXLS")]
public class SaveQueryResultExcelAction : ExplorerWindowToolAction
{
	private class ExcelRow
	{
		private Color _RowColor;

		[CompilerGenerated]
		private FontStyle _003CRowStyle_003Ek__BackingField;

		public object[] Values { get; set; }

		public GroupExcelRow ParentRow { get; set; }

		public int Level
		{
			get
			{
				if (ParentRow != null)
				{
					return ParentRow.Level + 1;
				}
				return 0;
			}
		}

		public int RowColorIndex { get; private set; }

		public Color RowColor
		{
			get
			{
				return _RowColor;
			}
			set
			{
				_RowColor = value;
				RowColorIndex = ColorTranslator.ToOle(value);
			}
		}

		public FontStyle RowStyle
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _003CRowStyle_003Ek__BackingField;
			}
			[CompilerGenerated]
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_003CRowStyle_003Ek__BackingField = value;
			}
		}

		public int InsertedIndex { get; set; }
	}

	private class GroupExcelRow : ExcelRow
	{
		public object GroupRowValue { get; set; }

		public string GroupRowTitle { get; set; }

		public List<ExcelRow> Childs { get; private set; }

		public GroupExcelRow()
		{
			Childs = new List<ExcelRow>();
		}
	}

	private class ExcelRangeStyle
	{
		public int FirstRowIndex { get; set; }

		public int LastRowIndex { get; set; }

		public int ColorIndex { get; set; }

		public bool Bold { get; set; }

		public bool Italic { get; set; }

		public bool Underline { get; set; }

		public bool Strikeout { get; set; }
	}

	private const int ROWS_COUNT_EXCEL_EXPORT_LIMIT = 65535;

	private const int EXCEL_DECIMAL_SUPPORTING_VERSION = 10;

	private StatusPanelStopedProgressBar _CurrentProgressBar;

	public override string Caption => "Передать в Excel";

	protected override bool? DirectIsAllowed()
	{
		InflexDataTable dataTable = base.DirectContext.DataView.DataTable;
		InflexDataGrid dataGrid = base.DirectContext.ExplorerWindow.DataGrid;
		GridView dataGridView = base.DirectContext.ExplorerWindow.DataGridView;
		return _CurrentProgressBar == null && dataTable != null && dataGrid != null && dataGridView != null && ((BaseView)dataGridView).DataRowCount > 0 && (dataGrid.VisibleGridColumns.Count > 0 || ((ColumnView)dataGridView).GroupCount > 0) && base.DirectContext.AppManager.RoleEnabled<ExcelExportRole>();
	}

	protected override void ProtectedDirectExecute()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Invalid comparison between Unknown and I4
		ExplorerWindow explorerWindow = base.DirectContext.ExplorerWindow;
		InflexDataGrid dataGrid = explorerWindow.DataGrid;
		GridView dataGridView = explorerWindow.DataGridView;
		if (dataGrid == null || dataGridView == null)
		{
			return;
		}
		IList<IGridColumn> visibleGridColumns = dataGrid.VisibleGridColumns;
		int dataRowCount = ((BaseView)dataGridView).DataRowCount;
		int count = visibleGridColumns.Count;
		int groupCount = ((ColumnView)dataGridView).GroupCount;
		if (dataRowCount == 0 || count + groupCount == 0)
		{
			return;
		}
		if (dataRowCount > 65535)
		{
			DialogResult val = MessageBox.Show("Количество экспортируемых данных превышает " + 65535 + " строк.\nВозможны проблемы при передаче данных в некоторые версии Microsoft Excel.\nВы уверены, что хотите продолжить?", "Внимание!", (MessageBoxButtons)4, (MessageBoxIcon)48);
			if ((int)val != 6)
			{
				return;
			}
		}
		explorerWindow.UpdateStatusText("Передача данных в Excel");
		base.DirectContext.IsDataBlocked = true;
		_CurrentProgressBar = explorerWindow.ProgressBarFactory.CreateStopedProgressBar("Вы уверены, что хотите остановить передачу данных в Excel?");
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
		Action<object> action3 = ExceptionBox.Show;
		Thread thread = new Thread(StartExportThread);
		thread.IsBackground = true;
		thread.Start(new object[7] { explorerWindow, dataGrid, dataGridView, action, progressBar, action2, action3 });
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
			Action<object> action3 = array[6] as Action<object>;
			if (explorerWindow != null && inflexDataGrid != null && val != null && action != null && statusPanelStopedProgressBar != null && action2 != null && action3 != null)
			{
				SaveDataToExcel(explorerWindow, inflexDataGrid, val, action, statusPanelStopedProgressBar, action2, action3);
			}
		}
	}

	private void SaveDataToExcel(ExplorerWindow explorerWindow, InflexDataGrid dataGrid, GridView dataGridView, Action<int> valueChanged, StatusPanelStopedProgressBar progressBar, Action endAction, Action<object> showExceptionAction)
	{
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		IList<IGridColumn> visibleGridColumns = dataGrid.VisibleGridColumns;
		int dataRowCount = ((BaseView)dataGridView).DataRowCount;
		int count = visibleGridColumns.Count;
		int groupCount = ((ColumnView)dataGridView).GroupCount;
		int columnSpan = ((count > groupCount) ? count : groupCount);
		int maxDataRow = 1000 / ((count <= 0) ? 1 : count);
		bool flag = ((ColumnView)dataGridView).SelectedRowsCount > 1;
		object obj = null;
		object obj2 = null;
		Type type = null;
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		try
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			obj = Activator.CreateInstance(Type.GetTypeFromProgID("Excel.Application"));
			Type type2 = obj.GetType();
			object obj3 = type2.InvokeMember("Version", BindingFlags.GetProperty, null, obj, null);
			string version = ((obj3 is string) ? ((string)obj3) : string.Empty);
			int version2 = ExtractVersionNumber(version);
			object obj4 = type2.InvokeMember("Workbooks", BindingFlags.GetProperty, null, obj, null);
			obj2 = obj4.GetType().InvokeMember("Add", BindingFlags.InvokeMethod, null, obj4, null);
			type = obj2.GetType();
			object obj5 = type.InvokeMember("Worksheets", BindingFlags.GetProperty, null, obj2, null);
			object obj6 = obj5.GetType().InvokeMember("Item", BindingFlags.GetProperty, null, obj5, new object[1] { 1 });
			Type type3 = obj6.GetType();
			string text = (string.IsNullOrEmpty(((Control)explorerWindow).Text) ? "SAUMI" : DataExportToExcelHelper.GetValidSheetName(((Control)explorerWindow).Text));
			type3.InvokeMember("Name", BindingFlags.SetProperty, null, obj6, new object[1] { text });
			Type excelCellsType = null;
			Type excelRangeType = null;
			Type cellFontType = null;
			SetColumnsHeaders(dataGrid, obj6, type3, count, ref excelCellsType, ref cellFontType);
			IValueToDisplayStringConverter[] array = new IValueToDisplayStringConverter[groupCount];
			IGridColumn[] array2 = new IGridColumn[groupCount];
			IList<IGridColumn> groupedGridColumns = dataGrid.GroupedGridColumns;
			for (int i = 0; i < groupCount; i++)
			{
				array2[i] = groupedGridColumns[i];
				array[i] = (IValueToDisplayStringConverter)array2[i];
			}
			List<ExcelRow> list = new List<ExcelRow>();
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			for (int j = 0; j < dataRowCount; j++)
			{
				if (progressBar.ShouldStop)
				{
					break;
				}
				bool flag2 = true;
				if (flag)
				{
					flag2 = ((ColumnView)dataGridView).IsRowSelected(j);
				}
				if (flag2)
				{
					object[] array3 = new object[count + ((ColumnView)dataGridView).GroupCount];
					for (int k = 0; k < count; k++)
					{
						IGridColumn gridColumn = visibleGridColumns[k];
						object value = InflexDataTable.GetValue(dataGrid.GetGridCellValue(j, gridColumn));
						if (value != null && value != DBNull.Value)
						{
							array3[k] = CastValueForVersionSupport(value, version2);
						}
					}
					for (int l = 0; l < groupCount; l++)
					{
						IGridColumn gridColumn2 = groupedGridColumns[l];
						object obj7 = array[l].ToString(array2[l], InflexDataTable.GetValue(dataGrid.GetGridCellValue(j, gridColumn2)), useUnits: true);
						if (obj7 != null && obj7 != DBNull.Value)
						{
							int num = l + count;
							array3[num] = obj7;
							if (!dictionary.ContainsKey(num))
							{
								dictionary.Add(num, gridColumn2.Title);
							}
						}
					}
					ExcelRow excelRow = new ExcelRow();
					excelRow.Values = array3;
					ExcelRow excelRow2 = excelRow;
					RowIndicatorStyle indicatorStyleByRowHandle = dataGrid.GetIndicatorStyleByRowHandle(j);
					if (indicatorStyleByRowHandle != null)
					{
						excelRow2.RowColor = indicatorStyleByRowHandle.Foreground;
						excelRow2.RowStyle = indicatorStyleByRowHandle.FontStyle;
					}
					list.Add(excelRow2);
				}
				((Control)explorerWindow).Invoke((Delegate)valueChanged, new object[1] { j * 50 / dataRowCount });
			}
			IEnumerable<ExcelRow> enumerable = list;
			foreach (KeyValuePair<int, string> item in dictionary)
			{
				enumerable = GroupBy(item.Key, enumerable, item.Value);
			}
			IList<ExcelRow> list2 = ((dictionary.Count > 0) ? ExpandAllGroupingRows(enumerable) : enumerable.ToList());
			int currentExcelRow = 2;
			int[] array4 = new int[groupCount];
			if (dictionary.Count == 0)
			{
				int num2 = currentExcelRow;
				SetStyles(dataGrid, obj6, type3, count, num2, list2.Count, ref excelCellsType, ref excelRangeType, ref cellFontType);
				InsertRows(list2, maxDataRow, count, type3, obj6, ref excelRangeType, ref currentExcelRow, progressBar, valueChanged, explorerWindow);
				SetRowIndicatorStyles(list2, count, type3, obj6, ref excelRangeType, num2);
			}
			else
			{
				for (int m = 0; m < array4.Length; m++)
				{
					array4[m] = -1;
				}
				for (int n = 0; n < list2.Count; n++)
				{
					if (list2[n] is GroupExcelRow groupExcelRow)
					{
						if (array4[groupExcelRow.Level] < 0)
						{
							array4[groupExcelRow.Level] = currentExcelRow + 1;
						}
						else
						{
							for (int num3 = array4.Length - 1; num3 >= groupExcelRow.Level; num3--)
							{
								int num4 = array4[num3];
								if (num4 >= 0)
								{
									int startRow = num4;
									int stopRow = currentExcelRow;
									SetGroup(startRow, stopRow, type3, obj6);
									currentExcelRow++;
									if (num3 == groupExcelRow.Level)
									{
										array4[num3] = currentExcelRow + 1;
									}
									else
									{
										array4[num3] = -1;
									}
								}
							}
						}
						SetGroupHeaderValue(currentExcelRow, groupExcelRow.Level, columnSpan, $"{groupExcelRow.GroupRowTitle}: {groupExcelRow.GroupRowValue}", type3, obj6, ref excelRangeType);
						currentExcelRow++;
						if (IsLastGroupRow(groupExcelRow))
						{
							int num5 = currentExcelRow;
							SetStyles(dataGrid, obj6, type3, count, num5, num5 + groupExcelRow.Childs.Count, ref excelCellsType, ref excelRangeType, ref cellFontType);
							InsertRows(groupExcelRow.Childs, maxDataRow, count, type3, obj6, ref excelRangeType, ref currentExcelRow, null, null, explorerWindow);
							SetRowIndicatorStyles(groupExcelRow.Childs, count, type3, obj6, ref excelRangeType, num5);
						}
						if (n == list2.Count - 1)
						{
							for (int num6 = array4.Length - 1; num6 >= 0; num6--)
							{
								int num7 = array4[num6];
								if (num7 >= 0)
								{
									int startRow2 = num7;
									int stopRow2 = currentExcelRow;
									SetGroup(startRow2, stopRow2, type3, obj6);
									currentExcelRow++;
								}
							}
						}
					}
					if (progressBar.ShouldStop)
					{
						break;
					}
					((Control)explorerWindow).Invoke((Delegate)valueChanged, new object[1] { 50 + n * 50 / list2.Count });
				}
			}
			((Control)explorerWindow).Invoke((Delegate)new Action(progressBar.HideItems));
			((Control)explorerWindow).Invoke((Delegate)new Action<string>(explorerWindow.UpdateStatusText), new object[1] { progressBar.ShouldStop ? "Передача данных в Excel была остановлена" : "Передача данных в Excel успешно завершена" });
			if (progressBar.ShouldStop)
			{
				CloseExcel(obj2, type);
				return;
			}
			object obj8 = type3.InvokeMember("Columns", BindingFlags.GetProperty, null, obj6, null);
			obj8.GetType().InvokeMember("Autofit", BindingFlags.InvokeMethod, null, obj8, null);
			type2.InvokeMember("Visible", BindingFlags.SetProperty, null, obj, new object[1] { true });
		}
		catch (Exception ex)
		{
			if (showExceptionAction != null && explorerWindow != null)
			{
				((Control)explorerWindow).Invoke((Delegate)showExceptionAction, new object[1] { ex });
			}
			if (obj != null && obj2 != null)
			{
				CloseExcel(obj2, type);
			}
		}
		finally
		{
			if (currentCulture != null)
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
			if (obj != null)
			{
				Marshal.ReleaseComObject(obj);
			}
			GC.GetTotalMemory(forceFullCollection: true);
			((Control)explorerWindow).Invoke((Delegate)endAction);
		}
	}

	private object CastValueForVersionSupport(object value, int version)
	{
		if (version < 10 && value is decimal)
		{
			return Convert.ToDouble(value);
		}
		return value;
	}

	private int ExtractVersionNumber(string version)
	{
		if (string.IsNullOrEmpty(version))
		{
			return -1;
		}
		string[] array = Regex.Split(version, "[^\\d]");
		if (array.Length > 0)
		{
			string s = array[0];
			int result = -1;
			if (int.TryParse(s, out result))
			{
				return result;
			}
		}
		return -1;
	}

	private void CloseExcel(object excelWorkbook, Type excelWorkbookType)
	{
		excelWorkbookType.InvokeMember("Close", BindingFlags.InvokeMethod, null, excelWorkbook, new object[1] { false });
	}

	private void InsertRows(IList<ExcelRow> rows, int maxDataRow, int columnsCount, Type excelWorksheetType, object excelWorksheet, ref Type excelRangeType, ref int currentExcelRow, StatusPanelStopedProgressBar progressBar, Action<int> valueChanged, ExplorerWindow window)
	{
		List<object[]> list = new List<object[]>();
		List<ExcelRow> list2 = new List<ExcelRow>();
		int count = rows.Count;
		for (int i = 0; i < count; i++)
		{
			if (progressBar != null && valueChanged != null)
			{
				if (progressBar.ShouldStop)
				{
					break;
				}
				((Control)window).Invoke((Delegate)valueChanged, new object[1] { 50 + i * 50 / count });
			}
			list.Add(rows[i].Values);
			list2.Add(rows[i]);
			if (list.Count > 0 && (list.Count >= maxDataRow || i == count - 1))
			{
				SetRows(currentExcelRow, list, columnsCount, excelWorksheetType, excelWorksheet, ref excelRangeType);
				for (int j = 0; j < list2.Count; j++)
				{
					list2[j].InsertedIndex = currentExcelRow + j;
				}
				currentExcelRow += list.Count;
				list.Clear();
				list2.Clear();
			}
		}
	}

	private void SetRowIndicatorStyles(IList<ExcelRow> rows, int columnsCount, Type excelWorksheetType, object excelWorksheet, ref Type excelRangeType, int currentExcelRow)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Invalid comparison between Unknown and I4
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Invalid comparison between Unknown and I4
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Invalid comparison between Unknown and I4
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Invalid comparison between Unknown and I4
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Invalid comparison between Unknown and I4
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Invalid comparison between Unknown and I4
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		List<ExcelRangeStyle> list = new List<ExcelRangeStyle>();
		int num = 0;
		for (int i = 0; i < rows.Count; i++)
		{
			int rowColorIndex = rows[i].RowColorIndex;
			FontStyle rowStyle = rows[i].RowStyle;
			if (i == rows.Count - 1)
			{
				ExcelRangeStyle excelRangeStyle = new ExcelRangeStyle();
				excelRangeStyle.FirstRowIndex = num + currentExcelRow;
				excelRangeStyle.LastRowIndex = i + currentExcelRow;
				excelRangeStyle.ColorIndex = rowColorIndex;
				excelRangeStyle.Bold = (rowStyle & 1) == 1;
				excelRangeStyle.Italic = (rowStyle & 2) == 2;
				excelRangeStyle.Strikeout = (rowStyle & 8) == 8;
				excelRangeStyle.Underline = (rowStyle & 4) == 4;
				ExcelRangeStyle item = excelRangeStyle;
				list.Add(item);
				break;
			}
			int num2 = i + 1;
			int rowColorIndex2 = rows[num2].RowColorIndex;
			FontStyle rowStyle2 = rows[num2].RowStyle;
			if (rowColorIndex != rowColorIndex2 || rowStyle != rowStyle2)
			{
				ExcelRangeStyle excelRangeStyle2 = new ExcelRangeStyle();
				excelRangeStyle2.FirstRowIndex = num + currentExcelRow;
				excelRangeStyle2.LastRowIndex = i + currentExcelRow;
				excelRangeStyle2.ColorIndex = rowColorIndex;
				excelRangeStyle2.Bold = (rowStyle & 1) == 1;
				excelRangeStyle2.Italic = (rowStyle & 2) == 2;
				excelRangeStyle2.Strikeout = (rowStyle & 8) == 8;
				excelRangeStyle2.Underline = (rowStyle & 4) == 4;
				ExcelRangeStyle item2 = excelRangeStyle2;
				list.Add(item2);
				num = num2;
			}
		}
		foreach (ExcelRangeStyle item3 in list)
		{
			object obj = excelWorksheetType.InvokeMember("Cells", BindingFlags.GetProperty, null, excelWorksheet, new object[2] { item3.FirstRowIndex, 1 });
			object obj2 = excelWorksheetType.InvokeMember("Cells", BindingFlags.GetProperty, null, excelWorksheet, new object[2]
			{
				item3.LastRowIndex,
				columnsCount + 1
			});
			object target = excelWorksheetType.InvokeMember("Range", BindingFlags.GetProperty, null, excelWorksheet, new object[2] { obj, obj2 });
			object obj3 = excelRangeType.InvokeMember("Font", BindingFlags.GetProperty, null, target, null);
			Type type = obj3.GetType();
			type.InvokeMember("Color", BindingFlags.SetProperty, null, obj3, new object[1] { item3.ColorIndex });
			type.InvokeMember("Bold", BindingFlags.SetProperty, null, obj3, new object[1] { item3.Bold });
			type.InvokeMember("Italic", BindingFlags.SetProperty, null, obj3, new object[1] { item3.Italic });
			type.InvokeMember("Strikethrough", BindingFlags.SetProperty, null, obj3, new object[1] { item3.Strikeout });
			type.InvokeMember("Underline", BindingFlags.SetProperty, null, obj3, new object[1] { item3.Underline });
		}
	}

	private void SetGroup(int startRow, int stopRow, Type excelWorksheetType, object excelWorksheet)
	{
		if (startRow <= stopRow)
		{
			object obj = excelWorksheetType.InvokeMember("Rows", BindingFlags.GetProperty, null, excelWorksheet, new object[1] { $"{startRow}:{stopRow - 1}" });
			obj.GetType().InvokeMember("Group", BindingFlags.InvokeMethod, null, obj, new object[0]);
		}
	}

	private void SetGroupHeaderValue(int currExcelRow, int groupIndex, int columnSpan, string header, Type excelWorksheetType, object excelWorksheet, ref Type excelRangeType)
	{
		object obj = excelWorksheetType.InvokeMember("Cells", BindingFlags.GetProperty, null, excelWorksheet, new object[2]
		{
			currExcelRow,
			groupIndex + 1
		});
		object obj2 = excelWorksheetType.InvokeMember("Cells", BindingFlags.GetProperty, null, excelWorksheet, new object[2] { currExcelRow, columnSpan });
		object obj3 = excelWorksheetType.InvokeMember("Range", BindingFlags.GetProperty, null, excelWorksheet, new object[2] { obj, obj2 });
		if ((object)excelRangeType == null)
		{
			excelRangeType = obj3.GetType();
		}
		excelRangeType.InvokeMember("MergeCells", BindingFlags.SetProperty, null, obj3, new object[1] { true });
		excelRangeType.InvokeMember("Value", BindingFlags.SetProperty, null, obj3, new object[1] { header });
		object obj4 = excelRangeType.InvokeMember("Font", BindingFlags.GetProperty, null, obj3, null);
		int num = 16 - groupIndex * 2;
		Type type = obj4.GetType();
		type.InvokeMember("Size", BindingFlags.SetProperty, null, obj4, new object[1] { (num < 10) ? 10 : num });
		type.InvokeMember("Italic", BindingFlags.SetProperty, null, obj4, new object[1] { true });
	}

	private void SetRows(int startRow, List<object[]> rowDataList, int columnsCount, Type excelWorksheetType, object excelWorksheet, ref Type excelRangeType)
	{
		if (columnsCount == 0)
		{
			return;
		}
		object[,] array = new object[rowDataList.Count, columnsCount];
		for (int i = 0; i < rowDataList.Count; i++)
		{
			for (int j = 0; j < columnsCount; j++)
			{
				array[i, j] = rowDataList[i][j];
			}
		}
		object obj = excelWorksheetType.InvokeMember("Range", BindingFlags.GetProperty, null, excelWorksheet, new object[2]
		{
			excelWorksheetType.InvokeMember("Cells", BindingFlags.GetProperty, null, excelWorksheet, new object[2] { startRow, 1 }),
			excelWorksheetType.InvokeMember("Cells", BindingFlags.GetProperty, null, excelWorksheet, new object[2]
			{
				startRow + rowDataList.Count - 1,
				columnsCount
			})
		});
		if ((object)excelRangeType == null)
		{
			excelRangeType = obj.GetType();
		}
		excelRangeType.InvokeMember("Value2", BindingFlags.SetProperty, null, obj, new object[1] { array });
	}

	private bool IsLastGroupRow(GroupExcelRow groupRow)
	{
		if (groupRow.Childs.Count > 0)
		{
			return !(groupRow.Childs[0] is GroupExcelRow);
		}
		return false;
	}

	private IList<ExcelRow> ExpandAllGroupingRows(IEnumerable<ExcelRow> groups)
	{
		List<ExcelRow> list = new List<ExcelRow>();
		foreach (ExcelRow group in groups)
		{
			if (group is GroupExcelRow groupExcelRow)
			{
				list.Add(groupExcelRow);
				list.AddRange(ExpandAllGroupingRows(groupExcelRow.Childs));
			}
		}
		return list;
	}

	private IEnumerable<ExcelRow> GroupBy(int groupIndex, IEnumerable<ExcelRow> rows, string title)
	{
		Dictionary<object, IList<ExcelRow>> dictionary = new Dictionary<object, IList<ExcelRow>>();
		List<ExcelRow> list = new List<ExcelRow>();
		if (rows.FirstOrDefault() is GroupExcelRow)
		{
			foreach (ExcelRow row in rows)
			{
				GroupExcelRow groupExcelRow = (GroupExcelRow)row;
				IEnumerable<ExcelRow> enumerable = GroupBy(groupIndex, groupExcelRow.Childs, title);
				foreach (ExcelRow item in enumerable)
				{
					item.ParentRow = groupExcelRow;
				}
				groupExcelRow.Childs.Clear();
				groupExcelRow.Childs.AddRange(enumerable);
			}
			list.AddRange(rows);
		}
		else
		{
			foreach (ExcelRow row2 in rows)
			{
				object key = row2.Values[groupIndex];
				IList<ExcelRow> value = null;
				if (!dictionary.TryGetValue(key, out value))
				{
					value = new List<ExcelRow>();
					dictionary.Add(key, value);
				}
				value?.Add(row2);
			}
			foreach (KeyValuePair<object, IList<ExcelRow>> item2 in dictionary)
			{
				GroupExcelRow groupExcelRow2 = new GroupExcelRow();
				groupExcelRow2.GroupRowValue = item2.Key;
				groupExcelRow2.GroupRowTitle = title;
				groupExcelRow2.Childs.AddRange(item2.Value);
				list.Add(groupExcelRow2);
			}
		}
		return list;
	}

	private void SetColumnsHeaders(InflexDataGrid dataGrid, object excelWorksheet, Type excelWorksheetType, int visibleColumnsCount, ref Type excelCellsType, ref Type cellFontType)
	{
		for (int i = 0; i < visibleColumnsCount; i++)
		{
			object obj = excelWorksheetType.InvokeMember("Cells", BindingFlags.GetProperty, null, excelWorksheet, new object[2]
			{
				1,
				i + 1
			});
			IGridColumn gridColumn = dataGrid.VisibleGridColumns[i];
			if ((object)excelCellsType == null)
			{
				excelCellsType = obj.GetType();
			}
			excelCellsType.InvokeMember("Value", BindingFlags.SetProperty, null, obj, new object[1] { gridColumn.Title });
			object obj2 = excelCellsType.InvokeMember("Font", BindingFlags.GetProperty, null, obj, null);
			if ((object)cellFontType == null)
			{
				cellFontType = obj2.GetType();
			}
			cellFontType.InvokeMember("Bold", BindingFlags.SetProperty, null, obj2, new object[1] { true });
		}
	}

	private void SetStyles(InflexDataGrid dataGrid, object excelWorksheet, Type excelWorksheetType, int visibleColumnsCount, int startRowIndex, int rowsCount, ref Type excelCellsType, ref Type excelRangeType, ref Type cellFontType)
	{
		for (int i = 0; i < visibleColumnsCount; i++)
		{
			IGridColumn column = dataGrid.VisibleGridColumns[i];
			object obj = excelWorksheetType.InvokeMember("Range", BindingFlags.GetProperty, null, excelWorksheet, new object[2]
			{
				excelWorksheetType.InvokeMember("Cells", BindingFlags.GetProperty, null, excelWorksheet, new object[2]
				{
					startRowIndex,
					i + 1
				}),
				excelWorksheetType.InvokeMember("Cells", BindingFlags.GetProperty, null, excelWorksheet, new object[2]
				{
					rowsCount + 1,
					i + 1
				})
			});
			if ((object)excelRangeType == null)
			{
				excelRangeType = obj.GetType();
			}
			excelRangeType.InvokeMember("NumberFormat", BindingFlags.SetProperty, null, obj, new object[1] { GetExcelNumberFormat(column) });
			Application.DoEvents();
		}
	}

	private string GetExcelNumberFormat(IGridColumn column)
	{
		if ((object)column.ColumnType == typeof(DateTime))
		{
			string text = string.Format("DD{0}MM{0}YYYY", CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator);
			if (!column.IsDate)
			{
				text += string.Format(" H{0}mm{0}ss", CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator);
			}
			return text;
		}
		if (column.TotalSummMode != TotalSummModeEnum.None && (object)column.ColumnType == typeof(decimal))
		{
			string text2 = "# ##0";
			int num = ((column.Accuracy >= 0) ? column.Accuracy : 0);
			if (num > 0)
			{
				text2 += CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
				text2 = text2.PadRight(text2.Length + num, '0');
			}
			return text2;
		}
		if ((object)column.ColumnType == typeof(decimal))
		{
			return "0";
		}
		return "@";
	}
}
