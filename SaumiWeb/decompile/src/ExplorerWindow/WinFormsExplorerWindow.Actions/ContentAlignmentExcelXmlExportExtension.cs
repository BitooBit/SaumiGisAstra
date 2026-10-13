using System.Drawing;

namespace WinFormsExplorerWindow.Actions;

public static class ContentAlignmentExcelXmlExportExtension
{
	public static string GetHorizontalAlignmentStringValue(this ContentAlignment alignment)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected I4, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		if ((int)alignment <= 32)
		{
			switch (alignment - 1)
			{
			default:
				if ((int)alignment == 16)
				{
					break;
				}
				if ((int)alignment == 32)
				{
					goto IL_005a;
				}
				goto IL_0066;
			case 0:
				break;
			case 1:
				goto IL_005a;
			case 3:
				goto IL_0060;
			case 2:
				goto IL_0066;
			}
			goto IL_0054;
		}
		if ((int)alignment <= 256)
		{
			if ((int)alignment == 64)
			{
				goto IL_0060;
			}
			if ((int)alignment == 256)
			{
				goto IL_0054;
			}
		}
		else
		{
			if ((int)alignment == 512)
			{
				goto IL_005a;
			}
			if ((int)alignment == 1024)
			{
				goto IL_0060;
			}
		}
		goto IL_0066;
		IL_0054:
		return "Left";
		IL_0066:
		return string.Empty;
		IL_005a:
		return "Center";
		IL_0060:
		return "Right";
	}

	public static string GetVerticalAlignmentStringValue(this ContentAlignment alignment)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected I4, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		if ((int)alignment <= 32)
		{
			switch (alignment - 1)
			{
			case 0:
			case 1:
			case 3:
				return "Top";
			case 2:
				goto IL_0066;
			}
			if ((int)alignment == 16 || (int)alignment == 32)
			{
				goto IL_005a;
			}
		}
		else if ((int)alignment <= 256)
		{
			if ((int)alignment == 64)
			{
				goto IL_005a;
			}
			if ((int)alignment == 256)
			{
				goto IL_0060;
			}
		}
		else if ((int)alignment == 512 || (int)alignment == 1024)
		{
			goto IL_0060;
		}
		goto IL_0066;
		IL_0066:
		return string.Empty;
		IL_0060:
		return "Bottom";
		IL_005a:
		return "Center";
	}
}
