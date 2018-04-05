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
  public sealed class SqliteServerDataReader : IDisposable
  {
    #region Private Variables
    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    private readonly ISqliteServerDataReaderWorker _worker;
    #endregion

    internal SqliteServerDataReader(ISqliteServerDataReaderWorker worker )
    {
      if (null == worker)
      {
        throw new ArgumentNullException( nameof(worker), "The worker cannot be null");
      }
      _worker = worker;
    }

    /// <summary>
    /// Check that, as far as we can tell, the database is ready.
    /// </summary>
    private void ThrowIfAny()
    {
      // disposed?
      ThrowIfDisposed();

      // make sure we have a valid reader.
      if (null == _worker)
      {
        throw new ArgumentNullException(nameof(_worker), "There was an issue creating the worker.");
      }
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

    /// <inheritdoc />
    public void Dispose()
    {
      //  done already?
      if (_disposed)
      {
        return;
      }

      try
      {
      }
      finally
      {
        // all done.
        _disposed = true;
      }
    }

    /// <summary>
    /// Execute read event.
    /// </summary>
    public void ExecuteReader()
    {
      ThrowIfAny();
      _worker.ExecuteReader();
    }

    /// <summary>
    /// Move the read pointer forward
    /// </summary>
    /// <returns>True if we have data to read, false otherwise.</returns>
    public bool Read()
    {
      ThrowIfAny();
      return _worker.Read();
    }

    /// <summary>
    /// Get the number of fields.
    /// </summary>
    public int FieldCount
    {
      get
      {
        ThrowIfAny();
        return _worker.FieldCount;
      }
    }

    /// <summary>
    /// Check if we have any rows of data
    /// </summary>
    public bool HasRows
    {
      get
      {
        ThrowIfAny();
        return _worker.HasRows;
      }
    }

    /// <summary>
    /// Get a value at a given colum name.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public object this[int i] => GetValue(i);

    /// <summary>
    /// Get a value at a given name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object this[string name] => GetValue(GetOrdinal(name));

    /// <summary>
    /// Retrieves the column index given its name.
    /// </summary>
    /// <param name="name">The mae of the column.</param>
    /// <returns>int the index</returns>
    public int GetOrdinal(string name)
    {
      ThrowIfAny();
      return _worker.GetOrdinal(name);
    }

    /// <summary>
    /// Retrieves the column as a string
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>string</returns>
    public string GetString(int i)
    {
      ThrowIfAny();
      try
      {
        return _worker.GetString(i);
      }
      catch (Exception e)
      {
        throw new SQLiteServerException( e.Message );
      }
    }

    /// <summary>
    /// Retrieves the column as a 16 bit integer
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>short</returns>
    public short GetInt16(int i)
    {
      ThrowIfAny();
      try
      {
        return _worker.GetInt16(i);
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <summary>
    /// Retrieves the column as a 32 bit integer
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>int</returns>
    public int GetInt32(int i)
    {
      ThrowIfAny();
      try
      {
        return _worker.GetInt32(i);
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <summary>
    /// Retrieves the column as a 64 bit integer
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>long</returns>
    public long GetInt64(int i)
    {
      ThrowIfAny();
      try
      {
        return _worker.GetInt64(i);
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <summary>
    /// Retrieves the column as double
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>double</returns>
    public double GetDouble(int i)
    {
      ThrowIfAny();
      try
      {
        return _worker.GetDouble(i);
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <summary>
    /// Get the field type for a given colum
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns></returns>
    public Type GetFieldType(int i)
    {
      ThrowIfAny();
      try
      {
        return _worker.GetFieldType(i);
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <summary>
    /// Get a raw value from the database.
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns></returns>
    public object GetValue(int i)
    {
      ThrowIfAny();
      try
      {
        return _worker.GetValue(i);
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <summary>
    /// Get the column name.
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>string the column name</returns>
    public string GetName(int i)
    {
      ThrowIfAny();
      try
      {
        return _worker.GetName(i);
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }
    
    /// <summary>
    /// Gets a value that indicates whether the column contains non-existent or missing values.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public bool IsDBNull(int i)
    {
      ThrowIfAny();
      try
      {
        return _worker.IsDBNull(i);
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <summary>
    /// Get the table name for a selected column.
    /// </summary>
    /// <param name="i"></param>
    /// <returns>string the table name</returns>
    public string GetTableName(int i)
    {
      ThrowIfAny();
      try
      {
        return _worker.GetTableName(i);
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }
  }
}
