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
using System.Threading.Tasks;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.Data;
using SQLiteServer.Data.Enums;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Data.Workers
{
  /// <inheritdoc />
  // ReSharper disable once InconsistentNaming
  internal class SQLiteServerCommandClientWorker : ISQLiteServerCommandWorker
  {
    #region Private Variables
    /// <inheritdoc />
    public int CommandTimeout { get; }

    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The SQLite command
    /// </summary>
    public string CommandText { get; }

    /// <summary>
    /// The SQLite Connection
    /// </summary>
    private readonly ConnectionsController _controller;

    /// <summary>
    /// This is the server Guid
    /// </summary>
    private string _serverGuid;

    /// <summary>
    /// Get the guid if we have one
    /// </summary>
    public string Guid
    {
      get { return _serverGuid; }
      set
      {
        _serverGuid = value;
      }
    }

    #endregion

    public SQLiteServerCommandClientWorker(string commandText, ConnectionsController controller, int commandTimeout)
    {
      if (null == controller)
      {
        throw new ArgumentNullException(nameof(controller));
      }

      CommandTimeout = commandTimeout;

      CommandText = commandText;
      _controller = controller;
    }

    /// <summary>
    /// Get the server guid
    /// </summary>
    /// <returns></returns>
    private async Task<string> CreateGuidAsync()
    { 
      var response = await _controller.SendAndWaitAsync( SQLiteMessage.CreateCommandRequest, Encoding.ASCII.GetBytes(CommandText), CommandTimeout).ConfigureAwait(false);
      switch (response.Message)
      {
        case SQLiteMessage.SendAndWaitTimeOut:
          throw new TimeoutException("There was a timeout error creating the Command.");

        case SQLiteMessage.CreateCommandResponse:
          return response.Get<string>();

        case SQLiteMessage.CreateCommandException:
          throw new SQLiteServerException(response.Get<string>());

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
    
    /// <inheritdoc />
    public async Task<int> ExecuteNonQueryAsync()
    {
      ThrowIfAny();
      Packet response;
      if (_serverGuid == null)
      {
        response = await _controller.SendAndWaitAsync(SQLiteMessage.ExecuteCommandNonQueryRequest, Encoding.ASCII.GetBytes(CommandText), CommandTimeout).ConfigureAwait( false );
      }
      else
      {
        response = await _controller.SendAndWaitAsync(SQLiteMessage.ExecuteNonQueryRequest, Encoding.ASCII.GetBytes(_serverGuid), CommandTimeout).ConfigureAwait( false );
      }
      switch (response.Message)
      {
        case SQLiteMessage.SendAndWaitTimeOut:
          throw new TimeoutException("There was a timeout error creating the Command.");

        case SQLiteMessage.ExecuteNonQueryResponse:
          var guiAndIndexRequest = Fields.Fields.Unpack(response.Payload).DeserializeObject<GuidAndIndexRequest>();
          Guid = guiAndIndexRequest.Guid;
          return guiAndIndexRequest.Index;

        case SQLiteMessage.ExecuteNonQueryException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException($"Unknown response {response.Message} from the server.");
      }
    }

    public void Cancel()
    {
      ThrowIfAny();

      // only cancel what we created
      if (null != Guid)
      {
        _controller.Send(SQLiteMessage.CancelCommandRequest, Encoding.ASCII.GetBytes(Guid));
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

        // The GUID could be null if there was an error creating the command.
        if (null != Guid)
        {
          _controller.Send(SQLiteMessage.DisposeCommand, Encoding.ASCII.GetBytes(Guid));
        }
      }
      finally
      {
        // all done.
        _disposed = true;
      }
    }

    public ISQLiteServerDataReaderWorker CreateReaderWorker()
    {
      // create the worker.
      return new SQLiteServerDataReaderClientWorker(_controller, this, CommandTimeout );
    }
  }
}
