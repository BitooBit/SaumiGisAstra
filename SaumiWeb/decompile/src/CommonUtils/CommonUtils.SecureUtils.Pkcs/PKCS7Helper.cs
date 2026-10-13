using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CommonUtils.SecureUtils.Pkcs;

public static class PKCS7Helper
{
	private const string PEM_PKCS7_CRYPTO_PRO_HEADER = "----- BEGIN PKCS7 SIGNED -----";

	private const string PEM_PKCS7_CRYPTO_PRO_FOOTER = "----- END PKCS7 SIGNED -----";

	public static byte[] ComputePKCS7DetachedSignIntoDER(byte[] message, X509Certificate2 certificate, X509IncludeOption includeOption = X509IncludeOption.EndCertOnly)
	{
		return ComputePKCS7DetachedSign(message, certificate, includeOption);
	}

	public static string ComputePKCS7DetachedSignIntoPEM(byte[] message, X509Certificate2 certificate, X509IncludeOption includeOption = X509IncludeOption.EndCertOnly)
	{
		byte[] signature = ComputePKCS7DetachedSign(message, certificate, includeOption);
		return ConvertDerToCryptoProPem(signature);
	}

	public static string ConvertDerToCryptoProPem(byte[] signature)
	{
		string value = Convert.ToBase64String(signature);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("----- BEGIN PKCS7 SIGNED -----");
		stringBuilder.AppendLine(value);
		stringBuilder.AppendLine("----- END PKCS7 SIGNED -----");
		return stringBuilder.ToString();
	}

	public static void CreatePKCS7DetachedSignFile(string sourceFile, string signatureFile, CertificateFileFormats format, X509Certificate2 certificate, X509IncludeOption includeOption = X509IncludeOption.EndCertOnly)
	{
		byte[] array = ComputePKCS7DetachedSignForFile(sourceFile, certificate, includeOption);
		switch (format)
		{
		case CertificateFileFormats.DER:
			File.WriteAllBytes(signatureFile, array);
			break;
		case CertificateFileFormats.PEM:
		{
			string contents = ConvertDerToCryptoProPem(array);
			File.WriteAllText(signatureFile, contents);
			break;
		}
		}
	}

	public static byte[] ComputePKCS7DetachedSignForFile(string sourceFile, X509Certificate2 certificate, X509IncludeOption includeOption = X509IncludeOption.EndCertOnly)
	{
		if (!File.Exists(sourceFile))
		{
			throw new FileNotFoundException("File for sign not found");
		}
		byte[] message = File.ReadAllBytes(sourceFile);
		return ComputePKCS7DetachedSign(message, certificate, includeOption);
	}

	public static byte[] ConvertPemToDer(string signature)
	{
		if (ContainsPemCryptoProParts(signature))
		{
			string[] array = signature.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			StringBuilder stringBuilder = new StringBuilder();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!text.Contains("----- BEGIN PKCS7 SIGNED -----") && !text.Contains("----- END PKCS7 SIGNED -----"))
				{
					stringBuilder.Append(text);
				}
			}
			string s = stringBuilder.ToString();
			return Convert.FromBase64String(s);
		}
		throw new Exception("Illegal PEM signature format");
	}

	public static bool VerifyFileSignature(string signatureFile, string dataFile)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		byte[] array = null;
		array = (IsCryptoProPemFile(signatureFile) ? TryExtractSignatureFromPem(signatureFile) : File.ReadAllBytes(signatureFile));
		byte[] array2 = File.ReadAllBytes(dataFile);
		ContentInfo val = new ContentInfo(array2);
		SignedCms val2 = new SignedCms(val, true);
		val2.Decode(array);
		if (val2.SignerInfos.Count == 0)
		{
			return false;
		}
		SignerInfoEnumerator enumerator = val2.SignerInfos.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignerInfo current = enumerator.Current;
			try
			{
				current.CheckSignature(false);
			}
			catch (CryptographicException)
			{
				return false;
			}
			if (current.CounterSignerInfos.Count <= 0)
			{
				continue;
			}
			SignerInfoEnumerator enumerator2 = current.CounterSignerInfos.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				SignerInfo current2 = enumerator2.Current;
				try
				{
					current2.CheckSignature(false);
				}
				catch (CryptographicException)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool IsCryptoProPemFile(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException("File not found");
		}
		string signature = File.ReadAllText(filePath);
		return ContainsPemCryptoProParts(signature);
	}

	private static byte[] TryExtractSignatureFromPem(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException("File not found");
		}
		string signature = File.ReadAllText(filePath);
		return ConvertPemToDer(signature);
	}

	private static bool ContainsPemCryptoProParts(string signature)
	{
		if (signature.IndexOf("----- BEGIN PKCS7 SIGNED -----", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return signature.IndexOf("----- END PKCS7 SIGNED -----", StringComparison.OrdinalIgnoreCase) >= 0;
		}
		return false;
	}

	public static byte[] ComputePKCS7DetachedSign(byte[] message, X509Certificate2 certificate, X509IncludeOption includeOption = X509IncludeOption.EndCertOnly)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		ContentInfo val = new ContentInfo(message);
		SignedCms val2 = new SignedCms(val, true);
		CmsSigner val3 = new CmsSigner(certificate);
		val3.IncludeOption = includeOption;
		val2.ComputeSignature(val3, false);
		return val2.Encode();
	}
}
