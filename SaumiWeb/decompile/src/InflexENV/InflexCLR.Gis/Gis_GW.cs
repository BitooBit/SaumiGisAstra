using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using InflexCLR.Win32;
using Microsoft.Win32;

namespace InflexCLR.Gis;

public static class Gis_GW
{
	private class MessageProcessor
	{
		private string _Result;

		private string _Exception;

		private int _ID;

		public string Result
		{
			get
			{
				return _Result;
			}
			set
			{
				_Result = value;
			}
		}

		public string Exception
		{
			get
			{
				return _Exception;
			}
			set
			{
				_Exception = value;
			}
		}

		public int ID
		{
			get
			{
				return _ID;
			}
			set
			{
				_ID = value;
			}
		}

		public MessageProcessor(string Mess)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(Mess);
			ID = int.Parse(xmlDocument.DocumentElement.Attributes["id"].Value);
			foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
			{
				if (childNode.Name == "result")
				{
					Result = childNode.InnerXml;
				}
				if (childNode.Name == "exception")
				{
					Exception = childNode.InnerXml;
				}
			}
		}

		public void Process()
		{
			if (!string.IsNullOrEmpty(Exception))
			{
				throw new Exception("mmp32.Exception: " + Exception);
			}
		}
	}

	private class MessageCreator
	{
		private static int _SeqID;

		private XmlDocument _Doc = new XmlDocument();

		private XmlNode _LastNode;

		private int _ID;

		public int ID
		{
			get
			{
				return _ID;
			}
			set
			{
				_ID = value;
			}
		}

		public MessageCreator()
		{
			_SeqID++;
			ID = _SeqID;
		}

		public void AddMethod(string FullName)
		{
			_Doc.LoadXml("<method/>");
			_Doc.InsertBefore(_Doc.CreateXmlDeclaration("1.0", "windows-1251", "yes"), _Doc.DocumentElement);
			XmlAttribute xmlAttribute = _Doc.CreateAttribute("fullname");
			xmlAttribute.Value = FullName;
			_Doc.DocumentElement.Attributes.Append(xmlAttribute);
			xmlAttribute = _Doc.CreateAttribute("id");
			xmlAttribute.Value = ID.ToString();
			_Doc.DocumentElement.Attributes.Append(xmlAttribute);
		}

		public void AddParametr(string Name, object Value)
		{
			_LastNode = _Doc.CreateElement("parameter");
			XmlAttribute xmlAttribute = _Doc.CreateAttribute("name");
			xmlAttribute.Value = Name;
			_LastNode.Attributes.Append(xmlAttribute);
			if (Value != null)
			{
				_LastNode.InnerText = Value.ToString();
			}
			_Doc.DocumentElement.AppendChild(_LastNode);
		}

		public void AddParametr(string Name)
		{
			AddParametr(Name, null);
		}

		public void AddParametrValue(string Name, object Value)
		{
			XmlNode xmlNode = _Doc.CreateElement(Name.ToLower());
			xmlNode.InnerText = Value.ToString();
			_LastNode.AppendChild(xmlNode);
		}

		public override string ToString()
		{
			return _Doc.OuterXml;
		}
	}

	public const int SAUMI_GIS_KEY = 73954;

	private static IntPtr _ToForm;

	private static ReceiveForm _Form;

	private static Dictionary<int, string> _ReceiveMessages;

	public static IntPtr ToForm
	{
		get
		{
			return _ToForm;
		}
		set
		{
			_ToForm = value;
		}
	}

	private static IntPtr _FromForm => ((Control)_Form).Handle;

	static Gis_GW()
	{
		_ReceiveMessages = new Dictionary<int, string>();
		_Form = new ReceiveForm();
	}

	private static void GetSaumiHandle()
	{
		try
		{
			int num = (int)Registry.GetValue("HKEY_CURRENT_USER\\Software\\XS\\Inflex\\Registered\\SAUMI30", "Handle", 0);
			ToForm = (IntPtr)num;
		}
		catch
		{
		}
	}

	public static void CreateMetafileByExtentWithSize(int ContextID, Gis_Extent Extent, string FileName, int DPI, int ImageWidth, int ImageHeight)
	{
		MessageCreator messageCreator = new MessageCreator();
		messageCreator.AddMethod("TGisGW.CreateMetafileByExtentWithSize");
		messageCreator.AddParametr("ContextID", ContextID);
		messageCreator.AddParametr("Extent");
		messageCreator.AddParametrValue("xmin", Extent.XMin);
		messageCreator.AddParametrValue("ymin", Extent.YMin);
		messageCreator.AddParametrValue("xmax", Extent.XMax);
		messageCreator.AddParametrValue("ymax", Extent.YMax);
		messageCreator.AddParametr("FileName", FileName);
		messageCreator.AddParametr("DPI", DPI);
		messageCreator.AddParametr("ImageWidth", ImageWidth);
		messageCreator.AddParametr("ImageHeight", ImageHeight);
		SendAndReceiveMessage(messageCreator);
	}

	public static void CreateMetafileByExtentWithScale(int ContextID, Gis_Extent Extent, string FileName, int DPI, double Scale)
	{
		MessageCreator messageCreator = new MessageCreator();
		messageCreator.AddMethod("TGisGW.CreateMetafileByExtentWithScale");
		messageCreator.AddParametr("ContextID", ContextID);
		messageCreator.AddParametr("Extent");
		messageCreator.AddParametrValue("xmin", Extent.XMin);
		messageCreator.AddParametrValue("ymin", Extent.YMin);
		messageCreator.AddParametrValue("xmax", Extent.XMax);
		messageCreator.AddParametrValue("ymax", Extent.YMax);
		messageCreator.AddParametr("FileName", FileName);
		messageCreator.AddParametr("DPI", DPI);
		messageCreator.AddParametr("Scale", Scale);
		SendAndReceiveMessage(messageCreator);
	}

	public static void CreateMetafileByLambdaSql(int ContextID, string LambdaSQL, string FileName, int DPI, int ImageWidth, int ImageHeight, double MarginFactor, int LayerIdForFind)
	{
		MessageCreator messageCreator = new MessageCreator();
		messageCreator.AddMethod("TGisGW.CreateMetafileByLambdaSql");
		messageCreator.AddParametr("ContextID", ContextID);
		messageCreator.AddParametr("LambdaSQL", LambdaSQL);
		messageCreator.AddParametr("FileName", FileName);
		messageCreator.AddParametr("DPI", DPI);
		messageCreator.AddParametr("ImageWidth", ImageWidth);
		messageCreator.AddParametr("ImageHeight", ImageHeight);
		messageCreator.AddParametr("MarginFactor", MarginFactor);
		messageCreator.AddParametr("LayerIdForFind", LayerIdForFind);
		SendAndReceiveMessage(messageCreator);
	}

	public static double GetMaximalScale(int ContextID, string LambdaSQL, int DPI, int ImageWidth, int ImageHeight, double MarginFactor)
	{
		MessageCreator messageCreator = new MessageCreator();
		messageCreator.AddMethod("TGisGW.GetMaximalScale");
		messageCreator.AddParametr("ContextID", ContextID);
		messageCreator.AddParametr("LambdaSQL", LambdaSQL);
		messageCreator.AddParametr("DPI", DPI);
		messageCreator.AddParametr("ImageWidth", ImageWidth);
		messageCreator.AddParametr("ImageHeight", ImageHeight);
		messageCreator.AddParametr("MarginFactor", MarginFactor);
		return double.Parse(SendAndReceiveMessage(messageCreator));
	}

	private static string SendAndReceiveMessage(MessageCreator mc)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		GetSaumiHandle();
		Message msg = WMHelper.CreateCopyDataMessage(_FromForm, _ToForm, mc.ToString(), 73954, out var ptr);
		WMHelper.Send(ref msg);
		if (ptr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(ptr);
		}
		if (IsReceiveMessage(mc.ID))
		{
			MessageProcessor messageProcessor = new MessageProcessor(GetReceiveMessage(mc.ID));
			RemoveReceiveMessage(mc.ID);
			messageProcessor.Process();
			return messageProcessor.Result;
		}
		TryUseGlobalExcepion();
		throw new Exception("Not Receive");
	}

	private static void TryUseGlobalExcepion()
	{
		int num = -1;
		if (_ReceiveMessages.ContainsKey(num))
		{
			MessageProcessor messageProcessor = new MessageProcessor(GetReceiveMessage(num));
			RemoveReceiveMessage(num);
			messageProcessor.Process();
		}
	}

	public static void AddReceiveMessage(string Mess)
	{
		MessageProcessor messageProcessor = new MessageProcessor(Mess);
		_ReceiveMessages.Add(messageProcessor.ID, Mess);
	}

	public static void RemoveReceiveMessage(int MessID)
	{
		_ReceiveMessages.Remove(MessID);
	}

	public static string GetReceiveMessage(int MessID)
	{
		return _ReceiveMessages[MessID];
	}

	public static bool IsReceiveMessage(int MessID)
	{
		return _ReceiveMessages.ContainsKey(MessID);
	}
}
