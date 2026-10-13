using System;
using System.Threading;
using System.Windows.Forms;
using InflexCore.Action;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(FileAction), GroupIndex = 200, Index = 200, GlyphUri = "New", LargeGlyphUri = "NewLarge")]
public class NewInflexObjectToolAction : ExplorerWindowToolAction
{
	private sealed class HackWindow : Form
	{
		private readonly NewInflexObjectAction _Action;

		public HackWindow(NewInflexObjectAction action)
		{
			_Action = action;
			((Form)this).ShowInTaskbar = false;
			((Form)this).WindowState = (FormWindowState)1;
			((Control)this).SetBoundsCore(-150, -150, 50, 50, (BoundsSpecified)15);
			((Form)this).Load += OnHackWindowLoaded;
		}

		private void OnHackWindowLoaded(object sender, EventArgs eventArgs)
		{
			Thread thread = new Thread(ExecuteAction);
			thread.Start();
		}

		private void ExecuteAction()
		{
			_Action.Execute();
			((Control)this).BeginInvoke((Delegate)new Action(base.Close));
		}
	}

	private NewInflexObjectAction _Action;

	public override string Caption => "Создать новый объект";

	public void SetInnerAction(NewInflexObjectAction action)
	{
		_Action = action;
	}

	protected override bool? DirectIsAllowed()
	{
		if (_Action != null && !base.DirectContext.DataView.InExecute())
		{
			return _Action.IsAllowed;
		}
		return false;
	}

	protected override void ProtectedDirectExecute()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (_Action != null)
		{
			HackWindow hackWindow = new HackWindow(_Action);
			((Form)hackWindow).ShowDialog();
		}
	}
}
