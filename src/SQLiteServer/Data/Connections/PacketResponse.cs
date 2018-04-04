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
  internal class PacketResponse
  {
    /// <summary>
    /// The unique string that we will be listening for.
    /// </summary>
    public string Guid { get; }

    /// <summary>
    /// The Guid 
    /// </summary>
    private const int GuidLen = 36;

    /// <summary>
    /// The message type.
    /// </summary>
    public SQLiteMessage Type { get; }

    /// <summary>
    /// The payload
    /// </summary>
    public byte[] Payload { get; }

    /// <summary>
    /// The packed request
    /// </summary>
    private byte[] _packed;

    public Packet Packet => new Packet( Type, Payload );

    public byte[] Packed
    {
      get
      {
        if (_packed != null)
        {
          return _packed;
        }
        _packed = Pack(Type, Payload, Guid);
        return _packed;
      }
    } 

    public PacketResponse(byte[] payload) 
    {
      //  we need to unpack everything.
      var offset = 0;
      var allowedLen = payload.Length;

      // the data len
      if (offset + sizeof(int) > allowedLen)
      {
        throw new ArgumentException("The given payload could not be unpacked");
      }
      var lengthDataAndGuid = BitConverter.ToInt32(payload, offset);
      offset += sizeof(int);

      // the type
      if (offset + sizeof(uint) > allowedLen)
      {
        throw new ArgumentException("The given payload could not be unpacked");
      }
      var type = BitConverter.ToUInt32(payload, offset);
      offset += sizeof(uint);

      // the data itself
      if (offset + lengthDataAndGuid > allowedLen)
      {
        throw new ArgumentException("The given payload could not be unpacked");
      }

      var lengthData = lengthDataAndGuid - GuidLen;
      var data = new byte[lengthData];
      Buffer.BlockCopy( payload, offset, data, 0, lengthData );
      offset += lengthData;

      // the GUID
      if (offset + sizeof(int) > allowedLen)
      {
        throw new ArgumentException("The given payload could not be unpacked");
      }
      var guid = new byte[GuidLen];
      Buffer.BlockCopy(payload, offset, guid, 0, GuidLen);
      offset += GuidLen;

      // the total size must match exactly.
      if (offset != allowedLen)
      {
        throw new ArgumentException("The given payload could not be unpacked");
      }

      // we can now set it all together.
      Type = (SQLiteMessage) type;
      Guid = Encoding.Default.GetString(guid);
      Payload = data;
    }

    public PacketResponse(SQLiteMessage type, byte[] payload) : 
      this(type, payload, System.Guid.NewGuid().ToString())
    {

    }

    public PacketResponse(SQLiteMessage type, byte[] payload, string guid)
    {
      // make sure that the guid is the len we want it to be
      // anything other than that and we have issues.
      if (guid == null)
      {
        throw new ArgumentNullException( nameof(guid), "The given Guid cannot be null." );
      }
      if (guid.Length != GuidLen)
      {
        throw new Exception($"The length of our guid ({guid}), has changed from the expected len({GuidLen}.");
      }
      Guid = guid;

      Type = type;
      Payload = payload;
    }

    /// <summary>
    /// Repack the type/data/guid so we can send it all in one go.
    /// </summary>
    private static byte[] Pack( SQLiteMessage type, byte[] payload, string guid )
    {
      // get the final size.
      // [int] payload len
      // [uint] type
      // [37] guid payload.
      var totalSize = sizeof(uint) + sizeof(int) + (payload?.Length ?? 0) + GuidLen;
      var bytes = new byte[totalSize];

      var bLen = BitConverter.GetBytes( (payload?.Length ?? 0)+ GuidLen);
      var bType = BitConverter.GetBytes((uint)type);
      var bGuid = Encoding.ASCII.GetBytes( guid);

      // add the len
      var dstOffset = 0;
      Buffer.BlockCopy(bLen, 0, bytes, dstOffset, sizeof(int));
      dstOffset += sizeof(int);

      Buffer.BlockCopy(bType, 0, bytes, dstOffset, sizeof(uint));
      dstOffset += sizeof(uint);

      if (payload != null && payload.Length > 0 )
      {
        Buffer.BlockCopy( payload, 0, bytes, dstOffset, payload.Length);
        dstOffset += payload.Length;
      }

      // finally the guid
      Buffer.BlockCopy(bGuid, 0, bytes, dstOffset, bGuid.Length);
      dstOffset += bGuid.Length;

      // sanity check
      if (dstOffset != totalSize)
      {
        throw new Exception("There was an issue re-creating the response packet.");
      }

      // we can now return the payload.
      return bytes;
    }
  }
}
