using System.Windows.Forms;

namespace InflexUtils.ExceptionBox;

public class ExceptionBox
{
	public static void Show(object exception)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		ExceptionBoxForm exceptionBoxForm = new ExceptionBoxForm(exception);
		((Form)exceptionBoxForm).ShowDialog();
	}
}
