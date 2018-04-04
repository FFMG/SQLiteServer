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
using SQLiteServer.Data.Enums;

namespace SQLiteServer.Data.Connections
{
  internal class Packets
  {
    /// <summary>
    /// The data lock
    /// </summary>
    private readonly object _lock = new object();

    /// <summary>
    /// Our current buffer, (unprocessed data)
    /// </summary>
    private byte[] _currentBuffer;

    /// <summary>
    /// The processed buffers.
    /// </summary>
    private readonly List<Packet> _packets = new List<Packet>();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="received"></param>
    /// <param name="bytesRead"></param>
    public void Queue(byte[] received, int bytesRead)
    {
      lock (_lock)
      {
        // update the buffer with received data
        UpdateBuffer(received, bytesRead);

        // process the buffer and add items to the queue.
        ProcessCurrentBuffer();
      }
    }

    /// <summary>
    /// Safely get all the packets that are currently in the list.
    /// We will then clear the list.
    /// </summary>
    /// <returns>The current list of packets.</returns>
    public IEnumerable<Packet> UnQueue()
    {
      lock (_lock)
      {
        // _copy_ the list of packets from our list.
        var packets = _packets.Select(i => i).ToList();

        // clear our current list
        _packets.Clear();

        // return the updated list.
        return packets;
      }
    }

    /// <summary>
    /// Update the buffer with new data.
    /// </summary>
    /// <param name="received"></param>
    /// <param name="bytesRead"></param>
    private void UpdateBuffer( byte[] received, int bytesRead)
    {
      //  sanity check
      if (received.Length < bytesRead)
      {
        throw new ArgumentOutOfRangeException( nameof(received), "The number of received bytes is less than what was read");
      }

      lock (_lock )
      {
        // do we have anything in the array?
        if (null == _currentBuffer)
        {
          // no we do not, so we will create a brand new array.
          // and move all the data in it.
          _currentBuffer = new byte[bytesRead];
          Array.Copy(received, _currentBuffer, bytesRead);

          // done
          return;
        }

        // we already have some data, so we want o add this item to it.
        // create a new buffer that contains both arrays.
        var resizedBuffer = new byte[_currentBuffer.Length + bytesRead];

        // then copy the original to the temp array
        Buffer.BlockCopy(_currentBuffer, 0, resizedBuffer, 0, _currentBuffer.Length);

        // then copy the new value after the original value.
        Buffer.BlockCopy(received, 0, resizedBuffer, _currentBuffer.Length, bytesRead);

        // so the current buffer is now the resized buffer.
        _currentBuffer = resizedBuffer;
      }
    }
    
    /// <summary>
    /// Process the current buffer and rest the values.
    /// create an array of valid packet types and payload.
    /// </summary>
    private void ProcessCurrentBuffer()
    {
      lock (_lock)
      {
        // we now have to see how many packets can be parsed out of what we have here.
        // the data is very straight forward.
        // the first 4 bytes are the length of the payload
        // the next 4 are the Type
        // and the rest is the payload.
        var offset = 0;
        const int minUsefulBufferSize = sizeof(int) + sizeof(uint);
        while (true)
        {
          // check that we have enough data to read at least the length and the type
          if (_currentBuffer.Length - (offset + minUsefulBufferSize ) < 0)
          {
            break;
          }

          // we have enough data to get at least the length.
          var length = BitConverter.ToInt32(_currentBuffer, offset);

          // we we now have enough data to read the type + the length
          if (_currentBuffer.Length - (offset + minUsefulBufferSize + length) < 0)
          {
            break;
          }

          // we have enough to read everything.
          var type = BitConverter.ToUInt32(_currentBuffer, offset + sizeof(int));
          var packet = new byte[length];
          Buffer.BlockCopy(_currentBuffer, offset + sizeof(int) + sizeof(uint), packet, 0, length);

          // add this packet to send as notification.
          _packets.Add(new Packet( (SQLiteMessage) type, packet ));

          // adjust the offset now with everything we read.
          // Length + Type + Payload
          // Have to have enough data as we checked earlier.
          offset += sizeof(int) + sizeof(uint) + length;
        }

        // finally, remove what has been processed.
        if (offset <= 0)
        {
          return;
        }

        //  did we process all the data there was to use?
        if (offset == _currentBuffer.Length)
        {
          _currentBuffer = null;
          return;
        }

        // we did not read everything, so we need to move forward by the offset.
        // the new length is the total size less the offset.
        var newLen = _currentBuffer.Length - offset;

        // copy the data over
        var resizeByte = new byte[newLen];
        Buffer.BlockCopy(_currentBuffer, offset, resizeByte, 0, newLen);

        // and then copy the data onto our current buffer..
        _currentBuffer = resizeByte;
      }
    }
  }
}
