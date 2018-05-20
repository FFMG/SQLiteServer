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
using System.Threading.Tasks;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Data.Workers;

namespace SQLiteServer.Data.SQLiteServer
{
  // ReSharper disable once InconsistentNaming
  public sealed class SQLiteServerCommand : IDisposable
  {
    #region Private Variables
    /// <summary>
    /// The command worker.
    /// </summary>
    private ISQLiteServerCommandWorker _worker;

    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The command we want to run
    /// </summary>
    public string CommandText { get; set; }

    /// <summary>
    /// Get set the command time out.
    /// </summary>
    public int CommandTimeout { get; set; }

    /// <summary>
    /// The SQLite server connection.
    /// </summary>
    private readonly SQLiteServerConnection _connection;
    #endregion

    public SQLiteServerCommand() : this( null, null )
    {
    }

    public SQLiteServerCommand(SQLiteServerConnection connection) : this(null, connection)
    {
    }

    public SQLiteServerCommand(string commandText, SQLiteServerConnection connection)
    {
      _connection = connection;
      CommandText = commandText;

      // set the connection timeout
      var builder = new SQLiteServerConnectionStringBuilder(connection?.ConnectionString);
      CommandTimeout = builder.DefaultTimeout;
    }

    /// <summary>
    /// Throws an exception if we are trying to execute something 
    /// After this has been disposed.
    /// Or if the connection/commandtext is not ready.
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
    /// Or if the connection/commandtext is not ready.
    /// If the worker as not been created ... we will do so now.
    /// </summary>
    private void ThrowIfAnyAndCreateWorker()
    {
      // Throw if any ...
      ThrowIfAny();

      // or create the worker if neeeded.
      if (null == _worker)
      {
        _worker = _connection.CreateCommandAsync(CommandText).Result;
      }
    }

    /// <summary>
    /// Do no do anything if we are waiting to reconnect.
    /// </summary>
    private async Task WaitIfConnectingAsync()
    {
      if (null == _connection)
      {
        return;
      }
      await _connection.WaitIfConnectingAsync().ConfigureAwait( false );
    }

    /// <summary>
    /// Throws an exception if anything is wrong.
    /// </summary>
    private void ThrowIfAny()
    {
      // disposed?
      ThrowIfDisposed();

      // valid transaction?
      ThrowIfBadConnection();

      // valid transaction?
      ThrowIfBadCommand();
    }

    /// <summary>
    /// Throw if the command is invalid.
    /// </summary>
    private void ThrowIfBadCommand()
    {
      if (string.IsNullOrWhiteSpace(CommandText))
      {
        throw new InvalidOperationException("CommandText must be specified");
      }
    }
    
    /// <summary>
    /// Throw if the database is not ready.
    /// </summary>
    private void ThrowIfBadConnection()
    {
      if (null == _connection)
      {
        throw new InvalidOperationException("The DBConnection must be non null.");
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
        _worker?.Dispose();
      }
      finally
      {
        // all done.
        _disposed = true;
      }
    }

    /// <summary>
    /// Execute the given query 
    /// </summary>
    /// <returns>The number of rows added/deleted/whatever depending on the query.</returns>
    public int ExecuteNonQuery()
    {
      return Task.Run(async () => await ExecuteNonQueryAsync().ConfigureAwait(false)).Result;
    }

    /// <summary>
    /// Execute the given query 
    /// </summary>
    /// <returns>The number of rows added/deleted/whatever depending on the query.</returns>
    public async Task<int> ExecuteNonQueryAsync()
    {
      await WaitIfConnectingAsync().ConfigureAwait( false );
      ThrowIfAnyAndCreateWorker();
      return _worker.ExecuteNonQuery();
    }


    /// <summary>
    /// Execute a read operation and return a reader that will alow us to access the data.
    /// </summary>
    /// <returns></returns>
    public SQLiteServerDataReader ExecuteReader()
    {
      return ExecuteReader( CommandBehavior.Default );
    }

    /// <summary>
    /// Execute a read operation and return a reader that will alow us to access the data.
    /// </summary>
    /// <returns></returns>
    public SQLiteServerDataReader ExecuteReader( CommandBehavior commandBehavior)
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAnyAndCreateWorker();

      try
      {
        // create the readeer
        var reader = new SQLiteServerDataReader(_worker.CreateReaderWorker(), _connection, commandBehavior);

        // execute the command
        reader.ExecuteReader();

        // read it.
        return reader;
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <summary>
    /// Execute a read operation and return a reader that will alow us to access the data.
    /// </summary>
    /// <returns></returns>
    public Task<SQLiteServerDataReader> ExecuteReaderAsync()
    {
      return Task.FromResult(ExecuteReader(CommandBehavior.Default));
    }

    /// <summary>
    /// Execute a read operation and return a reader that will alow us to access the data.
    /// </summary>
    /// <returns></returns>
    public Task<SQLiteServerDataReader> ExecuteReaderAsync(CommandBehavior commandBehavior)
    {
      return Task.FromResult(ExecuteReader(commandBehavior));
    }
  }
}
