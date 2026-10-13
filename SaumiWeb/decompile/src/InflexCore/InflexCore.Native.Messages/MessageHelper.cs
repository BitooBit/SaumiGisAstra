using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Xml.Serialization;
using InflexCore.log;

namespace InflexCore.Native.Messages;

public static class MessageHelper
{
	private class MessageForm : Form
	{
		protected override void WndProc(ref Message m)
		{
			if (((Message)(ref m)).Msg == 74 && PerformCopyData(ref m))
			{
				return;
			}
			if (((Message)(ref m)).Msg >= 1024 || ((Message)(ref m)).Msg == 74)
			{
				MessageHelperUserHandler userMessage = UserMessage;
				if (userMessage != null)
				{
					userMessage(ref m);
					return;
				}
			}
			((Form)this).WndProc(ref m);
		}
	}

	private const int _WM_COPYDATA = 74;

	private const int _WM_USER = 1024;

	private const int _IWM_BASE = 1065;

	private const int _IWM_PING = 1066;

	private const int _MESSAGE_INFO_KEY = 12918;

	private static Dictionary<Type, string> _PMCD_KEYS;

	private static Dictionary<string, Type> _PMCD_TYPES;

	private static Dictionary<int, MessageArgument> _Results;

	private static MessageForm _Form;

	private static bool _First;

	public static int WM_USER => 1024;

	public static int IWM_PING => 1066;

	public static int MESSAGE_INFO_KEY => 12918;

	public static IntPtr Handle => ((Control)_Form).Handle;

	public static event MessageHelperUserHandler UserMessage;

	public static event MessageHelperUserInflexHandler UserInflexMessage;

