using System;
using System.Collections.Generic;

namespace InflexCore.Action.ActionMenu;

public class InflexActionMenuItem : ICheckableItem
{
	private List<InflexActionMenuItem> _SubItems = new List<InflexActionMenuItem>();

	public List<InflexActionMenuItem> SubItems => _SubItems;

	public InflexAction Action { get; set; }

	public string Caption { get; set; }

	public bool IsSplitItem => Caption == "-";

	public string Name { get; set; }

	public string GlyphSource { get; set; }

	public bool IsCheckable { get; set; }

	public bool IsChecked { get; set; }

	public Action<bool> CheckedChangedHandler { get; set; }
}
