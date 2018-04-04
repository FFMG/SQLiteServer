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
using SQLiteServer.Data.Connections;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SQLiteServer.Data.Data;
using SQLiteServer.Data.Enums;
using SQLiteServer.Fields;

namespace SQLiteServer.Data.Workers
{
  // ReSharper disable once InconsistentNaming
  internal class SQLiteServerConnectionServerWorker : IDisposable, ISQLiteServerConnectionWorker
  {
    #region Command Information

    private struct CommandData
    {
      public ISQLiteServerCommandWorker Worker;
      public ISqliteServerDataReaderWorker Reader;
    }

    #endregion

    #region Client commands

    private readonly object _commandsLock = new object();

    private readonly Dictionary<string, CommandData> _commands = new Dictionary<string, CommandData>();

    #endregion

    #region Private Variables

    /// <summary>
    /// The actual SQLite connection.
    /// </summary>
    private readonly SQLiteConnection _connection;

    /// <summary>
    /// The contoller
    /// </summary>
    private readonly ConnectionsController _controller;

    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    #endregion

    public SQLiteServerConnectionServerWorker(string connectionString, ConnectionsController controller)
    {
      if (null == controller)
      {
        throw new ArgumentNullException(nameof(controller));
      }

      _controller = controller;
      _connection = new SQLiteConnection(connectionString);

      // we listen for messages right away
      // as we might not be the one who opens
      _controller.OnReceived += OnReceived;
    }

    /// <summary>
    /// Get the command guid if we have one
    /// </summary>
    /// <param name="guid"></param>
    /// <returns>return null or the command worker</returns>
    private ISQLiteServerCommandWorker GetCommandWorker(string guid)
    {
      if (!_commands.ContainsKey(guid))
      {
        return null;
      }

      return _commands[guid].Worker;
    }

    /// <summary>
    /// Handle a client request for a specific value type.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="response"></param>
    private void HandleExecuteReaderIndexRequest(Packet packet, Action<Packet> response )
    {
      //  get the guid
      try
      {
        // Get the index request.
        var indexRequest = Fields.Fields.Unpack(packet.Payload).DeserializeObject<IndexRequest>();

        // get the guid so we can look for that command
        var guid = indexRequest.Guid;
        lock (_commandsLock)
        {
          var command = GetCommandWorker(guid);
          if (command == null)
          {
            response(new Packet(SQLiteMessage.ExecuteReaderException, $"Invalid Command id sent to server for reader : {guid}."));
            return;
          }

          // we know that the command exists
          // so we can get the index
          var index = indexRequest.Index;
          var reader = _commands[guid].Reader;

          switch (packet.Type)
          {
            case SQLiteMessage.ExecuteReaderGetInt16Request:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.GetInt16(index)));
              break;

            case SQLiteMessage.ExecuteReaderGetInt32Request:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.GetInt32(index)));
              break;

