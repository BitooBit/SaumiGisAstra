using System;
using System.Linq;
using InflexCore;
using InflexCore.Action.Attributes;
using InflexData;
using InflexData.RestrictionQuery;
using WinFormsExplorerWindow.PrimitiveControls;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 100, Index = 300, GlyphUri = "SelectColumns")]
public class SelectColumnsAction : ExplorerWindowToolAction
{
	public override string Caption => "Выбрать колонки";

	protected override bool? DirectIsAllowed()
	{
		InflexDataView dataView = base.DirectContext.DataView;
		return dataView.DataQuery is RestrictionDataQuery && !dataView.InExecute();
	}

	protected override void ProtectedDirectExecute()
	{
		if (!(base.DirectContext.DataView.DataQuery is RestrictionDataQuery restrictionDataQuery))
		{
			return;
		}
		InflexReportTreeWindow inflexReportTreeWindow = new InflexReportTreeWindow();
		try
		{
			bool? flag = inflexReportTreeWindow.ShowWindow(restrictionDataQuery.Report, restrictionDataQuery.QueryColumns);
			if (!flag.HasValue || !flag.Value)
			{
				return;
			}
			QueryColumnCollection queryColumnCollection = new QueryColumnCollection();
			foreach (QueryColumn item in from c in inflexReportTreeWindow.ResultQueryColumnCollection
				where c.ForeignColumn == null
				orderby c.ExplorerColumn.SeqN ?? decimal.MaxValue
				select c)
			{
				queryColumnCollection.Add(item);
			}
			foreach (QueryColumn item2 in inflexReportTreeWindow.ResultQueryColumnCollection.Where((QueryColumn c) => c.ForeignColumn != null))
			{
				queryColumnCollection.Add(item2);
			}
			QueryColumnCollectionProvider queryColumnProvider = base.DirectContext.ExplorerWindow.QueryColumnProvider;
			queryColumnProvider.SetQueryColumns(restrictionDataQuery.Report, queryColumnCollection);
			if (inflexReportTreeWindow.RememberAsDefault)
			{
				queryColumnProvider.SaveQueryColumns(restrictionDataQuery.Report, queryColumnCollection);
			}
			base.DirectContext.DataView.UpdateDataQuery(executeQuery: true, background: true);
		}
		finally
		{
			((IDisposable)inflexReportTreeWindow)?.Dispose();
		}
	}
}
