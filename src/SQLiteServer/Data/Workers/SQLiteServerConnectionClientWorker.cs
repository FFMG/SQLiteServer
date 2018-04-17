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
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.SQLiteServer;

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
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The contoller
    /// </summary>
    private readonly ConnectionsController _controller;
    #endregion

    public SQLiteServerConnectionClientWorker(ConnectionsController controller, int commandTimeout)
    {
      if (null == controller)
      {
        throw new ArgumentNullException( nameof(controller));
      }
      _controller = controller;
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

    public ISQLiteServerCommandWorker CreateCommand(string commandText)
    {
      return new SQLiteServerCommandClientWorker( commandText, _controller, CommandTimeout);
    }

    /// <inheritdoc />
    public void BackupDatabase(
      SQLiteServerConnection destination,
      string destinationName,
      string sourceName,
      int pages,
      SQLiteServerBackupCallback callback,
      int retryMilliseconds)
    {
      //  check not disposed
      ThrowIfAny();

      // check destination is valid.
      if (destination == null)
      {
        throw new ArgumentNullException(nameof(destination));
      }

      // destination is open.
      if (destination.State != ConnectionState.Open)
      {
        throw new ArgumentException("Destination database is not open.", nameof(destination));
      }

      // validate the names
      if (destinationName == null)
      {
        throw new ArgumentNullException(nameof(destinationName));
      }
      if (sourceName == null)
      {
        throw new ArgumentNullException(nameof(sourceName));
      }

      // todo
      throw new NotImplementedException();
    }
  }
}
