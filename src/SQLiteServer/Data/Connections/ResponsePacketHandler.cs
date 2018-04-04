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
using System.Threading.Tasks;
using SQLiteServer.Data.Enums;

namespace SQLiteServer.Data.Connections
{
  internal class ResponsePacketHandler
  {
    /// <summary>
    /// The connection controller that will send/receive messages.
    /// </summary>
    private readonly ConnectionsController _connection;

    /// <summary>
    /// This is the response we got back
    /// </summary>
    private Packet _response;

    /// <summary>
    /// This is the expected GUID as a response.
    /// </summary>
    private string _guid;

    public ResponsePacketHandler(ConnectionsController connection )
    {
      if (null == connection)
      {
        throw new ArgumentNullException(nameof(connection));
      }
      _connection = connection;
    }
    
    public Packet SendAndWait(SQLiteMessage type, byte[] data, int timeout )
    {
      // listen for new messages.
      _response = null;
      _connection.OnReceived += OnReceived;

      // try and send.
      try
      {
        // the packet handler.
        var packet = new PacketResponse( type, data );

        // save the guid we are looking for.
        _guid = packet.Guid;

        // send the data and wait for a response.
        _connection.Send(SQLiteMessage.SendAndWaitRequest, packet.Packed );

        Task.Run(async () => {
          var start = DateTime.Now;
          while (_response == null )
          {
            await Task.Delay(100);
            var elapsed = (DateTime.Now - start).TotalMilliseconds;
            if (elapsed >= timeout)
            {
              // we timed out.
              break;
            }
          }
        }).Wait();

      }
      finally
      {
        // whatever happens, we are no longer listening
        _connection.OnReceived -= OnReceived;
      }

      // return what we found.
      return _response;
    }

    private void OnReceived(Packet packet, Action<Packet> response )
    {
      // is it the response we might be waiting for?
      if (packet.Type != SQLiteMessage.SendAndWaitResponse)
      {
        // it is not a response, so we are not really interested.
        return;
      }

      // it looks like a posible match
      // so we will try and unpack it and see if it is the actual response.
      var packetResponse = new PacketResponse(packet.Packed);
      if (packetResponse.Guid != _guid)
      {
        // not the response packet we were looking for.
        return;
      }

      // we cannot use the payload of packet.Payload as it is
      // it is the payload of "Types.SendAndWaitResponse"
      _response = new Packet(packetResponse.Payload);
    }
  }
}
