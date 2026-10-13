using System;
using System.Xml.Serialization;

namespace InflexCore.Native.Messages;

[Serializable]
public class MessageInfo
{
	public string Key { get; set; }

	public string MessageType { get; set; }

	public string Message { get; set; }

	public MessageInfo()
	{
	}

	public MessageInfo(string key, string messageType, string message)
	{
		Key = key;
		MessageType = messageType;
		Message = message;
	}

	public MessageInfo(string key, MessageArgument messageArgument)
	{
		Key = key;
		Type type = messageArgument.GetType();
		MessageType = type.FullName;
		XmlSerializer self = new XmlSerializer(type);
		Message = self.SerializeToString(messageArgument);
	}

	public string ToXmlString()
	{
		XmlSerializer self = new XmlSerializer(typeof(MessageInfo));
		return self.SerializeToString(this);
	}
}
