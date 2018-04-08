//This file is part of SQLiteServer.
//
//    SQLiteServer is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    SQLiteServer is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with SQLiteServer.  If not, see<https://www.gnu.org/licenses/gpl-3.0.en.html>.
using System;
using System.Text;

namespace SQLiteServer.Fields
{
  internal class Field
  {
    /// <summary>
    /// The variable field name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The variable data type.
    /// </summary>
    public FieldType Type { get; }

    /// <summary>
    /// The actual value.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Get the lenght of the value based on the data type.
    /// </summary>
    public int ValueLength
    {
      get
      {
        switch (Type)
        {
          case FieldType.Int16: return sizeof(short);
          case FieldType.Int32: return sizeof(int);
          case FieldType.Int64: return sizeof(long);
          case FieldType.Double: return sizeof(double);
          case FieldType.String: return ((string) Value).Length;
          case FieldType.Bytes: return ((byte[])Value).Length;

          default:
            throw new NotSupportedException("The given data type is not supported.");
        }
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// Field constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public Field(string name, Type type, object value) :
      this(name, TypeToFieldType(type), value)
    {
    }

    /// <summary>
    /// Field constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    private Field(string name, FieldType type, object value)
    {
      Name = name;
      Type = type;
      Value = value;
    }

    /// <inheritdoc />
    public Field(string name, short value) : this( name, FieldType.Int16, value )
    {
    }

    /// <inheritdoc />
    public Field(string name, int value) : this( name, FieldType.Int32, value )
    {
    }

    /// <inheritdoc />
    public Field(string name, long value) : this(name, FieldType.Int64, value)
    {
    }

    /// <inheritdoc />
    public Field(string name, string value) : this(name, FieldType.String, value)
    {
    }

    /// <inheritdoc />
    public Field(string name, double value) : this(name, FieldType.Double, value)
    {
    }

    /// <inheritdoc />
    public Field(string name, byte[] value) : this(name, FieldType.Bytes, value)
    {
    }
    
    /// <summary>
    /// Pack a 'Field' into bytes
    /// </summary>
    /// <returns></returns>
    public byte[] Pack()
    {
      // the data is packed as 
      // name length
      // name
      // Type
      // value length
      // value
      var totalLength = sizeof(int) + Name.Length +
                        sizeof(int) +
                        sizeof(int) + ValueLength;

      // putting it all together.
      var bytes = new byte[totalLength];

      var bLength = BitConverter.GetBytes(Name.Length);
      var bType = BitConverter.GetBytes((int) Type);
      var bValueLength = BitConverter.GetBytes(ValueLength);

      // 
      var offset = 0;
      Buffer.BlockCopy(bLength, 0, bytes, offset, sizeof(int));
      offset += sizeof(int);

      var bName = Encoding.ASCII.GetBytes(Name);
      Buffer.BlockCopy(bName, 0, bytes, offset, bName.Length);
      offset += bName.Length;

      Buffer.BlockCopy(bType, 0, bytes, offset, sizeof(int));
      offset += sizeof(int);

      Buffer.BlockCopy(bValueLength, 0, bytes, offset, sizeof(int));
      offset += sizeof(int);

      Buffer.BlockCopy(ObjectToBytes(), 0, bytes, offset, ValueLength);
      //offset += ValueLength;

      return bytes;
    }

    /// <summary>
    /// Unpack a bye array into a field.
    /// </summary>
    /// <returns></returns>
    public static Field Unpack(byte[] bytes)
    {
      if (null == bytes)
      {
        throw new ArgumentNullException(nameof(bytes));
      }

      // the data is packed as 
      // name length
      // name
      // Type
      // value length
      // value
      var totalLength = bytes.Length;

      // do we have enough for the name length?
      if (totalLength < sizeof(int))
      {
        throw new FieldException("The given array cannot be unpacked");
      }

      var offset = 0;

      // The name length
      var nameLength = BitConverter.ToInt32(bytes, offset);
      offset += sizeof(int);
      if (totalLength - offset < nameLength)
      {
        throw new FieldException("The given array cannot be unpacked");
      }

      // get the name
      var bName = new byte[nameLength];
      Buffer.BlockCopy(bytes, offset, bName, 0, nameLength);
      offset += nameLength;

      // Get the type
      if (totalLength - offset < sizeof(int))
      {
        throw new FieldException("The given array cannot be unpacked");
      }

      var iType = BitConverter.ToInt32(bytes, offset);
      offset += sizeof(int);

      // Get the object length
      if (totalLength - offset < sizeof(int))
      {
        throw new FieldException("The given array cannot be unpacked");
      }

      var objectLength = BitConverter.ToInt32(bytes, offset);
      offset += sizeof(int);
      var bObject = new byte[objectLength];

      Buffer.BlockCopy(bytes, offset, bObject, 0, objectLength);
      offset += objectLength;

      // the size has to match exactly.
      if (totalLength != offset)
      {
        throw new FieldException("The given array cannot be unpacked");
      }

      // putting it all together.
      return new Field(Encoding.Default.GetString(bName),
        (FieldType) iType,
        BytesToObject( bObject, (FieldType)iType)
      );
    }

    /// <summary>
    /// Given some bytes, we try and create an object.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private static object BytesToObject(byte[] bytes, FieldType type )
    {
      switch (type)
      {
        case FieldType.Int16:
          if (bytes.Length != sizeof(short))
          {
            throw new FieldException("The array of data is not the correct size.");
          }
          return BitConverter.ToInt16(bytes, 0);

        case FieldType.Int32:
          if (bytes.Length != sizeof(int))
          {
            throw new FieldException("The array of data is not the correct size.");
          }
          return BitConverter.ToInt32(bytes, 0);

        case FieldType.Int64:
          if (bytes.Length != sizeof(long))
          {
            throw new FieldException("The array of data is not the correct size.");
          }
          return BitConverter.ToInt64(bytes, 0);

        case FieldType.Double:
          if (bytes.Length != sizeof(double))
          {
            throw new FieldException("The array of data is not the correct size.");
          }
          return BitConverter.ToDouble(bytes, 0);

        case FieldType.String:
          return Encoding.Default.GetString(bytes);

        case FieldType.Bytes:
          return bytes;

        default:
          throw new NotSupportedException("The given data type is not supported.");
      }
    }

    /// <summary>
    /// Convert the current obekect to bytes.
    /// </summary>
    /// <returns></returns>
    private byte[] ObjectToBytes()
    {
      switch (Type)
      {
        case FieldType.Int16: return BitConverter.GetBytes((short)Value);
        case FieldType.Int32: return BitConverter.GetBytes((int)Value);
        case FieldType.Int64: return BitConverter.GetBytes((long)Value);
        case FieldType.String: return Encoding.ASCII.GetBytes((string)Value);
        case FieldType.Double: return BitConverter.GetBytes((double)Value);
        case FieldType.Bytes: return (byte[])Value;

        default:
          throw new NotSupportedException("The given data type is not supported.");
      }
    }

    /// <summary>
    /// Convert a system.Type to our field type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static FieldType TypeToFieldType(Type type)
    {
      if (type == typeof(short))
      {
        return FieldType.Int16;
      }

      if (type == typeof(int))
      {
        return FieldType.Int32;
      }

      if (type == typeof(long))
      {
        return FieldType.Int64;
      }

      if (type == typeof(double))
      {
        return FieldType.Double;
      }

      if (type == typeof(string))
      {
        return FieldType.String;
      }

      if (type == typeof(byte[]))
      {
        return FieldType.Bytes;
      }

      if (type == typeof(object))
      {
        return FieldType.Object;
      }

      // no idea what this is...
      throw new NotSupportedException( $"The given data type is not supported.");
    }
    
    /// <summary>
    /// Cast a field type to a system.Type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Type FieldTypeToType(FieldType type)
    {
      switch (type)
      {
        case FieldType.Int16:
          return typeof(short);

        case FieldType.Int32:
          return typeof(int);

        case FieldType.Int64:
          return typeof(long);

        case FieldType.Double:
          return typeof(double);

        case FieldType.String:
          return typeof(string);

        case FieldType.Bytes:
          return typeof(byte[]);

        case FieldType.Object:
          return typeof(object);

        default:
          // no idea what this is...
          throw new NotSupportedException($"The given data type is not supported.");
      }

    }
  }
}
