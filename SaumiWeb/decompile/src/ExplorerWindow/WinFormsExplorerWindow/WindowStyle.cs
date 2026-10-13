using System;
using System.Drawing;

namespace WinFormsExplorerWindow;

internal class WindowStyle : IDisposable
{
	public Image IconImage { get; set; }

	public Color Background { get; set; }

	public Color Foreground { get; set; }

	public void Dispose()
	{
		IconImage = null;
	}
}
