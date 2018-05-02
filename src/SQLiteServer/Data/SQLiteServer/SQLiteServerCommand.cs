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
using System.Threading.Tasks;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Data.Workers;

namespace SQLiteServer.Data.SQLiteServer
{
  // ReSharper disable once InconsistentNaming
  public sealed class SQLiteServerCommand : DbCommand
  {
    #region Private Variables
    /// <summary>
    /// The paramerters 
    /// </summary>
    private SQLiteServerDbParameterCollection _parameterCollection;

    /// <summary>
    /// The command worker.
    /// </summary>
    private ISQLiteServerCommandWorker _worker;

    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    /// <inheritdoc />
    public override string CommandText { get; set; }

    /// <inheritdoc />
    public override int CommandTimeout { get; set; }

    public override CommandType CommandType { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }

    protected override DbConnection DbConnection
    {
      get
      {
        ThrowIfDisposed();
        return _connection;
      }
      set
      {
        ThrowIfDisposed();
        if (_reader != null )
        {
          _reader.Close();
          _reader = null;
        }
        _connection = (SQLiteServerConnection)value;
      }
    }

    protected override DbTransaction DbTransaction { get; set; }
    public override bool DesignTimeVisible { get; set; }

    /// <inheritdoc />
    protected override DbParameterCollection DbParameterCollection
    {
      get
      {
        ThrowIfDisposed();
        return _parameterCollection;
      }
    }

    /// <summary>
    /// The SQLite server connection.
    /// </summary>
    private SQLiteServerConnection _connection;

    /// <summary>
    /// The current active reader.
    /// </summary>
    private SQLiteServerDataReader _reader;
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

      // create a blank parameter collection.
      _parameterCollection = new SQLiteServerDbParameterCollection();
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
        _worker = _connection.CreateCommand(CommandText);
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
          _worker?.Dispose();
          _reader?.Close();
        }
      }
      finally
      {
        base.Dispose(disposing);

        if (disposing)
        {
          // all done.
          _disposed = true;
        }
      }
    }

    /// <inheritdoc />
    public override int ExecuteNonQuery()
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAnyAndCreateWorker();
      return _worker.ExecuteNonQuery();
    }

    /// <summary>
    /// Execute the given query 
    /// </summary>
    /// <returns>The number of rows added/deleted/whatever depending on the query.</returns>
    public new Task<int> ExecuteNonQueryAsync()
    {
      // all the work/checks are done in the next ExecuteReader() function.
      return Task.FromResult(ExecuteNonQuery());
    }


    /// <summary>
    /// Execute a read operation and return a reader that will alow us to access the data.
    /// </summary>
    /// <returns></returns>
    public new SQLiteServerDataReader ExecuteReader()
    {
      // all the work/checks are done in the next ExecuteReader() function.
      return ExecuteReader( CommandBehavior.Default );
    }

    /// <summary>
    /// Execute a read operation and return a reader that will alow us to access the data.
    /// </summary>
    /// <returns></returns>
    public new SQLiteServerDataReader ExecuteReader( CommandBehavior commandBehavior)
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAnyAndCreateWorker();
      try
      {
        // create the readeer
        var reader = new SQLiteServerDataReader(_worker.CreateReaderWorker(), _connection, commandBehavior);

        // execute the command
        _reader.ExecuteReader();

        // read it.
        return _reader;
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <inheritdoc />
    public override object ExecuteScalar()
    {
      ThrowIfAny();

      // just read one row of data.
      using (var reader = ExecuteReader( CommandBehavior.SingleRow | CommandBehavior.SingleResult))
      {
        // then read that row... and if we have anything, return it.
        if (reader.Read() && (reader.FieldCount > 0))
        {
          return reader[0];
        }
      }
      return null;
    }

    public override void Prepare()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override void Cancel()
    {
      ThrowIfDisposed();
      _reader?.Close();
    }

    /// <inheritdoc />
    protected override DbParameter CreateDbParameter()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior commandBehavior)
    {
      return ExecuteReader(commandBehavior);
    }

    /// <summary>
    /// Execute a read operation and return a reader that will alow us to access the data.
    /// </summary>
    /// <returns></returns>
    public new Task<SQLiteServerDataReader> ExecuteReaderAsync()
    {
      return Task.FromResult(ExecuteReader(CommandBehavior.Default));
    }

    /// <summary>
    /// Execute a read operation and return a reader that will alow us to access the data.
    /// </summary>
    /// <returns></returns>
    public new Task<SQLiteServerDataReader> ExecuteReaderAsync(CommandBehavior commandBehavior)
    {
      return Task.FromResult(ExecuteReader(commandBehavior));
    }
  }
}
