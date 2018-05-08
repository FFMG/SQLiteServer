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
using System.Linq;

namespace SQLiteServer.Data.Data
{
  internal class RowInformation
  {
    /// <summary>
    /// The list of colums
    /// </summary>
    private readonly List<ColumnInformation> _columns = new List<ColumnInformation>();

    /// <summary>
    /// The number of items in the list.
    /// </summary>
    public int Count => _columns.Count;

    /// <summary>
    /// Add a column/name to the list.
    /// </summary>
    /// <param name="column"></param>
    public void Add(ColumnInformation column)
    {
      if (null == column)
      {
        throw new ArgumentNullException(nameof(column));
      }

      // check if we have duplicates
      if (GetOrDefault(column.Ordinal) != null )
      {
        throw new DuplicateNameException();
      }
      if (GetOrDefault(column.Name) != null)
      {
        throw new DuplicateNameException();
      }

      // add it to the list.
      _columns.Add(column);
    }

    /// <summary>
    /// Get a single column, 
    /// return null if it does not exist.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private ColumnInformation GetOrDefault(string name)
    {
      if (string.IsNullOrWhiteSpace(name))
      {
        throw new ArgumentException("The name cannot be empty or null");
      }
      return _columns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get a single column by index, return null if it does not exist.
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    private ColumnInformation GetOrDefault(int ordinal)
    {
      if (ordinal < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(ordinal), "The index cannot be negative.");
      }
      return _columns.FirstOrDefault(c => c.Ordinal == ordinal);
    }

    /// <summary>
    /// Get a single column by index
    /// We will throw if the value does not exist.
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public ColumnInformation Get(int ordinal)
    {
      var col = GetOrDefault(ordinal);
      if (null == col )
      {
        throw new ArgumentOutOfRangeException(nameof(ordinal), "The ordinal given does not exist.");
      }

      // return the column
      return col;
    }

    /// <summary>
    /// Get a single column, throw if it does not exist
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ColumnInformation Get(string name )
    {
      var col = GetOrDefault(name);
      if (null == col)
      {
        throw new ArgumentOutOfRangeException(nameof(name), "The name given does not exist.");
      }

      // return the column
      return col;
    }
  }
}
