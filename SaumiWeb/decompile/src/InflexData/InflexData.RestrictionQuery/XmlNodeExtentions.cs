using System.Xml;

namespace InflexData.RestrictionQuery;

internal static class XmlNodeExtentions
{
	public static bool IsChild(this XmlNode self, XmlNode tryChild)
	{
		for (XmlNode xmlNode = tryChild?.ParentNode; xmlNode != null; xmlNode = xmlNode.ParentNode)
		{
			if (xmlNode == self)
			{
				return true;
			}
		}
		return false;
	}
}
