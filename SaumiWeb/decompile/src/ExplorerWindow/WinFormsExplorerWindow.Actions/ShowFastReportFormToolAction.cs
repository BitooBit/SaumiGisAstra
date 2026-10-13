using System;
using System.Threading;
using System.Windows.Forms;
using InflexCore;
using InflexCore.Action;
using InflexCore.Action.Attributes;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(PrintAction), GlyphUri = "Print", GroupName = "Печатные формы", GroupOrder = 300, GroupGlyphUri = "PrintFolder")]
public class ShowFastReportFormToolAction : ExplorerWindowToolAction, IActionTypeExt
{
	private sealed class HackWindow : Form
	{
		private readonly ShowFastReportFormAction _Action;

		public HackWindow(ShowFastReportFormAction action)
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

	private const string _GlyphUri = "Print";

	private const string _GroupName = "Печатные формы";

	private const string _GroupGlyphUri = "PrintFolder";

	private readonly ShowFastReportFormAction _Action;

	public bool IsOneItemForm => _Action.OneItemForm;

	public override string Caption => _Action.Caption;

	public override string Name => base.Name + _Action.FormID;

	string IActionTypeExt.GlyphUri => "Print";

	InflexActionGroup IActionTypeExt.Group => ((IActionTypeExt)_Action).Group;

	int IActionTypeExt.Index => ActionTypeAttribute.DefaultIndex;

	string IActionTypeExt.LargeGlyphUri => "";

	ActionPosition IActionTypeExt.Position => ActionPosition.Up;

	public ShowFastReportFormToolAction(ShowFastReportFormAction action)
	{
		_Action = action;
		((IActionTypeExt)_Action).Group.GlyphUri = "PrintFolder";
		Initialize(action.Context);
	}

	protected override bool? DirectIsAllowed()
	{
		return _Action.IsAllowed;
	}

	protected override void AfterContextUpdate()
	{
		IInflexID[] objectIDs = base.DirectContext.ActionArgs.ObjectIDs;
		if (objectIDs != null && objectIDs.Length == 1 && !_Action.OneItemForm)
		{
			_Action.ExecuteActionIds = ((base.DirectContext.DataView != null) ? base.DirectContext.DataView.GetInflexIDs() : null);
		}
		else
		{
			_Action.ExecuteActionIds = base.DirectContext.ActionArgs.ObjectIDs;
		}
	}

	protected override bool ProtectedIsAllowedContext()
	{
		return _Action.IsAllowedContext;
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
