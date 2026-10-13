using System;
using System.Collections.Generic;
using System.Linq;
using BLToolkit.DataAccess;
using DevExpress.XtraGrid.Columns;
using InflexCore;
using InflexCore.DataImplementation.Accessors;
using InflexCore.DataImplementation.Reflection;
using InflexCore.log;

namespace WinFormsExplorerWindow.Config;

public abstract class ConfigAccessor : AppManager.InflexAccessorBase<ColumnConfig, ConfigAccessor>
{
	private string _ValueTypeDefault = "default:";

	private string _ValueTypeManual = "manual:";

	public QueryColumnCollection LoadColumns(IInflexReport report, string target, string userName, out IDictionary<string, Tuple<string, string, string, string, string>> gridColumnsSettings)
	{
		gridColumnsSettings = new Dictionary<string, Tuple<string, string, string, string, string>>();
		string text = BuildKey(report.ID, report.SubReportID, target, userName);
		string value = null;
		foreach (ColumnConfig item in from cc in SelectAll()
			orderby cc.Key.Length
			select cc)
		{
			if (text == item.Key)
			{
				value = item.Value;
				break;
			}
			if (text.StartsWith(item.Key))
			{
				value = item.Value;
			}
		}
		if (string.IsNullOrEmpty(value))
		{
			return null;
		}
		return ParseValue(report, value, out gridColumnsSettings);
	}

	public void SaveDefaultColumns(IInflexReport report, string target, string userName)
	{
		ColumnConfig columnConfig = InstanceColumnConfig(report.ID, report.SubReportID, target, userName, out var isInsert);
		columnConfig.Value = _ValueTypeDefault;
		SaveAndFlush(columnConfig, isInsert);
	}

	public void SaveColumns(IInflexReport report, string target, string userName, QueryColumnCollection collection, IList<GridColumn> realColumns)
	{
		ColumnConfig columnConfig = InstanceColumnConfig(report.ID, report.SubReportID, target, userName, out var isInsert);
		columnConfig.Value = BuildValue(collection, realColumns);
		SaveAndFlush(columnConfig, isInsert);
	}

	[InflexSqlQuery("delete from <T> where key = :key")]
	public abstract void Remove(string key);

	public void RemoveColumns(IInflexReport report, string target, string userName)
	{
		if (TryGetColumnConfig(report.ID, report.SubReportID, target, userName, out var columnConfig))
		{
			Remove(columnConfig.Key);
		}
	}

	private bool TryGetColumnConfig(decimal reportID, decimal subReportID, string target, string userName, out ColumnConfig columnConfig)
	{
		string text = BuildKey(reportID, subReportID, target, userName);
		columnConfig = SelectByKey(text);
		return columnConfig != null;
	}

	private ColumnConfig InstanceColumnConfig(decimal reportID, decimal subReportID, string target, string userName, out bool isInsert)
	{
		string text = BuildKey(reportID, subReportID, target, userName);
		ColumnConfig columnConfig = SelectByKey(text);
		isInsert = false;
		if (columnConfig == null)
		{
			isInsert = true;
			ColumnConfig columnConfig2 = new ColumnConfig();
			columnConfig2.Key = text;
			columnConfig = columnConfig2;
		}
		return columnConfig;
	}

	private void SaveAndFlush(ColumnConfig columnConfig, bool isInsert)
	{
		UseTransaction();
		try
		{
			if (isInsert)
			{
				new SqlQuery<ColumnConfig>(GetDbManager(), dispose: true).Insert(columnConfig);
			}
			else
			{
				new SqlQuery<ColumnConfig>(GetDbManager(), dispose: true).Update(columnConfig);
			}
			CommitTransaction();
		}
		catch (Exception)
		{
			RollbackTransaction();
		}
	}

