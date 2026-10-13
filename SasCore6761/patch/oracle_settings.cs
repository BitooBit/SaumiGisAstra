// Пишет Settings.xml так же, как AppServerUtils.ConfigurationUtils.SetConnectionSettings:
// DPAPI MachineKey, entropy=null, description="Settings", внутри — Oracle connection string.
// Собирать и запускать только под Wine (тот же prefix, что и служба).
using System;
using System.IO;
using System.Xml;
using CommonUtils.SecureUtils;

internal static class OracleSettings
{
	private static int Fail(string msg)
	{
		Console.Error.WriteLine("FAIL: " + msg);
		return 1;
	}

	private static int Main(string[] args)
	{
		if (args.Length < 1)
		{
			Console.Error.WriteLine("usage: oracle_settings.exe check <Settings.xml>");
			Console.Error.WriteLine("       oracle_settings.exe write <tns> <user> <Settings.xml> [Settings.xml...]");
			Console.Error.WriteLine("       password: env SASCORE_PASSWORD");
			return 2;
		}
		try
		{
			if (args[0] == "check")
			{
				if (args.Length < 2)
					return Fail("check: нет пути Settings.xml");
				return Check(args[1]);
			}
			if (args[0] == "write")
			{
				if (args.Length < 4)
					return Fail("write: tns user Settings.xml");
				string tns = args[1];
				string user = args[2];
				string pass = Environment.GetEnvironmentVariable("SASCORE_PASSWORD") ?? "";
				if (string.IsNullOrEmpty(pass))
					return Fail("нет SASCORE_PASSWORD");
				int rc = 0;
				for (int i = 3; i < args.Length; i++)
				{
					int n = Write(tns, user, pass, args[i]);
					if (n != 0)
						rc = n;
				}
				return rc;
			}
			return Fail("команда " + args[0]);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine("FAIL: " + ex.GetType().Name + ": " + ex.Message);
			if (ex.InnerException != null)
				Console.Error.WriteLine("  inner: " + ex.InnerException.Message);
			return 1;
		}
	}

	private static int Check(string path)
	{
		if (!File.Exists(path))
		{
			Console.WriteLine("NEED_REWRITE нет файла");
			return 2;
		}
		XmlDocument doc = new XmlDocument();
		doc.Load(path);
		XmlNode node = doc.SelectSingleNode("descendant::ConnectionSettings");
		if (node == null || string.IsNullOrEmpty(node.InnerText))
		{
			Console.WriteLine("NEED_REWRITE нет ConnectionSettings");
			return 2;
		}
		try
		{
			string description;
			string plain = DataProtectAPI.Decrypt(node.InnerText.Trim(), null, out description);
			if (string.IsNullOrEmpty(plain) || plain.IndexOf("DATA SOURCE=", StringComparison.OrdinalIgnoreCase) < 0)
			{
				Console.WriteLine("NEED_REWRITE не connection string");
				return 2;
			}
			Console.WriteLine("OK decrypt desc=" + description);
			return 0;
		}
		catch (Exception ex)
		{
			Console.WriteLine("NEED_REWRITE " + ex.Message);
			return 2;
		}
	}

	private static int Write(string tns, string user, string pass, string path)
	{
		string conn = string.Format(
			"DATA SOURCE={0};USER ID=\"{1}\";PASSWORD=\"{2}\";POOLING=FALSE;",
			tns, user, pass);
		string blob = DataProtectAPI.Encrypt(DataProtectAPI.KeyType.MachineKey, conn, null, "Settings");
		string dir = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
			Directory.CreateDirectory(dir);
		XmlWriterSettings xml = new XmlWriterSettings();
		xml.OmitXmlDeclaration = false;
		xml.Indent = false;
		using (XmlWriter w = XmlWriter.Create(path, xml))
		{
			w.WriteStartDocument();
			w.WriteStartElement("ComponentSettings");
			w.WriteElementString("ConnectionSettings", blob);
			w.WriteEndElement();
			w.WriteEndDocument();
		}
		Console.WriteLine("OK wrote " + path);
		return 0;
	}
}
