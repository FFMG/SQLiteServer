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
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using SQLiteServer.Data.Data;
using SQLiteServer.Data.Enums;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Fields;

namespace SQLiteServer.Data.Workers
{
  // ReSharper disable once InconsistentNaming
  internal class SQLiteServerConnectionServerWorker : IDisposable, ISQLiteServerConnectionWorker
  {
    #region Command Information
    /// <inheritdoc />
    public int CommandTimeout { get; }

    /// <summary>
    /// How often we want to send a 'busy' message to keep reminding
    /// the client that we are still busy.
    /// </summary>
    private const long DefaultBusyTimeout = 1000;

    private struct CommandData
    {
      public ISQLiteServerCommandWorker Worker;
      public ISQLiteServerDataReaderWorker Reader;
    }

    #endregion

    #region Client commands

    private readonly object _commandsLock = new object();

    private readonly Dictionary<string, CommandData> _commands = new Dictionary<string, CommandData>();

    #endregion

    #region Private Variables
    /// <summary>
    /// If the connection is locked or not.
    /// It if is not, then we can use it.
    /// </summary>
    private bool _connectionIsLocked;

    /// <summary>
    /// Our current connection.
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

    public SQLiteServerConnectionServerWorker(string connectionString, ConnectionsController controller, int commandTimeout)
    {
      if (null == controller)
      {
        throw new ArgumentNullException(nameof(controller));
      }

      CommandTimeout = commandTimeout;
      _controller = controller;
      _connection = new SQLiteConnection(connectionString);
      

      // we listen for messages right away
      // as we might not be the one who opens
      _controller.OnReceived += OnReceived;
    }

    /// <summary>
    /// Get the command reader if we have one
    /// </summary>
    /// <param name="guid"></param>
    /// <returns>return null or the command reader</returns>
    private ISQLiteServerDataReaderWorker GetCommandReader(string guid)
    {
      return !_commands.ContainsKey(guid) ? null : _commands[guid].Reader;
    }

    /// <summary>
    /// Get the command reader if we have one
    /// </summary>
    /// <param name="packet"></param>
    /// <returns>return null or the command reader</returns>
    private ISQLiteServerCommandWorker GetCommandWorker(Packet packet)
    {
      string guid;
      switch (packet.Message)
      {
        case SQLiteMessage.ExecuteReaderGetFieldTypeRequest:
        case SQLiteMessage.ExecuteReaderGetDataTypeNameRequest:
        case SQLiteMessage.ExecuteReaderGetNameRequest:
        case SQLiteMessage.ExecuteReaderGetTableNameRequest:
        case SQLiteMessage.ExecuteReaderRequest:
          var indexRequest = Fields.Fields.Unpack(packet.Payload).DeserializeObject<IndexRequest>();
          guid = indexRequest.Guid;
          break;

        case SQLiteMessage.ExecuteReaderHasRowsRequest:
        case SQLiteMessage.ExecuteReaderNextResultRequest:
        case SQLiteMessage.ExecuteReaderReadRequest:
        case SQLiteMessage.ExecuteNonQueryRequest:
        case SQLiteMessage.CreateCommandRequest:
        case SQLiteMessage.DisposeCommand:
        case SQLiteMessage.ExecuteReaderGetRowRequest:
          guid = packet.Get<string>();
          break;

        default:
          throw new ArgumentOutOfRangeException();
      }

      lock (_commandsLock)
      {
        return !_commands.ContainsKey(guid) ? null : _commands[guid].Worker;
      }
    }

    /// <summary>
    /// Handle a client request for a specific value type.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="response"></param>
    private void HandleExecuteReaderIndexRequest(Packet packet, Action<Packet> response )
    {
      try
      {
        // Get the index request.
        var indexRequest = Fields.Fields.Unpack(packet.Payload).DeserializeObject<IndexRequest>();

        // get the guid so we can look for that command
        var guid = indexRequest.Guid;
        lock (_commandsLock)
        {
          // get the reader
          var reader = GetCommandReader(guid);
          if (reader == null)
          {
            response(new Packet(SQLiteMessage.ExecuteReaderException, $"Invalid Command id sent to server for reader : {guid}."));
            return;
          }

          // and now get the index.
          var index = indexRequest.Index;
          switch (packet.Message)
          {
            case SQLiteMessage.ExecuteReaderGetDataTypeNameRequest:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.GetDataTypeName(index)));
              break;

            case SQLiteMessage.ExecuteReaderGetNameRequest:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.GetName(index)));
              break;

