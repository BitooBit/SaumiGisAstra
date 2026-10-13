using System.Security.Cryptography.Xml;
using System.Xml;

namespace CommonUtils.SecureUtils.Xml;

public abstract class SignedXmlBuilder
{
	public SignedXml Build(XmlDocument xmlDocument)
	{
		SignedXml val = CreateInstance(xmlDocument);
		Configure(val);
		return val;
	}

	public SignedXml Build(XmlElement xmlElement)
	{
		SignedXml val = CreateInstance(xmlElement);
		Configure(val);
		return val;
	}

	protected virtual SignedXml CreateInstance(XmlElement xmlElement)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return new SignedXml(xmlElement);
	}

	protected virtual SignedXml CreateInstance(XmlDocument xmlDocument)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return new SignedXml(xmlDocument);
	}

	protected virtual void Configure(SignedXml signedXml)
	{
	}
}
