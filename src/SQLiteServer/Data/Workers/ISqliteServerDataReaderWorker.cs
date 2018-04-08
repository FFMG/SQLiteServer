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

namespace SQLiteServer.Data.Workers
{
  internal interface ISqliteServerDataReaderWorker
  {
    /// <summary>
    /// Get a value at a given colum name.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    object this[int i ] { get; }

    /// <summary>
    /// Get a value at a given name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    object this[string name] { get; }

    /// <summary>
    /// Get the number of retrieved fields.
    /// </summary>
    int FieldCount { get; }

    /// <summary>
    /// Check if we have any rows of data.
    /// </summary>
    bool HasRows { get; }

    /// <summary>
    /// Execute a 'read' operation
    /// </summary>
    /// <param name="commandBehavior">The mae of the column.</param>
    /// <returns></returns>
    void ExecuteReader(CommandBehavior commandBehavior);

    /// <summary>
    /// Prepare to read the next value.
    /// </summary>
    /// <returns></returns>
    bool Read();

    /// <summary>
    /// Retrieves the column index given its name.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <returns>int the index</returns>
    int GetOrdinal(string name);

    /// <summary>
    /// Retrieves the column as a string
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>string</returns>
    string GetString(int i);

    /// <summary>
    /// Retrieves the column as a short
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>short</returns>
    short GetInt16(int i);

    /// <summary>
    /// Retrieves the column as an int
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>int</returns>
    int GetInt32(int i);

    /// <summary>
    /// Retrieves the column as a long
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>long</returns>
    long GetInt64(int i);

    /// <summary>
    /// Retrieves the column as a double
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>double</returns>
    double GetDouble(int i );

    /// <summary>
    /// Get the field type for a given colum
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns></returns>
    Type GetFieldType(int i);

    /// <summary>
    /// Get the field name for a given colum
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns></returns>
    string GetDataTypeName(int i);

    /// <summary>
    /// Get a raw value 
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns></returns>
    object GetValue(int i);

    /// <summary>
    /// Get the column name.
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>string the column name</returns>
    string GetName(int i);

    /// <summary>
    /// Get the table name for a selected column.
    /// </summary>
    /// <param name="i"></param>
    /// <returns>string the table name</returns>
    string GetTableName(int i);

    /// <summary>
    /// Gets a value that indicates whether the column contains non-existent or missing values.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    bool IsDBNull(int i);
  }
}