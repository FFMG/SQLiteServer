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
using System.Net;
using System.Threading.Tasks;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.Workers;

namespace SQLiteServer.Data.SQLiteServer
{
  // ReSharper disable once InconsistentNaming
  public sealed class SQLiteServerConnection : ICloneable, IDisposable
  {
    #region Private Variables
    /// <summary>
    /// The connection builder
    /// </summary>
    private readonly IConnectionBuilder _connectionBuilder;

    /// <summary>
    /// The actual sql connection.
    /// </summary>
    private ISQLiteServerConnectionWorker _worker;

    /// <summary>
    /// The connection string as given to us.
    /// </summary>
    private readonly string _givenConnectionString;

    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;
    #endregion

    public SQLiteServerConnection(string connectionString, IPAddress address, int port, int backlog, int heartBeatTimeOutInMs) :
      this( connectionString, new SocketConnectionBuilder(address, port, backlog, heartBeatTimeOutInMs))
    { 
    }

    internal SQLiteServerConnection(string connectionString, IConnectionBuilder connectionBuilder)
    {
      _connectionBuilder = connectionBuilder;
      _givenConnectionString = connectionString;
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
        throw new ObjectDisposedException(nameof(SQLiteServerCommand));
      }
    }

    /// <summary>
    /// Throws an exception if we are trying to execute something 
    /// After this has been disposed.
    /// </summary>
    private void ThrowIfNotOpen()
    {
      if (null == _worker)
      {
        throw new InvalidOperationException( "The worker is not ready, has the datbase been open?");
      }
    }

    /// <summary>
    /// Check that, as far as we can tell, the database is ready.
    /// </summary>
    private void ThrowIfAny()
    {
      // check disposed
      ThrowIfDisposed();

      // check if not open
      ThrowIfNotOpen();
    }
    #endregion

    public object Clone()
    {
      throw new NotImplementedException();
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
        _connectionBuilder.Dispose();
      }
      finally
      {
        // all done.
        _disposed = true;
      }
    }

    #region Database Operations
    /// <summary>
    /// Open the database using the connection string.
    /// </summary>
    public void Open()
    {
      // wait for a connection.
      Task.Run(async () =>
        await _connectionBuilder.ConnectAsync()
      ).Wait();

      Task.Run(async () =>
        _worker = await _connectionBuilder.OpenAsync( _givenConnectionString )
      ).Wait();

      _worker.Open();
    }

    /// <summary>
    /// Close the database and the related commands.
    /// </summary>
    public void Close()
    {
      // are we open?
      ThrowIfNotOpen();

      // close the worker.
      _worker?.Close();

      // disconnect everything
      _connectionBuilder.Disconnect();
    }
    #endregion
    
    #region Internal functions
    /// <summary>
    /// Create a SQL server command.
    /// </summary>
    /// <param name="commandText"></param>
    /// <returns></returns>
    internal ISQLiteServerCommandWorker CreateCommand(string commandText)
    {
      ThrowIfAny();
      return _worker.CreateCommand(commandText);
    }
    #endregion
  }
}
