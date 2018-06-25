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
using System.Threading;
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

    #region Throw if ...
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
    private async Task ThrowIfAnyAndCreateWorkerAsync()
    {
      // Throw if any ...
      ThrowIfAny();

      // or create the worker if neeeded.
      if (null == _worker)
      {
        _worker = await _connection.CreateCommandAsync(CommandText).ConfigureAwait( false );
      }
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
    #endregion

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
      try
      {
        return Task.Run(async () => await ExecuteNonQueryAsync( new CancellationToken() ).ConfigureAwait(false)).Result;
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

    /// <inheritdoc />
    public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
      // wait for a connection
      await WaitIfConnectingAsync().ConfigureAwait( false );

      // throw if anything is wrong then throw otherwise connect.
      await ThrowIfAnyAndCreateWorkerAsync().ConfigureAwait( false );

      // execute the query
      return await _worker.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
    }


    /// <summary>
    /// Execute a read operation and return a reader that will alow us to access the data.
    /// </summary>
    /// <returns></returns>
    public new SQLiteServerDataReader ExecuteReader(CommandBehavior commandBehavior)
    {
      try
      {
        return Task.Run(async () => await ExecuteReaderAsync( commandBehavior ).ConfigureAwait(false)).Result;
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
    /// Execute a read operation and return a reader that will alow us to access the data.
    /// </summary>
    /// <returns></returns>
    public new async Task<SQLiteServerDataReader> ExecuteReaderAsync(CommandBehavior commandBehavior)
    {
      await WaitIfConnectingAsync().ConfigureAwait( false );
      await ThrowIfAnyAndCreateWorkerAsync().ConfigureAwait(false);

      try
      {
        // create the readeer
        _reader = new SQLiteServerDataReader(_worker.CreateReaderWorker(), _connection, commandBehavior);

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
      ThrowIfDisposed();
      return new SQLiteServerDbParameter();
    }

    /// <inheritdoc />
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior commandBehavior)
    {
      return ExecuteReader(commandBehavior);
    }
  }
}
