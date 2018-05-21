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
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Data.Workers;

namespace SQLiteServer.Data.SQLiteServer
{
  /// <inheritdoc />
  // ReSharper disable once InconsistentNaming
  public sealed class SQLiteServerDataReader : DbDataReader
  {
    #region Private Variables
    /// <summary>
    /// The behavior of the datareader
    /// </summary>
    private readonly CommandBehavior _commandBehavior;

    /// <summary>
    /// The number of rows we have read.
    /// </summary>
    private int _rowsRead;

    /// <summary>
    /// Close the reader.
    /// </summary>
    private bool _closed;
     
    /// <summary>
    /// Have we disposed of everything?
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The database worker 
    /// </summary>
    private ISQLiteServerDataReaderWorker _worker;

    /// <summary>
    /// The connection
    /// </summary>
    private readonly SQLiteServerConnection _connection;
    #endregion

    internal SQLiteServerDataReader(ISQLiteServerDataReaderWorker worker, 
                                    SQLiteServerConnection connection,
                                    CommandBehavior commandBehavior)
    {
      if (null == worker)
      {
        throw new ArgumentNullException( nameof(worker), "The worker cannot be null");
      }
      _worker = worker;
      _connection = connection;
      _commandBehavior = commandBehavior;
    }

    /// <summary>
    /// Check that, as far as we can tell, the database is ready.
    /// </summary>
    private void ThrowIfAny()
    {
      // disposed?
      ThrowIfDisposed();

      // closed?
      ThrowIfClosed();

      // make sure we have a valid reader.
      if (null == _worker)
      {
        throw new ArgumentNullException(nameof(_worker), "There was an issue creating the worker.");
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
    /// Throws an exception if we are trying to execute something 
    /// After this has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
      if (_disposed)
      {
        throw new ObjectDisposedException(nameof(SQLiteServerDataReader));
      }
    }

    /// <summary>
    /// Throws an exception if we are trying to execute something 
    /// After this has been disposed.
    /// </summary>
    private void ThrowIfClosed()
    {
      if (IsClosed)
      {
        throw new SQLiteServerException( "The reader has been closed.");
      }
    }
    /// <inheritdoc />
    protected override void Dispose( bool disposing )
    {
      //  done already?
      if (_disposed)
      {
        return;
      }

      try
      {
        Close();
      }
      finally
      {
        // all done.
        _disposed = true;
      }
    }

    /// <inheritdoc />
    public override void Close()
    {
      // are we closing the connection?
      if ((_commandBehavior & CommandBehavior.CloseConnection) != 0)
      {
        _connection?.Close();
      }
      _worker = null;
      _closed = true;
    }

    /// <summary>
    /// Execute read event.
    /// </summary>
    public void ExecuteReader()
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAny();
      _worker.ExecuteReader(_commandBehavior);
    }

    public override bool NextResult()
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAny();

      if (!_worker.NextResult())
      {
        return false;
      }

      // we are at the start of rows read
      _rowsRead = 0;

      // success
      return true;
    }

    /// <inheritdoc />
    public override bool Read()
    {
      try
      {
        return Task.Run(async () => await ReadAsync(default(CancellationToken)).ConfigureAwait(false)).GetAwaiter().GetResult();
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

    public override async Task<bool> ReadAsync( CancellationToken cancellationToken )
    { 
      await WaitIfConnectingAsync().ConfigureAwait( false );
      ThrowIfAny();

      // if we only want the schema ... then there is nothing to read
      if ((_commandBehavior & CommandBehavior.SchemaOnly) != 0)
      {
        return false;
      }

      // if the caller only wants one row
      // then there is nothing else for us to read
      if ((_commandBehavior & CommandBehavior.SingleRow) != 0)
      {
        if (_rowsRead >= 1)
        {
          return false;
        }
      }

      if (!await _worker.ReadAsync( cancellationToken).ConfigureAwait( false ))
      {
        return false;
      }

      // we did read this row
      ++_rowsRead;
      return true;
    }

    public override int Depth { get; }

    /// <inheritdoc />
    public override bool IsClosed=> _closed;

    public override int RecordsAffected { get; }

    /// <inheritdoc />
    public override int FieldCount
    {
      get
      {
        WaitIfConnectingAsync().Wait();
        ThrowIfAny();
        return _worker.FieldCount;
      }
    }

    /// <inheritdoc />
    public override bool HasRows
    {
      get
      {
        WaitIfConnectingAsync().Wait();
        ThrowIfAny();
        return _worker.HasRows;
      }
    }

    /// <inheritdoc />
    public override object this[int i] => GetValue(i);

    /// <inheritdoc />
    public override object this[string name] => GetValue(GetOrdinal(name));

    /// <inheritdoc />
    public override int GetValues(object[] values)
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAny();

      // how many fields are we getting?
      var get = FieldCount > values.Length ? values.Length : FieldCount;

      // get them.
      for (var n = 0; n < get; n++)
      {
        values[n] = GetValue(n);
      }
      return get;
    }

    /// <inheritdoc />
    public override int GetOrdinal(string name)
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAny();
      return _worker.GetOrdinal(name);
    }