            case SQLiteMessage.ExecuteReaderGetTableNameRequest:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.GetTableName(index)));
              break;
              
            case SQLiteMessage.ExecuteReaderGetFieldTypeRequest:
              var iType = Field.TypeToFieldType(reader.GetFieldType(index));
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, (int)iType));
              break;

            default:
              response(new Packet(SQLiteMessage.ExecuteReaderException, $"The requested data type {packet.Message} is not supported."));
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
    /// Handle a field count request
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="response"></param>
    private void HandleExecuteReaderGuiRequest(Packet packet, Action<Packet> response)
    {
      //  get the guid
      try
      {
        var guid = packet.Get<string>();
        lock (_commandsLock)
        {
          var reader = GetCommandReader(guid);
          if (reader == null)
          {
            response(new Packet(SQLiteMessage.ExecuteReaderException, $"Invalid Command id sent to server for reader : {guid}."));
            return;
          }

          switch (packet.Message)
          {
            case SQLiteMessage.DisposeCommand:
              if (_commands.ContainsKey(guid))
              {
                _commands.Remove(guid);
              }
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, 1));
              break;

            case SQLiteMessage.ExecuteReaderReadRequest:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.Read() ? 1 : 0));
              break;

            case SQLiteMessage.ExecuteReaderNextResultRequest:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.NextResult() ? 1 : 0));
              break;

            case SQLiteMessage.ExecuteReaderHasRowsRequest:
              response(new Packet(SQLiteMessage.ExecuteReaderResponse, reader.HasRows ? 1 : 0));
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
    /// Execute the reader request for a given command... if we have one.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="response"></param>
    private void HandleExecuteReaderRequest(Packet packet, Action<Packet> response)
    {
      //  get the guid
      try
      {
        lock (_commandsLock)
        {
          var command = GetCommandWorker(packet);
          if (command == null)
          {
            response(new Packet(SQLiteMessage.ExecuteReaderException, $"Invalid Command id sent to server for reader."));
            return;
          }

          // Get the index request.
          var indexRequest = Fields.Fields.Unpack(packet.Payload).DeserializeObject<IndexRequest>();

          var reader = command.CreateReaderWorker();
          reader.ExecuteReader( (CommandBehavior)indexRequest.Index );

          // we know that the command exists
          // so we can simply update the value.
          _commands[indexRequest.Guid] = new CommandData
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
    private void HandleCancelRequest(Packet packet, Action<Packet> response)
    {
      //  get the guid
      try
      {
        lock (_commandsLock)
        {
          var command = GetCommandWorker(packet);
          if (command == null)
          {
            var guid = packet.Get<string>();
            response(new Packet(SQLiteMessage.ExecuteNonQueryException, $"Invalid Command id sent to server : {guid}."));
            return;
          }

          command.Cancel();
          response(new Packet(SQLiteMessage.ExecuteNonQueryResponse, 1));
        }
      }
      catch (Exception e)
      {
        response(new Packet(SQLiteMessage.ExecuteNonQueryException, e.Message));
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
        lock (_commandsLock)
        {
          var command = GetCommandWorker(packet);
          if (command == null )
          {
            var guid = packet.Get<string>();
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
    /// Send all column names as well as row data.
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="response"></param>
    private void HandleExecuteReaderGetRowRequest(Packet packet, Action<Packet> response)
    {
      // get the guid so we can look for that command
      var guid = packet.Get<string>();
      lock (_commandsLock)
      {
        // get the reader
        var reader = GetCommandReader(guid);
        if (reader == null)
        {
          response(new Packet(SQLiteMessage.ExecuteReaderException, $"Invalid Command id sent to server for reader : {guid}."));
          return;
        }

        // add the names
        var row = new RowInformation.RowData
        {
          Names = new List<string>(),
          Columns = new List<Field>(),
          Nulls = new List<bool>()
        };

        for (var i = 0; i < reader.FieldCount; ++i)
        {
          var name = reader.GetName(i);
          row.Names.Add(name);
        }

        if (reader.HasRows)
        {
          for (var i = 0; i < reader.FieldCount; ++i)
          {
            var isNull = reader.IsDBNull(i);
            var type = reader.GetFieldType(i);
            object value;
            if (isNull)
            {
              if( type == typeof(string) )
              {
                value = null;
              }
              else
              {
                value = Activator.CreateInstance(type);
              }
            }
            else
            {
              value = reader.GetValue(i);
            }
            row.Columns.Add(new Field(row.Names[i], type, value ));
            row.Nulls.Add(isNull);
          }
        }

        var field = Fields.Fields.SerializeObject( row);
        response(new Packet(SQLiteMessage.ExecuteReaderGetRowResponse, field.Pack()));
      }
    }

    private void HandleLockConnectionRequest(Packet packet, Action<Packet> response)
    {
      switch (packet.Message)
      {
        case SQLiteMessage.LockConnectionRequest:
          if (!WaitForLockedConnectionAsync(-1).Result)
          {
            response(new Packet(SQLiteMessage.LockConnectionException, "There was a timeout error obtaining the lock."));
            return;
          }

          _connectionIsLocked = true;
          response(new Packet(SQLiteMessage.LockConnectionResponse, 1));
          break;

        case SQLiteMessage.UnLockConnectionRequest:
          if (_connectionIsLocked == false)
          {
            response(new Packet(SQLiteMessage.LockConnectionException, "The connection is not locked."));
            return;
          }
          _connectionIsLocked = false;
          response(new Packet(SQLiteMessage.LockConnectionResponse, 1));
          break;
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

    private void OnReceived(Packet packet, Action<Packet> response)
    {
      var t1 = new Task(() => ExecuteReceived(packet, response) );
      var t2 = new Task(() => KeepBusyUntilTimeout(t1, packet, response) );

      // start the tasks
      t1.Start();
      t2.Start();
      var tasks = Task.WhenAll(t1, t2);
      try
      {
        tasks.Wait();
      }
      catch (AggregateException e)
      {
        if (e.InnerException != null)
        {
          throw e.InnerException;
        }
        throw;
      }
    }

    /// <summary>
    /// Keep sending 'busy' messages until the message arrives.
    /// </summary>
    /// <param name="executeTask"></param>
    /// <param name="packet"></param>
    /// <param name="response"></param>
    private void KeepBusyUntilTimeout( IAsyncResult executeTask, Packet packet, Action<Packet> response)
    {
      var busytimeoutMs = GetBusyTimeoutInMs( packet.Message );
      var watch = System.Diagnostics.Stopwatch.StartNew();
      var totalWatch = System.Diagnostics.Stopwatch.StartNew();
      while (!executeTask.IsCompleted)
      {
        // delay a little to give other thread a chance.
        Task.Yield();

        if (CommandTimeout > 0 && totalWatch.Elapsed.TotalSeconds > CommandTimeout)
        {
          response(new Packet(SQLiteMessage.SendAndWaitTimeOut, 1));
          var command = GetCommandWorker(packet);
          command.Cancel();
          break;
        }

          // check for delay
        if (watch.ElapsedMilliseconds < busytimeoutMs)
        {
          continue;
        }

        response(new Packet(SQLiteMessage.SendAndWaitBusy, 1));
        watch.Restart();
      }
      watch.Stop();
      totalWatch.Stop();
    }

    private long GetBusyTimeoutInMs(SQLiteMessage packetMessage)
    {
      switch (packetMessage)
      {
        case SQLiteMessage.CreateCommandException:
        case SQLiteMessage.ExecuteNonQueryRequest:
        case SQLiteMessage.ExecuteReaderRequest:
          return CommandTimeout == 0 ? DefaultBusyTimeout : Convert.ToInt64((CommandTimeout * 1000) * 0.10);

        default:
          return DefaultBusyTimeout;
      }
    }

    private void ExecuteReceived(Packet packet, Action<Packet> response)
    {
      switch ( packet.Message)
      {
        case SQLiteMessage.ExecuteReaderGetRowRequest:
          HandleExecuteReaderGetRowRequest( packet, response);
          break;

        case SQLiteMessage.ExecuteReaderGetFieldTypeRequest:
        case SQLiteMessage.ExecuteReaderGetDataTypeNameRequest:
        case SQLiteMessage.ExecuteReaderGetNameRequest:
        case SQLiteMessage.ExecuteReaderGetTableNameRequest:
          HandleExecuteReaderIndexRequest(packet, response );
          break;

        case SQLiteMessage.ExecuteReaderHasRowsRequest:
        case SQLiteMessage.ExecuteReaderNextResultRequest:
        case SQLiteMessage.ExecuteReaderReadRequest:
        case SQLiteMessage.DisposeCommand:
          HandleExecuteReaderGuiRequest(packet, response);
          break;

        case SQLiteMessage.CancelCommand:
          HandleCancelRequest(packet, response);
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

        case SQLiteMessage.LockConnectionRequest:
        case SQLiteMessage.UnLockConnectionRequest:
          HandleLockConnectionRequest(packet, response);
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
      // can we use this?
      ThrowIfAny();

      // wait for the connection
      if (!WaitForLockedConnectionAsync(CommandTimeout).Result)
      {
        throw new SQLiteServerException("Unable to obtain connection lock");
      }

      return new SQLiteServerCommandServerWorker( commandText, _connection, CommandTimeout);
    }

    public void Dispose()
    {
      // wait for the connection
      if (!WaitForLockedConnectionAsync(-1).Result)
      {
        throw new SQLiteServerException("Unable to obtain connection lock");
      }

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

    /// <inheritdoc />
    public SQLiteConnection LockConnection()
    {
      // can we use this?
      ThrowIfAny();

      // wait for the connection
      if (!WaitForLockedConnectionAsync( -1 ).Result)
      {
        throw new SQLiteServerException("Unable to obtain connection lock");
      }

      // lock the connection
      _connectionIsLocked = true;

      // return the connection
      return _connection;
    }

    /// <inheritdoc />
    public void UnLockConnection()
    {
      // can we use this?
      ThrowIfAny();

      // we can use the connection again.
      _connectionIsLocked = false;
    }

    /// <summary>
    /// Wait for the connection to be available.
    /// </summary>
    private async Task<bool> WaitForLockedConnectionAsync( int timeoutSeconds )
    {
      // wait for the connection to be availabe.
      if (false == _connectionIsLocked)
      {
        return true;
      }

      await Task.Run(async () => {
        var start = DateTime.Now;
        while ( _connectionIsLocked )
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

      return (_connectionIsLocked == false);
    }
  }
}
