using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace InflexData;

public class DataRowToInflexTreeNodeConverter : MarkupExtension, IMultiValueConverter
{
	private static IList<InflexTreeNode> _RootNodes;

	public static void UpdateTreeNodes(IList<InflexTreeNode> rootNodes)
	{
		_RootNodes = rootNodes;
	}

	private InflexTreeNode FindNode(InflexTreeNode parentNode, decimal id)
	{
		foreach (InflexTreeNode child in parentNode.Children)
		{
			if (child.ID == id)
			{
				return child;
			}
			if (child.HasChildren)
			{
				InflexTreeNode inflexTreeNode = FindNode(child, id);
				if (inflexTreeNode != null)
				{
					return inflexTreeNode;
				}
			}
		}
		return null;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return this;
	}

	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
