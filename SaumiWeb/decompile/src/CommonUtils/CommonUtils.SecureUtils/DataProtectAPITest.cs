using System;

namespace CommonUtils.SecureUtils;

public class DataProtectAPITest
{
	[STAThread]
	private static void Main(string[] args)
	{
		try
		{
			string text = "Hello, world!";
			string entropy = null;
			Console.WriteLine("Plaintext: {0}\r\n", text);
			string text2 = DataProtectAPI.Encrypt(DataProtectAPI.KeyType.UserKey, text, entropy, "My Data");
			Console.WriteLine("Encrypted: {0}\r\n", text2);
			string arg = DataProtectAPI.Decrypt(text2, entropy, out var description);
			Console.WriteLine("Decrypted: {0} <<<{1}>>>\r\n", arg, description);
		}
		catch (Exception innerException)
		{
			while (innerException != null)
			{
				Console.WriteLine(innerException.Message);
				innerException = innerException.InnerException;
			}
		}
	}
}