	private string BuildValue(QueryColumnCollection collection, IList<GridColumn> realColumns)
	{
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Invalid comparison between Unknown and I4
		string text = _ValueTypeManual;
		QueryColumn column;
		foreach (QueryColumn item in collection)
		{
			column = item;
			text += string.Format("{0}-{1}-{2}-{3}", new object[4]
			{
				column.ExplorerColumn.Report.ID,
				column.ExplorerColumn.ID,
				(column.ForeignColumn == null) ? "" : collection.IndexOf(column.ForeignColumn).ToString(),
				column.Visible ? "1" : "0"
			});
			GridColumn val = realColumns.FirstOrDefault((GridColumn col) => col.Caption == column.ExplorerColumn.ColumnName);
			if (val != null)
			{
				text += string.Format("-{0}-{1}-{2}-{3}-{4}", new object[5]
				{
					(val.GroupIndex == -1) ? string.Empty : val.GroupIndex.ToString(),
					(val.SortIndex == -1) ? string.Empty : val.SortIndex.ToString(),
					((int)val.SortOrder == 2) ? "1" : "0",
					(val.VisibleIndex == -1) ? string.Empty : val.VisibleIndex.ToString(),
					val.Width
				});
			}
			text += ';';
		}
		return text;
	}

	private QueryColumnCollection ParseValue(IInflexReport report, string value, out IDictionary<string, Tuple<string, string, string, string, string>> gridColumnsSettings)
	{
		gridColumnsSettings = new Dictionary<string, Tuple<string, string, string, string, string>>();
		try
		{
			value = value ?? string.Empty;
			if (value.ToLower().StartsWith(_ValueTypeManual.ToLower()))
			{
				value = value.Remove(0, _ValueTypeManual.Length);
				InflexColumnAccessor inflexColumnAccessor = base.AppManager.Accessor<InflexColumnAccessor>();
				QueryColumnCollection queryColumnCollection = new QueryColumnCollection();
				bool flag = false;
				string[] array = value.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string text in array)
				{
					string[] array2 = text.Split(new char[1] { '-' });
					string s = array2[0];
					string s2 = array2[1];
					string s3 = array2[2];
					string text2 = array2[3];
					IInflexColumn inflexColumn = inflexColumnAccessor.SelectByKey(decimal.Parse(s2), decimal.Parse(s));
					if (array2.Length == 9)
					{
						string item = array2[4];
						string item2 = array2[5];
						string item3 = array2[6];
						string item4 = array2[7];
						string item5 = array2[8];
						Tuple<string, string, string, string, string> value2 = new Tuple<string, string, string, string, string>(item, item2, item3, item4, item5);
						gridColumnsSettings.Add(inflexColumn.ColumnName, value2);
					}
					flag = flag || inflexColumn == null;
					QueryColumn queryColumn = new QueryColumn(inflexColumn, text2 != "0");
					int result = 0;
					if (int.TryParse(s3, out result))
					{
						queryColumn.ForeignColumn = queryColumnCollection[result];
					}
					queryColumnCollection.Add(queryColumn);
				}
				if (flag)
				{
					foreach (QueryColumn item6 in queryColumnCollection)
					{
						if (item6.ForeignColumn != null)
						{
							item6.Visible = false;
							item6.ForeignColumn.Visible = false;
						}
					}
					QueryColumnCollection queryColumnCollection2 = new QueryColumnCollection();
					foreach (QueryColumn item7 in queryColumnCollection)
					{
						if (item7.ExplorerColumn != null && item7.Visible)
						{
							queryColumnCollection2.Add(item7);
						}
					}
					queryColumnCollection = queryColumnCollection2;
				}
				if (queryColumnCollection.Count != 0)
				{
					return queryColumnCollection;
				}
				value = _ValueTypeDefault;
			}
			if (value.ToLower().StartsWith(_ValueTypeDefault.ToLower()))
			{
				QueryColumnCollection queryColumnCollection3 = new QueryColumnCollection();
				foreach (IInflexColumn item8 in from c in report.GetExplorerColumns()
					where c.ShowColumn && c.CanShow() && !c.IsRestriction()
					select c)
				{
					queryColumnCollection3.Add(new QueryColumn(item8, visible: true));
				}
				return queryColumnCollection3;
			}
			return null;
		}
		catch (Exception ex)
		{
			FastLog.Loger(this).Error((object)"QueryColumnCollection ParseValue(string value)", ex);
			return null;
		}
	}

	private string BuildKey(decimal reportID, decimal subReportID, string target, string userName)
	{
		return string.Format("ReportID={0};SubReportID={1};Target={2};UserName={3};", new object[4]
		{
			reportID.ToString(),
			subReportID.ToString(),
			target,
			userName
		}).ToUpper();
	}
}