	static MessageHelper()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		_PMCD_KEYS = new Dictionary<Type, string>();
		_PMCD_TYPES = new Dictionary<string, Type>();
		_Results = new Dictionary<int, MessageArgument>();
		_First = true;
		_Form = new MessageForm();
		IntPtr handle = ((Control)_Form).Handle;
		HwndSource val = HwndSource.FromHwnd(handle);
		if (val != null)
		{
			val.AddHook(new HwndSourceHook(OuterWindowProc));
		}
	}

	private static IntPtr OuterWindowProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
	{
		handled = false;
		return IntPtr.Zero;
	}

	public static void RegisterMessageArgumentType(string key, Type type)
	{
		Type typeFromHandle = typeof(MessageArgument);
		if (type.IsSubclassOf(typeFromHandle))
		{
			_PMCD_KEYS.Add(type, key);
			_PMCD_TYPES.Add(key, type);
		}
	}

	public static int Send(int handle, MessageArgument sendArgument)
	{
		MessageArgument resultArgument = null;
		return Send(handle, sendArgument, out resultArgument);
	}

	public static int Send(int handle, MessageArgument sendArgument, out MessageArgument resultArgument)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		using CopyDataMessage copyDataMessage = CreateCopyDataMessage((IntPtr)handle, sendArgument);
		Message m = copyDataMessage.Message;
		try
		{
			int num = 0;
			return SendWindowMessage(ref m);
		}
		finally
		{
			resultArgument = GetResultMessage(copyDataMessage);
		}
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	[DllImport("Kernel32", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
	private static extern uint GetCurrentWin32ThreadId();

	[DllImport("user32.dll")]
	private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetForegroundWindow(IntPtr hWnd);

	public static void CurrentApplicationBringToFront(IntPtr extHandle)
	{
		if (!SetForegroundWindow(Handle))
		{
			uint windowThreadProcessId = GetWindowThreadProcessId(extHandle, out var _);
			uint currentWin32ThreadId = GetCurrentWin32ThreadId();
			AttachThreadInput(currentWin32ThreadId, windowThreadProcessId, fAttach: true);
			try
			{
				bool flag = SetForegroundWindow(Handle);
			}
			finally
			{
				AttachThreadInput(currentWin32ThreadId, windowThreadProcessId, fAttach: false);
			}
		}
	}

	private static MessageArgument GetResultMessage(CopyDataMessage message)
	{
		MessageArgument value = null;
		if (_Results.TryGetValue(message.ID, out value))
		{
			_Results.Remove(message.ID);
		}
		return value;
	}

	private static CopyDataMessage CreateCopyDataMessage(IntPtr toHandle, MessageArgument argument)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		Type type = argument.GetType();
		string value = null;
		if (!_PMCD_KEYS.TryGetValue(type, out value))
		{
			throw new Exception(type.FullName + " not register for sending.");
		}
		string text = GetMessageInfo(argument).ToXmlString();
		text += "\0";
		CopyDataStruct copyDataStruct = new CopyDataStruct
		{
			dwData = (IntPtr)12918,
			lpData = Marshal.StringToHGlobalAnsi(text),
			cbData = text.Length
		};
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf((object)copyDataStruct));
		try
		{
			Marshal.StructureToPtr((object)copyDataStruct, intPtr, false);
			CopyDataMessage copyDataMessage = new CopyDataMessage();
			copyDataMessage.Message = Message.Create(toHandle, 74, ((Control)_Form).Handle, intPtr);
			copyDataMessage.Data = text;
			copyDataMessage.CopyDataStruct = copyDataStruct;
			copyDataMessage.UnmanagedPointer = intPtr;
			CopyDataMessage copyDataMessage2 = copyDataMessage;
			copyDataMessage2.Link(argument);
			return copyDataMessage2;
		}
		catch
		{
			Marshal.FreeHGlobal(intPtr);
			throw;
		}
	}

	public static int SendWindowMessage(ref Message m)
	{
		int num = SendMessage(((Message)(ref m)).HWnd, ((Message)(ref m)).Msg, ((Message)(ref m)).WParam, ((Message)(ref m)).LParam);
		((Message)(ref m)).Result = (IntPtr)num;
		return num;
	}

	public static void PostWindowMessage(ref Message m)
	{
		if (!PostMessage(((Message)(ref m)).HWnd, ((Message)(ref m)).Msg, ((Message)(ref m)).WParam, ((Message)(ref m)).LParam))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}

	private static bool PerformCopyData(ref Message m)
	{
		if (((Message)(ref m)).Msg != 74)
		{
			return false;
		}
		CopyDataStruct copyDataStruct = default(CopyDataStruct);
		copyDataStruct = (CopyDataStruct)Marshal.PtrToStructure(((Message)(ref m)).LParam, typeof(CopyDataStruct));
		if (copyDataStruct.dwData.ToInt32() != 12918)
		{
			return false;
		}
		string text = Marshal.PtrToStringAnsi(copyDataStruct.lpData);
		text = ((text.Length > copyDataStruct.cbData) ? text.Substring(0, copyDataStruct.cbData) : text);
		MessageInfo messageInfo = GetMessageInfo(text);
		if (messageInfo == null)
		{
			return false;
		}
		MessageArgument messageArgument = GetMessageArgument(messageInfo);
		if (messageArgument != null)
		{
			if (messageArgument is MessageAnswerArgument messageAnswerArgument)
			{
				_Results.Add(messageAnswerArgument.ParentID, messageArgument);
			}
			else
			{
				UserInflexMessage?.Invoke(ref m, messageArgument);
			}
			return true;
		}
		return false;
	}

	public static MessageInfo GetMessageInfo(MessageArgument argument)
	{
		string value = null;
		try
		{
			Activator.CreateInstance(argument.GetType());
		}
		catch (Exception)
		{
		}
		if (_PMCD_KEYS.TryGetValue(argument.GetType(), out value))
		{
			return new MessageInfo(value, argument);
		}
		return null;
	}

	public static MessageInfo GetMessageInfo(string xmlString)
	{
		try
		{
			XmlSerializer self = new XmlSerializer(typeof(MessageInfo));
			return self.DeserializeFromString<MessageInfo>(xmlString);
		}
		catch
		{
			return null;
		}
	}

	public static MessageArgument GetMessageArgument(MessageInfo messageInfo)
	{
		Type value = null;
		if (!_PMCD_TYPES.TryGetValue(messageInfo.Key, out value))
		{
			try
			{
				value = Type.GetType(messageInfo.MessageType);
				if ((object)value == null)
				{
					string value2 = messageInfo.MessageType.Split(new char[1] { '.' })[0].ToLower();
					Assembly assembly = null;
					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
					foreach (Assembly assembly2 in assemblies)
					{
						try
						{
							if (!string.IsNullOrEmpty(assembly2.Location) && assembly2.Location.ToLower().Contains(value2))
							{
								assembly = assembly2;
								break;
							}
						}
						catch (Exception ex)
						{
							FastLog.Loger<MessageInfo>().Error((object)"Assembly.Location", ex);
						}
					}
					if ((object)assembly != null)
					{
						value = assembly.GetType(messageInfo.MessageType);
					}
				}
			}
			catch (Exception ex2)
			{
				FastLog.Loger<MessageInfo>().Error((object)"type = Type.GetType(messageInfo.MessageType)", ex2);
			}
		}
		if ((object)value != null)
		{
			XmlSerializer self = new XmlSerializer(value);
			return self.DeserializeFromString<MessageArgument>(messageInfo.Message);
		}
		return null;
	}

	[DllImport("user32.dll")]
	private static extern int SendMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

	public static string SerializeToString(this XmlSerializer self, object obj)
	{
		string text = null;
		using MemoryStream memoryStream = new MemoryStream();
		self.Serialize(memoryStream, obj);
		memoryStream.Position = 0L;
		using StreamReader streamReader = new StreamReader(memoryStream);
		return streamReader.ReadToEnd();
	}

	public static T DeserializeFromString<T>(this XmlSerializer self, string str)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using StreamWriter streamWriter = new StreamWriter(memoryStream);
		streamWriter.Write(str);
		streamWriter.Flush();
		memoryStream.Position = 0L;
		try
		{
			return (T)self.Deserialize(memoryStream);
		}
		catch
		{
			return default(T);
		}
	}
}
