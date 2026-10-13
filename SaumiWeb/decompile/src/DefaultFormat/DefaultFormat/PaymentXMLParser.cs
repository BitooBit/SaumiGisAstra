using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Payment.Contracts;

namespace DefaultFormat;

public class PaymentXMLParser
{
	private Guid calcHash(string Data)
	{
		using (MD5 mD = new MD5CryptoServiceProvider())
		{
			byte[] array = mD.ComputeHash(Encoding.Default.GetBytes(Data));
			if (array.Length == 16)
			{
				return new Guid(array);
			}
		}
		return Guid.NewGuid();
	}

	private bool isFill(string str)
	{
		if (!string.IsNullOrEmpty(str))
		{
			return str.Trim().Length > 0;
		}
		return false;
	}

	private bool isNumber(string str)
	{
		return isNumber(str, 0);
	}

	private bool isNumber(string str, int len)
	{
		return Regex.IsMatch(str, (len == 0) ? "\\d+" : $"^\\d{{{len}}}$");
	}

	private Guid newGuidOtch(string xmlContent)
	{
		return calcHash(xmlContent);
	}

	private Guid newGuidDocument(string ident, XmlPayment pay)
	{
		if (pay.DateCreationPaymentDocument.HasValue && isFill(pay.NumberOfPaymentDocument) && isNumber(pay.NumberOfPaymentDocument))
		{
			DateTime value = pay.DateCreationPaymentDocument.Value;
			string numberOfPaymentDocument = pay.NumberOfPaymentDocument;
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
			string currencyDecimalSeparator = (numberFormatInfo.NumberDecimalSeparator = ".");
			numberFormatInfo.CurrencyDecimalSeparator = currencyDecimalSeparator;
			string currencyGroupSeparator = (numberFormatInfo.NumberGroupSeparator = "");
			numberFormatInfo.CurrencyGroupSeparator = currencyGroupSeparator;
			if (isFill(ident) && isNumber(ident))
			{
				return calcHash(string.Format(numberFormatInfo, "{0}{1:ddMMyyyy}{2}{3}", new object[4] { ident, value, numberOfPaymentDocument, pay.TotalPay }));
			}
			if (isFill(pay.PayerBankBIK) && isNumber(pay.PayerBankBIK, 9) && isFill(pay.GranteeBankBIK) && isNumber(pay.GranteeBankBIK, 9))
			{
				return calcHash(string.Format(numberFormatInfo, "{0:ddMMyyyy}{1}{2}{3}{4}", new object[5] { value, numberOfPaymentDocument, pay.PayerBankBIK, pay.GranteeBankBIK, pay.TotalPay }));
			}
		}
		return Guid.NewGuid();
	}

