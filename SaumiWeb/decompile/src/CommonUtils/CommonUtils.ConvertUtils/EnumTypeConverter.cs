using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace CommonUtils.ConvertUtils;

public class EnumTypeConverter : EnumConverter
{
	private Type _enumType;

	public EnumTypeConverter(Type type)
		: base(type)
	{
		_enumType = type;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
	{
		return (object)destType == typeof(string);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
	{
		FieldInfo field = _enumType.GetField(Enum.GetName(_enumType, value));
		DescriptionAttribute descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
		if (descriptionAttribute != null)
		{
			return descriptionAttribute.Description;
		}
		return value.ToString();
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
	{
		return (object)srcType == typeof(string);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		FieldInfo[] fields = _enumType.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			DescriptionAttribute descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
			if (descriptionAttribute != null && (string)value == descriptionAttribute.Description)
			{
				return Enum.Parse(_enumType, fieldInfo.Name);
			}
		}
		return Enum.Parse(_enumType, (string)value);
	}
}
