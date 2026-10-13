using System.Collections.Generic;
using System.Linq;
using InflexCore.Action.Attributes;
using InflexCore.Native.Action;

namespace InflexCore.Action;

[ActionType(GroupName = "Печатные формы")]
public class ShowFastReportFormAction : InflexAction, IActionTypeExt
{
	private const string _GroupName = "Печатные формы";

	private readonly IFastReport _FastReport;

	private string _Caption;

	private InflexActionGroup _Group;

	private IEnumerable<IInflexID> _ExecuteActionIds;

	public override string Caption => _Caption;

	public IEnumerable<IInflexID> ExecuteActionIds
	{
		get
		{
			if (_ExecuteActionIds == null)
			{
				return base.Context.ActionArgs.ObjectIDs;
			}
			return _ExecuteActionIds;
		}
		set
		{
			_ExecuteActionIds = value;
		}
	}

	public int FormID { get; set; }

	public bool OneItemForm { get; set; }

	private InflexActionGroup DefaultGroup { get; set; }

	InflexActionGroup IActionTypeExt.Group => _Group ?? DefaultGroup;

	int IActionTypeExt.Index => ActionTypeAttribute.DefaultIndex;

	string IActionTypeExt.LargeGlyphUri => "";

	string IActionTypeExt.GlyphUri => "";

	ActionPosition IActionTypeExt.Position => ActionPosition.Up;

	public ShowFastReportFormAction(IFastReport fastReport, ActionContext context)
	{
		DefaultGroup = new InflexActionGroup
		{
			Name = "Печатные формы"
		};
		_FastReport = fastReport;
		Initialize(context);
	}

	public void SetCaption(string value)
	{
		_Caption = value;
	}

	protected override bool DirectIsAllowedContext()
	{
		return _FastReport != null;
	}

	protected override bool? DirectIsAllowed()
	{
		return ExecuteActionIds?.Any() ?? false;
	}

	protected override void DirectExecute()
	{
		IInflexID[] items = ExecuteActionIds.ToArray();
		_FastReport.Preview(FormID, items);
	}

	public void SetChildGroup(InflexActionGroup group)
	{
		_Group = group;
		_Group.ParentGroup = DefaultGroup;
	}
}