            case SQLiteMessage.ExecuteReaderGetInt64Request:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.GetInt64(index)));
              break;

            case SQLiteMessage.ExecuteReaderGetDoubleRequest:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.GetDouble(index)));
              break;
              
            case SQLiteMessage.ExecuteReaderGetStringRequest:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.GetString(index)));
              break;

            case SQLiteMessage.ExecuteReaderGetFieldTypeRequest:
              var iType = Field.TypeToFieldType(reader.GetFieldType(index));
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, (int)iType));
              break;

            default:
              response(new Packet(SQLiteMessage.ExecuteReaderException, $"The requested data type {packet.Type} is not supported."));
              break;
          }
        }
      }
      catch (Exception e)
      {
        response(new Packet(SQLiteMessage.ExecuteReaderException, e.Message));
      }
    }

    /// <summary>
    /// Handle a client request for a specific value type.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="response"></param>
    private void HandleExecuteReaderNameRequest(Packet packet, Action<Packet> response)
    {
      //  get the guid
      try
      {
        // Get the index request.
        var nameRequest = Fields.Fields.Unpack(packet.Payload).DeserializeObject<NameRequest>();

        // get the guid so we can look for that command
        var guid = nameRequest.Guid;
        lock (_commandsLock)
        {
          var command = GetCommandWorker(guid);
          if (command == null)
          {
            response(new Packet(SQLiteMessage.ExecuteReaderException, $"Invalid Command id sent to server for reader : {guid}."));
            return;
          }

          // we know that the command exists
          // so we can get the index
          var name = nameRequest.Name;
          var reader = _commands[guid].Reader;

          switch (packet.Type)
          {
            case SQLiteMessage.ExecuteReaderGetOrdinalRequest:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.GetOrdinal(name)));
              break;

            default:
              response(new Packet(SQLiteMessage.ExecuteReaderException, $"The requested data type {packet.Type} is not supported."));
              break;
          }
        }
      }
      catch (Exception e)
      {
        response(new Packet(SQLiteMessage.ExecuteReaderException, e.Message));
      }
    }

    /// <summary>
    /// Handle a read request
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="response"></param>
    private void HandleExecuteReaderReadRequest(Packet packet, Action<Packet> response)
    {
      //  get the guid
      try
      {
        var guid = packet.Get<string>();
        lock (_commandsLock)
        {
          var command = GetCommandWorker(guid);
          if (command == null)
          {
            response(new Packet(SQLiteMessage.ExecuteReaderException, $"Invalid Command id sent to server for reader : {guid}."));
            return;
          }

          // we know that the command exists
          var reader = _commands[guid].Reader;
          var result = reader.Read();
          response(new Packet(SQLiteMessage.ExecuteReaderResponse, result ? 1 : 0));
        }
      }
      catch (Exception e)
      {
        response(new Packet(SQLiteMessage.ExecuteReaderException, e.Message));
      }
    }

    /// <summary>
    /// Execute the reader request for a given command... if we have one.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="response"></param>
    private void HandleExecuteReaderRequest(Packet packet, Action<Packet> response)
    {
      //  get the guid
      try
      {
        var guid = packet.Get<string>();
        lock (_commandsLock)
        {
          var command = GetCommandWorker(guid);
          if (command == null)
          {
            response(new Packet(SQLiteMessage.ExecuteReaderException, $"Invalid Command id sent to server for reader : {guid}."));
            return;
          }

          var reader = command.CreateReaderWorker();
          reader.ExecuteReader();
          // we know that the command exists
          // so we can simply update the value.
          _commands[guid] = new CommandData
          {
            Worker = command,
            Reader = reader
          };
          response(new Packet(SQLiteMessage.ExecuteReaderResponse, 1 ));
        }
      }
      catch (Exception e)
      {
        response(new Packet(SQLiteMessage.ExecuteReaderException, e.Message));
      }
    }

    /// <summary>
    /// Execute a command... assuming that all is good.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="response"></param>
    private void HandleExecuteNonQueryRequest(Packet packet, Action<Packet> response)
    {
      //  get the guid
      try
      {
        var guid = packet.Get<string>();
        lock (_commandsLock)
        {
          var command = GetCommandWorker(guid);
          if (command == null )
          {
            response(new Packet(SQLiteMessage.ExecuteNonQueryException, $"Invalid Command id sent to server : {guid}."));
            return;
          }

          var result = command.ExecuteNonQuery();
          response(new Packet(SQLiteMessage.ExecuteNonQueryResponse, result));
        }
      }
      catch (Exception e)
      {
        response(new Packet(SQLiteMessage.ExecuteNonQueryException, e.Message));
      }
    }

    /// <summary>
    /// Create a command and send it back to the caller.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="response"></param>
    private void HandleReceiveCommandRequest(Packet packet, Action<Packet> response)
    {
      // we need to create a mew command and return a unique Guid to the caller.
      // that way, we will habe a handshake of some sort when they ask us to execute.
      ISQLiteServerCommandWorker command;
      try
      {
        command = CreateCommand(packet.Get<string>());
      }
      catch (Exception e)
      {
        response(new Packet(SQLiteMessage.CreateCommandException, e.Message));
        return;
      }

      // the command was created, but has not been saved yet.
      // first add it to the list, and then send the response.
      var guid = Guid.NewGuid().ToString();
      lock (_commandsLock)
      {
        _commands.Add(guid, new CommandData
        {
          Worker = command,
          Reader = null
        });
      }

      // we can now send the response.
      response(new Packet(SQLiteMessage.CreateCommandResponse, guid));
    }

    /// <summary>
    /// Sent when the client wants us to dispose of a Command
    /// </summary>
    /// <param name="packet"></param>
    private void HandleDisposeCommand(Packet packet)
    {
      var guid = packet.Get<string>();
      lock (_commandsLock)
      {
        if (_commands.ContainsKey(guid))
        {
          _commands.Remove(guid);
        }
      }
    }

    private void OnReceived(Packet packet, Action<Packet> response)
    {
      switch( packet.Type )
      {
        case SQLiteMessage.ExecuteReaderGetInt16Request:
        case SQLiteMessage.ExecuteReaderGetInt32Request:
        case SQLiteMessage.ExecuteReaderGetInt64Request:
        case SQLiteMessage.ExecuteReaderGetDoubleRequest:
        case SQLiteMessage.ExecuteReaderGetStringRequest:
        case SQLiteMessage.ExecuteReaderGetFieldTypeRequest:
          HandleExecuteReaderIndexRequest(packet, response );
          break;

        case SQLiteMessage.ExecuteReaderGetOrdinalRequest:
          HandleExecuteReaderNameRequest(packet, response);
          break;

        case SQLiteMessage.ExecuteReaderReadRequest:
          HandleExecuteReaderReadRequest(packet, response);
          break;

        case SQLiteMessage.ExecuteReaderRequest:
          HandleExecuteReaderRequest(packet, response);
          break;

        case SQLiteMessage.ExecuteNonQueryRequest:
          HandleExecuteNonQueryRequest(packet, response);
          break;

        case SQLiteMessage.CreateCommandRequest:
          HandleReceiveCommandRequest(packet, response);
          break;

        case SQLiteMessage.DisposeCommand:
          HandleDisposeCommand(packet);
          break;
      }
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
    public void Open()
    {
      // open the connection
      _connection.Open();
    }

    /// <inheritdoc />
    public void Close()
    {
      _connection.Close();
    }

    /// <inheritdoc />
    public ISQLiteServerCommandWorker CreateCommand(string commandText)
    {
      ThrowIfAny();
      return new SQLiteServerCommandServerWorker( commandText, _connection);
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
        // stop receiving 
        _controller.OnReceived -= OnReceived;

        // remove our commands
        // @todo shall we tell the callers? Is it even posible to send a message now?
        _commands.Clear();
      }
      finally
      {
        _disposed = true;
      }

    }
  }
}
