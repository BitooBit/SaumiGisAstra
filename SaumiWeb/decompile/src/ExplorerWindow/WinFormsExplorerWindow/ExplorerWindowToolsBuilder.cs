using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using ExplorerWindowBase.Action;
using InflexCore.Action;
using InflexCore.log;
using InflexUtils;
using InflexUtils.PrimitiveControls;
using WinFormsExplorerWindow.Actions;
using WinFormsExplorerWindow.PrimitiveControls;
using WinFormsExplorerWindow.Properties;

namespace WinFormsExplorerWindow;

public class ExplorerWindowToolsBuilder : InflexActionExecuter
{
	private PeriodicalRefreshMenuBuilder _PeriodicalRefreshMenuBuilder;

	protected override void OnDispose()
	{
		PeriodicalRefreshMenuBuilder periodicalRefreshMenuBuilder = _PeriodicalRefreshMenuBuilder;
		if (periodicalRefreshMenuBuilder != null)
		{
			periodicalRefreshMenuBuilder.Dispose();
			_PeriodicalRefreshMenuBuilder = null;
		}
		base.OnDispose();
	}

	public void UpdateAllowedStateTool()
	{
		base.ExecuteActionCommand.RaiseCanExecuteChanged();
	}

	public void Update(ExplorerWindow explorerWindow, IEnumerable<InflexAction> toolActions)
	{
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Expected O, but got Unknown
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Invalid comparison between Unknown and I4
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Expected O, but got Unknown
		//IL_0806: Unknown result type (might be due to invalid IL or missing references)
		//IL_0810: Expected O, but got Unknown
		((ToolStrip)explorerWindow.MainMenu).Items.Clear();
		Dictionary<InflexAction, ToolActionTypeAttribute> dictionary = new Dictionary<InflexAction, ToolActionTypeAttribute>();
		Dictionary<ToolMultiAction, ToolActionTypeAttribute> dictionary2 = new Dictionary<ToolMultiAction, ToolActionTypeAttribute>();
		Dictionary<InflexMenuItem, int[]> dictionary3 = new Dictionary<InflexMenuItem, int[]>();
		PeriodicalRefreshAction action = null;
		foreach (InflexAction toolAction in toolActions)
		{
			InflexAction inflexAction = toolAction;
			ToolActionTypeAttribute toolActionTypeAttribute = Attribute.GetCustomAttribute(inflexAction.GetType(), typeof(ToolActionTypeAttribute)) as ToolActionTypeAttribute;
			if (toolAction is PeriodicalRefreshAction)
			{
				action = toolAction as PeriodicalRefreshAction;
			}
			else if (toolActionTypeAttribute != null)
			{
				if (inflexAction is ToolMultiAction key)
				{
					dictionary2.Add(key, toolActionTypeAttribute);
				}
				dictionary.Add(inflexAction, toolActionTypeAttribute);
			}
		}
		foreach (KeyValuePair<InflexAction, ToolActionTypeAttribute> item2 in dictionary)
		{
			KeyValuePair<InflexAction, ToolActionTypeAttribute> localAction = item2;
			if (localAction.Key is LinksAction || localAction.Key is HelpAction)
			{
				continue;
			}
			InflexMenuItem inflexMenuItem = new InflexMenuItem();
			inflexMenuItem.CommandParameter = localAction.Key;
			inflexMenuItem.Command = base.ExecuteActionCommand;
			inflexMenuItem.IsHiddenWhenDisabled = localAction.Key.IsHiddenWhenDisabled;
			((ToolStripItem)inflexMenuItem).Text = localAction.Key.Caption;
			((ToolStripItem)inflexMenuItem).Name = localAction.Key.Name;
			((ToolStripItem)inflexMenuItem).Tag = (from c in dictionary2
				where (object)c.Key.GetType() == localAction.Value.MultiActionType
				select c.Key.Caption).FirstOrDefault();
			InflexMenuItem inflexMenuItem2 = inflexMenuItem;
			ToolMultiAction toolMultiAction = localAction.Key as ToolMultiAction;
			if (toolMultiAction == null)
			{
				if (!string.IsNullOrEmpty(localAction.Value.GlyphUri))
				{
					((ToolStripItem)inflexMenuItem2).ImageScaling = (ToolStripItemImageScaling)0;
					object? obj = Resources.ResourceManager.GetObject(localAction.Value.GlyphUri, Resources.Culture);
					((ToolStripItem)inflexMenuItem2).Image = (Image)((obj is Image) ? obj : null);
					if (localAction.Key is ShowFastReportFormToolAction)
					{
						((ToolStripItem)inflexMenuItem2).Image = (Image)(object)Resources.PrintFolder1;
					}
				}
				if (localAction.Key is ICheckTool checkTool)
				{
					((ToolStripMenuItem)inflexMenuItem2).CheckOnClick = true;
					inflexMenuItem2.CheckTool = checkTool;
				}
			}
			if (!string.IsNullOrEmpty(localAction.Value.HotKey))
			{
				try
				{
					((ToolStripMenuItem)inflexMenuItem2).ShortcutKeyDisplayString = localAction.Value.HotKey;
					((ToolStripMenuItem)inflexMenuItem2).ShowShortcutKeys = true;
					KeysConverter val = new KeysConverter();
					object obj2 = ((TypeConverter)(object)val).ConvertFromString(localAction.Value.HotKey);
					Keys val2 = (Keys)obj2;
					if ((int)val2 == 27)
					{
						explorerWindow.Register(val2, localAction.Key);
					}
					else
					{
						((ToolStripMenuItem)inflexMenuItem2).ShortcutKeys = val2;
					}
				}
				catch (Exception ex)
				{
					FastLog.Loger(this).Error((object)"explorerWindow.InputBindings.Add(new KeyBinding(<Command>,<KeyGesture>));", ex);
				}
			}
			dictionary3.Add(inflexMenuItem2, new int[2]
			{
				localAction.Value.GroupIndex,
				localAction.Value.Index
			});
		}
		bool flag = true;
		foreach (KeyValuePair<ToolMultiAction, ToolActionTypeAttribute> item3 in dictionary2.OrderBy((KeyValuePair<ToolMultiAction, ToolActionTypeAttribute> c) => c.Value.Index))
		{
			KeyValuePair<ToolMultiAction, ToolActionTypeAttribute> localCategory = item3;
			InflexMenuItem inflexMenuItem3 = (from mi in dictionary3
				where ((ToolStripItem)mi.Key).Name == localCategory.Key.Name
				select mi.Key).FirstOrDefault();
			if (inflexMenuItem3 == null)
			{
				continue;
			}
			bool flag2 = true;
			int num = 0;
			foreach (KeyValuePair<InflexMenuItem, int[]> item4 in from i in dictionary3
				where localCategory.Key.Caption.Equals(((ToolStripItem)i.Key).Tag)
				orderby i.Value[0], i.Value[1]
				select i)
			{
				if (((ArrangedElementCollection)((ToolStripDropDownItem)inflexMenuItem3).DropDownItems).Count > 0 && num != item4.Value[0])
				{
					((ToolStripDropDownItem)inflexMenuItem3).DropDownItems.Add((ToolStripItem)new ToolStripSeparator());
				}
				num = item4.Value[0];
				((ToolStripDropDownItem)inflexMenuItem3).DropDownItems.Add((ToolStripItem)(object)item4.Key);
				if (item4.Key.CommandParameter is NewInflexObjectToolAction)
				{
					ExtendedToolStripButton createNewControl = explorerWindow.ActionsPanel.CreateNewControl;
					createNewControl.Command = base.ExecuteActionCommand;
					createNewControl.CommandParameter = item4.Key.CommandParameter;
					createNewControl.IsHiddenWhenDisabled = item4.Key.IsHiddenWhenDisabled;
				}
				if (item4.Key.CommandParameter is StopAction)
				{
					ExtendedToolStripButton stopUpdate = explorerWindow.ActionsPanel.StopUpdate;
					stopUpdate.Command = base.ExecuteActionCommand;
					stopUpdate.CommandParameter = item4.Key.CommandParameter;
					stopUpdate.IsHiddenWhenDisabled = item4.Key.IsHiddenWhenDisabled;
				}
				if (item4.Key.CommandParameter is RefreshAction)
				{
					ExtendedToolStripSplitButton updateControl = explorerWindow.ActionsPanel.UpdateControl;
					updateControl.Command = base.ExecuteActionCommand;
					updateControl.CommandParameter = item4.Key.CommandParameter;
					updateControl.IsHiddenWhenDisabled = item4.Key.IsHiddenWhenDisabled;
				}
				if (item4.Key.CommandParameter is ShowGisAction)
				{
					ExtendedToolStripButton mapControl = explorerWindow.ActionsPanel.MapControl;
					mapControl.Command = base.ExecuteActionCommand;
					mapControl.CommandParameter = item4.Key.CommandParameter;
					mapControl.IsHiddenWhenDisabled = item4.Key.IsHiddenWhenDisabled;
				}
				if (item4.Key.CommandParameter is ShowAnaliticWindowAction)
				{
					ExtendedToolStripButton analiticWindow = explorerWindow.ActionsPanel.AnaliticWindow;
					analiticWindow.Command = base.ExecuteActionCommand;
					analiticWindow.CommandParameter = item4.Key.CommandParameter;
					analiticWindow.IsHiddenWhenDisabled = item4.Key.IsHiddenWhenDisabled;
				}
				if (item4.Key.CommandParameter is ShowFastReportFormToolAction)
				{
					ShowFastReportFormToolAction showFastReportFormToolAction = (ShowFastReportFormToolAction)item4.Key.CommandParameter;
					if (!showFastReportFormToolAction.IsOneItemForm)
					{
						((ToolStripDropDownItem)explorerWindow.ActionsPanel.PrintControl).DropDownItems.Add((ToolStripItem)(object)GetMenuItem(item4));
					}
				}
				if (item4.Key.CommandParameter is IAbstractSaveAction)
				{
					((ToolStripDropDownItem)explorerWindow.ActionsPanel.SaveControl).DropDownItems.Add((ToolStripItem)(object)GetMenuItem(item4));
				}
				if (item4.Key.CommandParameter is ShowQueryTextAction)
				{
					((ToolStripDropDownItem)explorerWindow.ActionsPanel.SqlControl).DropDownItems.Add((ToolStripItem)(object)GetMenuItem(item4));
				}
				if (item4.Key.CommandParameter is CopyQueryTextAction)
				{
					((ToolStripDropDownItem)explorerWindow.ActionsPanel.SqlControl).DropDownItems.Add((ToolStripItem)(object)GetMenuItem(item4));
				}
				if (item4.Key.CommandParameter is SelectColumnsAction)
				{
					((ToolStripDropDownItem)explorerWindow.ActionsPanel.ViewControl).DropDownItems.Add((ToolStripItem)(object)GetMenuItem(item4));
				}
				if (item4.Key.CommandParameter is SaveColumnsSettingsAction)
				{
					((ToolStripDropDownItem)explorerWindow.ActionsPanel.ViewControl).DropDownItems.Add((ToolStripItem)(object)GetMenuItem(item4));
				}
				if (item4.Key.CommandParameter is ClearColumnsSettingsAction)
				{
					((ToolStripDropDownItem)explorerWindow.ActionsPanel.ViewControl).DropDownItems.Add((ToolStripItem)(object)GetMenuItem(item4));
				}
			}
			if (inflexMenuItem3.CommandParameter is SaveQueryResultAction)
			{
				if (((ArrangedElementCollection)((ToolStripDropDownItem)explorerWindow.ActionsPanel.SaveControl).DropDownItems).Count > 0)
				{
					((ToolStripDropDownItem)explorerWindow.ActionsPanel.SaveControl).DropDownItems.Add((ToolStripItem)new ToolStripSeparator());
				}
				foreach (InflexMenuItem item5 in (ArrangedElementCollection)((ToolStripDropDownItem)inflexMenuItem3).DropDownItems)
				{
					((ToolStripDropDownItem)explorerWindow.ActionsPanel.SaveControl).DropDownItems.Add((ToolStripItem)(object)CloneMenuItem(item5));
				}
			}
			if ((object)localCategory.Value.MultiActionType == null)
			{
				((ToolStrip)explorerWindow.MainMenu).Items.Add((ToolStripItem)(object)inflexMenuItem3);
			}
			flag = flag && flag2;
		}
		CreatePeriodicalRefreshAction(explorerWindow, action);
	}

