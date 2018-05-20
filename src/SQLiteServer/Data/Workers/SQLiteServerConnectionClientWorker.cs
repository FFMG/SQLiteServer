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
using System.Data.SQLite;
using System.Threading.Tasks;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.Enums;
using SQLiteServer.Data.Exceptions;

namespace SQLiteServer.Data.Workers
{
  // ReSharper disable once InconsistentNaming
  internal class SQLiteServerConnectionClientWorker : IDisposable, ISQLiteServerConnectionWorker
  {
    #region Command Information
    /// <inheritdoc />
    public int CommandTimeout { get; }
    #endregion

    #region Private
    /// <summary>
    /// If the connection is locked or not.
    /// It if is not, then we can use it.
    /// </summary>
    private SQLiteConnection _connection;

    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The contoller
    /// </summary>
    private readonly ConnectionsController _controller;

    /// <summary>
    /// The SQLiteConnection connection string
    /// </summary>
    private readonly string _connectionString;
    #endregion

    public SQLiteServerConnectionClientWorker(string connectionString, ConnectionsController controller, int commandTimeout)
    {
      if (null == controller)
      {
        throw new ArgumentNullException( nameof(controller));
      }
      _controller = controller;
      _connectionString = connectionString;
      CommandTimeout = commandTimeout;
    }

    public void Dispose()
    {
      //  done already?
      if (_disposed)
      {
        return;
      }

      ThrowIfAny();
      try
      {
        _controller.DisConnect();
      }
      finally
      {
        _disposed = true;
      }
    }

    public void Open()
    {
      // nothing to do.
    }

    public void Close()
    {
      //  close the connections
      _controller.DisConnect();
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
        throw new ObjectDisposedException("The connection has been disposed.");
      }
    }

    /// <summary>
    /// Check that, as far as we can tell, the database is ready.
    /// </summary>
    private void ThrowIfAny()
    {
      // check disposed
      ThrowIfDisposed();
    }
    #endregion

    /// <inheritdoc />
    public async Task<ISQLiteServerCommandWorker> CreateCommandAsync(string commandText)
    {
      return await Task.Run( 
        () => new SQLiteServerCommandClientWorker( commandText, _controller, CommandTimeout)
      );
    }

    /// <inheritdoc />
    public async Task<SQLiteConnection> LockConnectionAsync()
    {
      // can we use this?
      ThrowIfAny();

      // wait for the connection
      if (!await WaitForLockedConnectionAsync(-1).ConfigureAwait(false))
      {
        throw new SQLiteServerException("Unable to obtain connection lock");
      }

      // can we use this?
      ThrowIfAny();

      // wait for the connection
      if (!await WaitForLockedConnectionAsync(-1).ConfigureAwait(false))
      {
        throw new SQLiteServerException("Unable to obtain connection lock");
      }
      
      var response = await _controller.SendAndWaitAsync(SQLiteMessage.LockConnectionRequest, null, CommandTimeout).ConfigureAwait(false);
      switch (response.Message)
      {
        case SQLiteMessage.SendAndWaitTimeOut:
          throw new TimeoutException("There was a timeout error obtaining the lock.");

        case SQLiteMessage.LockConnectionResponse:
          _connection = new SQLiteConnection( _connectionString);
          _connection.Open();
          return _connection;

        case SQLiteMessage.LockConnectionException:
          throw new SQLiteServerException(response.Get<string>());

        default:
          throw new InvalidOperationException($"Unknown response {response.Message} from the server.");
      }
    }

    /// <inheritdoc />
    public async Task UnLockConnectionAsync()
    {
      if (_connection == null)
      {
        return;
      }

      var response = await _controller.SendAndWaitAsync(SQLiteMessage.UnLockConnectionRequest, null, CommandTimeout).ConfigureAwait(false);
      switch (response.Message)
      {
        case SQLiteMessage.SendAndWaitTimeOut:
          throw new TimeoutException("There was a timeout error obtaining the lock.");

        case SQLiteMessage.LockConnectionResponse:
          _connection.Close();
          _connection = null;
          break;

        case SQLiteMessage.LockConnectionException:
          throw new SQLiteServerException(response.Get<string>());

        default:
          throw new InvalidOperationException($"Unknown response {response.Message} from the server.");
      }
    }

    /// <summary>
    /// Wait for the connection to be available.
    /// </summary>
    private async Task<bool> WaitForLockedConnectionAsync(int timeoutSeconds)
    {
      // wait for the connection to be availabe.
      if (null == _connection)
      {
        return true;
      }

      await Task.Run(async () => {
        var start = DateTime.Now;
        while (null != _connection)
        {
          // give other threads time.
          await Task.Yield();

          var elapsed = (DateTime.Now - start).TotalSeconds;
          if (timeoutSeconds > 0 && elapsed >= timeoutSeconds)
          {
            // we timed out.
            break;
          }
        }
      }).ConfigureAwait(false);

      return (_connection == null);
    }
  }
}
