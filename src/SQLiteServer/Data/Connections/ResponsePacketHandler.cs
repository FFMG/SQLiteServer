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
    /// We will wait a tiny amount of time to give other threads a chance
    /// Before we check if we go a resoponse from the server.
    /// If this number is too big we might delay the response by 'n-1'
    /// </summary>
    private readonly int _waitForResponseSleepTime;

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

    public ResponsePacketHandler(ConnectionsController connection, int waitForResponseSleepTime )
    {
      if (null == connection)
      {
        throw new ArgumentNullException(nameof(connection));
      }
      _connection = connection;
      _waitForResponseSleepTime = waitForResponseSleepTime;
    }
    
    /// <summary>
    /// Send a request to the server and wait for a response.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    /// <param name="timeout">The max number of ms we will be waitng.</param>
    /// <returns>The response packet</returns>
    public async Task<Packet> SendAndWaitAsync(SQLiteMessage type, byte[] data, int timeout )
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
        
        await Task.Run(async () => {
          var watch = System.Diagnostics.Stopwatch.StartNew();
          while (true )
          {
            if (_response != null)
            {
              if (_response.Message == SQLiteMessage.SendAndWaitBusy)
              {
                watch.Restart();
                _response = null;
              }
              else
              {
                break;
              }
            }

            // delay a little to give other thread a chance.
            if (_waitForResponseSleepTime > 0)
            {
              await Task.Delay(_waitForResponseSleepTime).ConfigureAwait(false);
            }
            else
            {
              await Task.Yield();
            }

            // check for delay
            if (watch.ElapsedMilliseconds >= timeout)
            {
              // we timed out.
              break;
            }
          }
          watch.Stop();
        }).ConfigureAwait( false );

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
      if (packet.Message != SQLiteMessage.SendAndWaitResponse)
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
