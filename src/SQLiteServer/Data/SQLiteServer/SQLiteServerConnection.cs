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
using System.Data;
using System.Net;
using System.Threading.Tasks;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.Workers;

namespace SQLiteServer.Data.SQLiteServer
{
  // ReSharper disable once InconsistentNaming
  public sealed class SQLiteServerConnection : ICloneable, IDisposable
  {
    #region Private 
    /// <summary>
    /// The connection builder
    /// </summary>
    private readonly IConnectionBuilder _connectionBuilder;

    /// <summary>
    /// The actual sql connection.
    /// </summary>
    private ISQLiteServerConnectionWorker _worker;

    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;
    #endregion

    #region Public
    /// <summary>
    /// The given connection string
    /// </summary>
    public string ConnectionString { get; set; }

    public string Database { get { throw new NotSupportedException(); } }

    /// <summary>
    /// Get the current property
    /// </summary>
    // ReSharper disable once ConvertToAutoProperty
    public ConnectionState State { get; private set; }

    public string ServerVersion { get { throw new NotSupportedException(); } }
    #endregion

    public SQLiteServerConnection(string connectionString, IPAddress address, int port, int backlog, int heartBeatTimeOutInMs) :
      this( connectionString, new SocketConnectionBuilder(address, port, backlog, heartBeatTimeOutInMs))
    { 
    }

    internal SQLiteServerConnection(string connectionString, IConnectionBuilder connectionBuilder)
    {
      _connectionBuilder = connectionBuilder;
      ConnectionString = connectionString;
      State = ConnectionState.Closed;
    }

    #region Validations
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

    /// <summary>
    /// Throws an exception if we are trying to execute something 
    /// After this has been disposed.
    /// </summary>
    private void ThrowIfNotOpen()
    {
      if (State != ConnectionState.Open)
      {
        throw new InvalidOperationException("Source database is not open.");
      }
      
      if (null == _worker)
      {
        throw new InvalidOperationException( "The worker is not ready, has the datbase been open?");
      }
    }

    /// <summary>
    /// Check that, as far as we can tell, the database is ready.
    /// </summary>
    private void ThrowIfAny()
    {
      // check disposed
      ThrowIfDisposed();

      // check if not open
      ThrowIfNotOpen();
    }
    #endregion

    public object Clone()
    {
      throw new NotImplementedException();
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
        State = ConnectionState.Closed;
        _connectionBuilder.Dispose();
      }
      finally
      {
        // all done.
        _disposed = true;
      }
    }

    #region Database Operations
    /// <summary>
    /// Open the database using the connection string.
    /// </summary>
    public void Open()
    {
      if (State != ConnectionState.Closed)
      {
        throw new InvalidOperationException($"Cannot Open the database is not closed : {State}");
      }

      try
      {
        // we are connecting
        State = ConnectionState.Connecting;

        // re-open, we will change no state
        // and we will not check anything,
        OpenNoStateCheck();

        // we are now open
        State = ConnectionState.Open;
      }
      catch
      {
        // close whatever might have  been open
        OpenError();
        throw;
      }
    }

    /// <summary>
    /// Close the database and the related commands.
    /// </summary>
    public void Close()
    {
      // it might sound silly
      // but if we are connecting, we need to wait a little.
      // so we don't disconnect ... while connecting.
      WaitIfConnecting();

      // are we open?
      ThrowIfNotOpen();

      try
      {

        // close the worker.
        _worker?.Close();

        // disconnect everything
        _connectionBuilder.Disconnect();
      }
      finally
      {
        State = ConnectionState.Closed;
      }
    }

    public void ChangeDatabase(string databaseName)
    {
      throw new NotSupportedException();
    }
    #endregion

    #region Internal functions
    /// <summary>
    /// Reopen the connection.
    /// </summary>
    internal void ReOpen()
    {
      try
      {
        // we are connecting
        State = ConnectionState.Connecting;

        // close everything
        _connectionBuilder.Disconnect();
        _worker.Close();

        // re-open, we will change no state
        // and we will not check anything,
        OpenNoStateCheck();

        // we re-opened.
        State = ConnectionState.Open;
      }
      catch
      {
        // close whatever might have  been open
        OpenError();
        throw;
      }
    }

    private void OpenError()
    {
      _connectionBuilder?.Disconnect();
      _worker?.Close();
      _worker = null;
      State = ConnectionState.Broken;
    }

    private void OpenNoStateCheck()
    {
      // try and re-connect.
      Task.Run(async () =>
        await _connectionBuilder.ConnectAsync(this)
      ).Wait();

      Task.Run(async () =>
        _worker = await _connectionBuilder.OpenAsync(ConnectionString)
      ).Wait();

      _worker.Open();
    }

    /// <summary>
    /// Do no do anything if we are waiting to reconnect.
    /// </summary>
    internal void WaitIfConnecting()
    {
      // wait for 30 seconds.
      if (State != ConnectionState.Connecting)
      {
        return;
      }

      const int timeout = 30000;
      Task.Run(async () => {
        var start = DateTime.Now;
        while (State == ConnectionState.Connecting)
        {
          await Task.Yield();
          var elapsed = (DateTime.Now - start).TotalMilliseconds;
          if (elapsed >= timeout)
          {
            // we timed out.
            break;
          }
        }
      }).Wait();
    }

    /// <summary>
    /// Create a SQL server command.
    /// </summary>
    /// <param name="commandText"></param>
    /// <returns></returns>
    internal ISQLiteServerCommandWorker CreateCommand(string commandText)
    {
      WaitIfConnecting();
      ThrowIfAny();
      return _worker.CreateCommand(commandText);
    }
    #endregion
  }
}
