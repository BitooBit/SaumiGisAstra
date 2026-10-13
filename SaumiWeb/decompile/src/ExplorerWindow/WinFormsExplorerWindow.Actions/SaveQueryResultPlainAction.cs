using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using InflexCore.Action.Attributes;
using InflexData;
using InflexDataGridComponent;
using WinFormsExplorerWindow.Roles;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(SaveQueryResultAction), GlyphUri = "SaveAsText")]
public class SaveQueryResultPlainAction : ExplorerWindowToolAction
{
	private StatusPanelStopedProgressBar _CurrentProgressBar;

	public override string Caption => "Сохранить как текст...";

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
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Invalid comparison between Unknown and I4
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
		((FileDialog)val).Filter = "Текстовый файл (*.txt)|*.txt|Текстовый файл (разделитель кавычки) (*.csv)|*.csv|Текстовый файл (разделитель табуляция) (*.tsv)|*.tsv|Все файлы (*.*)|*.*";
		((FileDialog)val).Title = "Выбор файла для сохранения";
		SaveFileDialog val2 = val;
		DialogResult val3 = ((CommonDialog)val2).ShowDialog();
		if (string.IsNullOrEmpty(((FileDialog)val2).FileName) || (int)val3 != 1)
		{
			return;
		}
		base.DirectContext.IsDataBlocked = true;
		_CurrentProgressBar = explorerWindow.ProgressBarFactory.CreateStopedProgressBar("Вы уверены, что хотите остановить сохранение данных в файл?");
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
		thread.Start(new object[8]
		{
			explorerWindow,
			dataGrid,
			dataGridView,
			action,
			progressBar,
			action2,
			((FileDialog)val2).FileName,
			((FileDialog)val2).FilterIndex
		});
	}

	private void StartExportThread(object obj)
	{
		if (obj is object[] array && array.Length == 8)
		{
			ExplorerWindow explorerWindow = array[0] as ExplorerWindow;
			InflexDataGrid inflexDataGrid = array[1] as InflexDataGrid;
			object obj2 = array[2];
			GridView val = (GridView)((obj2 is GridView) ? obj2 : null);
			Action<int> action = array[3] as Action<int>;
			StatusPanelStopedProgressBar statusPanelStopedProgressBar = array[4] as StatusPanelStopedProgressBar;
			Action action2 = array[5] as Action;
			string text = array[6] as string;
			int filterIndex = (int)array[7];
			if (explorerWindow != null && inflexDataGrid != null && val != null && action != null && statusPanelStopedProgressBar != null && action2 != null && !string.IsNullOrEmpty(text))
			{
				SaveToText(explorerWindow, inflexDataGrid, val, action, statusPanelStopedProgressBar, action2, text, filterIndex);
			}
		}
	}

	private void SaveToText(ExplorerWindow explorerWindow, InflexDataGrid dataGrid, GridView dataGridView, Action<int> valueChanged, StatusPanelStopedProgressBar progressBar, Action endAction, string fileName, int filterIndex)
	{
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Invalid comparison between Unknown and I4
		string text = "";
		string text2 = "";
		string text3 = "";
		bool flag = false;
		IList<IGridColumn> visibleGridColumns = dataGrid.VisibleGridColumns;
		bool flag2 = ((ColumnView)dataGridView).SelectedRowsCount > 1;
		int[] array = new int[visibleGridColumns.Count];
		int dataRowCount = ((BaseView)dataGridView).DataRowCount;
		switch (filterIndex)
		{
		case 2:
			text = "\"";
			text2 = "\"";
			text3 = "\";\"";
			flag = true;
			break;
		case 3:
			text = "";
			text2 = "";
			text3 = "\t";
			flag = true;
			break;
		default:
		{
			text = "";
			text2 = "";
			text3 = " ";
			flag = false;
			for (int i = 0; i < visibleGridColumns.Count; i++)
			{
				IGridColumn gridColumn = visibleGridColumns[i];
				IValueToDisplayStringConverter valueToDisplayStringConverter = (IValueToDisplayStringConverter)gridColumn;
				array[i] = gridColumn.Title.Trim().Length;
				for (int j = 0; j < dataRowCount; j++)
				{
					string text4 = valueToDisplayStringConverter.ToString(gridColumn, InflexDataTable.GetValue(dataGrid.GetGridCellValue(j, gridColumn)), useUnits: false);
					array[i] = Math.Max(array[i], text4.Trim().Length);
				}
				array[i]++;
			}
			break;
		}
		}
		using (StreamWriter streamWriter = new StreamWriter(fileName, append: false, Encoding.Default))
		{
			streamWriter.Write(text);
			for (int k = 0; k < visibleGridColumns.Count; k++)
			{
				if (!flag)
				{
					text3 = new string(' ', Math.Abs(array[k] - visibleGridColumns[k].Title.Trim().Length));
				}
				streamWriter.Write(visibleGridColumns[k].Title.Trim() + text3);
			}
			streamWriter.Write(text2 + Environment.NewLine);
			for (int l = 0; l < dataRowCount; l++)
			{
				if (progressBar.ShouldStop)
				{
					break;
				}
				if (flag2 && !((ColumnView)dataGridView).IsRowSelected(l))
				{
					continue;
				}
				streamWriter.Write(text);
				for (int m = 0; m < visibleGridColumns.Count; m++)
				{
					IGridColumn gridColumn2 = visibleGridColumns[m];
					IValueToDisplayStringConverter valueToDisplayStringConverter2 = (IValueToDisplayStringConverter)gridColumn2;
					string text5 = valueToDisplayStringConverter2.ToString(gridColumn2, InflexDataTable.GetValue(dataGrid.GetGridCellValue(l, gridColumn2)), useUnits: false).Trim();
					if (flag)
					{
						text5 = text5.Replace("\"", "\"\"");
					}
					else
					{
						text3 = new string(' ', Math.Abs(array[m] - text5.Length));
					}
					streamWriter.Write(text5 + text3);
				}
				streamWriter.Write(text2 + Environment.NewLine);
				if (l % 1000 == 0)
				{
					((Control)explorerWindow).Invoke((Delegate)valueChanged, new object[1] { (l + 1) * 100 / dataRowCount });
				}
			}
			streamWriter.Close();
		}
		((Control)explorerWindow).Invoke((Delegate)endAction);
		if (!progressBar.ShouldStop)
		{
			DialogResult val = MessageBox.Show($"Создание файла \"{fileName}\" успешно завершено.\n\nОткрыть полученный файл для просмотра ассоцированным приложением?", "Сохранение данных в файл", (MessageBoxButtons)4, (MessageBoxIcon)32);
			if ((int)val == 6)
			{
				Process.Start(fileName);
			}
		}
	}
}
