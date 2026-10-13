using System;
using System.Collections.Generic;
using System.Linq;
using InflexCore.Action.Attributes;

namespace InflexCore.Action.ActionMenu;

public class InflexActionMenu
{
	private class FullMenuIndex
	{
		public int ParentIndex { get; set; }

		public int Index { get; set; }

		public override int GetHashCode()
		{
			return (ParentIndex + "-" + Index).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is FullMenuIndex fullMenuIndex)
			{
				return ParentIndex + "-" + Index == fullMenuIndex.ParentIndex + "-" + fullMenuIndex.Index;
			}
			return base.Equals(obj);
		}
	}

	private class DirectActionTypeExt : IActionTypeExt
	{
		public InflexActionGroup Group { get; set; }

		public int Index { get; set; }

		public string LargeGlyphUri { get; set; }

		public string GlyphUri { get; set; }

		public ActionPosition Position { get; set; }
	}

	private const int GROUP_COUNT_LIMIT = 3;

	private readonly ActionCollection _Actions;

	private List<InflexActionMenuItem> _Items = new List<InflexActionMenuItem>();

	private Dictionary<string, InflexActionGroup> _TemporaryGroups = new Dictionary<string, InflexActionGroup>();

	public List<InflexActionMenuItem> Items => _Items;

	public InflexActionMenu(ActionCollection actions)
	{
		_Actions = actions;
		BuildMenu();
	}

	private void BuildMenu()
	{
		List<InflexActionMenuItem> list = new List<InflexActionMenuItem>();
		List<InflexActionMenuItem> list2 = new List<InflexActionMenuItem>();
		Dictionary<InflexActionGroup, Dictionary<int, InflexActionMenuItem>> dictionary = new Dictionary<InflexActionGroup, Dictionary<int, InflexActionMenuItem>>();
		Dictionary<InflexActionGroup, IActionTypeExt> dictionary2 = new Dictionary<InflexActionGroup, IActionTypeExt>();
		int num = 0;
		foreach (InflexAction action in _Actions)
		{
			if (action is HandlerContextMenuAction)
			{
				continue;
			}
			num++;
			InflexActionMenuItem inflexActionMenuItem = new InflexActionMenuItem();
			inflexActionMenuItem.Action = action;
			inflexActionMenuItem.Caption = action.Caption;
			inflexActionMenuItem.Name = "contextActionMenuItem_" + num;
			InflexActionMenuItem inflexActionMenuItem2 = inflexActionMenuItem;
			if (action is ICheckableAction)
			{
				ICheckableAction checkableAction = (ICheckableAction)action;
				inflexActionMenuItem2.IsCheckable = true;
				inflexActionMenuItem2.IsChecked = checkableAction.IsChecked;
				inflexActionMenuItem2.CheckedChangedHandler = checkableAction.OnCheckStateChanged;
			}
			IActionTypeExt actionTypeExt = action as IActionTypeExt;
			if (Attribute.GetCustomAttribute(action.GetType(), typeof(ActionTypeAttribute)) is ActionTypeAttribute actionTypeAttribute)
			{
				if (actionTypeExt == null)
				{
					actionTypeExt = ConvertToExt(actionTypeAttribute);
				}
				else if (actionTypeExt.Group != null)
				{
					actionTypeExt.Group.GroupOrder = actionTypeAttribute.GroupOrder;
				}
			}
			else if (actionTypeExt != null && actionTypeExt.Group != null)
			{
				actionTypeExt.Group.GroupOrder = int.MaxValue;
			}
			if (actionTypeExt == null)
			{
				list.Add(inflexActionMenuItem2);
				continue;
			}
			inflexActionMenuItem2.GlyphSource = actionTypeExt.GlyphUri;
			if (actionTypeExt.Group == null)
			{
				if (actionTypeExt.Position == ActionPosition.Up)
				{
					list.Add(inflexActionMenuItem2);
				}
				else
				{
					list2.Add(inflexActionMenuItem2);
				}
				continue;
			}
			Dictionary<int, InflexActionMenuItem> value = null;
			if (!dictionary.TryGetValue(actionTypeExt.Group, out value))
			{
				value = new Dictionary<int, InflexActionMenuItem>();
				dictionary.Add(actionTypeExt.Group, value);
				dictionary2.Add(actionTypeExt.Group, actionTypeExt);
				for (InflexActionGroup parentGroup = actionTypeExt.Group.ParentGroup; parentGroup != null; parentGroup = parentGroup.ParentGroup)
				{
					if (!dictionary.ContainsKey(parentGroup))
					{
						dictionary.Add(parentGroup, new Dictionary<int, InflexActionMenuItem>());
						dictionary2.Add(parentGroup, actionTypeExt);
					}
				}
			}
			int num2 = actionTypeExt.Index;
			if (num2 >= 0)
			{
				value.Remove(actionTypeExt.Index);
			}
			else
			{
				num2 = (from g in value
					where g.Key < 0
					orderby g.Key descending
					select g.Key).FirstOrDefault();
				num2 = ((num2 == 0) ? int.MinValue : (num2 + 1));
			}
			value.Add(num2, inflexActionMenuItem2);
		}
		num = 0;
		IOrderedEnumerable<KeyValuePair<InflexActionGroup, Dictionary<int, InflexActionMenuItem>>> orderedEnumerable = from g in dictionary
			where g.Key.ParentGroup == null
			orderby g.Key.GroupOrder
			select g;
		int num3 = orderedEnumerable.Count();
		foreach (KeyValuePair<InflexActionGroup, Dictionary<int, InflexActionMenuItem>> item in orderedEnumerable)
		{
			num++;
			InflexActionMenuItem inflexActionMenuItem3 = new InflexActionMenuItem();
			inflexActionMenuItem3.Name = "contextGroupActionMenuItem_" + num;
			inflexActionMenuItem3.Caption = item.Key.Name;
			inflexActionMenuItem3.GlyphSource = item.Key.GlyphUri;
			inflexActionMenuItem3.SubItems.AddRange(CreateSubItmes(item.Key, dictionary, item.Value, ref num));
			bool flag = inflexActionMenuItem3.SubItems.Count > 3;
			if (item.Key.Name.StartsWith("%"))
			{
				flag = false;
			}
			List<InflexActionMenuItem> list3 = ((dictionary2[item.Key].Position == ActionPosition.Up) ? list : list2);
			if (list3.Count > 0 && (!flag || list3[list3.Count - 1].SubItems.Count == 0))
			{
				list3.Add(new InflexActionMenuItem
				{
					Caption = "-"
				});
			}
			if (flag && num3 > 1)
			{
				list3.Add(inflexActionMenuItem3);
			}
			else
			{
				list3.AddRange(inflexActionMenuItem3.SubItems);
			}
		}
		List<InflexActionMenuItem> handlerContextMenuActionItems = GetHandlerContextMenuActionItems(out var position);
		List<InflexActionMenuItem> list4 = ((position == ActionPosition.Up) ? list : list2);
		if (list4.Count > 0 && handlerContextMenuActionItems.Count > 0 && list4[list4.Count - 1].SubItems.Count == 0)
		{
			list4.Add(new InflexActionMenuItem
			{
				Caption = "-"
			});
		}
		list4.AddRange(handlerContextMenuActionItems);
		_Items.AddRange(list);
		if (list.Count > 0 && list2.Count > 0 && (list[list.Count - 1].SubItems.Count == 0 || list2[0].SubItems.Count == 0))
		{
			_Items.Add(new InflexActionMenuItem
			{
				Caption = "-"
			});
		}
		_Items.AddRange(list2);
		ClearDoubleSplitItems(_Items);
	}

	private void ClearDoubleSplitItems(List<InflexActionMenuItem> items)
	{
		if (items == null)
		{
			return;
		}
		int num = 0;
		bool flag = true;
		while (num != items.Count)
		{
			InflexActionMenuItem inflexActionMenuItem = items[num];
			if (inflexActionMenuItem.IsSplitItem && flag)
			{
				items.RemoveAt(num);
				continue;
			}
			ClearDoubleSplitItems(inflexActionMenuItem.SubItems);
			flag = inflexActionMenuItem.IsSplitItem;
			num++;
		}
		if (items.Count > 0 && items[items.Count - 1].IsSplitItem)
		{
			items.RemoveAt(items.Count - 1);
		}
	}

	private IEnumerable<InflexActionMenuItem> CreateSubItmes(InflexActionGroup group, Dictionary<InflexActionGroup, Dictionary<int, InflexActionMenuItem>> groups, Dictionary<int, InflexActionMenuItem> values, ref int i)
	{
		List<InflexActionMenuItem> list = new List<InflexActionMenuItem>();
		foreach (KeyValuePair<InflexActionGroup, Dictionary<int, InflexActionMenuItem>> item in groups.Where((KeyValuePair<InflexActionGroup, Dictionary<int, InflexActionMenuItem>> g) => group.Equals(g.Key.ParentGroup)))
		{
			i++;
			InflexActionMenuItem inflexActionMenuItem = new InflexActionMenuItem();
			inflexActionMenuItem.Name = "contextGroupActionMenuItem_" + i;
			inflexActionMenuItem.Caption = item.Key.Name;
			inflexActionMenuItem.GlyphSource = item.Key.GlyphUri;
			inflexActionMenuItem.SubItems.AddRange(CreateSubItmes(item.Key, groups, item.Value, ref i));
			list.Add(inflexActionMenuItem);
		}
		list.AddRange(from g in values
			orderby g.Key
			select g.Value);
		return list;
	}

	private IActionTypeExt ConvertToExt(ActionTypeAttribute attribute)
	{
		DirectActionTypeExt directActionTypeExt = new DirectActionTypeExt();
		directActionTypeExt.Index = attribute.Index;
		directActionTypeExt.GlyphUri = attribute.GlyphUri;
		directActionTypeExt.LargeGlyphUri = attribute.LargeGlyphUri;
		directActionTypeExt.Position = attribute.Position;
		DirectActionTypeExt directActionTypeExt2 = directActionTypeExt;
		if (!string.IsNullOrEmpty(attribute.GroupName))
		{
			if (!_TemporaryGroups.TryGetValue(attribute.GroupName.ToLower(), out var value))
			{
				InflexActionGroup inflexActionGroup = new InflexActionGroup();
				inflexActionGroup.Name = attribute.GroupName;
				inflexActionGroup.GlyphUri = attribute.GroupGlyphUri;
				inflexActionGroup.GroupOrder = attribute.GroupOrder;
				value = inflexActionGroup;
			}
			directActionTypeExt2.Group = value;
		}
		return directActionTypeExt2;
	}

	private List<InflexActionMenuItem> GetHandlerContextMenuActionItems(out ActionPosition position)
	{
		position = ActionPosition.Up;
		List<Dictionary<FullMenuIndex, HandlerContextMenuAction>> list = new List<Dictionary<FullMenuIndex, HandlerContextMenuAction>>();
		foreach (HandlerContextMenuAction item in from a in _Actions.OfType<HandlerContextMenuAction>()
			orderby a.Level
			select a)
		{
			if (item.Level >= 0)
			{
				while (list.Count <= item.Level)
				{
					list.Add(new Dictionary<FullMenuIndex, HandlerContextMenuAction>());
				}
				FullMenuIndex fullMenuIndex = new FullMenuIndex();
				fullMenuIndex.Index = item.MenuIndex;
				fullMenuIndex.ParentIndex = item.ParentMenuIndex;
				FullMenuIndex key = fullMenuIndex;
				list[item.Level].Remove(key);
				list[item.Level].Add(key, item);
			}
		}
		List<InflexActionMenuItem> list2 = new List<InflexActionMenuItem>();
		if (list.Count > 0)
		{
			foreach (KeyValuePair<FullMenuIndex, HandlerContextMenuAction> item2 in list[0])
			{
				InflexActionMenuItem inflexActionMenuItem = new InflexActionMenuItem();
				inflexActionMenuItem.Action = item2.Value;
				inflexActionMenuItem.Caption = item2.Value.Caption;
				inflexActionMenuItem.Name = "contextActionMenuItem_0_" + item2.Key.Index;
				InflexActionMenuItem inflexActionMenuItem2 = inflexActionMenuItem;
				list2.Add(inflexActionMenuItem2);
				FillContextMenu(list, inflexActionMenuItem2, item2.Key.Index, 1);
			}
		}
		return list2;
	}

	private void FillContextMenu(List<Dictionary<FullMenuIndex, HandlerContextMenuAction>> levels, InflexActionMenuItem menuItem, int parentIndex, int childLevel)
	{
		if (childLevel >= levels.Count)
		{
			return;
		}
		foreach (KeyValuePair<FullMenuIndex, HandlerContextMenuAction> item in from item in levels[childLevel]
			where item.Key.ParentIndex == parentIndex
			orderby item.Key.Index
			select item)
		{
			InflexActionMenuItem inflexActionMenuItem = new InflexActionMenuItem();
			inflexActionMenuItem.Action = item.Value;
			inflexActionMenuItem.Caption = item.Value.Caption;
			inflexActionMenuItem.Name = "contextActionMenuItem_" + childLevel + "_" + item.Key.Index;
			InflexActionMenuItem inflexActionMenuItem2 = inflexActionMenuItem;
			menuItem.SubItems.Add(inflexActionMenuItem2);
			FillContextMenu(levels, inflexActionMenuItem2, item.Key.Index, childLevel + 1);
		}
	}
}
