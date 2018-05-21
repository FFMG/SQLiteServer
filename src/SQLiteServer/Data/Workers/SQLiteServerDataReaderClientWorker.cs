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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.Data;
using SQLiteServer.Data.Enums;
using SQLiteServer.Data.Exceptions;

namespace SQLiteServer.Data.Workers
{
  // ReSharper disable once InconsistentNaming
  internal class SQLiteServerDataReaderClientWorker : ISQLiteServerDataReaderWorker
  {
    #region Private variables   
    /// <summary>
    /// Save the field names.
    /// </summary>
    private readonly Dictionary<int, string> _dataTypeName = new Dictionary<int, string>();

    /// <summary>
    /// The SQLite Connection
    /// </summary>
    private readonly ConnectionsController _controller;

    /// <summary>
    /// The command server guid.
    /// </summary>
    private SQLiteServerCommandClientWorker _parentCommand;

    /// <summary>
    /// The max amount of time we will wait for a response from the server.
    /// </summary>
    private readonly int _queryTimeouts;
    #endregion

    #region Row Information
    /// <summary>
    /// The current row... or null if we do no yet have it.
    /// </summary>
    private RowInformation _currentRowInformation;

    /// <summary>
    /// The current row header.
    /// </summary>
    private RowHeader _currentRowHeader;

    /// <summary>
    /// Make sure that we have a valid command guid from the server.
    /// Without a GUID we cannot tell the serevr what to do for us.
    /// </summary>
    private void ThrowIfNoCommand()
    {
      if (_parentCommand.Guid == null)
      {
        throw new ArgumentNullException(nameof(_parentCommand.Guid), "The given command is invalid.");
      }
    }

    /// <summary>
    /// Get the current row or go and fetch it.
    /// </summary>
    /// <returns></returns>
    private RowInformation GetRow()
    {
      // do we already have the information?
      if (null != _currentRowInformation)
      {
        return _currentRowInformation;
      }

      if ( null == _currentRowHeader )
      {
        throw new SQLiteServerException( "Missing row headers.");
      }
      
      // get the row.
      var row = GetGuiOnlyValueAsync<RowData>(SQLiteMessage.ExecuteReaderGetRowRequest ).Result;
      _currentRowInformation = new RowInformation(  _currentRowHeader );
      var ordinal = 0;
      for(var i =0; i < row.Columns.Count;++i )
      {
        var column = row.Columns[i];
        var isNull = row.Nulls[i];
        _currentRowInformation.Add( new ColumnInformation(column, ordinal++, column.Name, isNull ));
      }
      return _currentRowInformation;
    }

    /// <summary>
    /// Get a column information or a row for it.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private ColumnInformation GetColumn(int i)
    {
      // get the current row.
      var row = GetRow();

      // did we get anything?
      if (null == row)
      {
        return null;
      }

      // otherwise return the data for it.
      return row.Get(i);
    }

    /// <summary>
    /// Get a column information or a row for it.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private ColumnInformation GetColumn(string name )
    {
      // get the current row.
      var row = GetRow();

      // did we get anything?
      if (null == row)
      {
        // https://msdn.microsoft.com/en-us/library/system.data.common.dbdatareader.getordinal(v=vs.110).aspx
        throw new IndexOutOfRangeException($"Could not find column {name}!");
      }

      // otherwise return the data for it.
      return row.Get(name);
    }
    #endregion

    public SQLiteServerDataReaderClientWorker(ConnectionsController controller, SQLiteServerCommandClientWorker parentCommand, int queryTimeouts)
    {
      if (null == controller)
      {
        throw new ArgumentNullException( nameof(controller), "The controller cannot be null.");
      }
      _controller = controller;
      _parentCommand = parentCommand;
      _queryTimeouts = queryTimeouts;
    }

    /// <inheritdoc />
    public void ExecuteReader(CommandBehavior commandBehavior)
    {
      Packet response;
      if (null != _parentCommand.Guid)
      {
        var request = Fields.Fields.SerializeObject(new GuidAndIndexRequest
        {
          Guid = _parentCommand.Guid,
          Index = (int)commandBehavior
        });
        response = _controller.SendAndWaitAsync(SQLiteMessage.ExecuteReaderRequest, request.Pack(), _queryTimeouts)
          .Result;
      }
      else
      {
        var request = Fields.Fields.SerializeObject(new CommandTextAndIndexRequest
        {
          CommandText = _parentCommand.CommandText,
          Index = (int)commandBehavior
        });
        response = _controller.SendAndWaitAsync(SQLiteMessage.ExecuteCommandReaderRequest, request.Pack(), _queryTimeouts)
          .Result;
      }

      switch (response.Message)
      {
        case SQLiteMessage.SendAndWaitTimeOut:
          throw new TimeoutException("There was a timeout error creating the Command.");

        case SQLiteMessage.ExecuteReaderResponse:
          var fields = Fields.Fields.Unpack(response.Payload);
          _currentRowHeader = Fields.Fields.DeserializeObject<RowHeader>(fields);
          _parentCommand.Guid = _currentRowHeader.Guid;
          return;

        case SQLiteMessage.ExecuteReaderException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException($"Unknown response {response.Message} from the server.");
      }
    }

    /// <inheritdoc />
    public async Task<bool> ReadAsync( CancellationToken cancellationToken )
    {
      var rowHeader = await GetGuiOnlyValueAsync<RowHeader>(SQLiteMessage.ExecuteReaderReadRequest).ConfigureAwait(false);

      _currentRowHeader = rowHeader;
      _parentCommand.Guid = _currentRowHeader.Guid;

      // we need to reset some field now.
      _dataTypeName.Clear();
      _currentRowInformation = null;

      // if we have no rows, then we have nothing else to read.
      return rowHeader.HasRows;
    }

