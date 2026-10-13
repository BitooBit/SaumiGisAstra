using System;
using System.Timers;
using System.Windows.Forms;
using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
public class PeriodicalRefreshAction : ExplorerWindowToolAction
{
	private Timer _Timer;

	public override string Caption => "Обновлять периодически";

	public int Interval { get; set; }

	public PeriodicalRefreshAction()
	{
		_Timer = new Timer
		{
			AutoReset = false
		};
		_Timer.Elapsed += OnTimerElapsed;
	}

	private void OnTimerElapsed(object sender, ElapsedEventArgs e)
	{
		((Control)base.DirectContext.ExplorerWindow).BeginInvoke((Delegate)new Action<bool>(base.DirectContext.DataView.RefreshQuery), new object[1] { true });
		_Timer.Start();
	}

	protected override void OnBeforeContextDispose()
	{
		Timer timer = _Timer;
		if (timer != null)
		{
			timer.Stop();
			_Timer.Elapsed -= OnTimerElapsed;
			timer.Dispose();
			_Timer = null;
		}
		base.OnBeforeContextDispose();
	}

	protected override bool? DirectIsAllowed()
	{
		return base.DirectContext.DataView != null && !base.DirectContext.DataView.InExecute();
	}

	protected override void ProtectedDirectExecute()
	{
		int interval = Interval;
		Timer timer = _Timer;
		if (timer != null)
		{
			if (interval > 0)
			{
				timer.Interval = interval * 1000;
				timer.Start();
			}
			else
			{
				timer.Stop();
			}
		}
	}
}
