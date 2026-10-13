using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using WinFormsExplorerWindow.Actions;
using WinFormsExplorerWindow.PrimitiveControls;

namespace WinFormsExplorerWindow;

internal class PeriodicalRefreshMenuBuilder : IDisposable
{
	private const string CUSTOM = "CUSTOM";

	private readonly PeriodicalRefreshAction _Action = new PeriodicalRefreshAction();

	private ToolStripMenuItem _RootItem;

	private List<ToolStripMenuItem> _ChildItems = new List<ToolStripMenuItem>();

	public PeriodicalRefreshMenuBuilder(PeriodicalRefreshAction action)
	{
		_Action = action;
	}

	public ToolStripMenuItem[] CreateMenuItems()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		if (_RootItem == null)
		{
			ToolStripMenuItem val = new ToolStripMenuItem("Каждую минуту");
			((ToolStripItem)val).Click += SetAutoUpdateClick;
			((ToolStripItem)val).Tag = 60;
			ToolStripMenuItem val2 = new ToolStripMenuItem("Каждые 3 минуты");
			((ToolStripItem)val2).Click += SetAutoUpdateClick;
			((ToolStripItem)val2).Tag = 180;
			ToolStripMenuItem val3 = new ToolStripMenuItem("Каждые 5 минут");
			((ToolStripItem)val3).Click += SetAutoUpdateClick;
			((ToolStripItem)val3).Tag = 300;
			ToolStripMenuItem val4 = new ToolStripMenuItem("Каждые 10 минут");
			((ToolStripItem)val4).Click += SetAutoUpdateClick;
			((ToolStripItem)val4).Tag = 600;
			ToolStripMenuItem val5 = new ToolStripMenuItem("Не обновлять автоматически");
			val5.Checked = true;
			((ToolStripItem)val5).Click += SetAutoUpdateClick;
			((ToolStripItem)val5).Tag = -1;
			ToolStripMenuItem val6 = new ToolStripMenuItem("Установить вручную...");
			((ToolStripItem)val6).Click += SetAutoUpdateHandmade;
			((ToolStripItem)val6).Tag = "CUSTOM";
			_ChildItems.Add(val);
			_ChildItems.Add(val2);
			_ChildItems.Add(val3);
			_ChildItems.Add(val4);
			_ChildItems.Add(val5);
			_ChildItems.Add(val6);
		}
		return _ChildItems.ToArray();
	}

	private void SetAutoUpdateHandmade(object sender, EventArgs e)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		ToolStripMenuItem val = (ToolStripMenuItem)((sender is ToolStripMenuItem) ? sender : null);
		if (val != null && ((ToolStripItem)val).Tag != null && ((ToolStripItem)val).Tag.Equals("CUSTOM"))
		{
			bool flag = false;
			SetUpdateIntervalWindow setUpdateIntervalWindow = new SetUpdateIntervalWindow();
			setUpdateIntervalWindow.RefreshAction = _Action;
			if ((int)((Form)setUpdateIntervalWindow).ShowDialog() == 1)
			{
				flag = true;
			}
			if (flag)
			{
				SetCheckedForMenuItem(val);
			}
		}
	}

	private void SetCheckedForMenuItem(ToolStripMenuItem menuItem)
	{
		foreach (ToolStripMenuItem childItem in _ChildItems)
		{
			childItem.Checked = childItem == menuItem;
		}
	}

	private void SetAutoUpdateClick(object sender, EventArgs e)
	{
		ToolStripMenuItem val = (ToolStripMenuItem)((sender is ToolStripMenuItem) ? sender : null);
		if (val != null && ((ToolStripItem)val).Tag != null)
		{
			int interval = Convert.ToInt32(((ToolStripItem)val).Tag);
			if (_Action.IsAllowed == true)
			{
				_Action.Interval = interval;
				_Action.Execute();
			}
			SetCheckedForMenuItem(val);
		}
	}

	public void Dispose()
	{
		if (_ChildItems != null)
		{
			foreach (ToolStripMenuItem childItem in _ChildItems)
			{
				if (!((ToolStripItem)childItem).IsDisposed)
				{
					((Component)(object)childItem).Dispose();
				}
			}
			_ChildItems.Clear();
			_ChildItems = null;
		}
		if (_RootItem != null)
		{
			((ToolStripDropDownItem)_RootItem).DropDownItems.Clear();
			_RootItem = null;
		}
	}
}
