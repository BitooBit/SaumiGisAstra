using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InflexData;
using InflexUtils.PrimitiveControls;

namespace WinFormsExplorerWindow.PrimitiveControls;

public class TotalSumsPanel : DraggablePanel
{
	private readonly List<Label> _TotalSumsCaptions;

	private readonly List<Label> _TotalSumsValues;

	private MouseEventHandler _ChildControlMouseDown;

	public Label Title { get; private set; }

	public TotalSumsPanel()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		MouseEventHandler val = null;
		base._002Ector();
		if (val == null)
		{
			val = (MouseEventHandler)delegate(object sender, MouseEventArgs args)
			{
				((Control)this).OnMouseDown(args);
			};
		}
		_ChildControlMouseDown = val;
		((Control)this).SuspendLayout();
		((TableLayoutPanel)this).ColumnCount = 2;
		((TableLayoutPanel)this).ColumnStyles.Add(new ColumnStyle((SizeType)0));
		((TableLayoutPanel)this).ColumnStyles.Add(new ColumnStyle((SizeType)0));
		((TableLayoutPanel)this).RowStyles.Add(new RowStyle((SizeType)0));
		Label val2 = new Label();
		((Control)val2).AutoSize = true;
		((Control)val2).Margin = new Padding(2);
		((Control)val2).Font = new Font("Verdana", 8f, (FontStyle)1);
		Title = val2;
		((TableLayoutPanel)this).SetRow((Control)(object)Title, 0);
		((TableLayoutPanel)this).SetColumn((Control)(object)Title, 0);
		((TableLayoutPanel)this).SetColumnSpan((Control)(object)Title, 2);
		((ControlCollection)((TableLayoutPanel)this).Controls).Add((Control)(object)Title);
		((Control)this).ResumeLayout();
		_TotalSumsCaptions = new List<Label>();
		_TotalSumsValues = new List<Label>();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		Rectangle rectangle = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
		Pen val = new Pen(Color.FromArgb(20, 69, 118));
		try
		{
			e.Graphics.DrawRectangle(val, rectangle);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		((Control)this).OnPaint(e);
	}

	protected override void OnControlAdded(ControlEventArgs e)
	{
		if (e.Control != null)
		{
			e.Control.MouseDown += _ChildControlMouseDown;
		}
		((Control)this).OnControlAdded(e);
	}

	protected override void OnControlRemoved(ControlEventArgs e)
	{
		if (e.Control != null)
		{
			e.Control.MouseDown -= _ChildControlMouseDown;
		}
		((Control)this).OnControlRemoved(e);
	}

	private void UpdateTitle(TotalSumsKinds totalSumsKind)
	{
		switch (totalSumsKind)
		{
		case TotalSumsKinds.All:
			((Control)Title).Text = "Итоговые суммы";
			break;
		case TotalSumsKinds.Selected:
			((Control)Title).Text = "Итоговые суммы (выборочные)";
			break;
		case TotalSumsKinds.Filtered:
			((Control)Title).Text = "Итоговые суммы (отфильтрованные)";
			break;
		}
	}

	public void UpdateTotalSumPanel(IDictionary<IGridColumn, decimal> totalSummaries, bool rebuildPanel, TotalSumsKinds totalSumsKind)
	{
		UpdateTitle(totalSumsKind);
		UpdateTotalSumPanel(totalSummaries, rebuildPanel);
	}

	public void UpdateTotalSumPanel(IDictionary<IGridColumn, decimal> totalSummaries, bool rebuildPanel)
	{
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Expected O, but got Unknown
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Expected O, but got Unknown
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		if (!rebuildPanel)
		{
			if (_TotalSumsCaptions.Count == 0 || _TotalSumsValues.Count == 0)
			{
				return;
			}
			int num = -1;
			{
				foreach (KeyValuePair<IGridColumn, decimal> totalSummary in totalSummaries)
				{
					num++;
					IGridColumn key = totalSummary.Key;
					decimal value = totalSummary.Value;
					IValueToDisplayStringConverter valueToDisplayStringConverter = (IValueToDisplayStringConverter)key;
					((Control)_TotalSumsCaptions[num]).Text = $"{key.Title}:";
					((Control)_TotalSumsValues[num]).Text = valueToDisplayStringConverter.ToString(key, value, useUnits: true);
				}
				return;
			}
		}
		((Control)this).SuspendLayout();
		for (int num2 = _TotalSumsValues.Count - 1; num2 >= 0; num2--)
		{
			((ControlCollection)((TableLayoutPanel)this).Controls).Remove((Control)(object)_TotalSumsCaptions[num2]);
			((ControlCollection)((TableLayoutPanel)this).Controls).Remove((Control)(object)_TotalSumsValues[num2]);
			((TableLayoutStyleCollection)((TableLayoutPanel)this).RowStyles).RemoveAt(num2 + 1);
		}
		_TotalSumsCaptions.Clear();
		_TotalSumsValues.Clear();
		int num3 = -1;
		foreach (KeyValuePair<IGridColumn, decimal> totalSummary2 in totalSummaries)
		{
			num3++;
			IGridColumn key2 = totalSummary2.Key;
			decimal value2 = totalSummary2.Value;
			IValueToDisplayStringConverter valueToDisplayStringConverter2 = (IValueToDisplayStringConverter)key2;
			((TableLayoutPanel)this).RowStyles.Add(new RowStyle((SizeType)0));
			Label val = new Label();
			((Control)val).AutoSize = true;
			((Control)val).Margin = new Padding(2);
			((Control)val).Text = $"{key2.Title}:";
			Label val2 = val;
			_TotalSumsCaptions.Add(val2);
			((TableLayoutPanel)this).SetRow((Control)(object)val2, num3 + 1);
			((TableLayoutPanel)this).SetColumn((Control)(object)val2, 0);
			((ControlCollection)((TableLayoutPanel)this).Controls).Add((Control)(object)val2);
			Label val3 = new Label();
			((Control)val3).AutoSize = true;
			((Control)val3).Margin = new Padding(2);
			((Control)val3).Text = valueToDisplayStringConverter2.ToString(key2, value2, useUnits: true);
			Label val4 = val3;
			_TotalSumsValues.Add(val4);
			((TableLayoutPanel)this).SetRow((Control)(object)val4, num3 + 1);
			((TableLayoutPanel)this).SetColumn((Control)(object)val4, 1);
			((ControlCollection)((TableLayoutPanel)this).Controls).Add((Control)(object)val4);
		}
		((Control)this).ResumeLayout();
	}

	protected override void Dispose(bool disposing)
	{
		if (_TotalSumsValues != null)
		{
			_TotalSumsValues.Clear();
		}
		if (_TotalSumsCaptions != null)
		{
			_TotalSumsCaptions.Clear();
		}
		((Control)this).Dispose(disposing);
	}
}