	public IPaymentsFile ParseXml(string xmlContent)
	{
		List<XmlPayment> list = new List<XmlPayment>();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xmlContent);
		if (xmlDocument.DocumentElement == null)
		{
			throw new InvalidDataException("Файл имеет некорректный формат.");
		}
		XmlNode documentElement = xmlDocument.DocumentElement;
		string ident = null;
		if (documentElement.Attributes.Count > 0)
		{
			foreach (XmlAttribute attribute in documentElement.Attributes)
			{
				if (string.Compare(attribute.Name, "AdministratorINN", ignoreCase: true) == 0)
				{
					ident = attribute.Value;
					break;
				}
			}
		}
		XmlPaymentsFile xmlPaymentsFile = new XmlPaymentsFile();
		string innerText = xmlDocument.DocumentElement.Attributes["KOFK"].InnerText;
		string innerText2 = xmlDocument.DocumentElement.Attributes["FormatVersion"].InnerText;
		xmlPaymentsFile.TO_KOD_TOFK = innerText;
		xmlPaymentsFile.FormatVersion = innerText2;
		xmlPaymentsFile.GUID_OTCH = newGuidOtch(xmlContent);
		XmlNodeList elementsByTagName = xmlDocument.DocumentElement.GetElementsByTagName("Payment");
		xmlPaymentsFile.KOL = elementsByTagName.Count.ToString(CultureInfo.InvariantCulture);
		decimal sUM_TOTAL = 0m;
		foreach (XmlElement item in elementsByTagName)
		{
			XmlPayment xmlPayment = new XmlPayment();
			try
			{
				xmlPayment.CipherOfAccountingDocument = GetString(item.GetElementsByTagName("CipherOfAccountingDocument")[0]);
				string text = GetString(item.GetElementsByTagName("CodeKBK")[0]);
				xmlPayment.CodeKBK = ((!string.IsNullOrEmpty(text)) ? text.Replace(" ", "") : null);
				xmlPayment.CodeOKATO = GetString(item.GetElementsByTagName("CodeOKATO")[0]);
				xmlPayment.ContentsOfOperation = GetString(item.GetElementsByTagName("ContentsOfOperation")[0]);
				xmlPayment.DateChargeOffPayer = GetDate(item.GetElementsByTagName("DateChargeOffPayer")[0]);
				xmlPayment.DateCreationPaymentDocument = GetDate(item.GetElementsByTagName("DateCreationPaymentDocument")[0]);
				xmlPayment.DateOfAccountingDocument = GetDate(item.GetElementsByTagName("DateOfAccountingDocument")[0]);
				xmlPayment.IndicatorDateOfDocument = GetString(item.GetElementsByTagName("IndicatorDateOfDocument")[0]);
				xmlPayment.DateOfEnrolmentOfPayment = GetDate(item.GetElementsByTagName("DateOfEnrolmentOfPayment")[0]);
				xmlPayment.DatePaymentInBank = GetDate(item.GetElementsByTagName("DatePaymentInBank")[0]);
				xmlPayment.GranteeBankBIK = GetString(item.GetElementsByTagName("GranteeBankBIK")[0]);
				xmlPayment.GranteeBankName = GetString(item.GetElementsByTagName("GranteeBankName")[0]);
				xmlPayment.GranteeINN = GetString(item.GetElementsByTagName("GranteeINN")[0]);
				xmlPayment.GranteeKPP = GetString(item.GetElementsByTagName("GranteeKPP")[0]);
				xmlPayment.GranteeName = GetString(item.GetElementsByTagName("GranteeName")[0]);
				xmlPayment.NumberOfAccountingDocument = GetString(item.GetElementsByTagName("NumberOfAccountingDocument")[0]);
				xmlPayment.NumberOfBankAccountGrantee = GetString(item.GetElementsByTagName("NumberOfBankAccountGrantee")[0]);
				xmlPayment.NumberOfBankAccountPayer = GetString(item.GetElementsByTagName("NumberOfBankAccountPayer")[0]);
				xmlPayment.NumberOfPartialPayment = GetString(item.GetElementsByTagName("NumberOfPartialPayment")[0]);
				xmlPayment.NumberOfPaymentDocument = GetString(item.GetElementsByTagName("NumberOfPaymentDocument")[0]);
				xmlPayment.NumberOfPressCountBankGrantee = GetString(item.GetElementsByTagName("NumberOfPressCountBankGrantee")[0]);
				xmlPayment.NumberOfPressCountBankPayer = GetString(item.GetElementsByTagName("NumberOfPressCountBankPayer")[0]);
				xmlPayment.IndicatorNumberOfTaxDocument = GetString(item.GetElementsByTagName("IndicatorNumberOfTaxDocument")[0]);
				xmlPayment.OperationType = GetString(item.GetElementsByTagName("OperationType")[0]);
				xmlPayment.PayerBankBIK = GetString(item.GetElementsByTagName("PayerBankBIK")[0]);
				xmlPayment.PayerBankName = GetString(item.GetElementsByTagName("PayerBankName")[0]);
				xmlPayment.PayerINN = GetString(item.GetElementsByTagName("PayerINN")[0]);
				xmlPayment.PayerKPP = GetString(item.GetElementsByTagName("PayerKPP")[0]);
				xmlPayment.PayerName = GetString(item.GetElementsByTagName("PayerName")[0]);
				xmlPayment.PaymentType = GetString(item.GetElementsByTagName("PaymentType")[0]);
				xmlPayment.PaymentView = GetString(item.GetElementsByTagName("PaymentView")[0]);
				xmlPayment.PayOrder = GetString(item.GetElementsByTagName("PayOrder")[0]);
				xmlPayment.PurposeOfPayment = GetString(item.GetElementsByTagName("PurposeOfPayment")[0]);
				xmlPayment.ReasonOfPayment = GetString(item.GetElementsByTagName("ReasonOfPayment")[0]);
				xmlPayment.RemainderOfPayment = GetNumber(item.GetElementsByTagName("RemainderOfPayment")[0]);
				xmlPayment.StatusProcessingPerson = GetString(item.GetElementsByTagName("StatusProcessingPerson")[0]);
				xmlPayment.TaxPeriod = GetString(item.GetElementsByTagName("TaxPeriod")[0]);
				string g = GetString(item.GetElementsByTagName("ElectronicNumberOfAccountingDocument")[0]);
				try
				{
					xmlPayment.ElectronicNumberOfAccountingDocument = new Guid(g);
				}
				catch (Exception)
				{
					xmlPayment.ElectronicNumberOfAccountingDocument = newGuidDocument(ident, xmlPayment);
				}
				decimal num = (xmlPayment.TotalPay = GetNumber(item.GetElementsByTagName("TotalPay")[0]));
				sUM_TOTAL += num;
			}
			finally
			{
				list.Add(xmlPayment);
			}
		}
		xmlPaymentsFile.SUM_TOTAL = sUM_TOTAL;
		xmlPaymentsFile.XmlPayments = list;
		return (IPaymentsFile)(object)xmlPaymentsFile;
	}

	private DateTime GetDate(XmlNode XmlNode)
	{
		if (XmlNode != null)
		{
			try
			{
				return DateTime.Parse(XmlNode.InnerText);
			}
			catch
			{
			}
		}
		return DateTime.MinValue;
	}

	private decimal GetNumber(XmlNode XmlNode)
	{
		if (XmlNode != null)
		{
			string text = XmlNode.InnerText;
			if (!string.IsNullOrEmpty(text.Trim()))
			{
				string[] array = text.Split(new char[1] { '.' });
				if (array.Length > 1)
				{
					text = array[0] + "," + array[1];
				}
				text = text.Trim();
				try
				{
					return decimal.Parse(text);
				}
				catch
				{
				}
				return 0.00m;
			}
			return 0.00m;
		}
		return 0.00m;
	}

	private string GetString(XmlNode XmlNode)
	{
		if (XmlNode != null)
		{
			return XmlNode.InnerText;
		}
		return "";
	}
}
