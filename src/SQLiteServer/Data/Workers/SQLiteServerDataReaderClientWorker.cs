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
using System.Threading.Tasks;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.Data;
using SQLiteServer.Data.Enums;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Fields;

namespace SQLiteServer.Data.Workers
{
  // ReSharper disable once InconsistentNaming
  internal class SQLiteServerDataReaderClientWorker : ISQLiteServerDataReaderWorker
  {
    #region Private variables
    /// <summary>
    /// Save the ordinal values.
    /// </summary>
    private readonly Dictionary<string, int> _ordinals = new Dictionary<string, int>();

    /// <summary>
    /// Save the column name.
    /// </summary>
    private readonly Dictionary<int, string> _columnName = new Dictionary<int, string>();

    /// <summary>
    /// Save the table name for that column.
    /// </summary>
    private readonly Dictionary<int, string> _columnTableName = new Dictionary<int, string>();
    
    /// <summary>
    /// Save the field types.
    /// </summary>
    private readonly Dictionary<int, Type> _fieldTypes = new Dictionary<int, Type>();

    /// <summary>
    /// Save the field names.
    /// </summary>
    private readonly Dictionary<int, string> _dataTypeName = new Dictionary<int, string>();

    /// <summary>
    /// Get the number of fields.
    /// If the value is null, we will ask the server, otherwise it is cached.
    /// </summary>
    private int? _fieldCount;

    /// <summary>
    /// Check if we have any rows.
    /// If the value is null, we will ask the server, otherwise it is cached.
    /// </summary>
    private bool? _hasRows;

    /// <summary>
    /// The SQLite Connection
    /// </summary>
    private readonly ConnectionsController _controller;

    /// <summary>
    /// The command server guid.
    /// </summary>
    private readonly string _commandGuid;

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
      
