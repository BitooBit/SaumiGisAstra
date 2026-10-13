using System.Security.Cryptography.Xml;
using System.Xml;

namespace CommonUtils.SecureUtils.Xml;

public class SpecialSignedXml : SignedXml
{
	private const string SEARCH_ID_PREFIX_TEMPLATE = "//*[@*[translate(name(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{0}:{1}'] and @*[translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{2}']]";

	private const string SEARCH_ID_TEMPLATE = "//*[@*[translate(local-name(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{0}'] and @*[translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{1}']]";

	public string IDName { get; private set; }

	public string IDPrefix { get; private set; }

	public string IDNamespace { get; private set; }

	public SpecialSignedXml(XmlDocument document, string idName)
		: this(document, idName, null, null)
	{
	}

	public SpecialSignedXml(XmlDocument document, string idName, string idPrefix, string idNamespace)
		: base(document)
	{
		IDName = idName;
		IDPrefix = idPrefix;
		IDNamespace = idNamespace;
	}

	public SpecialSignedXml(XmlElement element, string idName)
		: this(element, idName, null, null)
	{
	}

	public SpecialSignedXml(XmlElement element, string idName, string idPrefix, string idNamespace)
		: base(element)
	{
		IDName = idName;
		IDPrefix = idPrefix;
		IDNamespace = idNamespace;
	}

	public override XmlElement GetIdElement(XmlDocument document, string idValue)
	{
		XmlNodeList xmlNodeList = null;
		if (IDPrefix == null)
		{
			string xpath = $"//*[@*[translate(local-name(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{IDName.ToLower()}'] and @*[translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{idValue.ToLower()}']]";
			xmlNodeList = document.SelectNodes(xpath);
		}
		else
		{
			XmlNameTable nameTable = new NameTable();
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(nameTable);
			xmlNamespaceManager.AddNamespace(IDPrefix, IDNamespace);
			string xpath2 = $"//*[@*[translate(name(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{IDPrefix.ToLower()}:{IDName.ToLower()}'] and @*[translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{idValue.ToLower()}']]";
			xmlNodeList = document.SelectNodes(xpath2, xmlNamespaceManager);
		}
		if (xmlNodeList == null || xmlNodeList.Count != 1)
		{
			return null;
		}
		return (XmlElement)xmlNodeList[0];
	}
}