    /// <inheritdoc />
    public bool NextResult()
    {
      return GetGuiOnlyValueAsync<int>(SQLiteMessage.ExecuteReaderNextResultRequest).Result != 0;
    }
    
    /// <inheritdoc />
    public int FieldCount => _currentRowHeader.FieldCount;

    /// <inheritdoc />
    public bool HasRows => _currentRowHeader.HasRows;

    /// <inheritdoc />
    public object this[int ordinal] => GetValue(ordinal);

    /// <inheritdoc />
    public object this[string name] => GetValue(GetOrdinal(name));

    /// <inheritdoc />
    public int GetOrdinal(string name)
    {
      if (null == _currentRowHeader)
      {
        throw new IndexOutOfRangeException();
      }
      return _currentRowHeader.GetOrdinal(name);
    }

    /// <summary>
    /// Get the result of a requres.t
    /// </summary>
    /// <param name="requestType"></param>
    /// <returns></returns>
    private async Task<T> GetGuiOnlyValueAsync<T>(SQLiteMessage requestType)
    {
      ThrowIfNoCommand();
      var response = await _controller.SendAndWaitAsync(requestType, Encoding.ASCII.GetBytes(_parentCommand.Guid), _queryTimeouts).ConfigureAwait(false);
      switch (response.Message)
      {
        case SQLiteMessage.SendAndWaitTimeOut:
          throw new TimeoutException("There was a timeout error executing the read request from the reader.");

        case SQLiteMessage.ExecuteReaderReadResponse:
        case SQLiteMessage.ExecuteReaderGetRowResponse:
          var fields = Fields.Fields.Unpack(response.Payload);
          return Fields.Fields.DeserializeObject<T>(fields);

        case SQLiteMessage.ExecuteRequestResponse:
          return response.Get<T>();

        case SQLiteMessage.ExecuteReaderException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException($"Unknown response {response.Message} from the server.");
      }
    }

    /// <summary>
    /// Get a value from the server by index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="requestType"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private async Task<T> GetIndexedValueAsync<T>(SQLiteMessage requestType, int index)
    {
      ThrowIfNoCommand();
       var getValue = new GuidAndIndexRequest()
      {
        Guid = _parentCommand.Guid,
        Index = index
      };
      var fields = Fields.Fields.SerializeObject(getValue);

      var response = await _controller.SendAndWaitAsync(requestType, fields.Pack(), _queryTimeouts).ConfigureAwait(false);
      switch (response.Message)
      {
        case SQLiteMessage.SendAndWaitTimeOut:
          throw new TimeoutException("There was a timeout error executing the read request from the reader.");

        case SQLiteMessage.ExecuteRequestResponse:
          return response.Get<T>();

        case SQLiteMessage.ExecuteReaderException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException($"Unknown response {response.Message} from the server.");
      }
    }
    
    /// <inheritdoc />
    public short GetInt16(int i)
    {
      var column = GetColumn(i);
      if (column.IsNull)
      {
        throw new InvalidCastException();
      }
      return column.Get<short>();
    }

    /// <inheritdoc />
    public int GetInt32(int i)
    {
      var column = GetColumn(i);
      if (column.IsNull)
      {
        throw new InvalidCastException();
      }
      return column.Get<int>();
    }

    /// <inheritdoc />
    public long GetInt64(int i)
    {
      var column = GetColumn(i);
      if (column.IsNull)
      {
        throw new InvalidCastException();
      }
      return column.Get<long>();
    }

    /// <inheritdoc />
    public double GetDouble(int i)
    {
      var column = GetColumn(i);
      if (column.IsNull)
      {
        throw new InvalidCastException();
      }
      return column.Get<double>();
    }

    /// <inheritdoc />
    public string GetString(int i)
    {
      var column = GetColumn(i);
      if (column.IsNull)
      {
        throw new InvalidCastException();
      }
      return column.Get<string>();
    }

    /// <inheritdoc />
    public string GetDataTypeName(int i)
    {
      if (_dataTypeName.ContainsKey(i))
      {
        return _dataTypeName[i];
      }

      // get the value
      var dataTypeName = GetIndexedValueAsync<string>(SQLiteMessage.ExecuteReaderGetDataTypeNameRequest, i).Result;

      // the name cannot be null, it just means
      // that it was created with no type
      //   `CREATE TABLE t1( z)`
      dataTypeName = dataTypeName ?? string.Empty;

      // save it
      _dataTypeName[i] = dataTypeName;

      // return it.
      return dataTypeName;
    }

    /// <inheritdoc />
    public Type GetFieldType(int i)
    {
      if (null == _currentRowHeader)
      {
        throw new InvalidOperationException();
      }
      return _currentRowHeader.GetType(i);
    }

    /// <inheritdoc />
    public object GetValue(int i)
    {
      // get the current colum, if we do not have one, we will throw.
      return GetColumn(i).Get<object>();
    }

    /// <inheritdoc />
    public bool IsDBNull(int i)
    {
      return GetColumn(i).IsNull;
    }

    /// <inheritdoc />
    public string GetName(int i)
    {
      if (null == _currentRowHeader)
      {
        throw new IndexOutOfRangeException();
      }
      return _currentRowHeader.GetName( i );
    }

    /// <inheritdoc />
    public string GetTableName(int i)
    {
      if (null == _currentRowHeader)
      {
        throw new IndexOutOfRangeException();
      }
      return _currentRowHeader.GetTableName(i);
    }
  }
}
