using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace DefaultFormat;

public class XmlValidator
{
	private readonly List<string> _Messages = new List<string>();

	public IList<string> Messages => _Messages;

	public bool Validate(string xmlContent, XmlReader xsdReader)
	{
		_Messages.Clear();
		XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
		xmlSchemaSet.Add(null, xsdReader);
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.Schemas = xmlSchemaSet;
		xmlReaderSettings.CloseInput = true;
		xmlReaderSettings.ValidationEventHandler += ValidationEventHandler;
		xmlReaderSettings.ValidationType = ValidationType.Schema;
		xmlReaderSettings.ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema | XmlSchemaValidationFlags.ProcessSchemaLocation | XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessIdentityConstraints;
		try
		{
			using StringReader input = new StringReader(xmlContent);
			using XmlReader xmlReader = XmlReader.Create(input, xmlReaderSettings);
			while (xmlReader.Read())
			{
			}
			return _Messages.Count <= 0;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private void ValidationEventHandler(object sender, ValidationEventArgs e)
	{
		switch (e.Severity)
		{
		case XmlSeverityType.Error:
			_Messages.Add($"Line({e.Exception.LineNumber},{e.Exception.LinePosition}) Error:  {e.Message}");
			break;
		case XmlSeverityType.Warning:
			_Messages.Add($"Line({e.Exception.LineNumber},{e.Exception.LinePosition}) Warning:  {e.Message}");
			break;
		}
	}
}
