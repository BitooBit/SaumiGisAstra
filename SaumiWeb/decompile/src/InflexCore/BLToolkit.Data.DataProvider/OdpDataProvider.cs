using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using BLToolkit.Aspects;
using BLToolkit.Common;
using BLToolkit.Data.Sql.SqlProvider;
using BLToolkit.Mapping;
using BLToolkit.Reflection;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace BLToolkit.Data.DataProvider;

public class OdpDataProvider : DataProviderBase
{
	public class OdpMappingSchema : MappingSchema
	{
		public override DataReaderMapper CreateDataReaderMapper(IDataReader dataReader)
		{
			return new OracleDataReaderMapper(this, dataReader);
		}

		public override DataReaderMapper CreateDataReaderMapper(IDataReader dataReader, NameOrIndexParameter nip)
		{
			return new OracleScalarDataReaderMapper(this, dataReader, nip);
		}

		[CLSCompliant(false)]
		public override sbyte ConvertToSByte(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return (sbyte)oracleDecimal.Value;
				}
				return base.DefaultSByteNullValue;
			}
			return base.ConvertToSByte(value);
		}

		public override short ConvertToInt16(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToInt16();
				}
				return base.DefaultInt16NullValue;
			}
			return base.ConvertToInt16(value);
		}

		public override int ConvertToInt32(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToInt32();
				}
				return base.DefaultInt32NullValue;
			}
			return base.ConvertToInt32(value);
		}

		public override long ConvertToInt64(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToInt64();
				}
				return base.DefaultInt64NullValue;
			}
			return base.ConvertToInt64(value);
		}

		public override byte ConvertToByte(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToByte();
				}
				return base.DefaultByteNullValue;
			}
			return base.ConvertToByte(value);
		}

		[CLSCompliant(false)]
		public override ushort ConvertToUInt16(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return (ushort)oracleDecimal.Value;
				}
				return base.DefaultUInt16NullValue;
			}
			return base.ConvertToUInt16(value);
		}

		[CLSCompliant(false)]
		public override uint ConvertToUInt32(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return (uint)oracleDecimal.Value;
				}
				return base.DefaultUInt32NullValue;
			}
			return base.ConvertToUInt32(value);
		}

		[CLSCompliant(false)]
		public override ulong ConvertToUInt64(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return (ulong)oracleDecimal.Value;
				}
				return base.DefaultUInt64NullValue;
			}
			return base.ConvertToUInt64(value);
		}

		public override float ConvertToSingle(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToSingle();
				}
				return base.DefaultSingleNullValue;
			}
			return base.ConvertToSingle(value);
		}

		public override double ConvertToDouble(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToDouble();
				}
				return base.DefaultDoubleNullValue;
			}
			return base.ConvertToDouble(value);
		}

		public override bool ConvertToBoolean(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.Value != 0m;
				}
				return base.DefaultBooleanNullValue;
			}
			return base.ConvertToBoolean(value);
		}

		public override DateTime ConvertToDateTime(object value)
		{
			if (value is OracleDate oracleDate)
			{
				if (!oracleDate.IsNull)
				{
					return oracleDate.Value;
				}
				return base.DefaultDateTimeNullValue;
			}
			return base.ConvertToDateTime(value);
		}

		public override decimal ConvertToDecimal(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.Value;
				}
				return base.DefaultDecimalNullValue;
			}
			return base.ConvertToDecimal(value);
		}

		public override Guid ConvertToGuid(object value)
		{
			if (value is OracleString oracleString)
			{
				if (!oracleString.IsNull)
				{
					return new Guid(oracleString.Value);
				}
				return base.DefaultGuidNullValue;
			}
			if (value is OracleBlob)
			{
				OracleBlob oracleBlob = (OracleBlob)value;
				if (!oracleBlob.IsEmpty)
				{
					return new Guid(oracleBlob.Value);
				}
				return base.DefaultGuidNullValue;
			}
			return base.ConvertToGuid(value);
		}

		public override string ConvertToString(object value)
		{
			if (value is OracleString oracleString)
			{
				if (!oracleString.IsNull)
				{
					return oracleString.Value;
				}
				return base.DefaultStringNullValue;
			}
			if (value is OracleXmlType)
			{
				OracleXmlType oracleXmlType = (OracleXmlType)value;
				if (!oracleXmlType.IsEmpty)
				{
					return oracleXmlType.Value;
				}
				return base.DefaultStringNullValue;
			}
			if (value is OracleClob)
			{
				OracleClob oracleClob = (OracleClob)value;
				if (!oracleClob.IsEmpty)
				{
					return oracleClob.Value;
				}
				return base.DefaultStringNullValue;
			}
			return base.ConvertToString(value);
		}

		public override Stream ConvertToStream(object value)
		{
			if (value is OracleXmlType)
			{
				OracleXmlType oracleXmlType = (OracleXmlType)value;
				if (!oracleXmlType.IsEmpty)
				{
					return oracleXmlType.GetStream();
				}
				return base.DefaultStreamNullValue;
			}
			return base.ConvertToStream(value);
		}

		public override XmlReader ConvertToXmlReader(object value)
		{
			if (value is OracleXmlType)
			{
				OracleXmlType oracleXmlType = (OracleXmlType)value;
				if (!oracleXmlType.IsEmpty)
				{
					return oracleXmlType.GetXmlReader();
				}
				return base.DefaultXmlReaderNullValue;
			}
			return base.ConvertToXmlReader(value);
		}

		public override XmlDocument ConvertToXmlDocument(object value)
		{
			if (value is OracleXmlType)
			{
				OracleXmlType oracleXmlType = (OracleXmlType)value;
				if (!oracleXmlType.IsEmpty)
				{
					return oracleXmlType.GetXmlDocument();
				}
				return base.DefaultXmlDocumentNullValue;
			}
			return base.ConvertToXmlDocument(value);
		}

		public override byte[] ConvertToByteArray(object value)
		{
			if (value is OracleBlob)
			{
				OracleBlob oracleBlob = (OracleBlob)value;
				if (!oracleBlob.IsEmpty)
				{
					return oracleBlob.Value;
				}
				return null;
			}
			if (value is OracleBinary oracleBinary)
			{
				if (!oracleBinary.IsNull)
				{
					return oracleBinary.Value;
				}
				return null;
			}
			if (value is OracleBFile)
			{
				OracleBFile oracleBFile = (OracleBFile)value;
				if (!oracleBFile.IsEmpty)
				{
					return oracleBFile.Value;
				}
				return null;
			}
			return base.ConvertToByteArray(value);
		}

		public override char[] ConvertToCharArray(object value)
		{
			if (value is OracleString oracleString)
			{
				if (!oracleString.IsNull)
				{
					return oracleString.Value.ToCharArray();
				}
				return null;
			}
			if (value is OracleClob)
			{
				OracleClob oracleClob = (OracleClob)value;
				if (!oracleClob.IsEmpty)
				{
					return oracleClob.Value.ToCharArray();
				}
				return null;
			}
			return base.ConvertToCharArray(value);
		}

		[CLSCompliant(false)]
		public override sbyte? ConvertToNullableSByte(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return (sbyte)oracleDecimal.Value;
				}
				return null;
			}
			return base.ConvertToNullableSByte(value);
		}

		public override short? ConvertToNullableInt16(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToInt16();
				}
				return null;
			}
			return base.ConvertToNullableInt16(value);
		}

		public override int? ConvertToNullableInt32(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToInt32();
				}
				return null;
			}
			return base.ConvertToNullableInt32(value);
		}

		public override long? ConvertToNullableInt64(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToInt64();
				}
				return null;
			}
			return base.ConvertToNullableInt64(value);
		}

		public override byte? ConvertToNullableByte(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToByte();
				}
				return null;
			}
			return base.ConvertToNullableByte(value);
		}

		[CLSCompliant(false)]
		public override ushort? ConvertToNullableUInt16(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return (ushort)oracleDecimal.Value;
				}
				return null;
			}
			return base.ConvertToNullableUInt16(value);
		}

		[CLSCompliant(false)]
		public override uint? ConvertToNullableUInt32(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return (uint)oracleDecimal.Value;
				}
				return null;
			}
			return base.ConvertToNullableUInt32(value);
		}

		[CLSCompliant(false)]
		public override ulong? ConvertToNullableUInt64(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return (ulong)oracleDecimal.Value;
				}
				return null;
			}
			return base.ConvertToNullableUInt64(value);
		}

		public override float? ConvertToNullableSingle(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToSingle();
				}
				return null;
			}
			return base.ConvertToNullableSingle(value);
		}

		public override double? ConvertToNullableDouble(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.ToDouble();
				}
				return null;
			}
			return base.ConvertToNullableDouble(value);
		}

		public override bool? ConvertToNullableBoolean(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.Value != 0m;
				}
				return null;
			}
			return base.ConvertToNullableBoolean(value);
		}

		public override DateTime? ConvertToNullableDateTime(object value)
		{
			if (value is OracleDate oracleDate)
			{
				if (!oracleDate.IsNull)
				{
					return oracleDate.Value;
				}
				return null;
			}
			return base.ConvertToNullableDateTime(value);
		}

		public override decimal? ConvertToNullableDecimal(object value)
		{
			if (value is OracleDecimal oracleDecimal)
			{
				if (!oracleDecimal.IsNull)
				{
					return oracleDecimal.Value;
				}
				return null;
			}
			return base.ConvertToNullableDecimal(value);
		}

		public override Guid? ConvertToNullableGuid(object value)
		{
			if (value is OracleString oracleString)
			{
				if (!oracleString.IsNull)
				{
					return new Guid(oracleString.Value);
				}
				return null;
			}
			if (value is OracleBlob)
			{
				OracleBlob oracleBlob = (OracleBlob)value;
				if (!oracleBlob.IsEmpty)
				{
					return new Guid(oracleBlob.Value);
				}
				return null;
			}
			return base.ConvertToNullableGuid(value);
		}

		public override object MapValueToEnum(object value, Type type)
		{
			if (value is OracleString oracleString)
			{
				value = (oracleString.IsNull ? null : oracleString.Value);
			}
			else if (value is OracleDecimal oracleDecimal)
			{
				value = ((!oracleDecimal.IsNull) ? ((object)oracleDecimal.Value) : null);
			}
			return base.MapValueToEnum(value, type);
		}

		public override object ConvertChangeType(object value, Type conversionType)
		{
			return base.ConvertChangeType(IsNull(value) ? null : value, conversionType);
		}

		public override bool IsNull(object value)
		{
			if (!(value is OracleDecimal oracleDecimal))
			{
				if (!(value is OracleString oracleString))
				{
					if (!(value is OracleDate oracleDate))
					{
						if (!(value is OracleTimeStamp oracleTimeStamp))
						{
							if (!(value is OracleTimeStampTZ oracleTimeStampTZ))
							{
								if (!(value is OracleTimeStampLTZ oracleTimeStampLTZ))
								{
									if (!(value is OracleBinary oracleBinary))
									{
										if (!(value is OracleIntervalDS oracleIntervalDS))
										{
											if (!(value is OracleIntervalYM oracleIntervalYM))
											{
												return base.IsNull(value);
											}
											return oracleIntervalYM.IsNull;
										}
										return oracleIntervalDS.IsNull;
									}
									return oracleBinary.IsNull;
								}
								return oracleTimeStampLTZ.IsNull;
							}
							return oracleTimeStampTZ.IsNull;
						}
						return oracleTimeStamp.IsNull;
					}
					return oracleDate.IsNull;
				}
				return oracleString.IsNull;
			}
			return oracleDecimal.IsNull;
		}
	}

	public class OracleDataReaderMapper : DataReaderMapper
	{
		private readonly OracleDataReader _dataReader;

		public OracleDataReaderMapper(MappingSchema mappingSchema, IDataReader dataReader)
			: base(mappingSchema, dataReader)
		{
			_dataReader = (OracleDataReader)dataReader;
		}

		public override Type GetFieldType(int index)
		{
			Type fieldType = _dataReader.GetFieldType(index);
			if ((object)fieldType != typeof(OracleXmlType) && (object)fieldType != typeof(OracleBlob))
			{
				fieldType = _dataReader.GetFieldType(index);
			}
			return fieldType;
		}

		public override object GetValue(object o, int index)
		{
			Type fieldType = _dataReader.GetFieldType(index);
			if ((object)fieldType == typeof(OracleXmlType))
			{
				OracleXmlType oracleXmlType = _dataReader.GetOracleXmlType(index);
				return base.MappingSchema.ConvertToXmlDocument(oracleXmlType);
			}
			if ((object)fieldType == typeof(OracleBlob))
			{
				OracleBlob oracleBlob = _dataReader.GetOracleBlob(index);
				return base.MappingSchema.ConvertToStream(oracleBlob);
			}
			if (!_dataReader.IsDBNull(index))
			{
				return _dataReader.GetValue(index);
			}
			return null;
		}

		public override bool GetBoolean(object o, int index)
		{
			return base.MappingSchema.ConvertToBoolean(GetValue(o, index));
		}

		public override char GetChar(object o, int index)
		{
			return base.MappingSchema.ConvertToChar(GetValue(o, index));
		}

		public override Guid GetGuid(object o, int index)
		{
			return base.MappingSchema.ConvertToGuid(GetValue(o, index));
		}

		[CLSCompliant(false)]
		public override sbyte GetSByte(object o, int index)
		{
			return (sbyte)_dataReader.GetDecimal(index);
		}

		[CLSCompliant(false)]
		public override ushort GetUInt16(object o, int index)
		{
			return (ushort)_dataReader.GetDecimal(index);
		}

		[CLSCompliant(false)]
		public override uint GetUInt32(object o, int index)
		{
			return (uint)_dataReader.GetDecimal(index);
		}

		[CLSCompliant(false)]
		public override ulong GetUInt64(object o, int index)
		{
			return (ulong)_dataReader.GetDecimal(index);
		}

		public override decimal GetDecimal(object o, int index)
		{
			return OracleDecimal.SetPrecision(_dataReader.GetOracleDecimal(index), 28).Value;
		}

		public override bool? GetNullableBoolean(object o, int index)
		{
			return base.MappingSchema.ConvertToNullableBoolean(GetValue(o, index));
		}

		public override char? GetNullableChar(object o, int index)
		{
			return base.MappingSchema.ConvertToNullableChar(GetValue(o, index));
		}

		public override Guid? GetNullableGuid(object o, int index)
		{
			return base.MappingSchema.ConvertToNullableGuid(GetValue(o, index));
		}

		[CLSCompliant(false)]
		public override sbyte? GetNullableSByte(object o, int index)
		{
			if (!_dataReader.IsDBNull(index))
			{
				return (sbyte)_dataReader.GetDecimal(index);
			}
			return null;
		}

		[CLSCompliant(false)]
		public override ushort? GetNullableUInt16(object o, int index)
		{
			if (!_dataReader.IsDBNull(index))
			{
				return (ushort)_dataReader.GetDecimal(index);
			}
			return null;
		}

		[CLSCompliant(false)]
		public override uint? GetNullableUInt32(object o, int index)
		{
			if (!_dataReader.IsDBNull(index))
			{
				return (uint)_dataReader.GetDecimal(index);
			}
			return null;
		}

		[CLSCompliant(false)]
		public override ulong? GetNullableUInt64(object o, int index)
		{
			if (!_dataReader.IsDBNull(index))
			{
				return (ulong)_dataReader.GetDecimal(index);
			}
			return null;
		}

		public override decimal? GetNullableDecimal(object o, int index)
		{
			if (!_dataReader.IsDBNull(index))
			{
				return OracleDecimal.SetPrecision(_dataReader.GetOracleDecimal(index), 28).Value;
			}
			return null;
		}
	}

	public class OracleScalarDataReaderMapper : ScalarDataReaderMapper
	{
		private readonly OracleDataReader _dataReader;

		private readonly Type _fieldType;

		public OracleScalarDataReaderMapper(MappingSchema mappingSchema, IDataReader dataReader, NameOrIndexParameter nameOrIndex)
			: base(mappingSchema, dataReader, nameOrIndex)
		{
			_dataReader = (OracleDataReader)dataReader;
			_fieldType = _dataReader.GetFieldType(base.Index);
			if ((object)_fieldType != typeof(OracleXmlType) && (object)_fieldType != typeof(OracleBlob))
			{
				_fieldType = _dataReader.GetFieldType(base.Index);
			}
		}

		public override Type GetFieldType(int index)
		{
			return _fieldType;
		}

		public override object GetValue(object o, int index)
		{
			if ((object)_fieldType == typeof(OracleXmlType))
			{
				OracleXmlType oracleXmlType = _dataReader.GetOracleXmlType(base.Index);
				return base.MappingSchema.ConvertToXmlDocument(oracleXmlType);
			}
			if ((object)_fieldType == typeof(OracleBlob))
			{
				OracleBlob oracleBlob = _dataReader.GetOracleBlob(base.Index);
				return base.MappingSchema.ConvertToStream(oracleBlob);
			}
			if (!_dataReader.IsDBNull(index))
			{
				return _dataReader.GetValue(base.Index);
			}
			return null;
		}

		public override bool GetBoolean(object o, int index)
		{
			return base.MappingSchema.ConvertToBoolean(GetValue(o, base.Index));
		}

		public override char GetChar(object o, int index)
		{
			return base.MappingSchema.ConvertToChar(GetValue(o, base.Index));
		}

		public override Guid GetGuid(object o, int index)
		{
			return base.MappingSchema.ConvertToGuid(GetValue(o, base.Index));
		}

		[CLSCompliant(false)]
		public override sbyte GetSByte(object o, int index)
		{
			return (sbyte)_dataReader.GetDecimal(base.Index);
		}

		[CLSCompliant(false)]
		public override ushort GetUInt16(object o, int index)
		{
			return (ushort)_dataReader.GetDecimal(base.Index);
		}

		[CLSCompliant(false)]
		public override uint GetUInt32(object o, int index)
		{
			return (uint)_dataReader.GetDecimal(base.Index);
		}

		[CLSCompliant(false)]
		public override ulong GetUInt64(object o, int index)
		{
			return (ulong)_dataReader.GetDecimal(base.Index);
		}

		public override decimal GetDecimal(object o, int index)
		{
			return OracleDecimal.SetPrecision(_dataReader.GetOracleDecimal(base.Index), 28).Value;
		}

		public override bool? GetNullableBoolean(object o, int index)
		{
			return base.MappingSchema.ConvertToNullableBoolean(GetValue(o, base.Index));
		}

		public override char? GetNullableChar(object o, int index)
		{
			return base.MappingSchema.ConvertToNullableChar(GetValue(o, base.Index));
		}

		public override Guid? GetNullableGuid(object o, int index)
		{
			return base.MappingSchema.ConvertToNullableGuid(GetValue(o, base.Index));
		}

		[CLSCompliant(false)]
		public override sbyte? GetNullableSByte(object o, int index)
		{
			if (!_dataReader.IsDBNull(index))
			{
				return (sbyte)_dataReader.GetDecimal(base.Index);
			}
			return null;
		}

		[CLSCompliant(false)]
		public override ushort? GetNullableUInt16(object o, int index)
		{
			if (!_dataReader.IsDBNull(index))
			{
				return (ushort)_dataReader.GetDecimal(base.Index);
			}
			return null;
		}

		[CLSCompliant(false)]
		public override uint? GetNullableUInt32(object o, int index)
		{
			if (!_dataReader.IsDBNull(index))
			{
				return (uint)_dataReader.GetDecimal(base.Index);
			}
			return null;
		}

		[CLSCompliant(false)]
		public override ulong? GetNullableUInt64(object o, int index)
		{
			if (!_dataReader.IsDBNull(index))
			{
				return (ulong)_dataReader.GetDecimal(base.Index);
			}
			return null;
		}

		public override decimal? GetNullableDecimal(object o, int index)
		{
			if (!_dataReader.IsDBNull(index))
			{
				return OracleDecimal.SetPrecision(_dataReader.GetOracleDecimal(base.Index), 28).Value;
			}
			return null;
		}
	}

	[Mixin(typeof(IDataParameter), "_oracleParameter")]
	[Mixin(typeof(IDbDataParameter), "_oracleParameter")]
	[Mixin(typeof(IDisposable), "_oracleParameter")]
	[Mixin(typeof(ICloneable), "_oracleParameter")]
	[CLSCompliant(false)]
	public abstract class OracleParameterWrap
	{
		protected OracleParameter _oracleParameter;

		public OracleParameter OracleParameter => _oracleParameter;

		protected object Value
		{
			[MixinOverride]
			set
			{
				if (value != null)
				{
					if (value is Guid guid)
					{
						value = guid.ToByteArray();
					}
					else if (value is Array && !(value is byte[]) && !(value is char[]))
					{
						_oracleParameter.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
					}
					else if (value is IConvertible)
					{
						IConvertible convertible = (IConvertible)value;
						TypeCode typeCode = convertible.GetTypeCode();
						switch (typeCode)
						{
						case TypeCode.Boolean:
							value = convertible.ToByte(null);
							break;
						case TypeCode.SByte:
						case TypeCode.UInt16:
						case TypeCode.UInt32:
						case TypeCode.UInt64:
							value = convertible.ToDecimal(null);
							break;
						case TypeCode.String:
							if (((string)value).Length == 0)
							{
								value = null;
							}
							break;
						default:
							if (value is Enum)
							{
								value = System.Convert.ChangeType(value, typeCode);
							}
							break;
						}
					}
				}
				_oracleParameter.Value = value;
			}
		}

		public static IDbDataParameter CreateInstance(OracleParameter oraParameter)
		{
			OracleParameterWrap oracleParameterWrap = TypeAccessor<OracleParameterWrap>.CreateInstanceEx();
			oracleParameterWrap._oracleParameter = oraParameter;
			return (IDbDataParameter)oracleParameterWrap;
		}

		public override string ToString()
		{
			return _oracleParameter.ToString();
		}
	}

	public const string NameString = "Odp";

	private string _parameterPrefix = "P";

	public override Type ConnectionType => typeof(OracleConnection);

	public override string Name => "Odp";

	public override int MaxBatchSize => 0;

	public string ParameterPrefix
	{
		get
		{
			return _parameterPrefix;
		}
		set
		{
			_parameterPrefix = (string.IsNullOrEmpty(value) ? null : value.ToUpper(CultureInfo.InvariantCulture));
		}
	}

	public OdpDataProvider()
	{
		MappingSchema = new OdpMappingSchema();
	}

	static OdpDataProvider()
	{
		Type type = typeof(OracleParameter).Assembly.GetType("Oracle.DataAccess.Client.OraDb_DbTypeTable");
		if ((object)type != null)
		{
			Hashtable hashtable = (Hashtable)type.InvokeMember("s_table", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField, null, null, Type.EmptyTypes);
			if (hashtable != null)
			{
				hashtable[typeof(DateTime[])] = OracleDbType.TimeStamp;
				hashtable[typeof(short[])] = OracleDbType.Int16;
				hashtable[typeof(int[])] = OracleDbType.Int32;
				hashtable[typeof(long[])] = OracleDbType.Int64;
				hashtable[typeof(float[])] = OracleDbType.Single;
				hashtable[typeof(double[])] = OracleDbType.Double;
				hashtable[typeof(decimal[])] = OracleDbType.Decimal;
				hashtable[typeof(TimeSpan[])] = OracleDbType.IntervalDS;
				hashtable[typeof(string[])] = OracleDbType.Varchar2;
				hashtable[typeof(OracleBFile[])] = OracleDbType.BFile;
				hashtable[typeof(OracleBinary[])] = OracleDbType.Raw;
				hashtable[typeof(OracleBlob[])] = OracleDbType.Blob;
				hashtable[typeof(OracleClob[])] = OracleDbType.Clob;
				hashtable[typeof(OracleDate[])] = OracleDbType.Date;
				hashtable[typeof(OracleDecimal[])] = OracleDbType.Decimal;
				hashtable[typeof(OracleIntervalDS[])] = OracleDbType.IntervalDS;
				hashtable[typeof(OracleIntervalYM[])] = OracleDbType.IntervalYM;
				hashtable[typeof(OracleRefCursor[])] = OracleDbType.RefCursor;
				hashtable[typeof(OracleString[])] = OracleDbType.Varchar2;
				hashtable[typeof(OracleTimeStamp[])] = OracleDbType.TimeStamp;
				hashtable[typeof(OracleTimeStampLTZ[])] = OracleDbType.TimeStampLTZ;
				hashtable[typeof(OracleTimeStampTZ[])] = OracleDbType.TimeStampTZ;
				hashtable[typeof(OracleXmlType[])] = OracleDbType.XmlType;
				hashtable[typeof(bool)] = OracleDbType.Byte;
				hashtable[typeof(Guid)] = OracleDbType.Raw;
				hashtable[typeof(sbyte)] = OracleDbType.Decimal;
				hashtable[typeof(ushort)] = OracleDbType.Decimal;
				hashtable[typeof(uint)] = OracleDbType.Decimal;
				hashtable[typeof(ulong)] = OracleDbType.Decimal;
				hashtable[typeof(bool[])] = OracleDbType.Byte;
				hashtable[typeof(Guid[])] = OracleDbType.Raw;
				hashtable[typeof(sbyte[])] = OracleDbType.Decimal;
				hashtable[typeof(ushort[])] = OracleDbType.Decimal;
				hashtable[typeof(uint[])] = OracleDbType.Decimal;
				hashtable[typeof(ulong[])] = OracleDbType.Decimal;
				hashtable[typeof(bool?)] = OracleDbType.Byte;
				hashtable[typeof(Guid?)] = OracleDbType.Raw;
				hashtable[typeof(sbyte?)] = OracleDbType.Decimal;
				hashtable[typeof(ushort?)] = OracleDbType.Decimal;
				hashtable[typeof(uint?)] = OracleDbType.Decimal;
				hashtable[typeof(ulong?)] = OracleDbType.Decimal;
				hashtable[typeof(DateTime?[])] = OracleDbType.TimeStamp;
				hashtable[typeof(short?[])] = OracleDbType.Int16;
				hashtable[typeof(int?[])] = OracleDbType.Int32;
				hashtable[typeof(long?[])] = OracleDbType.Int64;
				hashtable[typeof(float?[])] = OracleDbType.Single;
				hashtable[typeof(double?[])] = OracleDbType.Double;
				hashtable[typeof(decimal?[])] = OracleDbType.Decimal;
				hashtable[typeof(TimeSpan?[])] = OracleDbType.IntervalDS;
				hashtable[typeof(bool?[])] = OracleDbType.Byte;
				hashtable[typeof(Guid?[])] = OracleDbType.Raw;
				hashtable[typeof(sbyte?[])] = OracleDbType.Decimal;
				hashtable[typeof(ushort?[])] = OracleDbType.Decimal;
				hashtable[typeof(uint?[])] = OracleDbType.Decimal;
				hashtable[typeof(ulong?[])] = OracleDbType.Decimal;
				hashtable[typeof(XmlReader)] = OracleDbType.XmlType;
				hashtable[typeof(XmlDocument)] = OracleDbType.XmlType;
				hashtable[typeof(MemoryStream)] = OracleDbType.Blob;
				hashtable[typeof(XmlReader[])] = OracleDbType.XmlType;
				hashtable[typeof(XmlDocument[])] = OracleDbType.XmlType;
				hashtable[typeof(MemoryStream[])] = OracleDbType.Blob;
			}
		}
	}

	public override IDbConnection CreateConnectionObject()
	{
		return new OracleConnection();
	}

	public override IDbCommand CreateCommandObject(IDbConnection connection)
	{
		if (connection is OracleConnection oracleConnection)
		{
			OracleCommand oracleCommand = oracleConnection.CreateCommand();
			oracleCommand.BindByName = true;
			return oracleCommand;
		}
		return base.CreateCommandObject(connection);
	}

	public override IDbDataParameter CloneParameter(IDbDataParameter parameter)
	{
		OracleParameter oracleParameter = ((parameter is OracleParameterWrap) ? (parameter as OracleParameterWrap).OracleParameter : (parameter as OracleParameter));
		if (oracleParameter != null)
		{
			OracleParameter oracleParameter2 = (OracleParameter)oracleParameter.Clone();
			oracleParameter2.CollectionType = oracleParameter.CollectionType;
			return OracleParameterWrap.CreateInstance(oracleParameter2);
		}
		return base.CloneParameter(parameter);
	}

	public override void SetUserDefinedType(IDbDataParameter parameter, string typeName)
	{
		throw new NotSupportedException("UserDefinedType not supported");
	}

	public override DbDataAdapter CreateDataAdapterObject()
	{
		return new OracleDataAdapter();
	}

	public override bool DeriveParameters(IDbCommand command)
	{
		if (command is OracleCommand oracleCommand)
		{
			try
			{
				OracleCommandBuilder.DeriveParameters(oracleCommand);
			}
			catch (Exception ex)
			{
				throw new DataException($"{ex.Message}\nCommandText: {oracleCommand.CommandText}", ex);
			}
			return true;
		}
		return false;
	}

	public override object Convert(object value, ConvertType convertType)
	{
		switch (convertType)
		{
		case ConvertType.NameToQueryParameter:
			return ":" + value;
		case ConvertType.NameToParameter:
			if (ParameterPrefix != null)
			{
				return ParameterPrefix + value;
			}
			return value;
		case ConvertType.ParameterToName:
		{
			string text = (string)value;
			if (text.Length > 0)
			{
				if (text[0] == ':')
				{
					return text.Substring(1);
				}
				if (ParameterPrefix != null && text.ToUpper(CultureInfo.InvariantCulture).StartsWith(ParameterPrefix))
				{
					return text.Substring(ParameterPrefix.Length);
				}
			}
			break;
		}
		case ConvertType.ExceptionToErrorNumber:
			if (value is OracleException)
			{
				return ((OracleException)value).Number;
			}
			break;
		}
		return value;
	}

	public override void PrepareCommand(ref CommandType commandType, ref string commandText, ref IDbDataParameter[] commandParameters)
	{
		base.PrepareCommand(ref commandType, ref commandText, ref commandParameters);
		if (commandType == CommandType.Text)
		{
			commandText = commandText.Replace('\r', ' ');
		}
	}

	public override void AttachParameter(IDbCommand command, IDbDataParameter parameter)
	{
		OracleParameter oracleParameter = ((parameter is OracleParameterWrap) ? (parameter as OracleParameterWrap).OracleParameter : (parameter as OracleParameter));
		if (oracleParameter != null)
		{
			if (oracleParameter.CollectionType == OracleCollectionType.PLSQLAssociativeArray)
			{
				if (oracleParameter.Direction == ParameterDirection.Input || oracleParameter.Direction == ParameterDirection.InputOutput)
				{
					if (oracleParameter.Value is Array array && !(array is byte[]) && !(array is char[]))
					{
						oracleParameter.Size = array.Length;
						if (oracleParameter.DbType == DbType.String && oracleParameter.Direction == ParameterDirection.InputOutput)
						{
							int[] array2 = new int[oracleParameter.Size];
							for (int i = 0; i < oracleParameter.Size; i++)
							{
								array2[i] = 1024;
							}
							oracleParameter.ArrayBindSize = array2;
						}
					}
					if (oracleParameter.Size == 0)
					{
						return;
					}
					if (oracleParameter.Value is Stream[])
					{
						Stream[] array3 = (Stream[])oracleParameter.Value;
						for (int j = 0; j < oracleParameter.Size; j++)
						{
							if (!(array3[j] is OracleBFile) && !(array3[j] is OracleBlob) && !(array3[j] is OracleClob) && !(array3[j] is OracleXmlStream))
							{
								array3[j] = CopyStream(array3[j], (OracleCommand)command);
							}
						}
					}
					else if (oracleParameter.Value is XmlDocument[])
					{
						XmlDocument[] array4 = (XmlDocument[])oracleParameter.Value;
						object[] array5 = new object[oracleParameter.Size];
						switch (oracleParameter.OracleDbType)
						{
						case OracleDbType.XmlType:
						{
							for (int m = 0; m < oracleParameter.Size; m++)
							{
								array5[m] = ((array4[m].DocumentElement == null) ? ((object)DBNull.Value) : ((object)new OracleXmlType((OracleConnection)command.Connection, array4[m])));
							}
							oracleParameter.Value = array5;
							break;
						}
						case OracleDbType.Char:
						case OracleDbType.Clob:
						case OracleDbType.NClob:
						case OracleDbType.NChar:
						case OracleDbType.NVarchar2:
						case OracleDbType.Varchar2:
						{
							for (int l = 0; l < oracleParameter.Size; l++)
							{
								array5[l] = ((array4[l].DocumentElement == null) ? ((IConvertible)DBNull.Value) : ((IConvertible)array4[l].InnerXml));
							}
							oracleParameter.Value = array5;
							break;
						}
						case OracleDbType.BFile:
						case OracleDbType.Blob:
						case OracleDbType.Long:
						case OracleDbType.LongRaw:
						case OracleDbType.Raw:
						{
							for (int k = 0; k < oracleParameter.Size; k++)
							{
								if (array4[k].DocumentElement == null)
								{
									array5[k] = DBNull.Value;
									continue;
								}
								using MemoryStream memoryStream = new MemoryStream();
								array4[k].Save(memoryStream);
								array5[k] = memoryStream.GetBuffer();
							}
							oracleParameter.Value = array5;
							break;
						}
						}
					}
				}
				else if (oracleParameter.Direction == ParameterDirection.Output)
				{
					if (oracleParameter.DbType == DbType.String)
					{
						oracleParameter.Size = 1024;
						int[] array6 = new int[oracleParameter.Size];
						for (int n = 0; n < oracleParameter.Size; n++)
						{
							array6[n] = 1024;
						}
						oracleParameter.ArrayBindSize = array6;
					}
					else
					{
						oracleParameter.Size = 32767;
					}
				}
			}
			else if (oracleParameter.Value is Stream)
			{
				Stream stream = (Stream)oracleParameter.Value;
				if (!(stream is OracleBFile) && !(stream is OracleBlob) && !(stream is OracleClob) && !(stream is OracleXmlStream))
				{
					oracleParameter.Value = CopyStream(stream, (OracleCommand)command);
				}
			}
			else if (oracleParameter.Value is byte[])
			{
				byte[] array7 = (byte[])oracleParameter.Value;
				if (array7.Length > 32000)
				{
					oracleParameter.Value = CopyStream(array7, (OracleCommand)command);
				}
			}
			else if (oracleParameter.Value is XmlDocument)
			{
				XmlDocument xmlDocument = (XmlDocument)oracleParameter.Value;
				if (xmlDocument.DocumentElement == null)
				{
					oracleParameter.Value = DBNull.Value;
				}
				else
				{
					switch (oracleParameter.OracleDbType)
					{
					case OracleDbType.XmlType:
						oracleParameter.Value = new OracleXmlType((OracleConnection)command.Connection, xmlDocument);
						break;
					case OracleDbType.Char:
					case OracleDbType.Clob:
					case OracleDbType.NClob:
					case OracleDbType.NChar:
					case OracleDbType.NVarchar2:
					case OracleDbType.Varchar2:
					{
						using (TextWriter textWriter = new StringWriter())
						{
							xmlDocument.Save(textWriter);
							oracleParameter.Value = textWriter.ToString();
						}
						break;
					}
					case OracleDbType.BFile:
					case OracleDbType.Blob:
					case OracleDbType.Long:
					case OracleDbType.LongRaw:
					case OracleDbType.Raw:
					{
						using (MemoryStream memoryStream2 = new MemoryStream())
						{
							xmlDocument.Save(memoryStream2);
							oracleParameter.Value = memoryStream2.GetBuffer();
						}
						break;
					}
					}
				}
			}
			parameter = oracleParameter;
		}
		base.AttachParameter(command, parameter);
	}

	private static Stream CopyStream(Stream stream, OracleCommand cmd)
	{
		return CopyStream(BLToolkit.Common.Convert.ToByteArray(stream), cmd);
	}

	private static Stream CopyStream(byte[] bytes, OracleCommand cmd)
	{
		OracleBlob oracleBlob = new OracleBlob(cmd.Connection);
		oracleBlob.Write(bytes, 0, bytes.Length);
		return oracleBlob;
	}

	public override bool IsValueParameter(IDbDataParameter parameter)
	{
		OracleParameter oracleParameter = ((parameter is OracleParameterWrap) ? (parameter as OracleParameterWrap).OracleParameter : (parameter as OracleParameter));
		if (oracleParameter != null && oracleParameter.OracleDbType == OracleDbType.RefCursor && oracleParameter.Direction == ParameterDirection.Output)
		{
			return false;
		}
		return base.IsValueParameter(parameter);
	}

	public override IDbDataParameter CreateParameterObject(IDbCommand command)
	{
		IDbDataParameter dbDataParameter = base.CreateParameterObject(command);
		if (dbDataParameter is OracleParameter)
		{
			dbDataParameter = OracleParameterWrap.CreateInstance(dbDataParameter as OracleParameter);
		}
		return dbDataParameter;
	}

	public override IDbDataParameter GetParameter(IDbCommand command, NameOrIndexParameter nameOrIndex)
	{
		IDbDataParameter dbDataParameter = base.GetParameter(command, nameOrIndex);
		if (dbDataParameter is OracleParameter)
		{
			dbDataParameter = OracleParameterWrap.CreateInstance(dbDataParameter as OracleParameter);
		}
		return dbDataParameter;
	}

	public override ISqlProvider CreateSqlProvider()
	{
		return new OracleSqlProvider(this);
	}

	public override void Configure(NameValueCollection attributes)
	{
		string text = attributes["ParameterPrefix"];
		if (text != null)
		{
			ParameterPrefix = text;
		}
		base.Configure(attributes);
	}
}
