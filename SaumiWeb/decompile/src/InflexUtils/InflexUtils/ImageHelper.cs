using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace InflexUtils;

internal static class ImageHelper
{
	public static Bitmap CreateGrayScaleImage(Bitmap image)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		Bitmap val = new Bitmap(((Image)image).Width, ((Image)image).Height);
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		ColorMatrix colorMatrix = new ColorMatrix(new float[5][]
		{
			new float[5] { 0.3f, 0.3f, 0.3f, 0f, 0f },
			new float[5] { 0.59f, 0.59f, 0.59f, 0f, 0f },
			new float[5] { 0.11f, 0.11f, 0.11f, 0f, 0f },
			new float[5] { 0f, 0f, 0f, 1f, 0f },
			new float[5] { 0f, 0f, 0f, 0f, 1f }
		});
		ImageAttributes val3 = new ImageAttributes();
		val3.SetColorMatrix(colorMatrix);
		val2.DrawImage((Image)(object)image, new Rectangle(0, 0, ((Image)image).Width, ((Image)image).Height), 0, 0, ((Image)image).Width, ((Image)image).Height, (GraphicsUnit)2, val3);
		val2.Dispose();
		return val;
	}
}
