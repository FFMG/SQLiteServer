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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using SQLiteServer.Data.Enums;

namespace SQLiteServer.Data.Connections
{
  /// <summary>
  /// Socket class to check if we are connected or not.
  /// </summary>
  internal static class SocketExtensions
  {
    public static bool IsConnected(this Socket socket)
    {
      try
      {
        return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
      }
      catch (SocketException)
      {
        return false;
      }
      catch (ObjectDisposedException)
      {
        return false;
      }
    }
  }

  internal class ConnectionsController
  {
    /// <summary>
    /// What type of connection are we?
    /// Master? Client? or Unknown?
    /// </summary>
    private enum ConnectionType
    {
      Unknown,
      Server,
      Client
    }

    /// <summary>
    /// Information about the connection currently been handled.
    /// </summary>
    private struct ConnectionsControllerformation
    {
      public Socket Socket;
      public byte[] Payload;
    };

    #region Delegates
    /// <summary>
    /// The delegate to use with the event.
    /// </summary>
    /// <param name="isServer"></param>
    public delegate void DelegateOnConnected(bool isServer );

    /// <summary>
    /// The delegate for the event.
    /// </summary>
    /// <param name="packet"></param>
    public delegate void DelegateOnReceived(Packet packet, Action<Packet> response );

    /// <summary>
    /// delegate for the server disconnected message.
    /// </summary>
    public delegate void DelegateOnServerDisconnect();

    /// <summary>
    /// When we are commected
    /// The argument is true if we are master, false if we are client.
    /// </summary>
    public event DelegateOnConnected OnConnected = delegate { };

    /// <summary>
    /// When we lost connection with the server.
    /// </summary>
    public event DelegateOnServerDisconnect OnServerDisconnect = delegate { };

    /// <summary>
    /// When data is received
    /// </summary>
    public event DelegateOnReceived OnReceived = delegate { };
    #endregion

    #region Helper properties
    /// <summary>
    /// If we are connected or not.
    /// </summary>
    public bool Connected { get; private set; }

    /// <summary>
    /// If we are the master/server or not.
    /// </summary>
    public bool Server => _connectionType == ConnectionType.Server;

    /// <summary>
    /// If we are the client or not.
    /// </summary>
    public bool Client => _connectionType == ConnectionType.Client;
    #endregion

    #region Private variables
    /// <summary>
    /// Our lock
    /// </summary>
    private readonly object _lock = new object();

    /// <summary>
    /// Assuming we are master, this is our list of client sockets.
    /// </summary>
    private readonly List<Socket> _clients = new List<Socket>();

    /// <summary>
    /// Our type of connections.
    /// </summary>
    private ConnectionType _connectionType = ConnectionType.Unknown;

    /// <summary>
    /// Our socket... client or server.
    /// </summary>
    private readonly Socket _socket;

    /// <summary>
    /// The heartbeat timer so we can check connections.
    /// </summary>
    private Timer _heartBeatTimer;
    #endregion

    #region Configuration
    /// <summary>
    /// We will wait a tiny amount of time to give other threads a chance
    /// Before we check if we go a resoponse from the server.
    /// If this number is too big we might delay the response by 'n-1'
    /// </summary>
    private const int WaitForResponseSleepTime = 0;

    /// <summary>
    /// The IP address we want to connect to.
    /// </summary>
    private IPAddress Address { get; }

    /// <summary>
    /// Port number we will try and connect to.
    /// </summary>
    private int Port { get; }

    private int Backlog { get; }

    /// <summary>
    /// How often we want to check for timeouts.
    /// </summary>
    private int HeartBeatTimeOutInMs { get; }

    /// <summary>
    /// The packet sizes we will be receiving.
    /// The size does not really matter but I wouldn't make it too extreem
    /// If that number is too low then processing time will increase by a fair amount.
    /// I would suggest at least sizeof(int) + sizeof(uint) for the type + length
    /// </summary>
    private const int DefaultPacketSize = 2048;
    #endregion

    /// <summary>
    /// The current unprocessed data.
    /// Once we receive data we try and process it.
    /// This data could get as big as the data we are reading, (sizeof(int))
    /// </summary>
    private readonly Dictionary<Socket,Packets> _currentPackets = new Dictionary<Socket, Packets>();

