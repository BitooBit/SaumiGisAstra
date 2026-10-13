using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Payment.Contracts;

namespace DefaultFormat;

[Serializable]
[XmlType("PaymentsFile")]
[XmlRoot("PaymentsFile", IsNullable = false)]
public class XmlPaymentsFile : IPaymentsFile
{
	private List<XmlPayment> _Payments = new List<XmlPayment>();

	public string FROM_KOD_TOFK { get; set; }

	public string FROM_NAME_TOFK { get; set; }

	public string TO_KOD_TOFK { get; set; }

	public string TO_NAME_TOFK { get; set; }

	public string TO_BUDG_LEVEL { get; set; }

	public string TO_KOD_UBP { get; set; }

	public string TO_NAME_UBP { get; set; }

	public string LS_ADB { get; set; }

	public DateTime? DATE_OTCH { get; set; }

	public Guid GUID_OTCH { get; set; }

	public string KOD_DOC { get; set; }

	public string KOL { get; set; }

	public decimal SUM_TOTAL { get; set; }

	public string FormatVersion { get; set; }

	[XmlIgnore]
	public IEnumerable<IPayment> Payments => new List<IPayment>(_Payments.OfType<IPayment>());

	[XmlArrayItem("Payment", IsNullable = false)]
	[XmlArray("Payments", IsNullable = false)]
	public List<XmlPayment> XmlPayments
	{
		get
		{
			return _Payments;
		}
		set
		{
			_Payments = value;
		}
	}
}
