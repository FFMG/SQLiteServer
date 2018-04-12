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
using System.ComponentModel;
using System.Data.Common;

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
    private Hashtable _properties;
    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Construnct with no connection string.
    /// </summary>
    public SQLiteServerConnectionStringBuilder() : this( null )
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

    /// <summary>
    /// Gets/Sets the filename or memory to open on the connection string.
    /// </summary>
    public string DataSource
    {
      get
      {
        TryGetValue("data source", out var value);
        return value?.ToString();
      }
      set => this["data source"] = value;
    }
  }
}
