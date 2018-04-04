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
using SQLiteServer.Data.Enums;

namespace SQLiteServer.Data.Connections
{
  internal class Packet
  {
    /// <summary>
    /// The packed data.
    /// </summary>
    private byte[] _packed;

    /// <summary>
    /// The data type
    /// </summary>
    public SQLiteMessage Type { get; }

    /// <summary>
    /// The payload.
    /// </summary>
    public byte[] Payload { get; }

    /// <summary>
    /// Constructor with a byte array
    /// </summary>
    /// <param name="type"></param>
    /// <param name="payload"></param>
    public Packet( SQLiteMessage type, byte[] payload )
    {
      Payload = payload;
      Type = type;
    }

    /// <summary>
    /// Create with no payload.
    /// </summary>
    /// <param name="payload"></param>
    public Packet( byte[] payload)
    {
      // we need at least 4x bytes for lenght and 4x bytes for the type.
      if (payload.Length < sizeof(int) + sizeof(uint))
      {
        throw new ArgumentOutOfRangeException( nameof(payload), "The payload does not have a valid size.");
      }

      // get the len
      var length = BitConverter.ToInt32(payload, 0);

      // do we have enough for everything?
      var expectedLength = sizeof(int) + sizeof(uint) + length;
      if (expectedLength != payload.Length)
      {
        throw new ArgumentOutOfRangeException(nameof(payload), "The total payload size is either to small or too big.");
      }

      // we can now put everything to gether.
      Type = (SQLiteMessage)BitConverter.ToUInt32(payload, sizeof(uint));
      if (length == 0)
      {
        Payload = null;
      }
      else
      {
        Payload = new byte[length];
        Buffer.BlockCopy(payload, sizeof(int) + sizeof(uint), Payload, 0, length);
      }
    }

    /// <summary>
    /// Constructor with a string
    /// </summary>
    /// <param name="type"></param>
    /// <param name="payload"></param>
    public Packet(SQLiteMessage type, string payload)
    {
      Payload = payload == null ? null : Encoding.ASCII.GetBytes(payload);
      Type = type;
    }

    /// <summary>
    /// Constructor with a short.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="payload"></param>
    public Packet(SQLiteMessage type, short payload)
    {
      Payload = BitConverter.GetBytes(payload);
      Type = type;
    }

    /// <summary>
    /// Constructor with a double.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="payload"></param>
    public Packet(SQLiteMessage type, double payload)
    {
      Payload = BitConverter.GetBytes(payload);
      Type = type;
    }

    /// <summary>
    /// Constructor with an int.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="payload"></param>
    public Packet(SQLiteMessage type, int payload)
    {
      Payload = BitConverter.GetBytes(payload);
      Type = type;
    }

    /// <summary>
    /// Constructor with an int.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="payload"></param>
    public Packet(SQLiteMessage type, long payload)
    {
      Payload = BitConverter.GetBytes(payload);
      Type = type;
    }

    /// <summary>
    /// Get the packed length
    /// </summary>
    public int Length => sizeof(int) + sizeof(uint) + (Payload?.Length ?? 0);

    /// <summary>
    /// Create the full payload in bytes, that is te length + type + payload
    /// That wat we know what to look for.
    /// </summary>
    public byte[] Packed
    {
      get
      {
        if (_packed != null)
        {
          return _packed;
        }

        // the payload + length + type
        // is simply [int][uint][payload]
        var bLen = BitConverter.GetBytes(Payload?.Length ?? 0);
        var bType = BitConverter.GetBytes((uint) Type);

        // create the new array
        // [int]  len
        // [uint] type
        // [xx]   payload
        var bytes = new byte[Length];

        // add the len
        var dstOffset = 0;
        Buffer.BlockCopy(bLen, 0, bytes, dstOffset, sizeof(int));
        dstOffset += sizeof(int);

        Buffer.BlockCopy(bType, 0, bytes, dstOffset, sizeof(uint));
        dstOffset += sizeof(uint);

        if (Payload != null && Payload.Length > 0)
        {
          Buffer.BlockCopy(Payload, 0, bytes, dstOffset, Payload.Length);
          // dstOffset += payload.Length;
        }

        // save the packed bytes
        _packed = bytes;

        // return the bytes.
        return bytes;
      }
    }

    /// <summary>
    /// Get a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mustThrow"></param>
    /// <returns></returns>
    public T Get<T>(bool mustThrow = false )
    {
      // string
      if (typeof(T) == typeof(string))
      {
        return (T)Convert.ChangeType( GetString(mustThrow), typeof(T));
      }

      // short/ushort
      if (typeof(T) == typeof(short) ||
          typeof(T) == typeof(ushort))
      {
        return (T)Convert.ChangeType(GetShort(mustThrow), typeof(T));
      }

      // int/uint
      if (typeof(T) == typeof(int) ||
          typeof(T) == typeof(uint))
      {
        return (T)Convert.ChangeType(GetInt(mustThrow), typeof(T));
      }

      // long/ulong
      if (typeof(T) == typeof(long) ||
          typeof(T) == typeof(ulong))
      {
        return (T)Convert.ChangeType(GetLong(mustThrow), typeof(T));
      }

      // long/ulong
      if (typeof(T) == typeof(double))
      {
        return (T)Convert.ChangeType(GetDouble(mustThrow), typeof(T));
      }

      if (mustThrow)
      {
        throw new InvalidCastException( "Unable to cast to this data type." );
      }
      return default(T);
    }

    private double GetDouble(bool mustThrow)
    {
      try
      {
        return BitConverter.ToDouble(Payload, 0);
      }
      catch (Exception)
      {
        if (mustThrow)
        {
          throw;
        }
        return default(double);
      }
    }

    private short GetShort(bool mustThrow)
    {
      try
      {
        return BitConverter.ToInt16( Payload, 0 );
      }
      catch (Exception)
      {
        if (mustThrow)
        {
          throw;
        }
        return default(short);
      }
    }

    private int GetInt(bool mustThrow)
    {
      try
      {
        return BitConverter.ToInt32(Payload, 0);
      }
      catch (Exception)
      {
        if (mustThrow)
        {
          throw;
        }
        return default(int);
      }
    }

    private long GetLong(bool mustThrow)
    {
      try
      {
        return BitConverter.ToInt64(Payload, 0);
      }
      catch (Exception)
      {
        if (mustThrow)
        {
          throw;
        }
        return default(long);
      }
    }

    /// <summary>
    /// Get a string value if posible, if payload is null
    /// We will return null.
    /// </summary>
    /// <param name="mustThrow"></param>
    /// <returns></returns>
    private string GetString( bool mustThrow )
    {
      try
      {
        return Payload == null ? null : Encoding.Default.GetString(Payload);
      }
      catch (Exception)
      {
        if (mustThrow)
        {
          throw;
        }

        return default(string);
      }
    }
  }
}