      // get the row.
      var row = GetGuiOnlyValue<List<Field>>(SQLiteMessage.ExecuteReaderGetRowRequest );
      _currentRowInformation = new RowInformation();
      var ordinal = 0;
      foreach (var column in row)
      {
        _currentRowInformation.Add( new ColumnInformation(column, ordinal++, column.Name));
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
    #endregion

    public SQLiteServerDataReaderClientWorker(ConnectionsController controller, string commandGuid, int queryTimeouts)
    {
      if (null == controller)
      {
        throw new ArgumentNullException( nameof(controller), "The controller cannot be null.");
      }
      _controller = controller;
      _commandGuid = commandGuid;
      _queryTimeouts = queryTimeouts;
    }

    /// <inheritdoc />
    public void ExecuteReader(CommandBehavior commandBehavior)
    {
      var getValue = new IndexRequest()
      {
        Guid = _commandGuid,
        Index = (int)commandBehavior
      };
      var fields = Fields.Fields.SerializeObject(getValue);

      var response = _controller.SendAndWaitAsync(SQLiteMessage.ExecuteReaderRequest, fields.Pack(), _queryTimeouts).Result;
      switch (response.Message)
      {
        case SQLiteMessage.SendAndWaitTimeOut:
          throw new TimeoutException("There was a timeout error creating the Command.");

        case SQLiteMessage.ExecuteReaderResponse:
          var result = response.Get<int>();
          if( 1 != result )
          {
            throw new SQLiteServerException($"Received an unexpected error {result} from the server.");
          }
          return;

        case SQLiteMessage.ExecuteReaderException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException($"Unknown response {response.Message} from the server.");
      }
    }

    /// <inheritdoc />
    public bool Read()
    {
      if (GetGuiOnlyValue<int>(SQLiteMessage.ExecuteReaderReadRequest) == 0)
      {
        return false;
      }

      // we need to reset some field now.
      _dataTypeName.Clear();
      _fieldTypes.Clear();
      _currentRowInformation = null;
      return true;
    }

    /// <inheritdoc />
    public bool NextResult()
    {
      return GetGuiOnlyValue<int>(SQLiteMessage.ExecuteReaderNextResultRequest) != 0;
    }
    
    /// <inheritdoc />
    public int FieldCount
    {
      get
      {
        if (_fieldCount != null)
        {
          return (int) _fieldCount;
        }
        _fieldCount = GetGuiOnlyValue<int>(SQLiteMessage.ExecuteReaderFieldCountRequest);
        return (int)_fieldCount;
      }
    }

    /// <inheritdoc />
    public bool HasRows
    {
      get
      {
        if (_hasRows != null)
        {
          return (bool)_hasRows;
        }
        _hasRows = GetGuiOnlyValue<bool>(SQLiteMessage.ExecuteReaderHasRowsRequest);
        return (bool)_hasRows;
      }
    }

    /// <inheritdoc />
    public object this[int ordinal] => GetValue(ordinal);

    /// <inheritdoc />
    public object this[string name] => GetValue(GetOrdinal(name));

    /// <inheritdoc />
    public int GetOrdinal(string name)
    {
      var lname = name.ToLower();
      if (_ordinals.ContainsKey(lname))
      {
        return _ordinals[lname];
      }
      // get the value
      var value = GetNamedValueAsync<int>(SQLiteMessage.ExecuteReaderGetOrdinalRequest, lname).Result;

      // save it.
      _ordinals[lname] = value;

      // return it.
      return value;
    }

    /// <summary>
    /// Get the result of a requres.t
    /// </summary>
    /// <param name="requestType"></param>
    /// <returns></returns>
    private T GetGuiOnlyValue<T>(SQLiteMessage requestType)
    {
      var response = _controller.SendAndWaitAsync(requestType, Encoding.ASCII.GetBytes(_commandGuid), _queryTimeouts).Result;
      switch (response.Message)
      {
        case SQLiteMessage.SendAndWaitTimeOut:
          throw new TimeoutException("There was a timeout error executing the read request from the reader.");

        case SQLiteMessage.ExecuteReaderGetRowResponse:
          var bytes = response.Payload;
          return Field.Unpack(bytes).Get<T>();

        case SQLiteMessage.ExecuteReaderResponse:
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
    private T GetIndexedValue<T>(SQLiteMessage requestType, int index)
    {
      var getValue = new IndexRequest()
      {
        Guid = _commandGuid,
        Index = index
      };
      var fields = Fields.Fields.SerializeObject(getValue);

      var response = _controller.SendAndWaitAsync(requestType, fields.Pack(), _queryTimeouts).Result;
      switch (response.Message)
      {
        case SQLiteMessage.SendAndWaitTimeOut:
          throw new TimeoutException("There was a timeout error executing the read request from the reader.");

        case SQLiteMessage.ExecuteReaderResponse:
          return response.Get<T>();

        case SQLiteMessage.ExecuteReaderException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException($"Unknown response {response.Message} from the server.");
      }
    }

    /// <summary>
    /// Get a value from the server by name.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="requestType"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private async Task<T> GetNamedValueAsync<T>(SQLiteMessage requestType, string name )
    {
      var getValue = new NameRequest()
      {
        Guid = _commandGuid,
        Name = name
      };
      var fields = Fields.Fields.SerializeObject(getValue);

      var response = await _controller.SendAndWaitAsync(requestType, fields.Pack(), _queryTimeouts).ConfigureAwait( false );
      switch (response.Message)
      {
        case SQLiteMessage.SendAndWaitTimeOut:
          throw new TimeoutException("There was a timeout error executing the read request from the reader.");

        case SQLiteMessage.ExecuteReaderResponse:
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
      return GetIndexedValue<short>(SQLiteMessage.ExecuteReaderGetInt16Request, i);
    }

    /// <inheritdoc />
    public int GetInt32(int i)
    {
      return GetIndexedValue<int>( SQLiteMessage.ExecuteReaderGetInt32Request, i);
    }

    /// <inheritdoc />
    public long GetInt64(int i)
    {
      return GetIndexedValue<long>(SQLiteMessage.ExecuteReaderGetInt64Request, i);
    }

    /// <inheritdoc />
    public double GetDouble(int i)
    {
      return GetIndexedValue<double>(SQLiteMessage.ExecuteReaderGetDoubleRequest, i);
    }

    /// <inheritdoc />
    public string GetString(int i)
    {
      return GetIndexedValue<string>(SQLiteMessage.ExecuteReaderGetStringRequest, i);
    }

    /// <inheritdoc />
    public string GetDataTypeName(int i)
    {
      if (_dataTypeName.ContainsKey(i))
      {
        return _dataTypeName[i];
      }

      // get the value
      var dataTypeName = GetIndexedValue<string>(SQLiteMessage.ExecuteReaderGetDataTypeNameRequest, i);

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
      if (_fieldTypes.ContainsKey(i))
      {
        return _fieldTypes[i];
      }

      // get the value
      var fieldType = (FieldType)GetIndexedValue<int>(SQLiteMessage.ExecuteReaderGetFieldTypeRequest, i);
      var systemType = Field.FieldTypeToType(fieldType);

      // save it
      _fieldTypes[i] = systemType;

      // return it.
      return systemType;
    }

    /// <inheritdoc />
    public object GetValue(int i)
    {
      // get the current row.
      var column = GetColumn(i);
      if (null != column)
      {
        // get the value for this column.
        return column.Get<object>();
      }

      // get the data type
      var type = GetFieldType(i);

      if (type == typeof(short))
      {
        return GetInt16(i);
      }

      if (type == typeof(int))
      {
        return GetInt32(i);
      }

      if (type == typeof(long))
      {
        return GetInt64(i);
      }

      if (type == typeof(double))
      {
        return GetDouble(i);
      }
      
      if (type == typeof(string))
      {
        return GetString(i);
      }

      // not yet supported
      // we need byte[], short as well as double
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool IsDBNull(int i)
    {
      return GetIndexedValue<bool>(SQLiteMessage.ExecuteReaderGetIsDBNullRequest, i);
    }

    /// <inheritdoc />
    public string GetName(int i)
    {
      if (_columnName.ContainsKey(i))
      {
        return _columnName[i];
      }
      // get the name
      var name = GetIndexedValue<string>(SQLiteMessage.ExecuteReaderGetNameRequest, i);

      // set the value
      _columnName[i] = name;

      // return it.
      return name;
    }

    /// <inheritdoc />
    public string GetTableName(int i)
    {
      if (_columnTableName.ContainsKey(i))
      {
        return _columnTableName[i];
      }
      // get the name
      var name = GetIndexedValue<string>(SQLiteMessage.ExecuteReaderGetTableNameRequest, i);

      // the name cannot be null
      name = name ?? string.Empty;

      // set the value
      _columnTableName[i] = name;

      // return it.
      return name;
    }
  }
}
