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
using System.Runtime.InteropServices.WindowsRuntime;

namespace SQLiteServer.Data.SQLiteServer
{
  // ReSharper disable once InconsistentNaming
  public sealed class SQLiteServerTransaction : DbTransaction
  {
    #region Private
    /// <summary>
    /// The isolation level
    /// </summary>
    private readonly IsolationLevel _isolationLevel;

    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The connection that owns this transaction.
    /// </summary>
    private readonly SQLiteServerConnection _connection;

    private bool InTransaction { get; set; }
    #endregion

    internal SQLiteServerTransaction(SQLiteServerConnection connection, IsolationLevel  isolationLevel)
    {
      if (null == connection)
      {
        throw new ArgumentNullException(nameof(connection));
      }
      _connection = connection;

      _isolationLevel = isolationLevel;
      switch (isolationLevel)
      {
        case IsolationLevel.Unspecified:
          _isolationLevel = IsolationLevel.Serializable;
          break;

        case IsolationLevel.Serializable:
        case IsolationLevel.ReadCommitted:
          _isolationLevel = isolationLevel;
          break;

        default:
          throw new ArgumentOutOfRangeException(nameof(isolationLevel), "IsolationLevel not supported.");
      }

      // begin the transaction
      Begin();
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
          if (InTransaction)
          {
            Rollback();
          }
        }
      }
      finally
      {
        _disposed = true;
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
    internal void Begin()
    {
      ThrowIfDisposed();
      if (InTransaction)
      {
        throw new InvalidOperationException("Cannot begin, already in transaction.");
      }
      using (var cmd = new SQLiteServerCommand(_connection))
      {
        if (IsolationLevel == IsolationLevel.Serializable)
        {
          cmd.CommandText = "BEGIN IMMEDIATE";
        }
        else
        {
          cmd.CommandText = "BEGIN";
        }
        cmd.ExecuteNonQuery();
        InTransaction = true;
      }
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
        InTransaction = false;
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
        InTransaction = false;
      }
    }

    /// <inheritdoc />
    protected override DbConnection DbConnection
    {
      get
      {
        ThrowIfDisposed();
        throw new NotSupportedException();
      }
    }

    /// <inheritdoc />
    public override IsolationLevel IsolationLevel
    {
      get
      {
        ThrowIfDisposed();
        return _isolationLevel;
      }
    }
  }
}
