using System.Drawing;

namespace InflexUtils.PrimitiveControls.Popup;

internal struct GripBounds(Rectangle clientRectangle)
{
	private const int GripSize = 6;

	private const int CornerGripSize = 12;

	private Rectangle clientRectangle = clientRectangle;

	public Rectangle ClientRectangle => clientRectangle;

	public Rectangle Bottom
	{
		get
		{
			Rectangle result = ClientRectangle;
			result.Y = result.Bottom - 6 + 1;
			result.Height = 6;
			return result;
		}
	}

	public Rectangle BottomRight
	{
		get
		{
			Rectangle result = ClientRectangle;
			result.Y = result.Bottom - 12 + 1;
			result.Height = 12;
			result.X = result.Width - 12 + 1;
			result.Width = 12;
			return result;
		}
	}

	public Rectangle Top
	{
		get
		{
			Rectangle result = ClientRectangle;
			result.Height = 6;
			return result;
		}
	}

	public Rectangle TopRight
	{
		get
		{
			Rectangle result = ClientRectangle;
			result.Height = 12;
			result.X = result.Width - 12 + 1;
			result.Width = 12;
			return result;
		}
	}

	public Rectangle Left
	{
		get
		{
			Rectangle result = ClientRectangle;
			result.Width = 6;
			return result;
		}
	}

	public Rectangle BottomLeft
	{
		get
		{
			Rectangle result = ClientRectangle;
			result.Width = 12;
			result.Y = result.Height - 12 + 1;
			result.Height = 12;
			return result;
		}
	}

	public Rectangle Right
	{
		get
		{
			Rectangle result = ClientRectangle;
			result.X = result.Right - 6 + 1;
			result.Width = 6;
			return result;
		}
	}

	public Rectangle TopLeft
	{
		get
		{
			Rectangle result = ClientRectangle;
			result.Width = 12;
			result.Height = 12;
			return result;
		}
	}
}
