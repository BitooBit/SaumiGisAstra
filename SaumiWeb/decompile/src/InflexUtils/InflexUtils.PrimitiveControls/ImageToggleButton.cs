using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace InflexUtils.PrimitiveControls;

public class ImageToggleButton : ImageButton
{
	private bool _Checked;

	[Category("Appearance")]
	[Description("Image to show when the button is not in any other state.")]
	public override Image NormalImage
	{
		get
		{
			if (!Checked)
			{
				return base.NormalImage;
			}
			return base.DownImage;
		}
		set
		{
			base.NormalImage = value;
		}
	}

	public bool Checked
	{
		get
		{
			return _Checked;
		}
		set
		{
			_Checked = value;
			base.Image = NormalImage;
			FireCheckedChanged(value);
		}
	}

	public event Action<bool> CheckedChanged;

	private void FireCheckedChanged(bool isChecked)
	{
		CheckedChanged?.Invoke(isChecked);
	}

	protected override void OnClick(EventArgs e)
	{
		Checked = !Checked;
		((Control)this).OnClick(e);
	}
}
