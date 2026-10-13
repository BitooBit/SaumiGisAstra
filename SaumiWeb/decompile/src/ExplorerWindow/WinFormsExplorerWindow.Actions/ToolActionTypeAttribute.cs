using System;
using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

public class ToolActionTypeAttribute : ActionTypeAttribute
{
	public string HotKey { get; set; }

	public Type MultiActionType { get; set; }

	public int GroupIndex { get; set; }
}
