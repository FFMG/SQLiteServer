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
using System.Threading;
using System.Threading.Tasks;

namespace SQLiteServer.Data.Workers
{
  // ReSharper disable once InconsistentNaming
  internal class SQLiteServerDataReaderServerWorker : ISQLiteServerDataReaderWorker
  {
    #region Private Variables
    /// <summary>
    /// The SQLite command
    /// </summary>
    private readonly SQLiteCommand _command;

    /// <summary>
    /// The SQLite reader/
    /// </summary>
    private SQLiteDataReader _reader;
    #endregion

    public SQLiteServerDataReaderServerWorker(SQLiteCommand command)
    {
      if (null == command)
      {
        throw new ArgumentNullException(nameof(command));
      }
      _command = command;
    }

    #region Validation
    /// <summary>
    /// Throw an error if we have no valid reader.
    /// </summary>
    public void ThrowIfNoReader()
    {
      if (null == _reader)
      {
        throw new ArgumentNullException( nameof(_reader), "No reader currently available.");
      }
    }

    public void ThrowIfNoCommand()
    {
      if (null == _command)
      {
        throw new ArgumentNullException(nameof(_command), "No command currently available.");
      }
    }
    #endregion

    /// <inheritdoc />
    public void ExecuteReader(CommandBehavior commandBehavior)
    {
      ThrowIfNoCommand();
      _reader = _command.ExecuteReader(commandBehavior);
    }
    
    /// <inheritdoc />
    public async Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
      ThrowIfNoReader();
      return await _reader.ReadAsync( cancellationToken ).ConfigureAwait( false );
    }

    /// <inheritdoc />
    public bool NextResult()
    {
      ThrowIfNoReader();
      return _reader.NextResult();
    }

    /// <inheritdoc />
    public int FieldCount
    {
      get
      {
        ThrowIfNoReader();
        return _reader.FieldCount;
      }
    }

    /// <inheritdoc />
    public bool HasRows
    {
      get
      {
        ThrowIfNoReader();
        return _reader.HasRows;
      }
    }

    /// <inheritdoc />
    public object this[int i] => GetValue(i);

    /// <inheritdoc />
    public object this[string name] => GetValue(GetOrdinal(name));

    /// <inheritdoc />
    public int GetOrdinal(string name)
    {
      ThrowIfNoReader();
      return _reader.GetOrdinal( name );
    }

    /// <inheritdoc />
    public string GetString(int i)
    {
      ThrowIfNoReader();
      return _reader.GetString(i);
    }

    /// <inheritdoc />
    public short GetInt16(int i)
    {
      ThrowIfNoReader();
      return _reader.GetInt16(i);
    }

    /// <inheritdoc />
    public int GetInt32(int i)
    {
      ThrowIfNoReader();
      return _reader.GetInt32(i);
    }

    /// <inheritdoc />
    public long GetInt64(int i)
    {
      ThrowIfNoReader();
      return _reader.GetInt64(i);
    }

    /// <inheritdoc />
    public double GetDouble(int i)
    {
      ThrowIfNoReader();
      return _reader.GetDouble(i);
    }

    /// <inheritdoc />
    public string GetDataTypeName(int i)
    {
      ThrowIfNoReader();
      return _reader.GetDataTypeName(i);
    }

    /// <inheritdoc />
    public Type GetFieldType(int i)
    {
      ThrowIfNoReader();
      return _reader.GetFieldType(i);
    }

    /// <inheritdoc />
    public object GetValue(int i)
    {
      ThrowIfNoReader();
      return _reader.GetValue(i);
    }

    /// <inheritdoc />
    public bool IsDBNull(int i)
    {
      ThrowIfNoReader();
      return _reader.IsDBNull(i);
    }

    /// <inheritdoc />
    public string GetName(int i)
    {
      ThrowIfNoReader();
      return _reader.GetName(i);
    }

    /// <inheritdoc />
    public string GetTableName(int i)
    {
      ThrowIfNoReader();
      return _reader.GetTableName(i);
    }
  }
}
