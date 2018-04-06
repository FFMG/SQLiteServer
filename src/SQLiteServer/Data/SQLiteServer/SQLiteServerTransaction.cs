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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SQLiteServer.Data.SQLiteServer
{
  // ReSharper disable once InconsistentNaming
  public sealed class SQLiteServerTransaction : DbTransaction
  {
    #region Private
    /// <summary>
    /// The isolation levels
    /// </summary>
    private readonly Stack<IsolationLevel> _isolationLevels = new Stack<IsolationLevel>();

    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The connection that owns this transaction.
    /// </summary>
    private readonly SQLiteServerConnection _connection;

    // we are in transaction if we have one or more level.
    private bool InTransaction => _isolationLevels.Count > 0;

    #endregion

    internal SQLiteServerTransaction(SQLiteServerConnection connection )
    {
      if (null == connection)
      {
        throw new ArgumentNullException(nameof(connection));
      }
      _connection = connection;
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
          // rollbacl all the transactions.
          while (InTransaction)
          {
            Rollback();
          }

          // all done.
          _disposed = true;
        }
      }
      finally
      {
        // tell the parent to do the same.
        base.Dispose(disposing);
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
        throw new ObjectDisposedException(nameof(SQLiteServerCommand));
      }
    }
    #endregion

    /// <summary>
    /// Begin the transaction.
    /// </summary>
    internal void Begin(IsolationLevel isolationLevel)
    {
      ThrowIfDisposed();

      AddAndValidateIsolationLevel(isolationLevel);
      try
      {
        using (var cmd = new SQLiteServerCommand(_connection))
        {
          //  depending on the connection level...
          cmd.CommandText = IsolationLevel == IsolationLevel.Serializable ? "BEGIN IMMEDIATE" : "BEGIN";

          // execute that query
          cmd.ExecuteNonQuery();
        }
      }
      catch
      {
        // we are not in doing that level anymore
        _isolationLevels.Pop();
        throw;
      }
    }

    /// <summary>
    /// Add an isolation level to our stack
    /// Throw if not valid.
    /// </summary>
    /// <param name="isolationLevel"></param>
    /// <returns></returns>
    private void AddAndValidateIsolationLevel(IsolationLevel isolationLevel)
    {
      IsolationLevel level;
      switch (isolationLevel)
      {
        case IsolationLevel.Unspecified:
          level = IsolationLevel.Serializable;
          break;

        case IsolationLevel.Serializable:
        case IsolationLevel.ReadCommitted:
          level = isolationLevel;
          break;

        default:
          throw new ArgumentOutOfRangeException(nameof(isolationLevel), "IsolationLevel not supported.");
      }

      // add it to the list.
      _isolationLevels.Push(level);
    }

    /// <inheritdoc />
    public override void Commit()
    {
      ThrowIfDisposed();
      if (!InTransaction)
      {
        throw new InvalidOperationException("Already committed or rolled back.");
      }

      using (var cmd = new SQLiteServerCommand(_connection))
      {
        cmd.CommandText = "COMMIT";
        cmd.ExecuteNonQuery();

        // remove the last one in the list
        _isolationLevels.Pop();
      }
    }

    /// <inheritdoc />
    public override void Rollback()
    {
      ThrowIfDisposed();
      if (!InTransaction)
      {
        throw new InvalidOperationException("Already committed or rolled back.");
      }
      using (var cmd = new SQLiteServerCommand(_connection ))
      {
        cmd.CommandText = "ROLLBACK";
        cmd.ExecuteNonQuery();

        // remove the last one in the list
        _isolationLevels.Pop();
      }
    }

    /// <inheritdoc />
    protected override DbConnection DbConnection
    {
      get
      {
        ThrowIfDisposed();
        return _connection;
      }
    }

    /// <inheritdoc />
    public override IsolationLevel IsolationLevel
    {
      get
      {
        ThrowIfDisposed();
        return _isolationLevels.FirstOrDefault();
      }
    }
  }
}
