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
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.Enums;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Data.Workers
{
  /// <inheritdoc />
  // ReSharper disable once InconsistentNaming
  internal class SQLiteServerCommandClientWorker : ISQLiteServerCommandWorker
  {
    #region Constant

    private const int QueryTimeouts = 5000;
    #endregion

    #region Private Variables
    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The SQLite command
    /// </summary>
    private readonly string _commandText;

    /// <summary>
    /// The SQLite Connection
    /// </summary>
    private readonly ConnectionsController _controller;

    /// <summary>
    /// This is the server Guid
    /// </summary>
    private readonly string _serverGuid;
    #endregion

    public SQLiteServerCommandClientWorker(string commandText, ConnectionsController controller)
    {
      if (null == controller)
      {
        throw new ArgumentNullException( nameof(controller));
      }
      _commandText = commandText;
      _controller = controller;
      _serverGuid = null;

      var response = _controller.SendAndWaitAsync( SQLiteMessage.CreateCommandRequest, Encoding.ASCII.GetBytes(commandText), QueryTimeouts).Result;
      if (null == response)
      {
        throw new TimeoutException( "There was a timeout error creating the Command.");
      }

      switch (response.Message)
      {
        case SQLiteMessage.CreateCommandResponse:
          _serverGuid = response.Get<string>();
          break;

        case SQLiteMessage.CreateCommandException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException( $"Unknown response {response.Message} from the server.");
      }
    }

    /// <summary>
    /// Check that, as far as we can tell, the database is ready.
    /// </summary>
    private void ThrowIfAny()
    {
      // check if disposed.
      ThrowIfDisposed();

      // check if not open
      ThrowIfNoCommand();
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

    /// <summary>
    /// Make sure that we have a valid command guid from the server.
    /// Without a GUID we cannot tell the serevr what to do for us.
    /// </summary>
    private void ThrowIfNoCommand()
    {
      if (_serverGuid == null)
      {
        throw new ArgumentNullException( nameof(_serverGuid), "The given command is invalid.");
      }
    }
    
    public int ExecuteNonQuery()
    {
      ThrowIfAny();
      var response = _controller.SendAndWaitAsync(SQLiteMessage.ExecuteNonQueryRequest, Encoding.ASCII.GetBytes(_serverGuid), QueryTimeouts).Result;
      if (null == response)
      {
        throw new TimeoutException("There was a timeout error creating the Command.");
      }

      switch (response.Message)
      {
        case SQLiteMessage.ExecuteNonQueryResponse:
          return response.Get<int>();

        case SQLiteMessage.ExecuteNonQueryException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException($"Unknown response {response.Message} from the server.");
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
        ThrowIfAny();
      _controller.Send( SQLiteMessage.DisposeCommand, Encoding.ASCII.GetBytes(_serverGuid) );
      }
      finally
      {
        // all done.
        _disposed = true;
      }
    }

    public ISqliteServerDataReaderWorker CreateReaderWorker()
    {
      return new SqliteServerDataReaderClientWorker(_controller, _serverGuid, QueryTimeouts );
    }
  }
}
