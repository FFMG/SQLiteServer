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
using System.Net;
using System.Threading.Tasks;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Data.SQLiteServer;
using SQLiteServer.Data.Workers;

namespace SQLiteServer.Data.Connections
{
  internal class SocketConnectionBuilder : IConnectionBuilder
  {
    #region Private variables
    /// <summary>
    /// The parent connection.
    /// </summary>
    private SQLiteServerConnection _connection;

    /// <summary>
    /// The connection controller.
    /// </summary>
    private ConnectionsController _connectionController;

    /// <summary>
    /// The IP address we will connect to
    /// </summary>
    private readonly IPAddress _address;

    /// <summary>
    /// The port number
    /// </summary>
    private readonly int _port;

    /// <summary>
    /// The connection backlog
    /// </summary>
    private readonly int _backlog;

    /// <summary>
    /// How often we want to check for timouts.
    /// </summary>
    private readonly int _heartBeatTimeOutInMs;

    /// <summary>
    /// Number of ms before a query expires
    /// </summary>
    private readonly int _queryTimeoutMs;

    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;
    #endregion

    public SocketConnectionBuilder( int queryTimeoutMs, IPAddress address, int port, int backlog, int heartBeatTimeOutInMs)
    {
      _queryTimeoutMs = queryTimeoutMs;
      _address = address;
      _port = port;
      _backlog = backlog;
      _heartBeatTimeOutInMs = heartBeatTimeOutInMs;
    }

    /// <inheritdoc />
    public async Task<bool> ConnectAsync(SQLiteServerConnection connection)
    {
      // sanity check
      ThrowIfDisposed();

      // save the connection
      _connection = connection;

      // sanity check
      if (_connectionController?.Connected ?? false)
      {
        throw new SQLiteServerException("Already connected!");
      }

      _connectionController = new ConnectionsController(_address, _port, _backlog, _heartBeatTimeOutInMs);
      if (!_connectionController.Connect())
      {
        throw new SQLiteServerException("Unable to connected.");
      }

      while (!_connectionController.Connected)
      {
        await Task.Delay(_heartBeatTimeOutInMs); // arbitrary delay
      }

      if (!_connectionController.Connected)
      {
        throw new SQLiteServerException("Timmed out waiting for update.");
      }
      return true;
    }
    
    /// <inheritdoc />
    public void Disconnect()
    {
      // sanity check
      ThrowIfDisposed();

      if (_connectionController != null && _connectionController.Client)
      {
        _connectionController.OnServerDisconnect -= OnServerDisconnect;
      }

      _connectionController?.DisConnect();
      _connectionController = null;
    }

    private void OnServerDisconnect()
    {
      _connection?.ReOpen();
    }

    /// <inheritdoc />
    public Task<ISQLiteServerConnectionWorker> OpenAsync(string connectionString )
    {
      // sanity check
      ThrowIfDisposed();

      // we are now connected, (otherwise we would have thrown).
      // so we can now create the required worker.
      ISQLiteServerConnectionWorker worker;
      if (_connectionController.Server)
      {
        worker = new SQLiteServerConnectionServerWorker(connectionString, _connectionController, _queryTimeoutMs);
        return Task.FromResult(worker) ;
      }

      _connectionController.OnServerDisconnect += OnServerDisconnect;
      worker = new SQLiteServerConnectionClientWorker(_connectionController, _queryTimeoutMs);

      return Task.FromResult(worker);
    }

    /// <summary>
    /// Throws an exception if we are trying to execute something 
    /// After this has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
      if (_disposed)
      {
        throw new ObjectDisposedException(nameof(SQLiteServerCommand));
      }
    }

    public void Dispose()
    {
      //  done already?
      if (_disposed)
      {
        return;
      }

      try
      {
        Disconnect();
      }
      finally
      {
        // all done.
        _disposed = true;
      }
    }
  }
}
