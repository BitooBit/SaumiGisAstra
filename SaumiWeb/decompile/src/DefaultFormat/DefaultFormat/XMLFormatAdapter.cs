using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Payment.Contracts;
using Payment.FormatAdapters;

namespace DefaultFormat;

public class XMLFormatAdapter : FileFormatAdapter
{
	private readonly string[] _Extensions = new string[1] { "*.xml" };

	public override string Name => "XML формат файла поступлений (2007.04 - 2013.06)";

	public override string AdapterKey => "XML07041306";

	private PaymentXMLParser GetParser()
	{
		return new PaymentXMLParser();
	}

	protected override IPaymentsFile ImportPaymentsFileInternal(Stream input)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		DialogResult val = TopMostMessageBox.Show("Вы пытаетесь загрузить файл устаревшего XML-формата. \r\nОбращаем Ваше внимание на то, что для обеспечения однозначной идентификации каждого поступления \r\nзначение поля 'Электронный номер расчетного документа (ElectronicNumberOfAccountingDocument)' \r\nдолжно содержать уникальное значение в формате GUID (XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX).\r\nПри несоответствии поля описанному формату система выполнит автоматическую генерацию недостающих значений.\r\nВ этом случае, при повторной загрузке файла, станет возможным появление дубликатов поступлений.\r\nВо избежании появления дубликатов рекомендуем следовать требованиям, описанным выше.\r\nВыполнить загрузку?", "Внимание! Обратитесь к администратору.", (MessageBoxButtons)4, (MessageBoxIcon)48);
		if ((int)val == 7)
		{
			return null;
		}
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(input);
			PaymentXMLParser parser = GetParser();
			return parser.ParseXml(xmlDocument.InnerXml);
		}
		catch (Exception ex)
		{
			throw new Exception("Ошибка при анализе файла: \n" + ex.Message, ex);
		}
	}

	protected override IEnumerable<string> GetExtensionsInternal()
	{
		return (string[])_Extensions.Clone();
	}

	public IPayment CreateDefaultPayment()
	{
		return (IPayment)(object)new XmlPayment();
	}
}
