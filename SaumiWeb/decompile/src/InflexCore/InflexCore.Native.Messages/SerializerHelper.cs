using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace InflexCore.Native.Messages;

public class SerializerHelper
{
	public static Dictionary<Type, XmlSerializer> Serializers = new Dictionary<Type, XmlSerializer>();
}
