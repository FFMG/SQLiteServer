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
using System.Data.Common;
using System.Data.SQLite;
using System.Net;
using System.Threading.Tasks;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.Workers;

namespace SQLiteServer.Data.SQLiteServer
{
  // ReSharper disable once InconsistentNaming
  public sealed class SQLiteServerConnection : DbConnection
  {
    #region Private 
    /// <summary>
    /// Our current connection state
    /// </summary>
    private ConnectionState _connectionState;

    /// <summary>
    /// The current transaction, if we have one.
    /// </summary>
    private SQLiteServerTransaction _transaction;

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
    /// <inheritdoc />
    public override string ConnectionString { get; set; }

    /// <inheritdoc />
    public override string Database => throw new NotSupportedException();

    /// <inheritdoc />
    public override string DataSource
    {
      get
      {
        var builder = new SQLiteConnectionStringBuilder(ConnectionString);
        return builder.DataSource;
      }
    }

    /// <inheritdoc />
    // ReSharper disable once ConvertToAutoProperty
    public override ConnectionState State => _connectionState;

    /// <inheritdoc />
    public override string ServerVersion { get { throw new NotSupportedException(); } }
    #endregion

    public SQLiteServerConnection(string connectionString, IPAddress address, int port, int backlog, int heartBeatTimeOutInMs) :
      this( connectionString, new SocketConnectionBuilder(address, port, backlog, heartBeatTimeOutInMs))
    { 
    }

    internal SQLiteServerConnection(string connectionString, IConnectionBuilder connectionBuilder)
    {
      _connectionBuilder = connectionBuilder;
      ConnectionString = connectionString;
      _connectionState = ConnectionState.Closed;
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

    protected override void Dispose(bool disposing)
    {
      //  done already?
      if (_disposed)
      {
        return;
      }

      try
      {
        if (disposing)
        {
          // before closing
          RollbackOpenTransactions();
          _transaction?.Dispose();

          // we are now closed
          _connectionState = ConnectionState.Closed;
          _connectionBuilder.Dispose();

          // all done.
          _disposed = true;
        }
      }
      finally
      {
        // tell the parent to do the same.
        base.Dispose(disposing);
      }
    }

    /// <summary>
    /// Rollback the current transactions, if we have any.
    /// </summary>
    private void RollbackOpenTransactions()
    {
      _transaction?.RollbackOpenTransactions();
    }

    #region Database Operations
    public override void Open()
    {
      if (State != ConnectionState.Closed)
      {
        throw new InvalidOperationException($"Cannot Open the database is not closed : {State}");
      }

      try
      {
        // we are connecting
        _connectionState = ConnectionState.Connecting;

        // re-open, we will change no state
        // and we will not check anything,
        try
        {
          OpenWithNoValidationAsync().Wait();
        }
        catch (AggregateException e)
        {
          if (e.InnerException != null)
          {
            throw e.InnerException;
          }
          throw;
        }
        
        // we are now open
        _connectionState = ConnectionState.Open;
      }
      catch
      {
        // close whatever might have  been open
        OpenError();
        throw;
      }
    }

    /// <inheritdoc />
    public override void Close()
    {
      // it might sound silly
      // but if we are connecting, we need to wait a little.
      // so we don't disconnect ... while connecting.
      WaitIfConnectingAsync().Wait();

      // are we open?
      ThrowIfNotOpen();

      try
      {
        // rollback open transaction
        RollbackOpenTransactions();

        // close the worker.
        _worker?.Close();

        // disconnect everything
        _connectionBuilder.Disconnect();
      }
      finally
      {
        _connectionState = ConnectionState.Closed;
      }
    }

    /// <inheritdoc />
    public override void ChangeDatabase(string databaseName)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc />
    protected override DbCommand CreateDbCommand()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      if (null == _transaction)
      {
        _transaction = new SQLiteServerTransaction( this );
      }

      // and begin the transaction
      _transaction.Begin( isolationLevel );

      // return the transaction level.
      return _transaction;
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
        _connectionState = ConnectionState.Connecting;

        // close everything
        _connectionBuilder.Disconnect();
        _worker.Close();

        // re-open, we will change no state
        // and we will not check anything,
        OpenWithNoValidationAsync().Wait();

        // we re-opened.
        _connectionState = ConnectionState.Open;
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
      _connectionState = ConnectionState.Broken;
    }

    private async Task OpenWithNoValidationAsync()
    {
      // try and re-connect.
      await Task.Run(async () =>
        await _connectionBuilder.ConnectAsync(this)
      ).ConfigureAwait(false);

      await Task.Run(async () =>
        _worker = await _connectionBuilder.OpenAsync(ConnectionString)
      ).ConfigureAwait(false);

      _worker.Open();
    }

    /// <summary>
    /// Do no do anything if we are waiting to reconnect.
    /// </summary>
    internal async Task WaitIfConnectingAsync()
    {
      // wait for 30 seconds.
      if (State != ConnectionState.Connecting)
      {
        return;
      }

      const int timeout = 30000;
      await Task.Run(async () => {
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
      }).ConfigureAwait( false );
    }

    /// <summary>
    /// Create a SQL server command.
    /// </summary>
    /// <param name="commandText"></param>
    /// <returns></returns>
    internal ISQLiteServerCommandWorker CreateCommand(string commandText)
    {
      WaitIfConnectingAsync().Wait();

      ThrowIfAny();
      return _worker.CreateCommand(commandText);
    }
    #endregion
  }
}
