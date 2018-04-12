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
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using static System.ComponentModel.TypeDescriptor;

namespace SQLiteServer.Data.SQLiteServer
{
  /// <inheritdoc />
  // ReSharper disable once InconsistentNaming
  public sealed class SQLiteServerConnectionStringBuilder : DbConnectionStringBuilder
  {
    #region Private

    /// <summary>
    /// Properties of this class
    /// </summary>
    private readonly Hashtable _properties;

    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Construnct with no connection string.
    /// </summary>
    public SQLiteServerConnectionStringBuilder() : this(null)
    {
    }

    /// <inheritdoc />
    /// <summary>
    /// Construnct with a given connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to parse</param>
    public SQLiteServerConnectionStringBuilder(string connectionString)
    {
      _properties = new Hashtable(StringComparer.OrdinalIgnoreCase);
      GetProperties(_properties);

      if (!string.IsNullOrEmpty(connectionString))
      {
        ConnectionString = connectionString;
      }
    }

    private const string KeyUseUtf16Encoding = "UseUtf16Encoding";
    private const string KeyVersion = "Version";
    private const string KeyPooling = "Pooling";
    private const string KeyReadOnly = "Read Only";
    private const string KeyDataSource = "Data Source";
    private const string KeyUri = "Uri";
    private const string KeyFullUri = "FullUri";
    private const string KeySynchronous = "Synchronous";
    private const string KeyDefaultTimeOut = "Default Timeout";
    private const string KeyBusyTimeOut = "BusyTimeout";
    private const int DefaultVersion = 3;
    private const int DefaultDefaultTimeOut = 30;
    private const int DefaultBusyTimeOut = 0;
    private const string DefaultDataSource = "";
    private const string DefaultUri = null;
    private const string DefaultFullUri = null;
    private const bool DefaultPooling = false;
    private const bool DefaultReadOnly = false;
    private const SynchronizationModes DefaultSynchronous = SynchronizationModes.Normal;
    private const bool DefaultUseUtf16Encoding = false;

    private T GetValue<T>(string key, T defaultValue)
    {
      object value;
      if (!TryGetValue(key, out value))
      {
        return defaultValue;
      }

      // int/uint
      if (typeof(T) == typeof(int) ||
          typeof(T) == typeof(uint))
      {
        return (T) Convert.ChangeType(
          Convert.ToInt32(value, CultureInfo.CurrentCulture),
          typeof(T));
      }

      // string
      if (typeof(T) == typeof(string))
      {
        if (value == null)
        {
          return defaultValue;
        }

        if (value is string)
        {
          return (T) Convert.ChangeType(value, typeof(T));
        }
      }

      // bool
      if (typeof(T) == typeof(bool))
      {
        if (value == null)
        {
          return defaultValue;
        }

        bool returnValue;
        if (value is string)
        {
          var s = Convert.ToString(value).ToLower();
          switch (s)
          {
            case "true":
              returnValue = true;
              break;

            case "false":
              returnValue = false;
              break;

            default:
              returnValue = s != "0";
              break;
          }
        }
        else if (value is bool)
        {
          returnValue = (bool) value;
        }
        else
        {
          returnValue = Convert.ToInt64(value) != 0;
        }

        return (T) Convert.ChangeType(returnValue, typeof(T));
      }

      if (typeof(T) == typeof(SynchronizationModes))
      {
        SynchronizationModes returnValue;
        if (value is string)
        {
          try
          {
            returnValue = (SynchronizationModes) GetConverter(typeof(SynchronizationModes)).ConvertFrom(value);
          }
          catch
          {
            return defaultValue;
          }
        }
        else
        {
          returnValue = (SynchronizationModes) value;
        }

        return (T) Convert.ChangeType(returnValue, typeof(T));
      }

      // not sure what this is...
      throw new InvalidCastException("Unable to cast to this data type.");
    }

    /// <summary>
    /// If we are using utf16 encoding
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool UseUTF16Encoding
    {
      get { return GetValue(KeyUseUtf16Encoding, DefaultUseUtf16Encoding); }
      set { this[KeyUseUtf16Encoding] = value; }
    }

    /// <summary>
    /// Use Connection pooling or not.
    /// </summary>
    public bool Pooling
    {
      get { return GetValue(KeyPooling, DefaultPooling); }
      set { this[KeyPooling] = value; }
    }

    /// <summary>
    /// Get if the database is in readonly mode or not.
    /// </summary>
    public bool ReadOnly
    {
      get { return GetValue(KeyReadOnly, DefaultReadOnly);}
      set { this[KeyReadOnly] = value; }
    }

    /// <summary>
    /// Get the version number
    /// </summary>
    public int Version
    {
      get { return GetValue(KeyVersion, DefaultVersion); }
      set
      {
        if (value != DefaultVersion)
        {
          throw new NotSupportedException();
        }

        this["version"] = value;
      }
    }

    /// <summary>
    /// Gets the default command timeout.
    /// </summary>
    public int DefaultTimeout
    {
      get { return GetValue(KeyDefaultTimeOut, DefaultDefaultTimeOut); }
      set { this[KeyDefaultTimeOut] = value; }
    }

    /// <summary>
    /// Get the busy timeout to stop long queries.
    /// </summary>
    public int BusyTimeout
    {
      get { return GetValue(KeyBusyTimeOut, DefaultBusyTimeOut); }
      set { this[KeyBusyTimeOut] = value; }
    }

    /// <summary>
    /// Get the Synchronization Mode
    /// </summary>
    public SynchronizationModes SyncMode
    {
      get { return GetValue(KeySynchronous, DefaultSynchronous); }
      set { this[KeySynchronous] = value; }
    }

    /// <summary>
    /// Gets/Sets the filename or memory to open on the connection string.
    /// </summary>
    public string DataSource
    {
      get { return GetValue(KeyDataSource, DefaultDataSource); }
      set { this[KeyDataSource] = value; }
    }

    /// <summary>
    /// Alternate Data source.
    /// </summary>
    public string Uri
    {
      get { return GetValue(KeyUri, DefaultUri); }
      set { this[KeyUri] = value; }
    }

    /// <summary>
    /// Alternate Data source.
    /// </summary>
    public string FullUri
    {
      get { return GetValue(KeyFullUri, DefaultFullUri); }
      set { this[KeyFullUri] = value; }
    }
  }
}