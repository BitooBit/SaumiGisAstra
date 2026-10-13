using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InflexCore.Action;

public class ActionGlyphs
{
	private static Dictionary<string, ImageSource> _Glyphs = new Dictionary<string, ImageSource>();

	public static ImageSource Glyph(string uri)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (string.IsNullOrEmpty(uri))
		{
			return null;
		}
		if (_Glyphs.TryGetValue(uri, out var value))
		{
			return value;
		}
		try
		{
			value = (ImageSource)new BitmapImage(new Uri(uri, UriKind.Absolute));
			_Glyphs.Add(uri, value);
		}
		catch
		{
			value = null;
		}
		return value;
	}
}
