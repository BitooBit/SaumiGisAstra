using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Payment.Contracts;

namespace DefaultFormat;

internal class XbsFormatAdapter
{
	private readonly string[] _Extensions = new string[1] { "*.xbs" };

	public string Name => "XBS формат файла платежей (2013.06)";

	public string AdapterKey => "XBS1306";

	protected IPaymentsFile ImportPaymentsFileInternal(Stream input)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(input);
		string outerXml = xmlDocument.OuterXml;
		if (ValidateXml(outerXml, out var validationResultMessage))
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlPaymentsFile));
			using StringReader textReader = new StringReader(outerXml);
			return (IPaymentsFile)(object)(XmlPaymentsFile)xmlSerializer.Deserialize(textReader);
		}
		throw new XmlException(validationResultMessage);
	}

	private static bool ValidateXml(string fileContent, out string validationResultMessage)
	{
		XmlValidator xmlValidator = new XmlValidator();
		using StringReader input = new StringReader(Resource.XbsFormat);
		using XmlReader xsdReader = XmlReader.Create(input);
		if (xmlValidator.Validate(fileContent, xsdReader))
		{
			validationResultMessage = string.Empty;
			return true;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string message in xmlValidator.Messages)
		{
			stringBuilder.AppendLine(message);
		}
		validationResultMessage = stringBuilder.ToString();
		return false;
	}

	protected IEnumerable<string> GetExtensionsInternal()
	{
		return (string[])_Extensions.Clone();
	}
}
