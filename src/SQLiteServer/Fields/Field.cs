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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SQLiteServer.Fields
{
  internal class Field
  {
    /// <summary>
    ///  the placeholder of the number of items in the list.
    /// </summary>
    private const string ListTypeCount = "c";

    /// <summary>
    /// The simple list type.
    /// </summary>
    private const string ListTypeSimple = "s";

    /// <summary>
    /// Check if the given object is of type IList
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsIListType(object value)
    {
      var lvalue = value as IList;
      return lvalue != null && lvalue.GetType().IsGenericType;
    }

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
    private object Value { get; }

    /// <summary>
    /// Get the total packed length.
    /// name length
    /// name
    /// Type
    /// value length
    /// value
    /// </summary>
    private int TotalLength => sizeof(int) + Name.Length +
                               sizeof(int) +
                               sizeof(int) + ValueLength;

    private int? _valueLength;

    /// <summary>
    /// Get the lenght of the value based on the data type.
    /// </summary>
    public int ValueLength
    {
      get
      {
        if (null != _valueLength)
        {
          return (int) _valueLength;
        }
        switch (Type)
        {
          case FieldType.Int16:
            _valueLength = sizeof(short);
            break;
          case FieldType.Int32:
            _valueLength = sizeof(int);
            break;
          case FieldType.Int64:
            _valueLength = sizeof(long);
            break;
          case FieldType.Double:
            _valueLength = sizeof(double);
            break;
          case FieldType.StringNull:
          case FieldType.String:
            _valueLength = ((string)Value)?.Length ?? 0;
            break;
          case FieldType.Bytes:
            _valueLength = ((byte[])Value).Length;
            break;
          case FieldType.List:
            _valueLength = ((IList<Field>)Value).Sum(f => f.TotalLength);
            break;

          default:
            throw new NotSupportedException("The given data type is not supported.");
        }
        return (int)_valueLength;
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
    public Field(string name, IEnumerable value)
    {
      Name = name;
      if (IsIListType(value))
      {
        Type = FieldType.List;
        var enumerable = value as object[] ?? value.Cast<object>().ToArray();

        // the list
        Value = new List<Field>();

        // the number of items in the list
        ((List<Field>)Value).Add(new Field(ListTypeCount, FieldType.Int32, enumerable.Length));

        // populate the list.
        foreach (var v in enumerable)
        {
          if (null == v)
          {
            ((List<Field>)Value).Add(new Field(ListTypeSimple, FieldType.StringNull, null ));
          }
          else
          {
            ((List<Field>) Value).Add(new Field(ListTypeSimple, v.GetType(), v));
          }
        }
      }
      else
      {
        throw new NotSupportedException( "This IEnumerable is not supported." );
      }
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
    public Field(string name, string value) : this(name, value == null ? FieldType.StringNull : FieldType.String, value)
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

    public T Get<T>()
    {
      try
      {
        var value = GetNonEnumarableValue(typeof(T));
        return value == null ? default(T) : (T)Convert.ChangeType(value, typeof(T));
      }
      catch(InvalidCastException)
      {
        var value = TryGetList<T>();
        if (null != value)
        {
          return (T)Convert.ChangeType(value, typeof(T));
        }
        throw;
      }
    }

    public object GetNonEnumarableValue( Type type )
    { 
      if (type == typeof(bool))
      {
        return (GetLong()!=0);
      }

      if (type == typeof(string))
      {
        return GetString();
      }

      if (type == typeof(int) || 
          type == typeof(short) ||
          type == typeof(long)
          )
      {
        return GetLong();
      }

      if (type == typeof(double)
      )
      {
        return GetDouble();
      }

      if (type == typeof(int?) ||
          type == typeof(short?) ||
          type == typeof(long?)
      )
      {
        return GetNullableLong();
      }

      if (type == typeof(double?)
      )
      {
        return GetNullableDouble();
      }

      if (type == typeof(byte[]))
      {
        return ObjectToBytes();
      }

      if (type == typeof(object))
      {
        return Value;
      }
      throw new InvalidCastException("Unable to cast to this data type.");
    }

    private object TryGetList<T>()
    {
      var hasEmptyOrDefaultConstr =
        typeof(T).GetConstructor(System.Type.EmptyTypes) != null ||
        typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.Public)
          .Any(x => x.GetParameters().All(p => p.IsOptional));
      if (!hasEmptyOrDefaultConstr)
      {
        return null;
      }

      var result = Activator.CreateInstance<T>();
      if (!IsIListType(result))
      {
        return null;
      }

      var elementType = result.GetType().GetGenericArguments().Single();
      if (Type != FieldType.List)
      {
        ((IList) result).Add(GetNonEnumarableValue(elementType));
        return result;
      }

      var fields = (IList<Field>) Value;
      var parent = fields.First();
      for( var i = 0; i < parent.GetLong(); ++i )
      {
        //  we skip the first item as it is the parent.
        if ((i + 1) >= fields.Count)
        {
          throw new InvalidCastException("Unable to cast to this data type, not enough items in the list.");
        }
        // get that child.
        var child = fields[i + 1];

        // get the value
        object value;
        var childValue = child.GetNonEnumarableValue(elementType);
        if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
          if (childValue == null)
          {
            value = null;
          }
          else
          {
            var baseElementType = elementType.GenericTypeArguments.FirstOrDefault();
            value = Convert.ChangeType(childValue, baseElementType != null ? baseElementType : elementType);
          }
        }
        else
        {
          value = Convert.ChangeType(childValue, elementType);
        }

        // and add it to our list.
        ((IList)result).Add(value);
      }
      return result;
    }

    /// <summary>
    /// Get a string value from the various fields.
    /// </summary>
    /// <returns></returns>
    private string GetString()
    {
      switch (Type)
      {
        case FieldType.Int16:
          return Convert.ToString((short)Value);
        case FieldType.Int32:
          return Convert.ToString((int)Value);
        case FieldType.Int64:
          return Convert.ToString((long)Value);
        case FieldType.StringNull:
          return null;
        case FieldType.String:
          return (string)Value;
        case FieldType.Double:
          return Convert.ToString((string)Value);

        default:
          throw new InvalidCastException("Unable to cast to this data type to a string.");
      }
    }

    /// <summary>
    /// Convert most values to a long if posible.
    /// We will throw if it is not posible.
    /// </summary>
    /// <returns></returns>
    private long GetLong()
    {
      switch (Type)
      {
        case FieldType.Int16:
          return (short) Value;
        case FieldType.Int32:
          return (int) Value;
        case FieldType.Int64:
          return (long) Value;
        case FieldType.StringNull:
          return 0;
        case FieldType.String:
          return Convert.ToInt64((string) Value);
        case FieldType.Double:
          return Convert.ToInt64((double) Value);

        default:
          throw new InvalidCastException("Unable to cast to this data type to a long.");
      }
    }

    /// <summary>
    /// Convert most values to a double if posible.
    /// We will throw if it is not posible.
    /// </summary>
    /// <returns></returns>
    private double GetDouble()
    {
      switch (Type)
      {
        case FieldType.Int16:
          return (short)Value;
        case FieldType.Int32:
          return (int)Value;
        case FieldType.Int64:
          return (long)Value;
        case FieldType.StringNull:
          return 0;
        case FieldType.String:
          return Convert.ToDouble((string)Value);
        case FieldType.Double:
          return (double)Value;

        default:
          throw new InvalidCastException("Unable to cast to this data type to a double.");
      }
    }

    private long? GetNullableLong()
    {
      switch (Type)
      {
        case FieldType.Int16:
          return (short?)Value;
        case FieldType.Int32:
          return (int?)Value;
        case FieldType.Int64:
          return (long?)Value;
        case FieldType.StringNull:
          return null;
        case FieldType.String:
          return Convert.ToInt64((string)Value);
        case FieldType.Double:
          long? d = Convert.ToInt64((double) Value);
          return d;

        default:
          throw new InvalidCastException("Unable to cast to this data type to a long.");
      }
    }


    private double? GetNullableDouble()
    {
      switch (Type)
      {
        case FieldType.Int16:
          return (short?)Value;
        case FieldType.Int32:
          return (int?)Value;
        case FieldType.Int64:
          return (long?)Value;
        case FieldType.StringNull:
          return null;
        case FieldType.String:
          return Convert.ToDouble((string)Value);
        case FieldType.Double:
          return (double?)Value;

        default:
          throw new InvalidCastException("Unable to cast to this data type to a long.");
      }
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
      // putting it all together.
      var bytes = new byte[TotalLength];
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
      
      var name = GetNameFromBytes(bytes);
      var type = GetFieldTypeFromBytes(bytes);
      var value = GetValueAsObjectFromBytes(bytes);

      // putting it all together.
      return new Field(name, type, value );
    }

    private static object GetValueAsObjectFromBytes(byte[] bytes)
    {
      var type = GetFieldTypeFromBytes(bytes);
      var value = GetValueAsBytesFromBytes(bytes);

      switch (type)
      {
        case FieldType.Int16:
          if (value.Length != sizeof(short))
          {
            throw new FieldException("The array of data is not the correct size.");
          }
          return BitConverter.ToInt16(value, 0);

        case FieldType.Int32:
          if (value.Length != sizeof(int))
          {
            throw new FieldException("The array of data is not the correct size.");
          }
          return BitConverter.ToInt32(value, 0);

        case FieldType.Int64:
          if (value.Length != sizeof(long))
          {
            throw new FieldException("The array of data is not the correct size.");
          }
          return BitConverter.ToInt64(value, 0);

        case FieldType.Double:
          if (value.Length != sizeof(double))
          {
            throw new FieldException("The array of data is not the correct size.");
          }
          return BitConverter.ToDouble(value, 0);

        case FieldType.StringNull:
          return null;

        case FieldType.String:
          return Encoding.Default.GetString(value);

        case FieldType.Bytes:
          return value;

        case FieldType.List:
          // unpack the first field item.
          var parent = Unpack(value);
           
          // create the list with the parent first.
          var list = new List<Field> {parent};
          
          // we can the walk the list and look for the other items.
          // the must all have the correct name
          // and there must be the correct number of them
          if ((int) parent.Value < 0)
          {
            throw new FieldException("The array of data is not the correct size.");
          }

          // set the current offset, the total parent length
          var offset = parent.TotalLength;
          for (var i = 0; i < (int) parent.Value; ++i)
          {
            // the next block of data if the total size
            // less what we have already done
            var blockLen = value.Length - offset;

            // if we do not have enough then we cannot create another child.
            // and as we know, (from the parent), that we should have one
            // more item, we know that this is an error.
            if (blockLen <= 0)
            {
              throw new FieldException("The array of data is not the correct size.");
            }
            var block = new byte[blockLen];
            Buffer.BlockCopy(value, offset, block, 0, blockLen);
            var child = Unpack(block);
            list.Add(child);
            offset += child.TotalLength;
          }
          return list;

        default:
          throw new NotSupportedException("The given data type is not supported.");
      }
    }

    private static byte[] GetValueAsBytesFromBytes(byte[] bytes)
    {
      // the data is packed as 
      // name length
      // name
      // Type
      // value length
      // value

      // so we need the name len so we can workout the offset.
      var nameLength = GetNameLengthFromBytes(bytes);

      // the ofset is the size of the name +  the actual name + the field type.
      var offset = sizeof(int) + nameLength + sizeof(int);

      // the data we have to play with.
      var totalLength = bytes.Length;

      // we need another int for the lenght of the data itself.
      if (totalLength < offset + sizeof(int))
      {
        throw new FieldException("The given array cannot be unpacked");
      }

      // so the size of the data is ...
      var valueLen = BitConverter.ToInt32(bytes, offset);
      offset += sizeof(int);

      if (totalLength < offset + valueLen)
      {
        throw new FieldException("The given array cannot be unpacked");
      }
      var value = new byte[valueLen];

      Buffer.BlockCopy(bytes, offset, value, 0, valueLen);
      return value;
    }

    private static FieldType GetFieldTypeFromBytes(byte[] bytes)
    {
      // the data is packed as 
      // name length
      // name
      // Type
      // value length
      // value

      // so we need the name len so we can workout the offset.
      var nameLength = GetNameLengthFromBytes(bytes);

      // the ofset is the size of the name +  the actual name
      var offset = sizeof(int) + nameLength;

      // the data we have to play with.
      var totalLength = bytes.Length;

      // we need another int for the type
      if (totalLength < offset + sizeof(int))
      {
        throw new FieldException("The given array cannot be unpacked");
      }

      // we can return at least the field type.
      return (FieldType) BitConverter.ToInt32(bytes, offset);
    }

    private static int GetNameLengthFromBytes(byte[] bytes)
    {
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
      return BitConverter.ToInt32(bytes, 0);
    }

    private static string GetNameFromBytes(byte[] bytes)
    {
      // we first need the length we are after.
      var nameLength = GetNameLengthFromBytes(bytes);

      // the data is packed as 
      // name length
      // name
      // Type
      // value length
      // value
      var totalLength = bytes.Length;

      // the ofset is the size of the int
      const int offset = sizeof(int);

      // do we have enough for the name length?
      if (totalLength < offset + nameLength)
      {
        throw new FieldException("The given array cannot be unpacked");
      }

      var bName = new byte[nameLength];
      Buffer.BlockCopy(bytes, offset, bName, 0, nameLength);
      return Encoding.Default.GetString( bName );
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
        case FieldType.StringNull: return new byte[] { };
        case FieldType.Double: return BitConverter.GetBytes((double)Value);
        case FieldType.Bytes: return (byte[])Value;
        case FieldType.List:
        {
          var offset = 0;
          var bytes = new byte[ValueLength];
          foreach (var field in (IList<Field>)Value)
          {
            var length = field.TotalLength;
            Buffer.BlockCopy(field.Pack(), 0, bytes, offset, length );
            offset += length;
          }

          // return the bytes.
          return bytes;
        }

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
        case FieldType.StringNull:
          return typeof(string);

        case FieldType.Bytes:
          return typeof(byte[]);

        case FieldType.Object:
          return typeof(object);

        case FieldType.List:
          return typeof(IList);

        default:
          // no idea what this is...
          throw new NotSupportedException("The given data type is not supported.");
      }

    }
  }
}