    public ConnectionsController(IPAddress address, int port, int backlog, int heartBeatTimeOutInMs)
    {
      Address = address ?? IPAddress.Any;
      Port = port;
      Backlog = backlog;
      HeartBeatTimeOutInMs = heartBeatTimeOutInMs;

      // Create a TCP/IP socket.
      _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    #region Manage Connect
    /// <summary>
    /// Tell the client/server to connect.
    /// </summary>
    /// <returns></returns>
    public bool Connect()
    {
      // disconnect
      DisConnect();

      // we first try and be a server.
      if (ConnectAsServer())
      {
        return true;
      }

      if (ConnectClient())
      {
        return true;
      }

      // did not work
      return false;
    }

    private bool ConnectAsServer()
    {
      try
      {
        var localEndPoint = new IPEndPoint(Address, Port);

        // Bind the socket to the local endpoint and listen for incoming connections.
        _socket.Bind(localEndPoint);
        _socket.Listen(Backlog);

        // Start an asynchronous socket to listen for connections.
        _socket.BeginAccept(BeginServerAccept, _socket);
      }
      catch (Exception)
      {
        return false;
      }

      // we are now connected as the server
      OnServerConnected();

      return true;
    }

    private bool ConnectClient()
    {
      // Connect to a remote device.
      try
      {
        var localEndPoint = new IPEndPoint(Address, Port);

        // Connect to the remote endpoint.
        _socket.BeginConnect(localEndPoint, BeginClientConnect, _socket);
      }
      catch (Exception)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Called to indicate that we are connected as master.
    /// </summary>
    private void OnServerConnected()
    {
      // we are now of type 'master'
      _connectionType = ConnectionType.Server;

      // we do not start the heartbeat timer until we have clients
      // that are added to our list of clients.
      // there is no point in starting a timer thread if we have no clients to connect to.

      // we are now connected
      Connected = true;

      // send notification that we are connected.
      OnConnected( true);
    }

    /// <summary>
    /// Called to indicate that we are not connected as a client.
    /// </summary>
    private void OnClientConnected()
    {
      // we are of type 'client'
      _connectionType = ConnectionType.Client;

      // start the heartbeat in case we loose the master.
      StartHeartBeat();

      // we are now connected
      Connected = true;

      // send a notification to all the delegates.
      OnConnected( false);
    }
    #endregion

    #region Manage Disconnect
    /// <summary>
    /// Tell the client/server to disconnect.
    /// </summary>
    public void DisConnect()
    {
      // are we even connected?
      if (!Connected)
      {
        return;
      }
      // we are no longer connected.
      Connected = false;

      // save the current type
      var type = _connectionType;

      // do the default disconnects.
      DisConnectCommon();

      switch (type)
      {
        case ConnectionType.Server:
          DisConnectServer();
          break;

        case ConnectionType.Client:
          DisConnectClient();
          break;

        case ConnectionType.Unknown:
          break;
      }
    }

    /// <summary>
    /// Disconnect the client
    /// </summary>
    private void DisConnectClient()
    {
      try
      {
        _socket?.Shutdown(SocketShutdown.Both);
        _socket?.Close();
      }
      catch
      {
        // ignore
        // the sockets might be closed alread.
      }
    }

    /// <summary>
    /// Disconnect the server.
    /// </summary>
    private void DisConnectServer()
    {
      _socket.Close();
    }

    /// <summary>
    /// Disconnect the common values/
    /// </summary>
    private void DisConnectCommon()
    {
      StopHeartBeat();

      // regadless we are not master
      _connectionType = ConnectionType.Unknown;

      lock (_lock)
      {
        foreach (var client in _clients)
        {
          client.Disconnect(true);
        }
        _clients.Clear();
      }
    }
    #endregion

    /// <summary>
    /// This assumes that we are master, register a client socket.
    /// So we can try and keep track of it.
    /// </summary>
    /// <param name="client"></param>
    private void RegisterClientSocket(Socket client)
    {
      if (!Server)
      {
        throw new Exception("We are not the master, so we cannot register any clients." );
      }

      lock (_lock)
      {
        // add that client
        _clients.Add( client );

        // start the heartbeat
        StartHeartBeat();
      }
    }

    #region HeartBeat
    /// <summary>
    /// Start the Master/Client heartbeats to check connections.
    /// </summary>
    private void StartHeartBeat()
    {
      if (null != _heartBeatTimer)
      {
        return;
      }

      lock (_lock)
      {
        if (_heartBeatTimer != null)
        {
          return;
        }

        _heartBeatTimer = new Timer(HeartBeatTimeOutInMs)
        {
          AutoReset = false,
          Enabled = true
        };
        _heartBeatTimer.Elapsed += HeartBeat;
      }
    }

    /// <summary>
    /// Stop the heartbeats.
    /// </summary>
    private void StopHeartBeat()
    {
      if (_heartBeatTimer == null)
      {
        return;
      }

      lock (_lock)
      {
        if (_heartBeatTimer == null)
        {
          return;
        }

        _heartBeatTimer.Enabled = false;
        _heartBeatTimer.Stop();
        _heartBeatTimer.Dispose();
        _heartBeatTimer = null;
      }
    }

    /// <summary>
    /// Called durring the heart beat.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void HeartBeat(object source, ElapsedEventArgs e)
    {
      // stop the heartbeat/
      StopHeartBeat();

      // check the master and client.
      // they will restart the timeers if needed.
      HeartBeatMaster();
      HeartBeatClient();
    }
    
    /// <summary>
    /// If we are the master, check the heart beat.
    /// </summary>
    private void HeartBeatMaster()
    {
      if (!Server)
      {
        return;
      }

      // double check our client sockets.
      lock (_lock)
      {
        for (var i = 0; i < _clients.Count; ++i)
        {
          if (_clients[i].IsConnected())
          {
            continue;
          }

          _clients[i] = null;
        }

        // remove all the disconnected clients.
        _clients.RemoveAll(c => c == null);

        // only restart the heartbeat if we have one or more clients.
        // otherwise there is no point in checking all the time.
        if (_clients.Count > 0)
        {
          StartHeartBeat();
        }
      }
    }

    /// <summary>
    /// If we are a client, check the heartbeat.
    /// </summary>
    private void HeartBeatClient()
    {
      if (!Client)
      {
        return;
      }

      // check if we have lost connection with our server.
      if (_socket.IsConnected())
      {
        // we are still connected, so we can 
        // restart the timer and check again in a few seconds.
        StartHeartBeat();
        return;
      }

      // notify that we lost connection with the server.
      // not that although we are not connected, we are not closing the connection.
      OnServerDisconnect();

      // we are no longer connected
      DisConnect();

      // no need to restart the heartbeat, we already know it is not working.
    }
    #endregion

    #region Send data
    /// <summary>
    /// Send a message to the clients or to the master.
    /// @todo we need to fix this function as it is not a good use case.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    public void Send(SQLiteMessage type, byte[] data)
    {
      if (Server)
      {
        SendToClients(type, data);
      }
      else
      {
        SendToServer(type, data);
      }
    }

    public async Task<Packet> SendAndWaitAsync(SQLiteMessage type, byte[] data, int timeout )
    {
      // create the packer
      var packer = new ResponsePacketHandler( this, WaitForResponseSleepTime);

      // send and wait for the response.
      return await packer.SendAndWaitAsync( type, data, timeout ).ConfigureAwait( false );
    }

    /// <summary>
    /// Send a message to all the listening clients.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    private void SendToClients(SQLiteMessage type, byte[] data)
    {
      SendToClients( new Packet( type, data ));
    }

    /// <summary>
    /// Send a message to all the listening clients.
    /// </summary>
    /// <param name="data"></param>
    private void SendToClients( Packet data )
    {

      if (!Server)
      {
        throw new Exception( "We need to create a class to send a message to all the clients from one client.");
      }

      lock (_lock)
      {
        foreach (var client in _clients)
        {
          SendToClient( client, data.Packed);
        }
      }
    }

    /// <summary>
    /// Send the full payload to our client.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="payload"></param>
    private void SendToClient(Socket client, byte[] payload)
    {
      try
      {
        // are we still connected?
        if (client.IsConnected())
        {
          client.BeginSend(payload, 0, payload.Length, 0, SendCallback, client);
        }
      }
      catch (SocketException)
      {
        //  we need to log that we could not send this.
      }
    }

    /// <summary>
    /// Send a single message to the master.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    private void SendToServer(SQLiteMessage type, byte[] data)
    {
      SendToServer( new Packet(type, data));
    }

    private void SendToServer(Packet data)
    {
      if (Server)
      {
        // send it to myself...
        return;
      }

      // Begin sending the data to the remote device.
      _socket.BeginSend(data.Packed, 0, data.Length, 0, SendCallback, _socket);
    }
    #endregion

    #region Socket
    /// <summary>
    /// When a message has been sent to the socket.
    /// </summary>
    /// <param name="ar"></param>
    private void SendCallback(IAsyncResult ar)
    {
      try
      {
        // Retrieve the socket from the state object.
        var handler = (Socket)ar.AsyncState;

        // Complete sending the data to the remote device.
        handler.EndSend(ar);  // return the number of bytes sent.
      }
      catch (Exception )
      {
        // ignored
      }
    }

    private void BeginClientConnect(IAsyncResult ar)
    {
      try
      {
        // Retrieve the socket from the state object.
        var client = (Socket)ar.AsyncState;

        // Complete the connection.
        client.EndConnect(ar);

        // start receiving
        var packet = new ConnectionsControllerformation
        {
          Socket = client,
          Payload = new byte[DefaultPacketSize]
        };
        client.BeginReceive(packet.Payload, 0, DefaultPacketSize, SocketFlags.None, ReceiveCallback, packet);

        OnClientConnected();
      }
      catch
      {
        // ignored
      }
    }

    private void BeginServerAccept(IAsyncResult ar)
    {
      try
      {
        // Get the socket that handles the client request.
        // var listener = (Socket)ar.AsyncState;

        // Create the state object.
        var client = _socket.EndAccept(ar);

        try
        {
          // then listen for a new connection right away.
          // unless of course we are disconnected.
          // in that case, don't listen for anything else.
          _socket.BeginAccept(BeginServerAccept, _socket);

          // register this socket
          RegisterClientSocket(client);

          var packet = new ConnectionsControllerformation
          {
            Socket = client,
            Payload = new byte[DefaultPacketSize]
          };
          client.BeginReceive(packet.Payload, 0, DefaultPacketSize, SocketFlags.None, ReceiveCallback, packet);
        }
        catch
        {
          client.Disconnect(true);
        }
      }
      catch
      {
        // ignored
      }
    }
    
    private void ReceiveCallback(IAsyncResult ar)
    {
      try
      {
        // Retrieve the state object and the handler socket
        // from the asynchronous state object.
        var packet = (ConnectionsControllerformation)ar.AsyncState;

        // the client.
        var client = packet.Socket;

        // Read data from the socket. 
        var bytesRead = client.EndReceive(ar);
        if (bytesRead == 0)
        {
          return;
        }

        // There  might be more data, so store the data received so far.
        // state.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

        //  we received it all.
        HandleReceived(client, packet.Payload, bytesRead);

        // Not all data received. Get more.
        packet = new ConnectionsControllerformation
        {
          Socket = client,
          Payload = new byte[DefaultPacketSize]
        };
        client.BeginReceive(packet.Payload, 0, DefaultPacketSize, 0, ReceiveCallback, packet );
      }
      catch (ArgumentException )
      {
        // seen happening when we receive something after disconnecting.
        // the heartbeat will reemove the socket.
      }
      catch (SocketException )
      {
      }
      catch (ObjectDisposedException )
      {
        // the client socket might have been disconnected.
      }
    }
    #endregion

    #region Receive

    /// <summary>
    /// When we receive a payload we will handle it here.
    /// </summary>
    /// <param name="socket">The socket we received this from.</param>
    /// <param name="received"></param>
    /// <param name="bytesRead"></param>
    private void HandleReceived(Socket socket, byte[] received, int bytesRead)
    {
      // the packets we will be sending.
      lock (_lock)
      {
        if (!_currentPackets.ContainsKey(socket))
        {
          _currentPackets[socket] = new Packets();
        }

        // queue those bytes.
        _currentPackets[socket].Queue(received, bytesRead);
      }

      // get the current packets.
      var packets = _currentPackets[socket].UnQueue();

      // we can now process the packets
      // we are out of the lock as no global variables are updated anymore.
      // We don't want to be in the lock anyway as someone else might want to send something.
      // or call something that might cause another lock to be needed. 
      ProcessPackets( socket, packets.ToList().AsReadOnly() );
    }

    /// <summary>
    /// Proces all the packets that are ready to be sent.
    /// </summary>
    /// <param name="socket">The socket that sent all those messages.</param>
    /// <param name="packets"></param>
    private void ProcessPackets( Socket socket, IReadOnlyCollection<Packet> packets )
    {
      // so we have anything to process?
      if (!packets?.Any() ?? true )
      {
        return;
      }

      // we are now out of the lock, so we can send the data
      // this is what was received, so we can tell others about it.
      foreach (var packetTypeAndPayload in packets)
      {
        // if this is a 'sendreceived' type of message
        // we need to unpack it, so we can handle the response, (if there is one).
        SendOnReceived( socket, packetTypeAndPayload.Message, packetTypeAndPayload.Payload);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client">The client that sent messages.</param>
    /// <param name="type">The message type</param>
    /// <param name="payload"></param>
    private void SendOnReceived( Socket client, SQLiteMessage type, byte[] payload )
    {
      // are we a 'send and wait type'?
      // if we are then send... and wait for a response.
      // if no respinse is sent... send one.
      if (type == SQLiteMessage.SendAndWaitRequest)
      {
        var packet = new PacketResponse( payload );
        OnReceived(
          packet.Packet,
          (p) => 
          {
            // send whatever response the client wants us to.
            // but we need to send it back as a response.
            var response = new PacketResponse(SQLiteMessage.SendAndWaitResponse, p.Packed, packet.Guid );
            SendToClient(client, response.Packed);
          }
        );
        return;
      }

      OnReceived(
        new Packet( type, payload),
        (p) => {
          // send whatever response the client wants us to.
          SendToClient(client, p.Packed );
        }
      );
    }
    #endregion
  }
}