	private void CreatePeriodicalRefreshAction(ExplorerWindow explorerWindow, PeriodicalRefreshAction action)
	{
		if (_PeriodicalRefreshMenuBuilder == null)
		{
			_PeriodicalRefreshMenuBuilder = new PeriodicalRefreshMenuBuilder(action);
		}
		ToolStripMenuItem[] array = _PeriodicalRefreshMenuBuilder.CreateMenuItems();
		((ToolStripDropDownItem)explorerWindow.ActionsPanel.UpdateControl).DropDownItems.AddRange((ToolStripItem[])(object)array);
	}

	private InflexMenuItem CloneMenuItem(InflexMenuItem item)
	{
		InflexMenuItem inflexMenuItem = new InflexMenuItem();
		inflexMenuItem.CommandParameter = item.CommandParameter;
		inflexMenuItem.Command = item.Command;
		inflexMenuItem.IsHiddenWhenDisabled = item.IsHiddenWhenDisabled;
		((ToolStripItem)inflexMenuItem).Text = ((ToolStripItem)item).Text;
		((ToolStripItem)inflexMenuItem).Name = ((ToolStripItem)item).Name;
		((ToolStripItem)inflexMenuItem).Image = ((ToolStripItem)item).Image;
		((ToolStripItem)inflexMenuItem).ImageScaling = (ToolStripItemImageScaling)0;
		return inflexMenuItem;
	}

	private InflexMenuItem GetMenuItem(KeyValuePair<InflexMenuItem, int[]> item)
	{
		InflexMenuItem inflexMenuItem = new InflexMenuItem();
		inflexMenuItem.CommandParameter = item.Key.CommandParameter;
		inflexMenuItem.Command = item.Key.Command;
		inflexMenuItem.IsHiddenWhenDisabled = item.Key.IsHiddenWhenDisabled;
		((ToolStripItem)inflexMenuItem).Text = ((ToolStripItem)item.Key).Text;
		((ToolStripItem)inflexMenuItem).Name = ((ToolStripItem)item.Key).Name;
		((ToolStripItem)inflexMenuItem).Image = ((ToolStripItem)item.Key).Image;
		((ToolStripItem)inflexMenuItem).ImageScaling = (ToolStripItemImageScaling)0;
		return inflexMenuItem;
	}
}
