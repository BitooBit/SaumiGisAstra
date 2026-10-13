using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace InflexUtils.PrimitiveControls;

public class ExtendedContextMenu : ContextMenuStrip
{
	protected override void OnOpening(CancelEventArgs e)
	{
		UpdateOpeningChildsState();
		((ToolStripDropDown)this).OnOpening(e);
	}

	private void UpdateOpeningChildsState()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (((ArrangedElementCollection)((ToolStrip)this).Items).Count <= 0)
		{
			return;
		}
		foreach (ToolStripItem item in (ArrangedElementCollection)((ToolStrip)this).Items)
		{
			ToolStripItem val = item;
			if (val is ExtendedMenuItem extendedMenuItem)
			{
				extendedMenuItem.UpdateState();
			}
		}
	}
}
