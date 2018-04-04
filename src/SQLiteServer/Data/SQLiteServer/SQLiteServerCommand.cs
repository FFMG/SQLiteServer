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
    private readonly ISQLiteServerCommandWorker _worker;

    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The command we want to run
    /// </summary>
    private readonly string _commandText;

    /// <summary>
    /// The SQLite server connection.
    /// </summary>
    private readonly SQLiteServerConnection _connection;
    #endregion

    public SQLiteServerCommand(string commandText, SQLiteServerConnection connection)
    {
      _connection = connection;
      _commandText = commandText;
      _worker = connection.CreateCommand(commandText );
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
    /// Throws an exception if anything is wrong.
    /// </summary>
    private void ThrowIfAny()
    {
      // disposed?
      ThrowIfDisposed();
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
        _worker.Dispose();
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
      ThrowIfAny();
      return _worker.ExecuteNonQuery();
    }

    /// <summary>
    /// Execute a read operation and return a reader that will alow us to access the data.
    /// </summary>
    /// <returns></returns>
    public SqliteServerDataReader ExecuteReader()
    {
      ThrowIfAny();
      try
      {
        // create the readeer
        var reader = new SqliteServerDataReader(_worker.CreateReaderWorker());

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
  }
}
