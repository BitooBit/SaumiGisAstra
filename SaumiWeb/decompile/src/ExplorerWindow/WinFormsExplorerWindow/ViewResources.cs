using System.Drawing;

namespace WinFormsExplorerWindow;

public static class ViewResources
{
	private static readonly Color _ActiveBackgroundColor = Color.FromArgb(22, 89, 157);

	private static readonly Color _InactiveBackgroundColor = Color.FromArgb(49, 111, 173);

	private static readonly Color _ActiveForegroundColor = Color.White;

	private static readonly Color _InactiveForegroundColor = Color.FromArgb(139, 172, 206);

	public static Color ActiveBackgroundColor => _ActiveBackgroundColor;

	public static Color InactiveBackgroundColor => _InactiveBackgroundColor;

	public static Color ActiveForegroundColor => _ActiveForegroundColor;

	public static Color InactiveForegroundColor => _InactiveForegroundColor;
}
