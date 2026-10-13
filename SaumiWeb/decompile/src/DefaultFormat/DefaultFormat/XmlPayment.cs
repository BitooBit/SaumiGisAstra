using System;
using System.Xml.Serialization;
using Payment.Contracts;

namespace DefaultFormat;

[Serializable]
[XmlType("Payment")]
public class XmlPayment : IPayment
{
	public Guid ElectronicNumberOfAccountingDocument { get; set; }

	public string NumberOfPaymentDocument { get; set; }

	public DateTime? DateCreationPaymentDocument { get; set; }

	public string PaymentView { get; set; }

	public decimal TotalPay { get; set; }

	public DateTime? DatePaymentInBank { get; set; }

	public DateTime? DateChargeOffPayer { get; set; }

	public string StatusProcessingPerson { get; set; }

	public string PayerINN { get; set; }

	public string PayerKPP { get; set; }

	public string PayerName { get; set; }

	public string PayerBankName { get; set; }

	public string NumberOfBankAccountPayer { get; set; }

	public string PayerBankBIK { get; set; }

	public string NumberOfPressCountBankPayer { get; set; }

	public string GranteeINN { get; set; }

	public string GranteeKPP { get; set; }

	public string GranteeName { get; set; }

	public string GranteeBankName { get; set; }

	public string GranteeBankBIK { get; set; }

	public string NumberOfBankAccountGrantee { get; set; }

	public string NumberOfPressCountBankGrantee { get; set; }

	public string OperationType { get; set; }

	public string PayOrder { get; set; }

	public string NumberOfPartialPayment { get; set; }

	public string CipherOfAccountingDocument { get; set; }

	public string NumberOfAccountingDocument { get; set; }

	public DateTime? DateOfAccountingDocument { get; set; }

	public DateTime? Maturity { get; set; }

	public decimal? RemainderOfPayment { get; set; }

	public string ContentsOfOperation { get; set; }

	public string CodeKBK { get; set; }

	public string CodeOKATO { get; set; }

	public string ReasonOfPayment { get; set; }

	public string TaxPeriod { get; set; }

	public string IndicatorNumberOfTaxDocument { get; set; }

	public string IndicatorDateOfDocument { get; set; }

	public string PaymentType { get; set; }

	public string PurposeOfPayment { get; set; }

	public DateTime? DateOfEnrolmentOfPayment { get; set; }

	public string KBK { get; set; }

	public string TypeKBK { get; set; }

	public string AimCode { get; set; }

	public string NumBO { get; set; }

	public string ОКАТО { get; set; }

	public decimal Sum { get; set; }

	public string SumDirection { get; set; }

	public string MonthFinans { get; set; }

	public string Rezerv { get; set; }

	public string GmpSupplierBillId { get; set; }

	public string GmpPaymentSysId { get; set; }

	public string GmpPayerId { get; set; }

	[XmlIgnore]
	public string DateExcerpt
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public override string ToString()
	{
		return PurposeOfPayment;
	}
}
