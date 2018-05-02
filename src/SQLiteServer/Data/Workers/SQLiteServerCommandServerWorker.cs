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
using System.Data.SQLite;
using System.Threading.Tasks;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Data.SQLiteServer;

namespace SQLiteServer.Data.Workers
{
  // ReSharper disable once InconsistentNaming
  internal class SQLiteServerCommandServerWorker : ISQLiteServerCommandWorker
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
    private readonly SQLiteCommand _command;

    /// <summary>
    /// The SQLite Connection
    /// </summary>
    private readonly SQLiteConnection _connection;
    #endregion

    public SQLiteServerCommandServerWorker(string commandText, SQLiteConnection connection, int commandTimeout)
    {
      if (null == connection)
      {
        throw new ArgumentNullException(nameof(connection));
      }
      _connection = connection;

      if (null == commandText)
      {
        throw new ArgumentNullException(nameof(commandText));
      }

      CommandTimeout = commandTimeout;

      try
      {
        _command = new SQLiteCommand(commandText, connection);
      }
      catch (SQLiteException e )
      {
        throw new SQLiteServerException( e.Message, e.InnerException );
      }
    }

    /// <summary>
    /// Check that, as far as we can tell, the database is ready.
    /// </summary>
    private void ThrowIfAny()
    {
      // disposed?
      ThrowIfDisposed();

      // check if not open
      ThrowIfNotOpen();
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
    /// Throws an exception if we are trying to execute something 
    /// and the database is not ready.
    /// </summary>
    private void ThrowIfNotOpen()
    {
      // do we even have a connection at all?
      if (null == _connection)
      {
        throw new InvalidOperationException("The connection is not ready, has the datbase been open?");
      }

      //  is the database ready?
      if (_connection.State != ConnectionState.Open )
      {
        throw new InvalidOperationException( $"The connection is not ready, has the datbase been open ({_connection.State})?");
      }
    }

    /// <inheritdoc />
    public void Cancel()
    {
      ThrowIfAny();
      try
      {
        _command.Cancel();
      }
      catch (SQLiteException e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <inheritdoc />
    public int ExecuteNonQuery()
    {
      ThrowIfAny();
      try
      {
        var queryResult = 0;
        var t1 = new Task(() => queryResult = _command.ExecuteNonQuery());
        var t2 = new Task(() => KeepBusyUntilTimeout(t1));

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
        return queryResult;
      }
      catch (SQLiteException e)
      {
        throw new SQLiteServerException( e.Message );
      }
    }

    /// <summary>
    /// Keep sending 'busy' messages until the message arrives.
    /// </summary>
    /// <param name="executeTask"></param>
    private void KeepBusyUntilTimeout(IAsyncResult executeTask )
    {
      // do we even need to wait?
      if (CommandTimeout == 0)
      {
        return;
      }

      var totalWatch = System.Diagnostics.Stopwatch.StartNew();
      while (!executeTask.IsCompleted)
      {
        // delay a little to give other thread a chance.
        Task.Yield();

        if (totalWatch.Elapsed.TotalSeconds > CommandTimeout)
        {
          totalWatch.Stop();
          _command.Cancel();
          throw new TimeoutException("There was a timeout error executing the read request from the reader.");
        }
      }
      totalWatch.Stop();
    }

    public ISQLiteServerDataReaderWorker CreateReaderWorker()
    {
      return new SQLiteServerDataReaderServerWorker( _command );
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
        _command.Dispose();
      }
      finally
      {
        // all done.
        _disposed = true;
      }
    }
  }
}
