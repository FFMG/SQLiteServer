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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SQLiteServer.Fields
{
  internal class Fields
  {
    /// <summary>
    /// All the fields in our list.
    /// </summary>
    private readonly List<Field> _fields;

    /// <summary>
    /// Get the number of items in our fields.
    /// </summary>
    public int Count => _fields.Count;

    public Fields()
    {
      _fields = new List<Field>();
    }
    
    /// <summary>
    /// Add a single field to our list.
    /// </summary>
    /// <param name="field"></param>
    public void Add(Field field)
    {
      if (null == field)
      {
        throw new ArgumentException( nameof(field));
      }
      _fields.Add( field );
    }

    /// <summary>
    /// Add a field to our list, by name/type/value
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public void Add(string name, Type type, object value)
    {
      Add( new Field(name, type, value ));
    }

    /// <summary>
    /// Check if we have a default contructor
    /// Otherwise we cannot parse this item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static bool HasDefaultConstructor<T>()
    {
      var typeOfT = typeof(T);
      return typeOfT.IsValueType || typeOfT.GetConstructor(Type.EmptyTypes) != null;
    }
    
    /// <summary>
    /// Parse an object into all the 'Fields'
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Fields SerializeObject<T>(T value)
    {
      if (!HasDefaultConstructor<T>())
      {
        throw new FieldsException( $"The variable type, {nameof(value)}, does not have a default constructor");
      }

      if (null == value)
      {
        throw new ArgumentException(nameof(value));
      }
      
      var fields = new Fields();

      // add all the items.
      foreach (var field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        .Where(f => f.GetCustomAttribute<CompilerGeneratedAttribute>() == null))
      {
        fields.Add(field.Name, field.FieldType, field.GetValue(value));
      }
      return fields;
    }
    
    /// <summary>
    /// Parse an object into all the 'Fields'
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T DeserializeObject<T>()
    {
      if (!HasDefaultConstructor<T>())
      {
        throw new FieldsException($"The variable type, {nameof(T)}, does not have a default constructor");
      }

      // create the instance
      var result = Activator.CreateInstance<T>();

      // add all the items.
      foreach (var field in _fields )
      {
        var fi = typeof(T).GetField(field.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (fi == null)
        {
          continue;
        }
        fi.SetValue(result, field.Get<object>() );
      }
      return result;
    }
    
    /// <summary>
    /// Parse an object into all the 'Fields'
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fields"></param>
    /// <returns></returns>
    public static T DeserializeObject<T>(Fields fields )
    {
      if (null == fields)
      {
        throw new ArgumentException(nameof(fields ));
      }
      return fields.DeserializeObject<T>();
    }

    /// <summary>
    /// Pack all the fieds into one byte array.
    /// </summary>
    /// <returns></returns>
    public byte[] Pack()
    {
      // all the bytes in one big array.
      var packs = new List<byte[]>();
      foreach (var field in _fields)
      {
        packs.Add( field.Pack() );
      }

      // all the data.
      var totalLength = packs.Sum(p => p.Length) + packs.Count * sizeof(int);
      var bytes = new byte[totalLength];
      var offset = 0;
      foreach (var pack in packs)
      {
        var bPackLength = BitConverter.GetBytes(pack.Length);
        Buffer.BlockCopy(bPackLength, 0, bytes, offset, sizeof(int));
        offset += sizeof(int);
        Buffer.BlockCopy(pack, 0, bytes, offset, pack.Length);
        offset += pack.Length;
      }
      return bytes;
    }

    /// <summary>
    /// Pack all the fieds into one byte array.
    /// </summary>
    /// <returns></returns>
    public static Fields Unpack(byte[] bytes )
    {
      if (null == bytes)
      {
        throw new ArgumentNullException(nameof(bytes));
      }

      // the result
      var fields = new Fields();

      // all the data.
      var totalLength = bytes.Length;
      var offset = 0;
      while(totalLength - offset > 0 )
      {
        // do we have enough to read the length?
        if (totalLength+offset < sizeof(int))
        {
          throw new FieldsException("The given array cannot be unpacked");
        }
        var fieldLength = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        if (totalLength - offset < fieldLength)
        {
          throw new FieldsException("The given array cannot be unpacked");
        }

        var bField = new byte[fieldLength];
        Buffer.BlockCopy(bytes, offset, bField, 0, fieldLength);
        offset += fieldLength;

        fields.Add( Field.Unpack(bField));
      }
      return fields;
    }
  }
}