    /// <inheritdoc />
    public override bool GetBoolean(int i)
    {
      // https://www.sqlite.org/datatype3.html
      return GetInt32(i) != 0;
    }

    /// <inheritdoc />
    public override byte GetByte(int i)
    {
      // no conversion is made, the data _must_ be a byte.
      // https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqldatareader.getbyte(v=vs.110).aspx
      var value = GetInt32(i);
      if (value >= byte.MinValue && value <= byte.MaxValue)
      {
        return unchecked((byte)( value & byte.MaxValue ));
      }
      throw new SQLiteServerException("Invalid cast");
    }

    /// <inheritdoc />
    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override char GetChar(int i)
    {
      return Convert.ToChar( Convert.ToUInt16( GetInt16(i) ));
    }

    /// <inheritdoc />
    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Guid GetGuid(int i)
    {
      try
      {
        // get as a string
        var s = GetString(i);

        // try and cast it to a Guid.
        return Guid.Parse(s);
      }
      catch (FormatException)
      {
        // as per the doc, it means that this is not a 'Guid' string.
        // https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqldatareader.getguid
        throw new InvalidCastException();
      }
      catch (SQLiteServerException)
      {
        throw;
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <inheritdoc />
    public override string GetString(int i)
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAny();
      try
      {
        return _worker.GetString(i);
      }
      catch (InvalidCastException)
      {
        throw;
      }
      catch (Exception e)
      {
        throw new SQLiteServerException( e.Message );
      }
    }

    /// <inheritdoc />
    public override DateTime GetDateTime(int ordinal)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override short GetInt16(int i)
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAny();
      try
      {
        return _worker.GetInt16(i);
      }
      catch (InvalidCastException )
      {
        throw;
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <inheritdoc />
    public override int GetInt32(int i)
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAny();
      try
      {
        return _worker.GetInt32(i);
      }
      catch (InvalidCastException )
      {
        throw;
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <inheritdoc />
    public override long GetInt64(int i)
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAny();
      try
      {
        return _worker.GetInt64(i);
      }
      catch (InvalidCastException)
      {
        throw;
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    /// <inheritdoc />
    public override float GetFloat(int i)
    {
      return Convert.ToSingle(GetDouble(i));
    }

    /// <inheritdoc />
    public override decimal GetDecimal(int i)
    {
      return Convert.ToDecimal(GetDouble(i));
    }

    /// <inheritdoc />
    public override double GetDouble(int i)
    {
      WaitIfConnectingAsync().Wait();
      ThrowIfAny();
      try
      {
        return _worker.GetDouble(i);
      }
      catch (InvalidCastException)
      {
        throw;
      }
      catch (Exception e)
      {
        throw new SQLiteServerException(e.Message);
      }
    }

    public override IEnumerator GetEnumerator()
    {
      return new DbEnumerator(this, ((_commandBehavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection));
    }

    /// <inheritdoc />
    public override string GetDataTypeName(int i)
    {
      // @see https://www.tutorialspoint.com/sqlite/sqlite_data_types.htm
      if (i < 0 || i > FieldCount)
      {
        throw new IndexOutOfRangeException("Trying to get a name type out of range.");
      }
      return _worker.GetDataTypeName(i);
    }

    /// <inheritdoc />
    public override Type GetFieldType(int i)
    {
      // @see https://www.tutorialspoint.com/sqlite/sqlite_data_types.htm
      WaitIfConnectingAsync().Wait();
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

    /// <inheritdoc />
    public override object GetValue(int i)
    {
      WaitIfConnectingAsync().Wait();
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

    /// <inheritdoc />
    public override string GetName(int i)
    {
      WaitIfConnectingAsync().Wait();
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

    /// <inheritdoc />
    public override bool IsDBNull(int i)
    {
      WaitIfConnectingAsync().Wait();
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
      WaitIfConnectingAsync().Wait();
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
